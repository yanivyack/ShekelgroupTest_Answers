using ENV.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace ENV.Remoting
{
    public class WsdlSystinetTranslator
    {
        XmlDocument wsdl = new XmlDocument();
        string _prefix;
        public WsdlSystinetTranslator(string wsdlFile, string prefix)
        {
            _prefix = prefix;
            wsdl.Load(wsdlFile);
        }

        Dictionary<string, string> _messageToType = new Dictionary<string, string>();
        Dictionary<string, List<XsdElement>> _complexTypes = new Dictionary<string, List<XsdElement>>();
        internal class XsdElement
        {
            public string Name;
            public string Type;
            public bool nillable;
            public bool isArray;
            public override string ToString()
            {
                return Name + " (type=" + Type + ")" + (isArray ? "array" : "");
            }
        }
        public void ApplyTo(SystinetSoapRequestResponseXmlTransformer tr)
        {
            foreach (XmlNode rootItems in wsdl.LastChild)
            {
                if (rootItems.LocalName == "types")
                {
                    foreach (XmlNode schema in rootItems.ChildNodes)
                    {
                        if (schema.LocalName == "schema")
                        {
                            foreach (XmlNode schemaItems in schema.ChildNodes)
                            {
                                if (schemaItems.LocalName == "complexType" || schemaItems.LocalName == "element")
                                {
                                    var elements = new List<XsdElement>();
                                    var complexType = schemaItems;
                                    if (schemaItems.LocalName == "element")
                                        complexType = schemaItems.LastChild;

                                    if (complexType != null)
                                        foreach (XmlNode sequence in complexType.ChildNodes)
                                        {
                                            if (sequence.LocalName == "sequence")
                                            {
                                                foreach (XmlNode element in sequence.ChildNodes)
                                                {
                                                    bool allowNull = false;
                                                    var x = element.Attributes["nillable"];
                                                    if (x != null)
                                                        allowNull = x.InnerText == "true";
                                                    bool array = false;
                                                    x = element.Attributes["maxOccurs"];
                                                    if (x != null)
                                                        array = x.InnerText != "0";
                                                    elements.Add(new XsdElement
                                                    {
                                                        Name = element.Attributes["name"].InnerText,
                                                        Type = element.Attributes["type"].InnerText.Substring(4),
                                                        nillable = allowNull,
                                                        isArray = array
                                                    });
                                                }
                                            }
                                        }
                                    _complexTypes.Add(schemaItems.Attributes["name"].InnerText, elements);
                                }
                                if (schemaItems.LocalName == "element")
                                {

                                }
                            }
                        }
                    }
                }

                if (rootItems.LocalName == "portType")
                {
                    foreach (XmlNode operation in rootItems.ChildNodes)
                    {
                        if (operation.LocalName == "operation")
                        {
                            string name = operation.Attributes["name"].InnerText;
                            string outputType = null;
                            foreach (XmlNode output in operation.ChildNodes)
                            {
                                if (output.LocalName == "output")
                                    outputType = _messageToType[output.Attributes["message"].InnerText.Substring(4)];
                            }

                            tr.AddOperationMapping(name, new SystinetRequestReponseNamespaceMapping(this, outputType)
                            {
                                Prefix = _prefix,
                                ReponseNamespace = "com.edeveloper.cw",
                            });
                        }
                    }
                }
                else if (rootItems.LocalName == "message")
                {
                    _messageToType.Add(rootItems.Attributes["name"].InnerText, rootItems.FirstChild.Attributes["element"].InnerText.Substring(4));
                }
                else if (rootItems.LocalName == "binding")
                {

                }
            }
        }

        internal void setTypeAttribute(XmlElement root, string type, string prefix)
        {
            if (!_complexTypes.ContainsKey(type))
                prefix = "d";
            var typeAttribute = root.OwnerDocument.CreateAttribute("type", XmlSchema.InstanceNamespace);
            typeAttribute.Value = prefix + ":" + type;
            root.Attributes.Append(typeAttribute);
        }

        internal void PopulateBasedOn(List<XsdElement> sequence, XmlElement result, XmlNode input)
        {
            foreach (var item in sequence)
            {
                bool found = false;
                List<XsdElement> itemType;
                bool complexType = _complexTypes.TryGetValue(item.Type, out itemType);
                var itemElement = result.OwnerDocument.CreateElement(item.Name, result.NamespaceURI);
                result.AppendChild(itemElement);

                Action<XmlNode> addItem = matchingItem =>
                {
                    if (matchingItem.LocalName == item.Name)
                    {
                        found = true;
                        if (complexType && itemType[0].isArray)
                        {
                            var arrayItemElement = result.OwnerDocument.CreateElement(itemType[0].Name, result.NamespaceURI);
                            itemElement.AppendChild(arrayItemElement);
                            setTypeAttribute(arrayItemElement, itemType[0].Type, _prefix);
                            var childItems = _complexTypes[itemType[0].Type];
                            if (childItems.Count == 1 && !childItems[0].isArray)
                            {
                                List<XsdElement> grandChildItems;
                                if (_complexTypes.TryGetValue(childItems[0].Type, out grandChildItems))
                                {
                                    if (grandChildItems.Count == 1 && grandChildItems[0].isArray)
                                    {
                                        var x = result.OwnerDocument.CreateElement(childItems[0].Name, result.NamespaceURI);
                                        setTypeAttribute(x, childItems[0].Type, _prefix);
                                        var y = result.OwnerDocument.CreateElement(grandChildItems[0].Name, result.NamespaceURI);
                                        setTypeAttribute(y, grandChildItems[0].Type, _prefix);
                                        x.AppendChild(y);
                                        arrayItemElement.AppendChild(x);
                                        arrayItemElement = y;
                                        childItems = _complexTypes[grandChildItems[0].Type];
                                    }
                                }
                            }
                            PopulateBasedOn(childItems, arrayItemElement, matchingItem);
                        }
                        else if (complexType)
                        {
                            PopulateBasedOn(itemType, itemElement, matchingItem);
                        }
                        else
                            itemElement.InnerText = matchingItem.InnerText;

                    }
                };
                foreach (XmlNode matchingItem in input.ChildNodes)
                {
                    addItem(matchingItem);
                }

                foreach (XmlNode matchingItem in input.Attributes)
                {
                    addItem(matchingItem);
                }
                if (found)
                    setTypeAttribute(itemElement, item.Type, complexType ? _prefix : "d");
                else if (item.Name == "value" && !string.IsNullOrEmpty(input.InnerText))
                {
                    setTypeAttribute(itemElement, item.Type, complexType ? _prefix : "d");
                    itemElement.InnerText = input.InnerText;
                }
                else
                {
                    var attr = result.OwnerDocument.CreateAttribute("i", "nil", XmlSchema.InstanceNamespace);
                    attr.Value = "true";
                    itemElement.Attributes.Append(attr);
                }
            }


        }

        internal List<XsdElement> GetComplexType(string outputType)
        {
            return _complexTypes[outputType];
        }
    }
}
