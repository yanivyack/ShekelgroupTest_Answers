using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace ENV.UI
{
    public partial class ToolboxControlsDialog : System.Windows.Forms.Form
    {
        static Dictionary<string, Dictionary<string, List<string>>> controlsDictionary = new Dictionary<string, Dictionary<string, List<string>>>();
        private IToolboxService _toolboxService;
        Action<ToolboxItem> _createTool;
        public ToolboxControlsDialog(IToolboxService toolboxService,Action<ToolboxItem> createTool)
        {
            _toolboxService = toolboxService;
            _createTool = createTool;
            InitializeComponent();
        }

        public void AddAssembly(string assemblyPath, bool publicTypesOnly)
        {
            AppDomainSetup ads = new AppDomainSetup();
            ads.ApplicationBase = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            AppDomain appDomainForAssemblyLoading = AppDomain.CreateDomain(
                "AppDomainForAssemblyLoading", null, ads);

            System.Collections.Generic.SortedDictionary<string,
                System.Collections.Generic.SortedDictionary<string, TreeNode>> nodes =
                    new System.Collections.Generic.SortedDictionary<string,
                        System.Collections.Generic.SortedDictionary<string, TreeNode>>();
            AssemblyName assemblyName = null;
            ImageListManager imageList = new ImageListManager();
            imageList.SetToTreeView(treeView,selectedList);
            
            /*imageList.Add("Assembly", Resources.Assembly);
            imageList.Add("Namespace", Resources.Namespace);*/
            try
            {
                assemblyName = ((AssemblyNameProxy)
                appDomainForAssemblyLoading.CreateInstanceAndUnwrap(
                    typeof(AssemblyNameProxy).Assembly.FullName,
                    typeof(AssemblyNameProxy).FullName)).GetAssemblyName(assemblyPath);
                ResolveEventHandler resolve =
                    delegate (object sender, ResolveEventArgs args)
                    {
                        return args.Name == Assembly.GetExecutingAssembly().FullName ?
                            Assembly.GetExecutingAssembly() : null;
                    };
                AppDomain.CurrentDomain.AssemblyResolve += resolve;
                AssemblyLoader assemblyLoader = (AssemblyLoader)
                    appDomainForAssemblyLoading.CreateInstanceFromAndUnwrap(
                    Assembly.GetExecutingAssembly().Location,
                    typeof(AssemblyLoaderClass).FullName);
                AppDomain.CurrentDomain.AssemblyResolve -= resolve;
                var namespaces = new Dictionary<string, List<string>>();

                
                assemblyLoader.DoOnTypes(assemblyName, System.IO.Path.GetDirectoryName(assemblyPath),
                    new AssemblyLoaderClientClass(
                    delegate (string typeNamespace, ToolboxItem item)
                    {
                        try
                        {


                          

                         //   if (toolboxService.IsSupported(toolboxService.SerializeToolboxItem(item), designerHost))
                            {
                                item.Lock();
                                System.Windows.Forms.TreeNode node = new TreeNode();
                                DoOnUIThread(
                                    delegate ()
                                    {
                                        node = new TreeNode(item.DisplayName);
                                        node.Tag = item;
                                          imageList.Add(item.TypeName, item.Bitmap);
                                          node.ImageIndex = imageList.GetImageIndexFor(item.TypeName);
                                          node.SelectedImageIndex = imageList.GetImageIndexFor(item.TypeName);
                                    });

                                System.Collections.Generic.SortedDictionary<string, TreeNode>
                                    componentNodes;
                                if (!nodes.TryGetValue(typeNamespace, out componentNodes))
                                {
                                    componentNodes =
                                        new System.Collections.Generic.SortedDictionary<
                                            string, TreeNode>();
                                    nodes.Add(typeNamespace, componentNodes);
                                }
                                componentNodes.Add(item.DisplayName, node);
                            }
                        }
                        catch (Exception e)
                        {
                            try
                            {
                                reportMessage(
                                    string.Format(
                                        "Firefly Toolbox error - Exception occured on load of " +
                                        item.TypeName +
                                        " the exception is:\n" + e));
                            }
                            catch { }
                        }
                    }), publicTypesOnly);
                if (namespaces.Count > 0)
                    controlsDictionary.Add(assemblyName.FullName.Remove(assemblyName.FullName.IndexOf(",")), namespaces);
            }
            catch (Exception e)
            {
                try
                {
                    DoOnUIThread(
                        delegate ()
                        {
                            System.Windows.Forms.TreeNode node = new TreeNode();
                            node.Text = "Error loading " + assemblyName + " - " +
                                        e.ToString();
                                      treeView.Nodes.Add(node);
                        });

                    reportMessage(
                        string.Format(
                            "Firefly Toolbox error - Exception occured on load of " +
                            assemblyName.ToString() +
                            " the exception is:\n" + e));
                    ReflectionTypeLoadException le = e as ReflectionTypeLoadException;
                    if (le != null)
                        foreach (Exception ie in le.LoaderExceptions)
                        {
                            reportMessage(
                                string.Format(
                                    "loader exception exception exception is:\n" +
                                    ie));
                        }
                }
                catch { }
            }
            finally
            {
                AppDomain.Unload(appDomainForAssemblyLoading);
            }
            SelectionHandler selectionHandler = new SelectionHandler(treeView);
            
            if (nodes.Count > 0)
            {
                DoOnUIThread(
                    delegate
                    {
                          treeView.BeginUpdate();
                          try
                          {
                              System.Windows.Forms.TreeNode assemblyNode =
                                  new TreeNode(assemblyName.Name);
                              //assemblyNode.ImageIndex = imageList.GetImageIndexFor("Assembly");
                              //assemblyNode.SelectedImageIndex = imageList.GetImageIndexFor("Assembly");
                              treeView.Nodes.Add(assemblyNode);
                              foreach (System.Collections.Generic.KeyValuePair<string,
                                  System.Collections.Generic.SortedDictionary
                                      <string, TreeNode>>
                                  pair in nodes)
                              {
                                  TreeNode namespaceNode = new TreeNode(pair.Key);
                                  //namespaceNode.ImageIndex = imageList.GetImageIndexFor("Namespace");
                                  //namespaceNode.SelectedImageIndex = imageList.GetImageIndexFor("Namespace");
                                  assemblyNode.Nodes.Add(namespaceNode);
                                  foreach (TreeNode n in pair.Value.Values)
                                  {
                                      namespaceNode.Nodes.Add(n);
                                      selectionHandler.Loaded(n);
                                  }
                                  imageList.CommitToUi();
                              }
                          }
                          finally
                          {
                              treeView.EndUpdate();
                              if (treeView.SelectedNode != null)
                              {
                                  treeView.Update();
                                  treeView.Select();
                              }
                          }
                    });
            }
        }

        private void reportMessage(string v)
        {

        }

        private void DoOnUIThread(Action p)
        {
            p();
        }
        class SelectionHandler
        {
            string[] _selectedStack = new string[] { };
            bool _reloading = false;
            public SelectionHandler(System.Windows.Forms.TreeView tv)
            {
                tv.AfterSelect += delegate (object sender, TreeViewEventArgs e)
                {
                    if (_reloading)
                        return;
                    if (tv.SelectedNode == null)
                        _selectedStack = null;
                    else
                        _selectedStack = CreateStack(tv.SelectedNode);
                };
            }

            public void Reloadion()
            {
                _reloading = true;
            }

            string[] CreateStack(TreeNode tn)
            {
                List<string> l = new List<string>();
                while (tn != null)
                {
                    l.Insert(0, tn.Text);
                    tn = tn.Parent;
                }
                return l.ToArray();
            }


            public void Loaded(TreeNode n)
            {
                if (_selectedStack == null)
                    return;
                string[] ar = CreateStack(n);
                if (ar.Length != _selectedStack.Length)
                    return;
                for (int i = 0; i < ar.Length; i++)
                {
                    if (ar[i] != _selectedStack[i])
                        return;
                }
                TreeNode parent = n.Parent;
                while (parent != null)
                {
                    parent.Expand();
                    parent = parent.Parent;
                }
                n.TreeView.SelectedNode = n;
            }

            public void EndLoading()
            {
                _reloading = false;
            }
        }

        interface AssemblyLoader
        {
            void DoOnTypes(AssemblyName assemblyName, string path, AssemblyLoaderClient client, bool publicTypesOnly);
        }
        interface AssemblyLoaderClient
        {
            void DoOnToolboxItemForType(string typeNamespace, ToolboxItem toolboxItem, byte[] bitmapData);



        }
        class AssemblyLoaderClass : MarshalByRefObject, AssemblyLoader
        {
            public void DoOnTypes(AssemblyName assemblyName, string path, AssemblyLoaderClient client, bool publicTypesOnly)
            {
                ResolveEventHandler resolve =
                     delegate (object sender, ResolveEventArgs args)
                     {
                         var an = Path.Combine(path, args.Name.Split(',')[0]);
                         if (File.Exists(an + ".dll"))
                             return Assembly.LoadFile(an + ".dll");
                         if (File.Exists(an + ".exe"))
                             return Assembly.LoadFile(an + ".exe");
                         return null;
                     };
                AppDomain.CurrentDomain.AssemblyResolve += resolve;
                Assembly assembly = Assembly.Load(assemblyName);
                foreach (var item in assembly.GetReferencedAssemblies())
                {

                    var x = System.IO.Path.Combine(path, item.Name + ".dll");
                    if (File.Exists(x))
                        Assembly.LoadFrom(x);
                    else

                        Assembly.Load(item);

                }

                foreach (Type type in assembly.GetTypes())
                {
                    if (type.IsAbstract) continue;
                    if (type.IsNestedPrivate) continue;
                    if (publicTypesOnly && !type.IsVisible) continue;
                    if (!typeof(System.ComponentModel.IComponent).IsAssignableFrom(type))
                        continue;

                    ToolboxItem item = CreateToolboxItem(type, assembly.GetName());
                    if (item == null) continue;
                    client.DoOnToolboxItemForType(type.Namespace, item,
                        (byte[])TypeDescriptor.GetConverter(item.Bitmap).ConvertTo(item.Bitmap, typeof(byte[])));
                }
                AppDomain.CurrentDomain.AssemblyResolve -= resolve;
            }
        }
        private static ToolboxItem CreateToolboxItem(Type type, AssemblyName name)
        {
            ToolboxItemAttribute attribute1 = (ToolboxItemAttribute)
                TypeDescriptor.GetAttributes(type)[typeof(ToolboxItemAttribute)];
            ToolboxItem item1 = null;
            if (!attribute1.IsDefaultAttribute())
            {
                Type type1 = attribute1.ToolboxItemType;
                if (type1 != null)
                {
                    item1 = CreateToolboxItemInstance(type1, type);
                    if (item1 != null)
                    {
                        if (name != null)
                        {
                            item1.AssemblyName = name;
                        }
                    }
                }
            }
            if (((item1 == null) && (attribute1 != null)) && !attribute1.Equals(ToolboxItemAttribute.None))
            {
                item1 = new ToolboxItem(type);
                if (name != null)
                {
                    item1.AssemblyName = name;
                }
            }
            return item1;
        }
        
        class ImageListManager
        {
            System.Windows.Forms.ImageList _il;
            Dictionary<string, int> _keyIndex = new Dictionary<string, int>();
            int _lastImage = -1;
            public void SetToTreeView(System.Windows.Forms.TreeView tv)
            {
                
            }
            internal void SetToTreeView(System.Windows.Forms.TreeView treeView, ListView selectedList)
            {
                treeView.ImageList = _il;
                selectedList.LargeImageList = _il;
                selectedList.SmallImageList = _il;
                selectedList.StateImageList = _il;

            }

            List<Image> _waitingImages = new List<Image>();
            public ImageListManager()
            {

                _il = new ImageList();
            }

            public void Add(string key, Icon icon)
            {
                if (!_keyIndex.ContainsKey(key))
                {
                    _keyIndex.Add(key, ++_lastImage);
                    _il.Images.Add(icon);
                }
            }
            public void Add(string key, Image image)
            {
                if (!_keyIndex.ContainsKey(key))
                {
                    _keyIndex.Add(key, ++_lastImage);
                    _waitingImages.Add(image);
                }
            }

            public int GetImageIndexFor(string key)
            {
                return _keyIndex[key];
            }

            public void CommitToUi()
            {
                _il.Images.AddRange(_waitingImages.ToArray());
                _waitingImages = new List<Image>();
            }

            
        }
        private static ToolboxItem CreateToolboxItemInstance(Type toolboxItemType, Type toolType)
        {
            ToolboxItem item1 = null;
            ConstructorInfo info1 = toolboxItemType.GetConstructor(new Type[] { typeof(Type) });
            if (info1 != null)
            {
                return (ToolboxItem)info1.Invoke(new object[] { toolType });
            }
            info1 = toolboxItemType.GetConstructor(new Type[0]);
            if (info1 != null)
            {
                item1 = (ToolboxItem)info1.Invoke(new object[0]);
                item1.Initialize(toolType);
            }
            return item1;
        }
        delegate void DoOnToolboxItemForType(string typeNamespace, ToolboxItem toolboxItem);
        class AssemblyLoaderClientClass : MarshalByRefObject, AssemblyLoaderClient
        {
            DoOnToolboxItemForType _doOnToolboxItemForType;



            public AssemblyLoaderClientClass(DoOnToolboxItemForType doOnToolboxItemForType)
            {
                _doOnToolboxItemForType = doOnToolboxItemForType;

            }

            public void DoOnToolboxItemForType(string typeNamespace, ToolboxItem toolboxItem, byte[] bitmapData)
            {
                using (MemoryStream stream = new MemoryStream(bitmapData))
                {
                    Bitmap theBitmap = (Bitmap)Bitmap.FromStream(stream);
                    theBitmap.MakeTransparent();
                    toolboxItem.Bitmap =
                        (Bitmap)TypeDescriptor.GetConverter(typeof(Bitmap)).ConvertFrom(bitmapData);
                    _doOnToolboxItemForType(typeNamespace, toolboxItem);
                }
            }

        }

        class myDataObject : IDataObject
        {
            IDataObject _data;
            public myDataObject(IDataObject data)
            {
                _data = data;
            }
            public object GetData(Type format)
            {
                return _data.GetData(format);
            }

            public object GetData(string format)
            {
                return _data.GetData(format);
            }

            public object GetData(string format, bool autoConvert)
            {
                return _data.GetData(format, autoConvert);
            }

            public bool GetDataPresent(Type format)
            {
                
                return _data.GetDataPresent(format);
            }

            public bool GetDataPresent(string format)
            {
                
                return _data.GetDataPresent(format);
            }

            public bool GetDataPresent(string format, bool autoConvert)
            {
                
                return _data.GetDataPresent(format, autoConvert);
            }

            public string[] GetFormats()
            {
                
                return _data.GetFormats();
            }

            public string[] GetFormats(bool autoConvert)
            {
                
                return _data.GetFormats(autoConvert);
            }

            public void SetData(object data)
            {
                _data.SetData(data);
            }

            public void SetData(Type format, object data)
            {
                _data.SetData(format, data);
            }

            public void SetData(string format, object data)
            {
                _data.SetData(format, data);
            }

            public void SetData(string format, bool autoConvert, object data)
            {
                _data.SetData(format, autoConvert, data);
            }
        }

        public void AddAssembly(Assembly assembly)
        {
            try
            {
                var namespaces = new Dictionary<string, List<string>>();
                foreach (var type in assembly.GetTypes())
                {
                    if (!type.IsSubclassOf(typeof(Firefly.Box.UI.Form)) &&
                        !type.IsSubclassOf(typeof(Firefly.Box.Printing.ReportLayout)))
                    {
                        if (type.IsSubclassOf(typeof(Control)) || 
                            type.IsSubclassOf(typeof(Firefly.Box.UI.FontScheme)) ||
                            type.IsSubclassOf(typeof(ENV.UI.CustomHelp)) ||
                            type.IsSubclassOf(typeof(ToolStrip)) ||
                            type.IsSubclassOf(typeof(MenuStrip)) ||
                            type.IsSubclassOf(typeof(Firefly.Box.UI.ColorScheme))
                            )
                        {
                            if (namespaces.ContainsKey(type.Namespace))
                                namespaces[type.Namespace].Add(type.Name);
                            else
                            {
                                var types = new List<string> { type.Name };
                                namespaces.Add(type.Namespace, types);
                            }
                        }
                    }
                }
                if (namespaces.Count > 0)
                    controlsDictionary.Add(assembly.FullName.Remove(assembly.FullName.IndexOf(",")), namespaces);
            }
            catch
            {
            }
        }

        internal void Clear()
        {
            controlsDictionary.Clear();
        }

        public void FillTree()
        {
            foreach (var ns in controlsDictionary)
            {
                var node = new TreeNode(ns.Key);
                node.Nodes.Add("");
                treeView.Nodes.Add(node);
            }
        }

        private void tree_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            return;
            e.Node.Nodes.Clear();
            if (e.Node.Parent == null)
            {
                if (controlsDictionary.ContainsKey(e.Node.Text))
                {
                    foreach (var ns in controlsDictionary[e.Node.Text])
                    {
                        var tn = new TreeNode(ns.Key);
                        tn.Nodes.Add("");
                        e.Node.Nodes.Add(tn);
                    }
                }
            }
            else
            {
                if (controlsDictionary[e.Node.Parent.Text].ContainsKey(e.Node.Text))
                {
                    foreach (var control in controlsDictionary[e.Node.Parent.Text][e.Node.Text])
                    {
                        e.Node.Nodes.Add(control);
                    }
                }
            }
        }

        private void tree_KeyDown(object sender, KeyEventArgs e)
        {

            if (e.KeyCode == Keys.Space)
            {
                //AddTooboxItem();
                AddToList();
            }
        }

        private void AddToList()
        {
            if (treeView.SelectedNode.Nodes.Count == 0)
            {
                var x = selectedList.Items.Add(treeView.SelectedNode.Text);
                x.Tag = treeView.SelectedNode.Tag;
                x.ImageIndex = treeView.SelectedNode.ImageIndex;
                EnableButtons();
            }
        }

        private void EnableButtons()
        {
            btnLeft.Enabled = selectedList.SelectedItems.Count > 0;
            btnAddToToolbox.Enabled = selectedList.Items.Count > 0;
            btnAddToForm.Enabled = selectedList.Items.Count > 0;
        }

        private void RemoveFromList()
        {
            var x = selectedList.SelectedItems;
            foreach (ListViewItem item in x)
            {
                selectedList.Items.Remove(item);
            }

            
            EnableButtons();
        }

        private void tree_DoubleClick(object sender, EventArgs e)
        {
            AddToList();
        }

        private void btnRight_Click(object sender, EventArgs e)
        {
            AddToList();
        }

        private void btnLeft_Click(object sender, EventArgs e)
        {
            RemoveFromList();
        }

        private void selectedList_SelectedIndexChanged(object sender, EventArgs e)
        {
            EnableButtons();
        }

        private void btnAddToToolbox_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in selectedList.Items)
            {
                _toolboxService.AddToolboxItem((ToolboxItem) item.Tag);
            }
            _toolboxService.SetSelectedToolboxItem(null);
            this.Close();
            _toolboxService.Refresh();
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in selectedList.Items)
            {
                _createTool((ToolboxItem)item.Tag);
            }
            this.Close();
        }
    }
}
