using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using ENV.Data.Storage;
using ENV.IO;
using Firefly.Box;
using Firefly.Box.Data.Advanced;
using Firefly.Box.Data.DataProvider;
using Org.BouncyCastle.Asn1.Ocsp;

namespace ENV.Data.DataProvider
{
    public class XmlEntityDataProvider : IEntityDataProvider
    {
        public static bool UseReadOnlyAccess = false;
        public static XmlEntityDataProvider Instance
        {
            get
            {
                var x = Firefly.Box.Context.Current["XmlEntityDataProvider"] as XmlEntityDataProvider;
                if (x == null)
                {
                    x = new XmlEntityDataProvider();

                    //      x = new Firefly.Box.Data.DataProvider.UnderConstruction.InMemoryDatabase();
                    Firefly.Box.Context.Current["XmlEntityDataProvider"] = x;
                }
                return x;
            }
        }
        public void Dispose()
        {

        }
        private static bool IsStupidMenuColumn(ColumnBase column)
        {
            return column.Name == "Didn't find item name" && column.Caption == "Menu";
        }

        internal interface XmlDataProviderHelper : IDisposable
        {
            void LoadTo(XmlDocument doc);
            void SaveChanges(XmlDocument doc);
        }
        public static bool SaveAnsiAsUTF8 { get; set; }
        class XmlColumnProviderHelper : XmlDataProviderHelper
        {
            Firefly.Box.Data.ByteArrayColumn _column;
            XmlEntityDataProvider _parent;

            public XmlColumnProviderHelper(Firefly.Box.Data.ByteArrayColumn column, XmlEntityDataProvider parent)
            {
                _column = column;
                _parent = parent;
            }

            public void Dispose()
            {
                _parent._activeColumnEntities.Remove(_column);
            }

            System.Text.Encoding GetEncoding()
            {
                var c = _column as ByteArrayColumn;
                if (c != null)
                    if (c.ContentType == ByteArrayColumnContentType.Unicode)
                        return ByteArrayColumn.UnicodeWithoutGaps;
                    else if (c.ContentType == ByteArrayColumnContentType.Ansi)
                        return ENV.IO.XmlHelper.DefaultEncoding;
                return new System.Text.UTF8Encoding(false);
            }

            public void LoadTo(XmlDocument doc)
            {
                if (_column.Value != null)
                {
                    var bac = _column as ENV.Data.ByteArrayColumn;
                    if (bac != null && bac.ContentType == ByteArrayColumnContentType.Unicode)
                    {
                        if (bac.Value.Length > 2 && bac.Value[1] == 0)
                            doc.LoadXml(bac.ToString());
                        else
                            doc.LoadXml(System.Text.Encoding.UTF8.GetString(bac.Value));
                        return;
                    }
                    try
                    {
                        using (var ms = new MemoryStream(_column.Value))
                        {
                            doc.Load(ms);
                        }
                    }
                    catch (XmlException)
                    {
                        if (bac.ContentType == ByteArrayColumnContentType.Ansi)
                        {
                            using (var ms = new StreamReader(new MemoryStream(_column.Value), LocalizationInfo.Current.OuterEncoding, false))
                            {
                                doc.Load(ms);
                                return;
                            }
                        }
                        throw;
                    }
                }
            }

            public void SaveChanges(XmlDocument doc)
            {

                var e = GetEncoding();

                using (var ms = new MemoryStream())
                {
                    using (var sww = new StreamWriter(ms, e))
                    {
                        doc.Save(sww);
                    }
                    _column.Value = e.GetBytes(FixResultXml(e, ms));
                }


            }
        }

        class UrlXmlDataProviderHelper : XmlDataProviderHelper
        {

            XmlEntityDataProvider _parent;
            string _key;
            string _fileName;
            public UrlXmlDataProviderHelper(string filename, XmlEntityDataProvider parent, string key)
            {
                _parent = parent;
                _key = key;
                _fileName = filename;
            }

            public void Dispose()
            {

                _parent._activeFileEntities.Remove(_key);
            }

            public void LoadTo(XmlDocument doc)
            {

                doc.Load(_fileName);
            }

            public void SaveChanges(XmlDocument doc)
            {

            }
        }
        class FileXmlDataProviderHelper : XmlDataProviderHelper
        {
            System.IO.Stream _stream;
            XmlEntityDataProvider _parent;
            string _key;
            bool _readonly;
            public FileXmlDataProviderHelper(string filename, XmlEntityDataProvider parent, string key, bool @readonly)
            {
                _readonly = @readonly;
                _parent = parent;
                _key = key;
                if (@readonly)
                    _stream = new FileStream(filename, FileMode.Open, FileAccess.Read);
                else
                    _stream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            }

            public void Dispose()
            {
                _stream.Close();
                _stream.Dispose();
                _parent._activeFileEntities.Remove(_key);
            }

            public void LoadTo(XmlDocument doc)
            {
                if (_stream.Length != 0)
                    doc.Load(_stream);
            }

            public void SaveChanges(XmlDocument doc)
            {
                if (_readonly)
                    return;
                _stream.SetLength(0);

                var enc = ENV.IO.XmlHelper.DefaultEncoding;
                using (var ms = new MemoryStream())
                {
                    using (var sw = new StreamWriter(ms, enc))
                    {
                        doc.Save(sw);
                    }
                    using (var sw = new StreamWriter(_stream, enc))
                        sw.Write(FixResultXml(enc, ms));
                }
            }


        }
        private static string FixResultXml(System.Text.Encoding enc, MemoryStream ms)
        {
            return enc.GetString(ms.ToArray()).Replace(" />", "/>")
                .Replace("<?xml version=\"1.0\" encoding=\"utf-8\" standalone=\"no\"?>", "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\" ?>")
                .Replace("<?xml version=\"1.0\" encoding=\"iso-8859-1\" standalone=\"no\"?>", "<?xml version=\"1.0\" encoding=\"ISO-8859-1\" standalone=\"no\" ?>")
            .Replace("<?xml version=\"1.0\" encoding=\"utf-16\" standalone=\"no\"?>", "<?xml version=\"1.0\" encoding=\"UTF-16\" standalone=\"no\" ?>")
            .Replace("<mgns3:SplitDutyVATIndicator/>", "");
        }
        internal class XmlFileEntityDataProvider
        {


            XmlDocument _doc = new XmlDocument();
            XmlNamespaceManager _ns;
            XmlDataProviderHelper _helper;

            public XmlFileEntityDataProvider(XmlDataProviderHelper helper, Dictionary<string, string> namespaces)
            {

                _helper = helper;

                try
                {

                    _helper.LoadTo(_doc);

                }
                catch
                {



                }
                if (_doc.ChildNodes.Count == 0)
                {
                    _doc.AppendChild(_doc.CreateXmlDeclaration("1.0", "UTF-8", "no"));
                }
                _ns = new XmlNamespaceManager(_doc.NameTable);
                foreach (var item in namespaces)
                {
                    _ns.AddNamespace(item.Key, item.Value);

                }




            }

            Dictionary<string, DataSetDataProvider> _datasets = new Dictionary<string, DataSetDataProvider>();
            public IRowsSource ProvideRowSource(Firefly.Box.Data.Entity entity)
            {
                var e = GetXmlEntity(entity);
                if (_schemaRoot == null)
                    _schemaRoot = e.GetSchemaRoot();
                var p = e.GetPath();
                var y = p.Split('/');
                DataSetDataProvider edp;
                if (!_datasets.TryGetValue(p, out edp))
                {
                    edp = new DataSetDataProvider();
                    var rowSource = ((IEntityDataProvider)edp).ProvideRowsSource(entity);

                    XmlNode parent = _doc;
                    IterateXml(e, y, 0, parent, rowSource);
                    edp.Dispose();
                    _datasets.Add(p, edp);
                }

                if (edp.DataSet.Tables.Count > 0)
                    edp.DataSet.Tables[0].TableName = e.EntityName;

                return new XmlRowsSource(((IEntityDataProvider)edp).ProvideRowsSource(entity), this, e, y);
            }
            SchemaItem _schemaRoot;

            SchemaItem GetSchemaItemFor(XmlNode node, XmlEntity e)
            {
                if (_schemaRoot == null)
                    return null;
                if (node == _doc)
                    return null;
                SchemaItem result;

                if (node.ParentNode == _doc)
                {
                    result = e.GetSchemaRoot();

                }
                else
                {
                    var parent = GetSchemaItemFor(node.ParentNode, e);
                    string nodeName = node.LocalName;
                    foreach (var item in e.Namespaces)
                    {
                        if (node.NamespaceURI.Equals(item.Value, StringComparison.InvariantCultureIgnoreCase))
                        {
                            nodeName = item.Key + ":" + nodeName;
                            break;
                        }
                    }

                    result = parent.GetSchemaFor(nodeName);
                }
                return result;
            }


            internal bool CompareName(XmlNode xmlNode, string elementName, XmlEntity e)
            {
                elementName = elementName.TrimEnd();
                if (elementName.StartsWith(":"))
                    elementName = elementName.Substring(1);
                if (elementName.Contains(":"))
                {
                    var namespaceInSchema = elementName.Remove(elementName.IndexOf(':'));
                    return xmlNode.LocalName == elementName.Substring(elementName.IndexOf(':') + 1) &&
                        (e.Namespaces[namespaceInSchema] == xmlNode.NamespaceURI || xmlNode.ParentNode is XmlDocument);


                }
                return xmlNode.LocalName == elementName;
            }
            Dictionary<string, string> _keysFromSchemaToKeysInThisXml = new Dictionary<string, string>();
            bool FindNamespaceInThisXml(string schemaKey, XmlEntity e, out string xmlKey)
            {

                if (_keysFromSchemaToKeysInThisXml.TryGetValue(schemaKey, out xmlKey))
                    return true;
                if (_doc.DocumentElement == null)
                    return false;
                var namespaceUrl = e.Namespaces[schemaKey];
                foreach (XmlAttribute item in _doc.DocumentElement.Attributes)
                {
                    if (item.Name.StartsWith("xmlns:") && namespaceUrl.Equals(item.Value, StringComparison.InvariantCultureIgnoreCase))
                    {
                        xmlKey = item.Name.Substring("xmlns:".Length);
                        _keysFromSchemaToKeysInThisXml.Add(schemaKey, xmlKey);
                        return true;
                    }
                }
                return false;
            }
            string GetXmlnsFreeAlias()
            {
                int j = 1;
                string mgnsName = null;
                bool found = false;
                do
                {
                    found = false;
                    mgnsName = "mgns" + j++;

                    if (_doc.DocumentElement != null)
                    {
                        foreach (XmlAttribute item in _doc.DocumentElement.Attributes)
                        {
                            if (item.Name == "xmlns:" + mgnsName)
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                    if (!found)
                        return mgnsName;
                } while (true);

            }

            XmlElement AddChild(XmlNode parent, string name, XmlEntity e)
            {
                Action afterAdd = delegate { };
                XmlElement child;
                {
                    int i = name.IndexOf(':');
                    if (i >= 0)
                    {
                        var y = name.Split(':');
                        var namespaceSchema = e.Namespaces[y[0]];

                        string namespaceInThisXmlFile;
                        if (!FindNamespaceInThisXml(y[0], e, out namespaceInThisXmlFile))
                        {
                            if (parent.NamespaceURI.Equals(namespaceSchema, StringComparison.InvariantCultureIgnoreCase) && parent.Prefix == "")

                            {
                                namespaceInThisXmlFile = "";
                            }
                            else
                            {
                                var mgnsName = GetXmlnsFreeAlias();
                                namespaceInThisXmlFile = mgnsName;
                                _keysFromSchemaToKeysInThisXml.Add(y[0], namespaceInThisXmlFile);
                                var x = _doc.CreateAttribute("xmlns:" + mgnsName);
                                x.Value = namespaceSchema;

                                afterAdd = () =>
                                {
                                    _doc.DocumentElement.SetAttributeNode(x);
                                    afterAdd = () => { };
                                };
                                if (!(parent is XmlDocument))
                                {
                                    afterAdd();
                                }
                            }
                        }




                        child = _doc.CreateElement(namespaceInThisXmlFile, y[1], namespaceSchema);
                        if (_doc.DocumentElement == null)
                        {
                            _doc.AppendChild(child);
                            afterAdd();
                            return child;
                        }


                    }

                    else
                        child = _doc.CreateElement(name);
                }
                var si = GetSchemaItemFor(parent, e);
                if (si != null)
                {
                    var csi = si.GetSchemaFor(name);

                    var cOrder = csi._order;
                    if (parent.ChildNodes.Count == 0)
                    {
                        parent.AppendChild(child);
                        return child;
                    }

                    {
                        var firstOrder = GetSchemaItemFor(parent.FirstChild, e)._order;
                        if (cOrder < firstOrder)
                        {
                            parent.InsertBefore(child, parent.FirstChild);
                            return child;
                        }
                    }
                    {
                        for (int i = parent.ChildNodes.Count - 1; i >= 0; i--)
                        {
                            var item = parent.ChildNodes[i];
                            var isi = GetSchemaItemFor(item, e);

                            if (isi != null && isi._order <= cOrder)
                            {
                                parent.InsertAfter(child, item);
                                return child;
                            }
                        }
                    }
                    parent.AppendChild(child);


                }
                parent.AppendChild(child);
                return child;
            }

            int _rowSourceNumber = 0;
            internal class XmlRowsSource : IRowsSource
            {
                class XmlRow : IRow
                {
                    IRow _row;
                    IRowStorage _storage;
                    XmlRowsSource _parent;

                    public XmlRow(IRow row, XmlRowsSource parent, IRowStorage storage)
                    {
                        _row = row;
                        _parent = parent;
                        _storage = storage;
                    }

                    public void Delete(bool verifyRowHasNotChangedSinceLoaded)
                    {
                        var result = new NodeIdValueSaver();
                        _storage.GetValue(_parent._entity.NodeId).SaveTo(result);
                        var node = _parent._parent.GetNode(result.id);
                        _parent._parent.DeleteElement(node);
                        _row.Delete(verifyRowHasNotChangedSinceLoaded);
                    }


                    public void Update(IEnumerable<ColumnBase> columnsOriginal, IEnumerable<IValue> valuesOriginal, bool verifyRowHasNotChangedSinceLoaded)
                    {
                        var result = new NodeIdValueSaver();
                        _storage.GetValue(_parent._entity.NodeId).SaveTo(result);
                        var node = _parent._parent.GetNode(result.id);


                        var columns = new List<ColumnBase>(columnsOriginal);
                        var values = new List<IValue>(valuesOriginal);

                        for (int i = columns.Count - 1; i >= 0; i--)
                        {
                            if (columns[i] is XmlId)
                            {
                                columns.RemoveAt(i);
                                values.RemoveAt(i);
                            }
                        }

                        _row.Update(columns, values, verifyRowHasNotChangedSinceLoaded);
                        _parent.UpdateElement(columns, values, node);
                    }



                    public void Lock()
                    {
                        _row.Lock();
                    }

                    public void ReloadData()
                    {
                        _row.ReloadData();
                    }

                    public bool IsEqualTo(IRow row)
                    {
                        var r = row as XmlRow;
                        if (r != null)
                            return _row.IsEqualTo(r._row);
                        return false;
                    }

                    public void Unlock()
                    {
                        _row.Unlock();
                    }

                    public IRow GetHostedRow()
                    {

                        return _row;
                    }
                }



                IRowsSource _source;
                XmlFileEntityDataProvider _parent;
                XmlEntity _entity;
                string[] _path;
                public XmlRowsSource(IRowsSource source, XmlFileEntityDataProvider parent, XmlEntity entity, string[] path)
                {
                    _path = path;
                    _entity = entity;
                    _source = source;
                    _parent = parent;
                    _parent._rowSourceNumber++;
                    _entity.SetActiveRowSource(this);
                }

                public void Dispose()
                {
                    _entity.SetActiveRowSource(null);
                    _source.Dispose();
                    _parent._rowSourceNumber--;
                    if (_parent._rowSourceNumber == 0)
                    {
                        _parent.SaveAndDispose();
                    }
                }
                class XmlRowsProvider : IRowsProvider
                {
                    IRowsProvider _provider;
                    XmlRowsSource _parent;

                    public XmlRowsProvider(IRowsProvider provider, XmlRowsSource parent)
                    {
                        _provider = provider;
                        _parent = parent;
                    }

                    public IRowsReader FromStart()
                    {
                        return new XmlRowsReader(_provider.FromStart(), _parent);
                    }

                    public IRowsReader From(IFilter filter, bool reverse)
                    {
                        return new XmlRowsReader(_provider.From(filter, reverse), _parent);
                    }

                    public IRowsReader From(IRow row, bool reverse)
                    {
                        return new XmlRowsReader(_provider.From(((XmlRow)row).GetHostedRow(), reverse), _parent);
                    }

                    public IRowsReader FromEnd()
                    {
                        return new XmlRowsReader(_provider.FromEnd(), _parent);
                    }

                    public IRowsReader After(IRow row, bool reverse)
                    {
                        return new XmlRowsReader(_provider.After(((XmlRow)row).GetHostedRow(), reverse), _parent);
                    }

                    public IRowsReader Find(IFilter filter, bool reverse)
                    {
                        return new XmlRowsReader(_provider.Find(filter, reverse), _parent);
                    }
                }
                class XmlRowsReader : IRowsReader
                {
                    IRowsReader _reader;
                    XmlRowsSource _parent;

                    public XmlRowsReader(IRowsReader reader, XmlRowsSource parent)
                    {
                        _reader = reader;
                        _parent = parent;
                    }

                    public void Dispose()
                    {
                        _reader.Dispose();
                    }

                    public bool Read()
                    {
                        return _reader.Read();
                    }

                    public IRow GetRow(IRowStorage c)
                    {
                        return new XmlRow(_reader.GetRow(c), _parent, c);
                    }

                    public IRow GetJoinedRow(Firefly.Box.Data.Entity e, IRowStorage c)
                    {
                        return new XmlRow(_reader.GetJoinedRow(e, c), _parent, c);
                    }
                }


                public IRowsProvider CreateReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool disableCache)
                {
                    return new XmlRowsProvider(_source.CreateReader(selectedColumns, where, sort, joins, disableCache), this);
                }

                public IRowsReader ExecuteReader(IEnumerable<ColumnBase> selectedColumns, IFilter where, Sort sort, IEnumerable<IJoin> joins, bool lockAllRows)
                {
                    return new XmlRowsReader(_source.ExecuteReader(selectedColumns, where, sort, joins, lockAllRows), this);
                }

                public IRowsReader ExecuteCommand(IEnumerable<ColumnBase> selectedColumns, IFilter filter, Sort sort, bool firstRowOnly, bool shouldBeOnlyOneRowThatMatchesTheFilter, bool lockAllRows)
                {
                    return new XmlRowsReader(_source.ExecuteCommand(selectedColumns, filter, sort, firstRowOnly, shouldBeOnlyOneRowThatMatchesTheFilter, lockAllRows), this);
                }
                class NodeIdValueSaver : IValueSaver
                {
                    public int id = -1;
                    public void SaveInt(int value)
                    {
                        id = value;
                    }

                    public void SaveDecimal(decimal value, byte precision, byte scale)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveString(string value, int length, bool fixedWidth)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveAnsiString(string value, int length, bool fixedWidth)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveNull()
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveDateTime(DateTime value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveTimeSpan(TimeSpan value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveBoolean(bool value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveByteArray(byte[] value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveEmptyDateTime()
                    {
                        throw new NotImplementedException();
                    }
                }
                class XmlValueSaver : IValueSaver
                {
                    public string Result;
                    public void SaveInt(int value)
                    {
                        Result = value.ToString();
                    }

                    public void SaveDecimal(decimal value, byte precision, byte scale)
                    {
                        var format = "0";
                        if (scale > 0)
                        {
                            format += "." + new string('0', scale);
                        }
                        Result = value.ToString(format, CultureInfo.InvariantCulture);
                    }

                    public void SaveString(string value, int length, bool fixedWidth)
                    {
                        Result = value.TrimEnd();
                        var x = Result.IndexOf('\0');
                        if (x >= 0)
                            Result = Result.Remove(x);
                    }

                    public void SaveAnsiString(string value, int length, bool fixedWidth)
                    {
                        SaveString(value, length, fixedWidth);
                    }

                    public void SaveNull()
                    {
                        Result = null;
                    }

                    public void SaveDateTime(DateTime value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveTimeSpan(TimeSpan value)
                    {
                        throw new NotImplementedException();
                    }

                    public void SaveBoolean(bool value)
                    {
                        Result = value ? "1" : "0";
                    }

                    public void SaveByteArray(byte[] value)
                    {
                        Result = ByteArrayColumn.UnicodeWithoutGaps.GetString(value);

                    }

                    public void SaveEmptyDateTime()
                    {
                        throw new NotImplementedException();
                    }
                }
                public IRow Insert(IEnumerable<ColumnBase> columnsOriginal, IEnumerable<IValue> valuesOriginal, IRowStorage storage, IEnumerable<ColumnBase> selectedColumns)
                {
                    var columns = new List<ColumnBase>(columnsOriginal);
                    var values = new List<IValue>(valuesOriginal);
                    XmlElement e = null;
                    NodeIdHelper insertedElementsHelper = null;
                    XmlNode parent = null;
                    for (int i = columns.Count - 1; i >= 0; i--)
                    {
                        if (columns[i] is NodeId)
                        {

                            var v = new NodeIdValueSaver();
                            values[i].SaveTo(v);
                            if (v.id > 0)
                            {

                                _tempNewElements.TryGetValue(v.id, out insertedElementsHelper);
                                if (insertedElementsHelper != null)
                                {
                                    e = (XmlElement)_parent._nodeById[v.id];
                                }
                                _tempNewElements.Remove(v.id);
                            }
                            columns.RemoveAt(i);
                            values.RemoveAt(i);
                        }
                        else
                            if (columns[i] is ParentId)
                        {
                            var v = new NodeIdValueSaver();
                            values[i].SaveTo(v);
                            if (!_parent._nodeById.TryGetValue(v.id, out parent))
                            {
                                parent = null;
                            }
                            columns.RemoveAt(i);
                            values.RemoveAt(i);
                        }
                        else if (!columns[i].WasSet)
                        {
                            var tc = columns[i] as TextColumn;
                            var nc = columns[i] as NumberColumn;

                            if (tc != null && tc._defaultWasSet || nc != null && nc._defaultWasSet)
                            {
                            }

                            else
                            {
                                columns.RemoveAt(i);
                                values.RemoveAt(i);
                            }
                        }
                    }
                    //verify Parent
                    if (parent != null)
                    {
                        var x = parent;
                        for (int i = _path.Length - 2; i >= 0; i--)
                        {
                            if (i == 0)
                            {
                                if (!(x is XmlDocument))
                                {
                                    parent = null;
                                    break;
                                }
                            }
                            else if (!_parent.CompareName(x, _path[i], _entity))
                            {
                                parent = null;
                                break;
                            }
                            else x = x.ParentNode;

                        }
                    }
                    //find or create parent
                    if (parent == null)
                    {
                        XmlNode y = _parent._doc;
                        for (int i = 0; i < _path.Length; i++)
                        {
                            if (i == 0)
                                y = _parent._doc;
                            else if (i == _path.Length - 1)
                            {
                                parent = y;
                            }
                            else
                            {
                                bool found = false;
                                for (int j = y.ChildNodes.Count - 1; j >= 0; j--)
                                {
                                    if (_parent.CompareName(y.ChildNodes[j], _path[i], _entity))
                                    {
                                        found = true;
                                        y = y.ChildNodes[j];
                                        break;
                                    }
                                }
                                if (!found)
                                {

                                    var x = _parent.AddChild(y, _path[i], _entity);
                                    y = x;
                                }
                            }
                        }
                    }

                    var z = _path[_path.Length - 1];



                    XmlElement ie = null;
                    if (ie != null)
                    {
                        if (!ie.HasChildNodes)
                        {
                            e = ie;
                            UpdateElement(columns, values, e);
                            AddExtraElements(e);
                            var ide = _parent.GetIdOf(e);
                            using (
                                var r = _source.ExecuteCommand(columnsOriginal,
                                    new nodeIdFilter(ide, _entity.NodeId), new Sort(), true, true,
                                    false))
                            {
                                {
                                    if (!r.Read())
                                        throw new InvalidOperationException(
                                            "The xml element should already exist in the memory database");
                                }
                                var row = r.GetRow(storage);
                                row.Update(columns, values, false);
                                storage.SetValue(_entity.NodeId, new IdValue(ide));
                                _entity.NodeId.Value = ide;
                                return row;
                            }

                        }
                    }

                    if (e == null)
                    {

                        e = _parent.AddChild(parent, z, _entity);

                    }


                    UpdateElement(columns, values, e);
                    AddExtraElements(e);
                    var ide2 = _parent.GetIdOf(e);
                    columns.Add(_entity.NodeId);
                    values.Add(new IdValue(ide2));
                    columns.Add(_entity.ParentId);
                    values.Add(new IdValue(_parent.GetIdOf(parent)));
                    XmlRow result;
                    if (insertedElementsHelper != null)
                    {
                        result = insertedElementsHelper.Update(columns, values, storage);
                    }
                    else
                    {
                        result = new XmlRow(_source.Insert(columns, values, storage, selectedColumns), this, storage);

                    }
                    storage.SetValue(_entity.NodeId, new IdValue(ide2));
                    _entity.NodeId.Value = ide2;

                    return result;
                }

                XmlElement FindElement(XmlNode parent, string name)
                {
                    foreach (var item in parent.ChildNodes)
                    {
                        var ie = item as XmlElement;
                        if (ie != null)
                        {
                            if (_parent.CompareName(ie, name, _entity))
                            {
                                return ie;
                            }
                        }
                    }
                    return null;
                }

                void AddExtraElements(XmlElement e)
                {
                    foreach (var ex in _entity.ExtraElements)
                    {
                        if (FindElement(e, ex) == null)
                            _parent.DoOnChildElement(_entity, e, ex, false, y => { }, true, false);

                    }
                }

                class nodeIdFilter : IFilter, IFilterItem
                {

                    int _id;
                    NodeId _column;

                    public nodeIdFilter(int id, NodeId column)
                    {
                        _id = id;
                        _column = column;
                    }

                    public void AddTo(IFilterBuilder builder)
                    {
                        builder.AddEqualTo(_column, this);
                    }

                    public void SaveTo(IFilterItemSaver saver)
                    {
                        saver.SaveInt(_id);
                    }
                    public bool IsAColumn()
                    {
                        return false;
                    }
                }


                void UpdateElement(List<ColumnBase> columns, List<IValue> values, XmlNode node)
                {
                    for (int i = 0; i < columns.Count; i++)
                    {
                        var value = new XmlValueSaver();
                        values[i].SaveTo(value);

                        var x = !string.IsNullOrEmpty(value.Result) || !columns[i].AllowNull || (columns[i] is ByteArrayColumn);
                        _parent.DoOnChildElement(_entity, node, columns[i].Name, _entity.AttributeColumns.Contains(columns[i]), y =>
                        {
                            if (!string.IsNullOrEmpty(value.Result))
                            {
                                if (columns[i] is Firefly.Box.Data.ByteArrayColumn && value.Result.StartsWith("<![CDATA[") && value.Result.EndsWith("]]>"))
                                {
                                    y.AppendChild(_parent._doc.CreateCDataSection(value.Result.Substring("<![CDATA[".Length, value.Result.Length - "<![CDATA[".Length - 3)));
                                }
                                else
                                    y.InnerText = value.Result;
                            }
                        },
                                                 x, x);
                    }
                    _parent._wasChanged = true;
                }
                public bool IsOrderBySupported(Sort sort)
                {
                    return _source.IsOrderBySupported(sort);
                }
                class NodeIdHelper
                {
                    private IRow r;
                    private myRowStorage rs;

                    public NodeIdHelper(myRowStorage rs, IRow r)
                    {
                        this.rs = rs;
                        this.r = r;
                    }

                    internal XmlRow Update(List<ColumnBase> columns, List<IValue> values, IRowStorage storage)
                    {
                        rs.SetStorage(storage);
                        r.Update(columns, values, false);
                        return (XmlRow)r;
                    }
                }
                Dictionary<int, NodeIdHelper> _tempNewElements = new Dictionary<int, NodeIdHelper>();

                internal void CreateNewElementAndUpdateNodeId(ColumnCollection columns)
                {
                    var cols = new List<ColumnBase>();
                    var vals = new List<IValue>();
                    foreach (var item in columns)
                    {
                        if (item.Entity != _entity)
                            continue;
                        cols.Add(item);
                        vals.Add(new ColumnBridgeToIValue(item));
                    }
                    var rs = new myRowStorage();
                    var r = Insert(cols, vals, rs, columns);
                    _tempNewElements.Add(_entity.NodeId.Value, new NodeIdHelper(rs, r));
                }
                class myRowStorage : IRowStorage
                {



                    public myRowStorage()
                    {


                    }

                    public IValue GetValue(ColumnBase column)
                    {
                        return _storage.GetValue(column);
                    }

                    IRowStorage _storage;
                    Dictionary<ColumnBase, IValueLoader> _vals = new Dictionary<ColumnBase, IValueLoader>();
                    public void SetValue(ColumnBase column, IValueLoader value)
                    {
                        if (_storage != null)
                            _storage.SetValue(column, value);
                        else _vals.Add(column, value);
                    }

                    internal void SetStorage(IRowStorage storage)
                    {
                        _storage = storage;
                        foreach (var item in _vals)
                        {
                            _storage.SetValue(item.Key, item.Value);
                        }
                    }
                }
                class ColumnBridgeToIValue : IValue
                {
                    ColumnBase _col;
                    public ColumnBridgeToIValue(ColumnBase c)
                    {
                        _col = c;
                    }
                    public void SaveTo(IValueSaver saver)
                    {
                        _col.SaveYourValueToDb(saver);
                    }
                }
            }

            public void DeleteElement(XmlNode node)
            {
                RemoveChildNodesOf(node);
                node.ParentNode.RemoveChild(node);

                _wasChanged = true;
            }

            void RemoveChildNodesOf(XmlNode node)
            {
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    var e = childNode as XmlElement;
                    int id = -1;
                    if (e != null && _idByNode.TryGetValue(e, out id))
                    {
                        var path = "";
                        XmlNode r = e;
                        while (r.ParentNode != null)
                        {
                            path = "/" + r.Name + path;
                            r = r.ParentNode;
                        }
                        DataSetDataProvider ds;
                        if (_datasets.TryGetValue(path, out ds))
                        {
                            foreach (var row in ds.DataSet.Tables[0].Select("NodeId=" + id))
                            {
                                row.Table.Rows.Remove(row);
                            }
                        }

                    }
                    RemoveChildNodesOf(childNode);
                }
            }

            public XmlNode GetNode(int id)
            {
                return _nodeById[id];
            }

            void SaveAndDispose()
            {

                if (_wasChanged)
                {
                    _helper.SaveChanges(_doc);

                }

                _helper.Dispose();

            }

            public long CountRows(Firefly.Box.Data.Entity entity)
            {
                var e = GetXmlEntity(entity);
                var p = e.GetPath();
                DataSetDataProvider edp;
                if (_datasets.TryGetValue(p, out edp))
                {
                    return ((IEntityDataProvider)edp).CountRows(entity);
                }
                return 0;
            }

            void IterateXml(XmlEntity e, string[] path, int positionInPath, XmlNode parent, IRowsSource rowSource)
            {

                if (positionInPath == 0)
                    if (!string.IsNullOrEmpty(path[0]))
                        throw new InvalidOperationException();
                    else
                        IterateXml(e, path, 1, parent, rowSource);
                else
                {
                    foreach (XmlNode childNode in parent.ChildNodes)
                    {
                        var el = childNode as XmlElement;
                        if (el != null)
                        {
                            if (CompareName(el, path[positionInPath], e))
                            {
                                if (positionInPath < path.Length - 1)
                                {
                                    IterateXml(e, path, positionInPath + 1, childNode, rowSource);
                                }
                                else
                                {

                                    var cols = new List<ColumnBase>();
                                    var vals = new List<IValue>();
                                    foreach (var column in e.Columns)
                                    {
                                        cols.Add(column);
                                        if (column is NodeId)
                                            vals.Add(new IdValue(GetIdOf(el)));
                                        else if (column is ParentId)
                                            vals.Add(new IdValue(GetIdOf(el.ParentNode)));
                                        else
                                        {
                                            string val = null;
                                            if (!column.AllowNull)
                                                val = "";
                                            var name = column.Name;
                                            var specialMenuStuff = IsStupidMenuColumn(column) && column is ByteArrayColumn;
                                            if (specialMenuStuff)
                                            {
                                                name = "Menu";
                                                val = null;
                                            }
                                            DoOnChildElement(e, el, name, e.AttributeColumns.Contains(column), y =>
                                            {
                                                if (specialMenuStuff)

                                                    val = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><Application><MenusRepository><Menus><Menu>" + y.InnerXml + "</Menu></Menus></MenusRepository></Application>";
                                                else
                                                    val = y.InnerText;

                                            }, false, false);
                                            vals.Add(new ColumnValue(column, val));
                                        }
                                    }
                                    rowSource.Insert(cols.ToArray(), vals.ToArray(), new dummyRowStorage(), cols);

                                }
                            }
                        }
                    }

                }
            }



            void DoOnChildElement(XmlEntity e, XmlNode el, string columnName, bool isAttrubite, Action<XmlNode> whatToDo, bool shouldCreate, bool shouldCreatePathParents)
            {
                if (e.UseInnerTextAsColumnContent && e.GetPath().EndsWith(columnName) || columnName.StartsWith("../") && CompareName(el, columnName.Substring(3), e))
                    whatToDo(el);
                else
                {
                    var iterate = el;
                    string n = columnName;
                    while (n.StartsWith("../"))
                    {
                        n = n.Substring(3);
                        iterate = iterate.ParentNode;
                    }
                    while (n.Contains("/"))
                    {
                        string s = n.Remove(n.IndexOf('/'));
                        bool foundpath = false;
                        foreach (XmlNode node in iterate.ChildNodes)
                        {
                            var elc = node as XmlElement;
                            if (elc != null && CompareName(elc, s, e))
                            {
                                iterate = elc;
                                n = n.Substring(s.Length + 1);
                                foundpath = true;
                                break;
                            }
                        }
                        if (!foundpath)
                        {
                            if (shouldCreatePathParents)
                            {

                                var z = AddChild(iterate, s, e);
                                n = n.Substring(s.Length + 1);
                                iterate = z;
                            }
                            else
                                return;
                        }

                    }
                    if (isAttrubite)
                    {
                        bool found = false;
                        foreach (XmlNode node in iterate.Attributes)
                        {
                            var elc = node as XmlAttribute;
                            if (elc != null && CompareName(elc, n, e))
                            {
                                whatToDo(elc);
                                found = true;
                                break;
                            }
                        }
                        if (!found && shouldCreate)
                        {
                            var name = n;
                            var parent = iterate;
                            XmlAttribute attr;
                            int i = name.IndexOf(':');
                            if (i >= 0)
                            {
                                var y = name.Split(':');
                                var namespaceSchema = e.Namespaces[y[0]];

                                string namespaceInThisXmlFile;
                                if (!FindNamespaceInThisXml(y[0], e, out namespaceInThisXmlFile))
                                {
                                    if (parent.NamespaceURI.Equals(namespaceSchema, StringComparison.InvariantCultureIgnoreCase) && parent.Prefix == "")

                                    {
                                        namespaceInThisXmlFile = "";
                                    }
                                    else
                                    {
                                        var mgnsName = GetXmlnsFreeAlias();
                                        namespaceInThisXmlFile = mgnsName;
                                        _keysFromSchemaToKeysInThisXml.Add(y[0], namespaceInThisXmlFile);
                                        var x = _doc.CreateAttribute("xmlns:" + mgnsName);
                                        x.Value = namespaceSchema;
                                        _doc.DocumentElement.SetAttributeNode(x);
                                    }
                                }
                                attr = _doc.CreateAttribute(namespaceInThisXmlFile, y[1], namespaceSchema);
                            }
                            else
                                attr = _doc.CreateAttribute(n);

                            whatToDo(attr);
                            iterate.Attributes.Append(attr);

                        }
                    }
                    else
                    {
                        bool found = false;

                        foreach (XmlNode node in iterate)
                        {
                            var elc = node as XmlElement;
                            if (elc != null && CompareName(elc, n, e))
                            {
                                whatToDo(elc);
                                found = true;
                                break;
                            }
                        }
                        if (!found && shouldCreate)
                        {
                            var y = AddChild(iterate, n, e);
                            whatToDo(y);


                        }
                    }
                }

            }




            class dummyRowStorage : IRowStorage
            {
                public IValue GetValue(ColumnBase column)
                {
                    throw new NotImplementedException();
                }

                public void SetValue(ColumnBase column, IValueLoader value)
                {

                }
            }

            int _lastId = 0;
            Dictionary<int, XmlNode> _nodeById = new Dictionary<int, XmlNode>();
            Dictionary<XmlNode, int> _idByNode = new Dictionary<XmlNode, int>();
            bool _wasChanged;

            int GetIdOf(XmlNode node)
            {
                if (_idByNode.ContainsKey(node))
                    return _idByNode[node];
                if (node is XmlDocument)
                {
                    _idByNode.Add(node, 0);
                    _nodeById.Add(0, node);
                    return 0;
                }
                _lastId++;
                _nodeById.Add(_lastId, node);
                _idByNode.Add(node, _lastId);
                return _lastId;
            }


        }
        class ColumnValue : IValue, IValueLoader
        {
            ColumnBase _column;
            string _val;

            public ColumnValue(ColumnBase column, string val)
            {
                _column = column;
                _val = val;

            }

            public void SaveTo(IValueSaver saver)
            {
                _column.LoadFrom(this).SaveTo(saver);
            }


            public bool IsNull()
            {
                return _val == null;
            }

            public Number GetNumber()
            {
                if (String.IsNullOrEmpty(_val))
                    return 0;
                try
                {
                    return decimal.Parse(_val, System.Globalization.CultureInfo.InvariantCulture);
                }
                catch (Exception ex)
                {


                    return 0;
                }
            }

            public string GetString()
            {
                return _val;
            }

            public DateTime GetDateTime()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetTimeSpan()
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean()
            {
                if (_val == null)
                    return false;
                var va = _val.Trim();
                return va == "1" || va == "true";
            }

            public byte[] GetByteArray()
            {
                if (_val == null)
                    return null;
                return ByteArrayColumn.ToUnicodeByteArray(_val);
            }
        }
        class IdValue : IValue, IValueLoader
        {
            int _id;

            public IdValue(int id)
            {
                _id = id;
            }

            public void SaveTo(IValueSaver saver)
            {
                saver.SaveInt(_id);
            }

            public bool IsNull()
            {
                throw new NotImplementedException();
            }

            public Number GetNumber()
            {
                return _id;
            }

            public string GetString()
            {
                throw new NotImplementedException();
            }

            public DateTime GetDateTime()
            {
                throw new NotImplementedException();
            }

            public TimeSpan GetTimeSpan()
            {
                throw new NotImplementedException();
            }

            public bool GetBoolean()
            {
                throw new NotImplementedException();
            }

            public byte[] GetByteArray()
            {
                throw new NotImplementedException();
            }
        }
        static XmlEntity GetXmlEntity(Firefly.Box.Data.Entity e)
        {
            foreach (var column in e.Columns)
            {
                var d = column as Firefly.Box.Data.DateColumn;
                if (d != null)
                    d.Storage = new XmlStringDateStorage();
                var t = column as Firefly.Box.Data.TimeColumn;
                if (t != null)
                    t.Storage = new XmlStringTimeStorage();
                var ba = column as ENV.Data.ByteArrayColumn;
                if (ba != null && !ba._wasStorageSet && (ba.ContentType == ByteArrayColumnContentType.Ansi || ba.ContentType == ByteArrayColumnContentType.BinaryAnsi || ba.ContentType == ByteArrayColumnContentType.BinaryUnicode))
                {
                    if (IsStupidMenuColumn(ba))
                        ba.Storage = new UTF8ByteArrayStorage();
                    else
                        ba.Storage = new AnsiStringByteArrayStorage();
                }
            }
            return (XmlEntity)e;
        }

        Dictionary<string, XmlFileEntityDataProvider> _activeFileEntities =
            new Dictionary<string, XmlFileEntityDataProvider>();
        Dictionary<Firefly.Box.Data.ByteArrayColumn, XmlFileEntityDataProvider> _activeColumnEntities =
            new Dictionary<Firefly.Box.Data.ByteArrayColumn, XmlFileEntityDataProvider>();
        public IRowsSource ProvideRowsSource(Firefly.Box.Data.Entity entity)
        {
            var xe = entity as XmlEntity;
            if (xe != null)
            {
                var c = xe.GetSourceColumn();
                if (!ReferenceEquals(c, null))
                {
                    XmlFileEntityDataProvider result;
                    if (!_activeColumnEntities.TryGetValue(c, out result))
                    {
                        if (object.ReferenceEquals(c._boundParameterColumnForXmlDataSource, null) || !_activeColumnEntities.TryGetValue(c._boundParameterColumnForXmlDataSource, out result))//f14478
                        {
                            result = new XmlFileEntityDataProvider(new XmlColumnProviderHelper(c, this), xe.Namespaces);
                            _activeColumnEntities.Add(c, result);
                        }
                    }
                    return result.ProvideRowSource(entity);
                }
            }
            {


                string fileName = getFileName(entity);
                var key = fileName.ToUpper();
                var useReadOnly = entity.ReadOnly;
                if (!UseReadOnlyAccess)
                    useReadOnly = false;
                if (useReadOnly)
                    key += ":readonly";
                XmlFileEntityDataProvider result;
                if (!_activeFileEntities.TryGetValue(key, out result))
                {
                    XmlDataProviderHelper h;
                    if (fileName.StartsWith("http", StringComparison.InvariantCultureIgnoreCase))
                        h = new UrlXmlDataProviderHelper(fileName, this, key);
                    else
                        h = new FileXmlDataProviderHelper(fileName, this, key, useReadOnly);
                    result = new XmlFileEntityDataProvider(h, xe.Namespaces);
                    _activeFileEntities.Add(key, result);
                }
                return result.ProvideRowSource(entity);
            }
        }
        string getFileName(Firefly.Box.Data.Entity e)
        {

            var result = e.EntityName.ToString().Trim();
            if (!string.IsNullOrWhiteSpace(FilesPath) && !result.StartsWith("http", StringComparison.InvariantCultureIgnoreCase) && !Path.IsPathRooted(result))
                result = Path.Combine(FilesPath, result);
            return PathDecoder.DecodePath(result);
        }


        public ITransaction BeginTransaction()
        {
            throw new NotImplementedException();
        }
        public bool RequiresTransactionForLocking
        {
            get
            {
                return false;
            }
        }

        public bool SupportsTransactions
        {
            get { return false; }
        }

        public string Name { get; internal set; }
        public string FilesPath { get; set; }

        public bool Contains(Firefly.Box.Data.Entity entity)
        {
            return Common.FileExists(GetFilePath(entity));
        }

        string GetFilePath(Firefly.Box.Data.Entity entity)
        {
            var path = entity.EntityName;
            if (!string.IsNullOrEmpty(FilesPath) && !System.IO.Path.IsPathRooted(path) && !path.StartsWith(".."))
                path = System.IO.Path.Combine(ENV.PathDecoder.DecodePath(FilesPath), path);
            return path;
        }

        public long CountRows(Firefly.Box.Data.Entity entity)
        {
            string fileName = getFileName(entity);
            var key = fileName.ToUpper();
            XmlFileEntityDataProvider result;
            if (_activeFileEntities.TryGetValue(key, out result))
            {
                return result.CountRows(entity);
            }
            return 0;
        }

        public void Drop(Firefly.Box.Data.Entity entity)
        {
            try
            {
                System.IO.File.Delete(GetFilePath(entity));
            }
            catch { }
        }

        public void Truncate(Firefly.Box.Data.Entity entity)
        {
            throw new NotImplementedException();
        }
    }
    public class XmlEntity : Entity
    {
        public System.Data.DataSet DataSetForDebugView;
        string _elementPath;
        internal string GetPath()
        {
            return _elementPath;
        }
        internal SchemaItem GetSchemaRoot()
        {
            if (_schemaItem != null)
            {
                return _schemaItem.GetRoot();
            }
            return null;
        }
        public void SetXmlSource(ByteArrayColumn byteArrayColumn)
        {
            _sourceColumn = byteArrayColumn;
        }

        ByteArrayColumn _sourceColumn;
        readonly List<ColumnBase> _attributeColumns = new List<ColumnBase>();

        [PrimaryKey]
        public readonly NodeId NodeId = new NodeId();
        public readonly ParentId ParentId = new ParentId();
        /// <summary>ByNode</summary>
        public readonly Sort SortByNode = new Sort
        {
            Caption = "ByNode",
            Unique = true
        };
        public XmlEntity(string entityName, string caption, string elementPath, XmlEntityDataProvider dataProvider) :
            base(entityName, caption, dataProvider)
        {
            _elementPath = elementPath;
            SortByNode.Add(ParentId, NodeId);
        }
        SchemaItem _schemaItem;
        public XmlEntity(string entityName, string caption, SchemaItem schemaItem, XmlEntityDataProvider dataProvider) :
            this(entityName, caption, schemaItem.FullPath, dataProvider)
        {
            _schemaItem = schemaItem;

        }
        public bool UseInnerTextAsColumnContent { get; set; }
        Dictionary<string, string> _nameSpaces = new Dictionary<string, string>();
        public Dictionary<string, string> Namespaces { get { return _nameSpaces; } }

        public List<ColumnBase> AttributeColumns
        {
            get { return _attributeColumns; }
        }

        public readonly List<string> ExtraElements = new List<string>();

        internal ENV.Data.ByteArrayColumn GetSourceColumn()
        {
            return _sourceColumn;
        }
        internal void CreateNewElementAndUpdateNodeId(ColumnCollection columns)
        {
            if (_xmlRowsSource == null)
                throw new InvalidOperationException("An element can only be created for an active xml entity");
            _xmlRowsSource.CreateNewElementAndUpdateNodeId(columns);


        }

        XmlEntityDataProvider.XmlFileEntityDataProvider.XmlRowsSource _xmlRowsSource;
        internal void SetActiveRowSource(XmlEntityDataProvider.XmlFileEntityDataProvider.XmlRowsSource xmlRowsSource)
        {
            if (xmlRowsSource == null)
                _xmlRowsSource = null;
            else if (_xmlRowsSource != null)
                throw new InvalidOperationException("There is already an xml rows source attached to this entity");
            _xmlRowsSource = xmlRowsSource;
        }

        internal void SetSourceTo(XmlEntity entity)
        {
            if (!ReferenceEquals(_sourceColumn, null) && (ReferenceEquals(entity._sourceColumn, null) || ReferenceEquals(entity._sourceColumn.Value, null)))
                entity._sourceColumn = _sourceColumn;
        }
    }
    public class NodeId : XmlId
    {
        public NodeId()
            : base("NodeId")
        {
        }
    }
    public class ParentId : XmlId
    {
        public ParentId()
            : base("ParentId")
        {
        }
    }
    public class XmlId : NumberColumn
    {
        public XmlId(string s)
            : base(s)
        {

        }
    }
    public class SchemaItem
    {
        public string ElementName { get; internal set; }

        public SchemaItem(string name)
        {
            ElementName = name;
            InitMembersUsingReflection();
        }

        protected virtual void InitMembersUsingReflection()
        {


            RelativePath = "../" + ElementName;
        }

        internal int _order = -1;


        string _fullPath;
        string _relativePath;
        List<SchemaItem> __children;
        List<SchemaItem> _children
        {
            get
            {
                if (__children != null)
                    return __children;
                __children = new List<SchemaItem>();
                foreach (var item in this.GetType().GetFields())
                {
                    if (typeof(SchemaItem).IsAssignableFrom(item.FieldType))
                    {
                        if (item.FieldType == this.GetType())
                            throw new InvalidOperationException("Cant a parent and a child with the same type");
                        var x = (SchemaItem)item.GetValue(this);
                        x._order = _children.Count;
                        _children.Add(x);

                    }
                }
                return __children;
            }
        }
        public string RelativePath
        {
            get { return _relativePath; }
            set
            {
                _relativePath = value;

                foreach (var el in _children)
                {
                    if (_relativePath.StartsWith("../"))
                        el.RelativePath = el.ElementName;
                    else
                        el.RelativePath = _relativePath + "/" + el.ElementName;
                }



            }
        }
        public static implicit operator string(SchemaItem e)
        {
            return e.RelativePath;
        }

        bool _searchedForParent = false;
        SchemaItem __parent;
        SchemaItem _parent
        {
            get
            {
                if (_searchedForParent)
                    return __parent;
                else
                {

                    _searchedForParent = true;
                    var parentType = GetType().DeclaringType;
                    if (parentType == null || typeof(SchemaItem).IsAssignableFrom(parentType))
                    {
                        __parent = (SchemaItem)System.Activator.CreateInstance(parentType);
                    }
                }
                return __parent;

            }
        }

        internal string FullPath
        {
            get
            {
                if (_fullPath == null)
                {
                    var myType = this.GetType();
                    if (myType == typeof(SchemaItem))
                        return "";
                    _fullPath = "/" + ElementName;
                    if (_parent != null)
                    {
                        foreach (var item in _parent._children)
                        {
                            if (item.GetType() == this.GetType())
                            {
                                _fullPath = _parent.FullPath + "/" + ElementName;
                            }
                        }
                    }

                }
                return _fullPath;

            }

        }
        internal SchemaItem GetRoot()
        {
            if (_parent == null)
                return this;
            return _parent.GetRoot();
        }

        internal SchemaItem GetSchemaFor(string name)
        {
            foreach (var item in _children)
            {
                if (item.ElementName == name)
                    return item;
            }
            return null;
        }
    }
}
