using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using ENV.Security;
using ENV.UI.Menus;
using Firefly.Box;
using Firefly.Box.UI.Advanced;

namespace ENV
{
    public class MenuManager
    {
        static MenuManager _currentInternalForTesting = new MenuManager();
        internal static MenuManager Current { get { return _currentInternalForTesting; } }
        Firefly.Box.Context _myContext = Firefly.Box.Context.Current;

        public static void DoOnMenuManagers(Action<MenuManager> what)
        {
            what(_currentInternalForTesting);
            var x = Common.ContextTopMostForm as IHaveAMenu;
            if (x != null)
                x.DoOnMenu(what);


        }



        Dictionary<int, ToolStrip[]> _menus = new Dictionary<int, ToolStrip[]>();
        Dictionary<string, List<ToolStripItem>> _itemsMap = new Dictionary<string, List<ToolStripItem>>();
        Dictionary<ToolStripItem, List<ToolStripButton>> _children = new Dictionary<ToolStripItem, List<ToolStripButton>>();
        Dictionary<ToolStripButton, ToolStripItem> _parents = new Dictionary<ToolStripButton, ToolStripItem>();

        Dictionary<ToolStripItem, MenuEntry> _defaultState = new Dictionary<ToolStripItem, MenuEntry>();
        ContextMenuState MainUIState;



        void Apply(ContextMenuState state)
        {
            Context.Current.InvokeUICommand(
            () =>
            {
                var l = new List<MenuEntry>();
                foreach (var menuEntry in _defaultState)
                {
                    MenuEntry x;
                    if (state.TryGetState(menuEntry.Key, out x))
                        l.Add(x);
                    else
                        menuEntry.Value.Apply();
                }
                l.ForEach(e => e.Apply());
            });
        }

        bool TryGetParentValue(ToolStripButton toolStripButton, out ToolStripItem parentItem)
        {
            return _parents.TryGetValue(toolStripButton, out parentItem);
        }
        public void Map(string name, ToolStripItem item, Func<bool> condition, bool manageItemEnabled)
        {
            if (item == null)
                return;
            List<ToolStripItem> items;
            if (!_itemsMap.TryGetValue(name, out items))
            {
                items = new List<ToolStripItem>();
                _itemsMap.Add(name, items);
            }
            items.Add(item);
            _defaultState.Add(item, new MenuEntry(this, MainUIState, item, condition, manageItemEnabled));
            item.Disposed += delegate
            {
                items.Remove(item);
                _defaultState.Remove(item);
            };
        }

        public static void ApplyToContextMenu(ContextMenuStrip contextMenu, IHaveAMenu haveMenu)
        {
            ENV.Common.RunOnLogicContext(contextMenu, () => DoOnMenuManagers(mm => mm.ApplyTo(haveMenu)));
        }

        bool TryGetValueDefaultState(ToolStripItem item, out MenuEntry me)
        {
            return _defaultState.TryGetValue(item, out me);
        }

        bool TryGeyItems(Text name, out List<ToolStripItem> result)
        {
            return _itemsMap.TryGetValue(name, out result);
        }
        void SetVisibleToChildrenOf(ContextMenuState mm, ToolStripItem parent, bool visible, bool enabled)
        {
            Action<ToolStripButton, bool> setChildVisible =
                (item, v) =>
                {
                    v = v && AreAllParentsAllowedAndFirstParentAvailable(parent, mm);
                    if (item.Available == v) return;
                    item.Visible = v;
                    if (item.Owner != null)
                        FixSeparators(item.Owner);
                };

            List<ToolStripButton> childToolBarItems;
            if (_children.TryGetValue(parent, out childToolBarItems))
            {
                foreach (var childToolBarItem in childToolBarItems)
                {
                    if (visible)
                    {
                        MenuEntry me;
                        if (mm.TryGetState(childToolBarItem, out me) || _defaultState.TryGetValue(childToolBarItem, out me))
                        {
                            setChildVisible(childToolBarItem, me.Visible);
                            continue;
                        }
                    }
                    setChildVisible(childToolBarItem, visible);
                }
            }

            var x = parent as ToolStripDropDownItem;
            if (x != null && x.HasDropDownItems)
                foreach (ToolStripItem dropDownItem in x.DropDownItems)
                {
                    var v = false;
                    var e = false;
                    MenuEntry me = null;
                    if (!mm.TryGetState(dropDownItem, out me))
                        _defaultState.TryGetValue(dropDownItem, out me);
                    if (visible)
                    {
                        if (me != null)
                        {
                            v = me.Visible;
                            e = v && enabled && me.GetEnabled(dropDownItem);
                        }
                        else
                        {
                            v = true;
                            e = enabled;
                        }
                    }
                    if (!AllowEnabledHiddenMenuItems)
                    {
                        if (me != null)
                            me.SetMenuItemEnabled(e);
                        else
                            dropDownItem.Enabled = e;
                    }
                    SetVisibleToChildrenOf(mm, dropDownItem, v, e);
                }

        }
        public void MapAsChild(ToolStripMenuItem parent, ToolStripButton childToolStripItem)
        {

            List<ToolStripButton> list;
            if (!_children.TryGetValue(parent, out list))
            {
                list = new List<ToolStripButton>();
                _children.Add(parent, list);
                parent.Disposed += delegate { _children.Remove(parent); };
            }
            list.Add(childToolStripItem);
            _parents[childToolStripItem] = parent;
            childToolStripItem.Disposed += delegate { _parents.Remove(childToolStripItem); };
        }
        public void MapMainMenu(int menuKey, params ToolStrip[] menuControls)
        {


            if (!UserSettings.ShowToolStrip)
            {
                var x = new List<ToolStrip>(menuControls);
                x.RemoveAll(strip =>
                {

                    if (strip.GetType() == typeof(ToolStrip))
                    {
                        strip.Visible = false;
                        return true;
                    }
                    return false;
                });
                menuControls = x.ToArray();
            }
            _menus.Add(menuKey, menuControls);
            if (_menuForm != null)
            {
                foreach (var menuControl in menuControls)
                    _menuForm.Controls.Remove(menuControl);
            }
        }
        int _activeMenu = -1;


        bool IsMenuActive(ToolStrip owner)
        {
            if (owner == null)
                return false;
            if (!_menus.ContainsKey(_activeMenu))
                return false;
            foreach (var ts in _menus[_activeMenu])
            {
                if (owner == ts)
                    return true;
            }
            return false;
        }
        bool _showMenu = true;
        ToolStrip _activeToolBar;
        public bool Activate(int menuKey, bool showMenu = true, Text parentMenu = null)
        {
            _showMenu = showMenu;
            if (!_menus.ContainsKey(menuKey))
            {
                if (_contextMenuMap == null)
                    return false;
                if (_menuForm == null)
                    return false;
                //build menu based on context menu map
                var x = _contextMenuMap.Create(menuKey) as ContextMenuStripBase;
                if (x != null)
                {
                    MenuStrip ms = new MenuStripBase();
                    var tb = new ToolStrip();

                    var arr = new List<ToolStrip>();
                    arr.Add(ms);
                    var pop = tb.Items;
                    if (!UserSettings.ShowToolStrip)
                    {
                        tb = null;
                        pop = null;
                    }
                    else arr.Add(tb);
                    ms.SuspendLayout();

                    x.PopulateMenu(this, ms.Items, pop);
                    ms.ResumeLayout();
                    ms.PerformLayout();
                    _menus.Add(menuKey, arr.ToArray());
                }


            }
            if (_activeMenu != -1 && Text.IsNullOrEmpty(parentMenu))
            {
                if (!UserSettings.VersionXpaCompatible)
                    return false;
                else
                {
                    Context.Current.InvokeUICommand(
                        () =>
                        {
                            SendMenuItems(menuKey,
                                items =>
                                {
                                    foreach (var ts in _menus[_activeMenu])
                                    {
                                        if (ts is MenuStrip)
                                            ts.Items.AddRange(items);
                                    }
                                    if (!_itemsToRemoveOnDeactivate.ContainsKey(menuKey))
                                        _itemsToRemoveOnDeactivate[menuKey] = new List<ToolStripItem>();
                                    _itemsToRemoveOnDeactivate[menuKey].AddRange(items);
                                });
                        });
                    return true;
                }
            }
            _activeMenu = menuKey;
            Firefly.Box.Context.Current.InvokeUICommand(
                () =>
                {
                    var m = _menus[menuKey];
                    if (Text.IsNullOrEmpty(parentMenu))
                    {
                        for (var i = m.Length - 1; i >= 0; i--)
                        {
                            var toolStripse = m[i];
                            toolStripse.Enabled = true;
                            toolStripse.Visible = true;
                            if (_menuForm != null)
                            {
                                if (_menuForm.MainMenuStrip == null)
                                {
                                    var z = toolStripse as MenuStrip;
                                    if (z != null)
                                    {
                                        _menuForm.MainMenuStrip = z;
                                        if (!showMenu)
                                            z.Visible = showMenu;
                                    }
                                    else _activeToolBar = toolStripse;
                                }
                                _menuForm.Controls.Add(toolStripse);
                            }
                        }
                    }
                    else
                    {
                        parentMenu = parentMenu.TrimEnd();
                        if (parentMenu.ToString().EndsWith("\\"))
                            parentMenu = parentMenu.Remove(parentMenu.Length - 1);
                        List<ToolStripItem> r = null;
                        if (_itemsMap.TryGetValue(parentMenu, out r))
                        {
                            foreach (var item in r)
                            {
                                var x = item as ToolStripDropDownItem;
                                if (x != null)
                                    SendMenuItems(menuKey, a => x.DropDownItems.AddRange(a));
                            }
                        }
                    }

                });
            return true;
        }

        void SendMenuItems(int menuKey, Action<ToolStripItem[]> toMe)
        {
            ToolStrip cm = null;
            if (_contextMenuMap != null)
                cm = _contextMenuMap.Create(menuKey);
            if (cm == null)
                cm = _menus[menuKey][0];
            if (cm != null)
            {
                var rm = cm as ENV.UI.Menus.ContextMenuStripBase;
                if (rm != null)
                    rm.InitMenus();
                var iHaveMM = cm as IHaveAMenu;
                if (iHaveMM != null)
                    iHaveMM.DoOnMenu(mm => mm._control = _control);
                var l = new List<ToolStripItem>();
                foreach (ToolStripItem iii in cm.Items)
                    l.Add(iii);
                toMe(l.ToArray());
            }
        }

        Dictionary<int, List<ToolStripItem>> _itemsToRemoveOnDeactivate = new Dictionary<int, List<ToolStripItem>>();

        public Bool Deactivate(int menuKey)
        {
            if (!_menus.ContainsKey(menuKey))
                return false;

            if (_activeMenu != -1 && UserSettings.VersionXpaCompatible)
            {
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        if (_itemsToRemoveOnDeactivate.ContainsKey(menuKey) && _itemsToRemoveOnDeactivate[menuKey] != null)
                            foreach (var ts in _menus[_activeMenu])
                                _itemsToRemoveOnDeactivate[menuKey].ForEach(i => ts.Items.Remove(i));
                    });

                return true;
            }

            _activeMenu = -1;
            Form mdi = _menuForm;
            Firefly.Box.Context.Current.InvokeUICommand(
                () =>
                {
                    foreach (var toolStripse in _menus[menuKey])
                    {
                        if (_menuForm != null)
                            _menuForm.Controls.Remove(toolStripse);
                        toolStripse.Visible = false;
                        toolStripse.Enabled = false;
                        if (mdi.MainMenuStrip == toolStripse)
                            mdi.MainMenuStrip = null;
                    }
                });
            return true;
        }

        public void SetCurrentStateAsDefault()
        {
            _myContext = Firefly.Box.Context.Current;
            Firefly.Box.Context.Current.InvokeUICommand(
                () =>
                {
                    foreach (var x in _menus)
                    {
                        foreach (var toolStrip in x.Value)
                        {
                            if (!toolStrip.Enabled)
                                toolStrip.Enabled = true;
                        }
                    }
                });
            foreach (var entry in _defaultState)
                entry.Value.SetCurrentStateAsDefault();
            Firefly.Box.Context.Current.InvokeUICommand(
                () =>
                {
                    foreach (var x in _menus)
                    {
                        foreach (var toolStrip in x.Value)
                        {
                            toolStrip.Visible = x.Key == _activeMenu;
                            toolStrip.Enabled = x.Key == _activeMenu;
                            var z = toolStrip as MenuStrip;
                            if (!_showMenu && z != null && x.Key == _activeMenu)
                                z.Visible = false;
                        }
                    }
                });
        }

        internal void ForeachMenu(Action<ToolStrip> action)
        {
            foreach (var toolStripse in _menus)
            {
                foreach (var t in toolStripse.Value)
                {
                    action(t);
                }
            }
        }

        ENV.UI.ContextMenuMap _contextMenuMap;
        public void SetMenuMap(ENV.UI.ContextMenuMap contextMenuMap)
        {
            _contextMenuMap = contextMenuMap;
        }

        public class ContextMenuState
        {
            Dictionary<ToolStripItem, MenuEntry> _state = new Dictionary<ToolStripItem, MenuEntry>();
            Dictionary<string, Action<MenuEntry>> _keysThatWereTouched = new Dictionary<string, Action<MenuEntry>>();

            MenuManager _menuManager;

            public ContextMenuState(MenuManager menuManager)
            {
                _menuManager = menuManager;
            }

            internal bool TryGetState(ToolStripItem childToolBarItem, out MenuEntry me)
            {
                return _state.TryGetValue(childToolBarItem, out me);
            }
            public void Apply()
            {
                _menuManager.Apply(this);
            }

            internal void DoOnMenuName(Text name, string property, Action<MenuEntry> what)
            {
                List<ToolStripItem> result;
                if (_menuManager.TryGeyItems(name, out result))
                {
                    foreach (var toolStripItem in result)
                    {
                        MenuEntry me;
                        if (_state.TryGetValue(toolStripItem, out me))
                            what(me);
                        else if (_menuManager.TryGetValueDefaultState(toolStripItem, out me))
                        {
                            var x = me.Clone(this);
                            _state.Add(toolStripItem, x);
                            what(x);
                        }
                    }
                }
                _keysThatWereTouched[name + "." + property] = what;
            }



            public void ApplyTo(ContextMenuState contextState)
            {
                foreach (var key in _keysThatWereTouched)
                {
                    var i = key.Key.LastIndexOf('.');
                    contextState.DoOnMenuName(key.Key.Substring(0, i), key.Key.Substring(i + 1), m => key.Value(m));
                }
            }

            internal void Reset()
            {
                var x = _keysThatWereTouched;
                _keysThatWereTouched = new Dictionary<string, Action<MenuEntry>>();
                _state.Clear();
                Apply();

            }
        }



        static void DoOnMenuName(Text name, string property, Action<MenuEntry> what)
        {
            DoOnMenuManagers(m => m.ContextState.DoOnMenuName(name, property, what));
        }
        public static void ResetMenu()
        {
            DoOnMenuManagers(m => m.ContextState.Reset());
        }

        public static void EnableMenu(Text name, bool on)
        {
            DoOnMenuName(name, "Enabled", entry => entry.Enabled = on);

        }

        public static void ShowMenu(Text name, bool on)
        {
            DoOnMenuName(name, "Visible", entry => entry.Visible = on);

        }

        public static void CheckMenu(Text name, bool on)
        {
            DoOnMenuName(name, "Checked", entry => entry.Checked = on);

        }
        internal static void SetMenuText(Text name, Text text)
        {
            DoOnMenuName(name, "Text", entry => entry.Text = text);

        }


        ContextStatic<ContextMenuState> _contextState;
        public ContextMenuState ContextState
        {
            get
            {
                return _contextState.Value;
            }
        }

        public static bool AllowEnabledHiddenMenuItems { get; set; }

        internal static string GetMenuText(ToolStripItem m)
        {
            string result = m.Text;
            DoOnMenuManagers(mm =>
            {
                MenuEntry me;
                if (mm._defaultState.TryGetValue(m, out me))
                {
                    result = me.OriginalText;
                }
            });
            return result;
        }

        internal static string GetOriginalMenuText(ToolStripItem m)
        {
            string result = m.Text;
            if (!ENV.UserSettings.VersionXpaCompatible)
                DoOnMenuManagers(mm =>
                {
                    MenuEntry me;
                    if (mm._defaultState.TryGetValue(m, out me))
                    {
                        result = me.Text;
                    }
                });
            return result;
        }

        System.Windows.Forms.Form _menuForm;
        public MenuManager(System.Windows.Forms.Form menuForm, ENV.UI.ContextMenuMap menuMap)
            : this(menuForm)
        {
            SetMenuMap(menuMap);
        }
        public MenuManager(System.Windows.Forms.Form menuForm)
            : this()
        {
            _menuForm = menuForm;
            _control = menuForm;
            _menuForm.Disposed +=
                (sender, args) =>
                {
                    foreach (var m in _menus)
                    {
                        foreach (var ts in m.Value)
                        {
                            if (!ts.IsDisposed)
                                ts.Dispose();
                        }
                    }
                };
            MenuStripBase ms = null;
            if (_menuForm.MainMenuStrip != null)
            {
                ms = _menuForm.MainMenuStrip as MenuStripBase;
            }
            ToolStrip tb = null;
            foreach (var item in _menuForm.Controls)
            {
                var ts = item as ToolStrip;
                if (ts != null && !(ts is StatusStrip))
                {
                    tb = ts;
                    DetermineEnabled(tb);
                    break;
                }
            }
            if (ms != null)
            {
                DetermineEnabled(ms);
                ms.Init(this, tb);

                MapMainMenu(999, ms, tb);
                Activate(999);

            }
        }

        Control _control;
        public MenuManager(ContextMenuStrip strip)
            : this()
        {

            _control = strip;
        }
        internal MenuManager()
        {
            this.MainUIState = new ContextMenuState(this);
            _contextState = new ContextStatic<ContextMenuState>(() => new ContextMenuState(this));
        }

        bool AreAllParentsAllowedAndFirstParentAvailable(ToolStripItem item, ContextMenuState mm)
        {
            var i = item;
            if (i.OwnerItem != null && !i.OwnerItem.Available) return false;
            while (i.OwnerItem != null)
            {
                i = i.OwnerItem;

                MenuEntry me;
                if (mm.TryGetState(i, out me) || _defaultState.TryGetValue(i, out me))
                {
                    if (!me.CheckCondition()) return false;
                }
            }
            return true;
        }




        internal class MenuEntry
        {
            ContextMenuState _parent;
            System.Windows.Forms.ToolStripItem _item;
            Func<bool> _condition;
            bool _visible;
            bool _enabled;
            bool _manageItemEnabled;
            bool _checked;
            string _text;
            string _toolTipText;

            public bool Checked
            {
                set
                {
                    if (value == _checked) return;
                    _checked = value;
                    Apply();
                }
            }

            public bool Enabled
            {
                set
                {
                    if (value == _enabled) return;
                    _enabled = value;
                    Apply();
                }
            }

            public bool GetEnabled(ToolStripItem item)
            {
                return _manageItemEnabled ? _enabled : item.Enabled;
            }

            bool _hideToolstrip;
            public bool Visible
            {
                set
                {
                    if (value == _visible) return;
                    _visible = value;
                    Apply();
                }
                get { return _visible && _condition(); }
            }

            public bool CheckCondition() { return _condition(); }

            public Text Text
            {
                set
                {
                    if (value == _text) return;
                    _text = value;
                    Apply();
                }
                get
                {
                    return Languages.Translate(_text);
                }
            }

            public string OriginalText { get { return _text; } }

            MenuManager _mSystem;
            internal MenuEntry(MenuManager system, ContextMenuState parent, ToolStripItem item, Func<bool> condition, bool manageItemEnabled)
            {
                _mSystem = system;
                _parent = parent;
                _item = item;
                _condition = condition;
                _manageItemEnabled = manageItemEnabled;
                SetCurrentStateAsDefault();
            }

            public void Apply()
            {
                Context.Current.InvokeUICommand(
                    () =>
                    {
                        _item.Text = Languages.Translate(_text);
                        _item.ToolTipText = Languages.Translate(_toolTipText);
                        var m = _item as ToolStripMenuItem;
                        if (m != null)
                            m.Checked = _checked;

                        var v = _visible && _condition();

                        var isAvailable = v;
                        var i = _item;
                        while (isAvailable && i.OwnerItem != null)
                        {
                            i = i.OwnerItem;
                            isAvailable = i.Available;
                        }

                        var b = _item as ToolStripButton;
                        Action fixSeparators = () => { };

                        var menuActive = _mSystem.IsMenuActive(_item.Owner);
                        var toolStrip = _mSystem._activeToolBar;

                        if (b != null)
                        {
                            b.Checked = _checked;

                            ToolStripItem parentItem;
                            if (v && _mSystem.TryGetParentValue(b, out parentItem))
                                v = parentItem.Available && _mSystem.AreAllParentsAllowedAndFirstParentAvailable(parentItem, _parent);

                            if (menuActive)
                            {
                                if (v != _item.Available)
                                    fixSeparators = () => FixSeparators(_item.Owner);
                            }
                        }




                        var e = (AllowEnabledHiddenMenuItems || isAvailable) && GetEnabled(_item);
                        _item.Visible = v;
                        SetMenuItemEnabled(e);

                        fixSeparators();

                        _mSystem.SetVisibleToChildrenOf(_parent, _item, v, e);
                        if (menuActive && toolStrip != null)
                        {
                            if (!v && !_hideToolstrip)
                            {
                                bool hasVisibleButton = false;
                                foreach (var tsi in toolStrip.Items)
                                {
                                    var b1 = tsi as ToolStripItem;
                                    if (b1 != null && !(b1 is ToolStripSeparator) && b1.Available)
                                        hasVisibleButton = true;
                                }
                                if (!hasVisibleButton)
                                {
                                    _hideToolstrip = true;
                                    toolStrip.Visible = false;
                                }
                            }
                            if (v && (_hideToolstrip || !toolStrip.Visible && toolStrip.Enabled))
                            {
                                _hideToolstrip = false;
                                toolStrip.Visible = true;
                            }
                        }
                    });
            }

            public void SetMenuItemEnabled(bool enabled)
            {
                if (_manageItemEnabled) _item.Enabled = enabled;
            }

            public MenuEntry Clone(ContextMenuState parent)
            {
                return new MenuEntry(_mSystem, parent, _item, _condition, _manageItemEnabled) { _text = this._text, _toolTipText = this._toolTipText, _visible = this._visible, _enabled = this._enabled, _checked = this._checked };
            }

            public void SetCurrentStateAsDefault()
            {
                _text = _item.Text;
                _toolTipText = _item.ToolTipText;
                _visible = _item.Available;
                _enabled = _item.Enabled;
                var b = _item as ToolStripButton;
                if (b != null)
                    _checked = b.Checked;
                var m = _item as ToolStripMenuItem;
                if (m != null)
                    _checked = m.Checked;
            }

            public void ApplyTo(MenuEntry other)
            {
                other._text = _text;
                other._toolTipText = _toolTipText;
                other._visible = _visible;
                other._enabled = _enabled;
                other._checked = _checked;

            }
        }



        static void FixSeparators(ToolStrip toolStrip)
        {
            var x = 0;
            while (x < toolStrip.Items.Count && !(toolStrip.Items[x] is ToolStripSeparator))
                x++;
            while (x < toolStrip.Items.Count && toolStrip.Items[x] is ToolStripSeparator)
            {
                var separator = toolStrip.Items[x];
                var separatorShouldBeVisible = false;
                x++;
                while (x < toolStrip.Items.Count && !(toolStrip.Items[x] is ToolStripSeparator))
                    if (toolStrip.Items[x++].Available) separatorShouldBeVisible = true;
                separator.Available = separatorShouldBeVisible;
            }
        }

        public void Map(string name, System.Windows.Forms.ToolStripItem item)
        {
            Map(name, item, () => true);
        }

        public void Map(string name, System.Windows.Forms.ToolStripItem item, bool manageItemEnabled)
        {
            Map(name, item, () => true, manageItemEnabled);
        }

        public void Map(string name, System.Windows.Forms.ToolStripItem item, Func<bool> condition)
        {
            Map(name, item, condition, true);
        }















        public static void Clear()
        {
            _currentInternalForTesting = new MenuManager();

        }



        static ContextStatic<Dictionary<string, ImageToButton>> _images = new ContextStatic<Dictionary<string, ImageToButton>>(() => new Dictionary<string, ImageToButton>());

        class ImageToButton
        {
            string _originalPath;
            string _lastPath;
            int _imageHeight;

            System.Drawing.Image _lastImage;

            public ImageToButton(string originalPath, int imageHeight)
            {
                _originalPath = originalPath;
                _imageHeight = imageHeight;
                _lastPath = PathDecoder.DecodePath(originalPath);
                _lastImage = GetMenuImage(_lastPath, _imageHeight);
            }

            List<ToolStripItem> _items = new List<ToolStripItem>();

            public void Bind(ToolStripItem tsb)
            {
                tsb.Image = _lastImage;
                _items.Add(tsb);
                tsb.Disposed += (s, e) => { _items.Remove(tsb); };
            }

            public void PathDecoderChanged()
            {
                var s = PathDecoder.DecodePath(_originalPath);
                if (s != _lastPath)
                {
                    _lastPath = s;
                    _lastImage = GetMenuImage(_lastPath, _imageHeight);
                    foreach (var toolStripItem in _items)
                    {
                        toolStripItem.Image = _lastImage;
                    }
                }
            }
        }

        static bool _registeredPathDecodeObserver = false;
        public static void BindImage(ToolStripItem tsb, string imagePath)
        {
            {
                ImageToButton itb;
                if (!_images.Value.TryGetValue(imagePath, out itb))
                {
                    _images.Value.Add(imagePath, itb = new ImageToButton(imagePath, tsb.Height));
                }
                itb.Bind(tsb);
                if (!_registeredPathDecodeObserver)
                {
                    _registeredPathDecodeObserver = true;
                    PathDecoder.Change += () => RefreshImages();
                }
            }

        }

        static void RefreshImages()
        {
            foreach (var vals in _images.Value.Values)
            {
                vals.PathDecoderChanged();
            }
        }
        public static void EnableContextItem(ToolStripItem toolStripItem, Command command)
        {
            var isEnabled = true;
            ENV.Common.RunOnLogicContext(toolStripItem.GetCurrentParent(), () => isEnabled = command.Enabled);
            toolStripItem.Enabled = isEnabled;
        }
        public static void DetermineEnabled(ToolStrip menuStrip)
        {
            Action x = () => menuStrip.Enabled = menuStrip.Visible && !Context.Current.Busy;
            EventHandler eh = (s, a) => { if (menuStrip.Visible && !Context.Current.Busy) menuStrip.Enabled = true; };
            Firefly.Box.Context.Current.BusyChanged += x;
            menuStrip.VisibleChanged += eh;
            menuStrip.Disposed += delegate
            {
                Context.Current.BusyChanged -= x;
                menuStrip.VisibleChanged -= eh;
            };
        }
        public void Run(ToolStripItem menuItem, Action what)
        {
            Run(menuItem, what, false);
        }
        public void RunOnActiveContext(ToolStripItem menuItem, Action what)
        {
            Common.RunOnLogicContext(menuItem.Owner, () => Run(menuItem, what, true));
        }
        public void RunOnContext(string contextName, ToolStripItem menuItem, Action what)
        {
            Run(menuItem, what, false, contextName);
        }
        public void RunOnActiveContext(Action what)
        {
            Firefly.Box.UI.Form.RunInActiveLogicContext(() =>
                Run(null, what, true));
        }

        void Run(Action what)
        {
            Run(null, what);
        }

        void Run(ToolStripItem menuItem, Action what, bool onActiveContext, string contextName = null)
        {
            if (menuItem != null)
                ENV.UserMethods.SetMenu(menuItem);
            var activeContext = Firefly.Box.Context.Current;
            var z = what;
            what = () =>
            {
                if (onActiveContext && !InSdi())
                    activeContext.BeginInvoke(z);
                else if (Text.IsNullOrEmpty(contextName))
                    _myContext.BeginInvoke(z);
                else
                {
                    contextName = contextName.TrimEnd();
                    foreach (var cont in Firefly.Box.Context.ActiveContexts)
                    {
                        if (string.Compare(contextName, UserMethods.GetContextName(cont),
                                StringComparison.InvariantCultureIgnoreCase) == 0)
                        {
                            var x = cont;
                            cont.BeginInvoke(z);
                            return;
                        }
                    }
                    _myContext.BeginInvoke(z);
                }

            };
            if (IgnoreCloseAllTasks)
            {
                what();
                return;
            }


            _control.BeginInvoke(new Action(
                () =>
                {
                    //workarround bug introduced by microsoft to windows forms menu in .net 4.5 and affects all applications built in .net 4 that are run on machines that installed .net 4.5
                    // when the bug will be fixed, the first if will return false, and the workarround will cease to exist.
                    {
                        if (
                            (bool)
                            System.Reflection.Assembly.GetAssembly(typeof(System.Windows.Forms.ToolStripManager)).
                                GetType(
                                    "System.Windows.Forms.ToolStripManager+ModalMenuFilter").GetProperty(
                                        "InMenuMode",
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Static).GetValue(null, new object[0]))
                            System.Reflection.Assembly.GetAssembly(typeof(System.Windows.Forms.ToolStripManager)).
                                GetType(
                                    "System.Windows.Forms.ToolStripManager+ModalMenuFilter").GetMethod(
                                        "ExitMenuMode",
                                        System.Reflection.BindingFlags.NonPublic |
                                        System.Reflection.BindingFlags.Static).Invoke(null, new object[0]);
                    }
                    what();
                }));
        }
        public void CloseActiveControllersAndRun(ToolStripItem menuItem, Action runTask)
        {
            Run(menuItem,
                () =>
                {
                    Firefly.Box.UI.Form.LastClickedControl = null;
                    if (IgnoreCloseAllTasks || (InSdi()))
                    {
                        runTask();
                        return;
                    }
                    if (!UserSettings.Version8Compatible)
                        Context.Current.CloseAllTasksAndRun(runTask);
                    else
                    {
                        Action Run1 = () =>
                        {
                            itemsInRunMenuStack++;
                            try
                            {
                                runTask();
                            }
                            finally
                            {
                                itemsInRunMenuStack--;
                            }
                        };
                        if (itemsInRunMenuStack == 0)
                        {
                            var t = Firefly.Box.Context.Current.ActiveTasks;
                            bool ok = true;
                            if (t.Count > 0)
                            {
                                ControllerBase.SendInstanceBasedOnTaskAndCallStack(t[t.Count - 1],
                                    y => ok = y._application != null && !y._application.InStart);
                            }
                            if (ok)
                            {
                                Run1();
                                return;
                            }
                        }

                        Context.Current.CloseAllTasksAndRun(Run1);
                    }
                });
        }
        public static bool IgnoreCloseAllTasks = false;
        static int itemsInRunMenuStack = 0;
        public void CloseActiveControllersAndRun(Action runTask)
        {
            CloseActiveControllersAndRun(null, runTask);
        }

        bool InSdi()
        {
            return _menuForm != null && !Common.IsRootMDI(_menuForm);
        }


        public void Raise(Text text)
        {
            ENV.UserMethods.Instance.KBPut(text);
        }

        public void Raise(Keys keys)
        {
            ENV.UserMethods.Instance.KBPut(keys);
        }

        internal static void RaiseStatic(Command command)
        {
            Firefly.Box.UI.Form.RunInActiveLogicContext(() =>
                ENV.UserMethods.Instance.KBPut(command));
        }

        public void Raise(Command command, params object[] args)
        {
            ENV.UserMethods.Instance.KBPut(command, args);
        }
        static ContextStatic<Dictionary<string, Image>> _imageCache = new ContextStatic<Dictionary<string, Image>>(() => new Dictionary<string, Image>());
        static Image GetMenuImage(string imageLocation, int imageMaxHeight)
        {
            string path = PathDecoder.DecodePath(imageLocation);
            try
            {
                if (Text.IsNullOrEmpty(path))
                    return null;
                Image image;
                if (!_imageCache.Value.TryGetValue(path, out image))
                {
                    image = ENV.Utilities.ImageLoader.Load(path);
                    if (image == null)
                    {
                        return null;
                    }
                    if (image.Height > imageMaxHeight)
                        image = new Bitmap(image, image.Width * imageMaxHeight / image.Height, imageMaxHeight);
                    var bmp = image as Bitmap;
                    if (bmp != null)
                        try { bmp.MakeTransparent(Color.FromArgb(192, 192, 192)); }
                        catch { }
                    _imageCache.Value.Add(path, bmp);
                    return bmp;
                }
                return image;
            }
            catch (Exception e)
            {
                ENV.ErrorLog.WriteToLogFile(e, "");
            }
            return null;
        }
        public void SetImageByIndex(ToolStripItem item, int imageIndex)
        {
            var x = GetMenuImage("%MenuImages%" + imageIndex + ".png", item.Height);
            if (x != null)
                item.Image = x;
            else
            {
                var b = new Bitmap(19, 19);
                var f = new Font("Arial", 8);
                using (var g = Graphics.FromImage(b))
                {
                    g.DrawString(imageIndex.ToString(), f, Brushes.Black, 0, 0);
                }
            }

        }

        public void ApplyTo(IHaveAMenu haveAMenu)
        {
            haveAMenu.DoOnMenu(m =>
            {
                ContextState.ApplyTo(m.ContextState);
                m.ContextState.Apply();
                m._myContext = _myContext;

                var cm = m._control as ContextMenuStrip;
                if (cm != null)
                {
                    if (m._itemsToRemoveOnDeactivate.Count == 0)
                    {
                        Context.Current.InvokeUICommand(
                            () =>
                            {
                                foreach (var key in _itemsToRemoveOnDeactivate.Keys)
                                {
                                    SendMenuItems(key, items =>
                                    {
                                        cm.Items.AddRange(items);
                                        m._itemsToRemoveOnDeactivate[key] = new List<ToolStripItem>(items);
                                    });
                                }
                            });
                    }
                }
            });
        }

    }


    public interface IHaveAMenu
    {
        void DoOnMenu(Action<MenuManager> what);
    }
    namespace UI.Menus
    {
        class MenuCollection
        {
            List<IMenuEntry> _items = new List<IMenuEntry>();

            Dictionary<int, ToolBarGroup> _toolBarGroups = new Dictionary<int, ToolBarGroup>();

            public void Add(params IMenuEntry[] items)
            {
                this._items.AddRange(items);
            }
            public void PopulateMenu(MenuManager mm, ToolStripItemCollection dropDownItems, ToolStripItemCollection toolbox = null)
            {
                {
                    int i = 0;
                    foreach (var item in _items)
                    {
                        item.AddToPulldownMenu(mm, x => dropDownItems.Insert(i++, x));
                    }

                }
                if (toolbox != null)
                {
                    int i = 0;
                    var sortedToolbarGroups = new List<KeyValuePair<int, ToolBarGroup>>(_toolBarGroups);
                    sortedToolbarGroups.Sort((a, b) => a.Key - b.Key);


                    foreach (var item in sortedToolbarGroups)
                    {
                        if (toolbox.Count > 0 && (!(toolbox[toolbox.Count - 1] is ToolStripSeparator)))
                            toolbox.Insert(i++, new ToolStripSeparator());
                        item.Value.AddToToolbox(mm, x => toolbox.Insert(i++, x));
                    }
                }
            }
            public void PopulateContextMenu(MenuManager mm, ToolStripItemCollection dropDownItems)
            {
                {
                    int i = 0;
                    foreach (var item in _items)
                    {
                        item.AddToContextMenu(mm, x => dropDownItems.Insert(i++, x));
                    }

                }

            }
            public ToolBarGroup AddToolBarGroup()
            {
                var x = new ToolBarGroup();
                int max = -1;
                foreach (var item in _toolBarGroups)
                {
                    if (item.Key > max)
                        max = item.Key;
                }
                _toolBarGroups.Add(max + 1, x);
                return x;
            }
            public ToolBarGroup GetToolBarGroup(int groupIndex)
            {
                ToolBarGroup r;
                if (!_toolBarGroups.TryGetValue(groupIndex, out r))
                {
                    _toolBarGroups.Add(groupIndex, r = new ToolBarGroup());
                }
                return r;

            }

            internal IEnumerable<IMenuEntry> getItems()
            {
                return _items;
            }
        }
        [DesignerCategory("")]
        public class ContextMenuStripBase : ContextMenuStrip, IHaveAMenu, Firefly.Box.UI.Advanced.IContextMenu
        {
            public void Add(params IMenuEntry[] items)
            {
                _menuCollection.Add(items);
            }
            internal protected void InitBasedOn(string assemblyName, string applicationMdiMenuClassName)
            {
                try
                {
                    var x = AbstractFactory.CreateInstance(assemblyName, applicationMdiMenuClassName) as MenuStripBase;
                    if (x != null)
                    {
                        _menuCollection = x.GetMenuCollection();
                    }
                }
                catch { }

            }
            public ToolBarGroup GetToolBarGroup(int groupIndex)
            {
                return _menuCollection.GetToolBarGroup(groupIndex);
            }
            public ContextMenuStripBase(System.ComponentModel.IContainer container) : base(container)
            {
                myConstructor();
            }

            public ContextMenuStripBase() : base()
            {
                myConstructor();
            }
            void myConstructor()
            {
                _menuManager = new MenuManager(this);
            }
            protected MenuManager _menuManager;
            MenuCollection _menuCollection = new MenuCollection();
            bool _inited;
            public void DoOnMenu(Action<MenuManager> action)
            {
                action(_menuManager);
            }
            internal void InitMenus()
            {
                if (!_inited)
                {
                    _inited = true;
                    _menuCollection.PopulateContextMenu(_menuManager, this.Items);
                    _menuManager.SetCurrentStateAsDefault();
                }
            }
            protected override void OnOpening(System.ComponentModel.CancelEventArgs e)
            {
                InitMenus();
                MenuManager.ApplyToContextMenu(this, this);
                FireOnOpeningEvent(Items);
                DisplayedItems.Clear();
                SetDisplayedItems();
                e.Cancel = DisplayedItems.Count == 0;
                base.OnOpening(e);
            }

            private void FireOnOpeningEvent(ToolStripItemCollection items)
            {
                foreach (var item in items)
                {
                    var x = item as SuspendedToolStripMenuItem;
                    if (x != null)
                    {
                        x.OnContextOpening();
                        FireOnOpeningEvent(x.DropDownItems);
                    }

                }
            }

            protected T Create<T>()
            {
                return AbstractFactory.Create<T>();
            }

            internal void PopulateMenu(MenuManager menuManager, ToolStripItemCollection items, ToolStripItemCollection pop)
            {
                _menuCollection.PopulateMenu(menuManager, items, pop);
            }

            bool IContextMenu.IsEmpty()
            {
                InitMenus();
                return Items.Count == 0;
            }
        }
        [DesignerCategory("")]
        public class MenuStripBase : System.Windows.Forms.MenuStrip
        {
            public static bool MultiLineMenu = true;
            MenuCollection _menuCollection = new MenuCollection();

            public MenuStripBase()
            {

                if (!MultiLineMenu)
                {
                    CanOverflow = true;
                }
                else
                    LayoutStyle = ToolStripLayoutStyle.Flow;
                Padding = new Padding(0);
                Size = new Size(600, 19);
            }

            public IEnumerable<IMenuEntry> MenuEntries { get { return this._menuCollection.getItems(); } }



            protected T Create<T>()
            {
                return AbstractFactory.Create<T>();
            }
            public void Add(params IMenuEntry[] items)
            {
                _menuCollection.Add(items);
            }
            public ToolBarGroup GetToolBarGroup(int groupIndex)
            {
                return _menuCollection.GetToolBarGroup(groupIndex);
            }

            internal MenuCollection GetMenuCollection()
            {
                return _menuCollection;
            }
            bool _addItems = true;

            protected override void OnItemAdded(ToolStripItemEventArgs e)
            {
                if (DesignMode && _addItems)
                {
                    _addItems = false;
                    Init(new MenuManager(), null);
                }
                base.OnItemAdded(e);

            }
            internal void Init(MenuManager mm, ToolStrip ts)
            {
                _menuCollection.PopulateMenu(mm, this.Items, ts == null ? null : ts.Items);
            }
            protected override void OnItemRemoved(ToolStripItemEventArgs e)
            {
                if (DesignMode && Items.Count == 0)
                    _addItems = true;
                base.OnItemRemoved(e);
            }
        }

        public interface IMenuEntry
        {
            void AddToPulldownMenu(MenuManager mm, Action<ToolStripItem> add);
            void AddToContextMenu(MenuManager mm, Action<ToolStripItem> add);
            void AddToToolbar(MenuManager mm, Action<ToolStripItem> add);
        }
        public class Separator : IMenuEntry
        {
            public string Name { get; set; }
            public Separator(string Name = "")
            {
                this.Name = Name;
            }
            public void AddToPulldownMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                var item = new System.Windows.Forms.ToolStripSeparator();
                mm.Map(Name ?? "", item);
                add(item);
            }

            public void AddToToolbar(MenuManager mm, Action<ToolStripItem> add)
            {
                var item = new System.Windows.Forms.ToolStripSeparator();
                mm.Map(Name ?? "", item);
                add(item);
            }
            public void AddToContextMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                this.AddToPulldownMenu(mm, add);
            }
        }
        public class RaiseCommand : MenuEntry
        {
            Command _command;
            object[] _args;
            public RaiseCommand(string caption, Command command, params object[] args) : base(caption)
            {
                _args = args;
                _command = command;
            }



            protected override void DecorateMenuItem(ToolStripItem item, MenuManager mm)
            {
                item.Click += (s, e) =>
                {
                    if (!string.IsNullOrWhiteSpace(ContextName))
                        mm.RunOnContext(ContextName, item, () => mm.Raise(_command, _args));
                    else
                        mm.RunOnActiveContext(item, () => mm.Raise(_command, _args));
                };
            }

        }

        public class ManagedCommand : MenuEntry
        {
            Command _command;
            object[] _args;
            public ManagedCommand(string caption, Command command, params object[] args) : base(caption)
            {
                _command = command;
                _args = args;
            }

            protected override void DecoratePullDownMenu(SuspendedToolStripMenuItem item, MenuManager mm, bool contextMenu)
            {
                if (contextMenu)
                {
                    item.Click += (s, e) =>
                    {

                        if (!string.IsNullOrWhiteSpace(ContextName))
                            mm.RunOnContext(ContextName, item, () => mm.Raise(_command, _args));
                        else
                            mm.RunOnActiveContext(item, () => mm.Raise(_command, _args));
                    };



                    item.ContextOpening += () =>
                    {
                        MenuManager.EnableContextItem(item, _command);
                    };

                }
                else
                {
                    _command.BindMenuItem(item);
                }
            }


            protected override void DecorateMenuItem(ToolStripItem item, MenuManager mm)
            {
                if (_commandsAvailableInFilterMode.Contains(_command))
                {
                    item.Visible = !_filterCommands.Contains(_command);

                    TemplateModeFilter.SetTemplateModeMenus(item);
                }
            }

            protected override void DecorateToolBox(ToolStripButton item, MenuManager mm)
            {
                _command.BindMenuItem(item);
            }
            protected override bool LetMenuManagerManageEnabled()
            {
                return false;
            }
            static HashSet<Command> _filterCommands = new HashSet<Command>() { Commands.ClearCurrentValueInTemplate, Commands.ClearTemplate, Commands.TemplateFromValues, Commands.TemplateToValues, Commands.TemplateExpression },
                _commandsAvailableInFilterMode = new HashSet<Command>(_filterCommands) { Command.UndoChangesInRowAndExit, Command.Copy, Command.Cut, Command.Help, Command.Paste, Command.SelectAll, Command.SetFocusedControlValueToNull, Command.UndoEditing, Command.ExpandTextBox, Command.Expand };
        }
        public class WindowsList : IMenuEntry
        {
            public void AddToPulldownMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                var x = new WindowListToolStripMenuItem();
                add(x);

            }

            public void AddToToolbar(MenuManager mm, Action<ToolStripItem> add)
            {

            }
            public void AddToContextMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                this.AddToPulldownMenu(mm, add);
            }
        }
        public class MenuEntry : IMenuEntry, IEnumerable<IMenuEntry>
        {
            public string Caption;
            public Image Image;
            ToolBarGroup _toolBarGroup;
            public ToolBarGroup ToolBarGroup
            {
                get
                {
                    return _toolBarGroup;
                }
                set
                {
                    if (_toolBarGroup != null && _toolBarGroup != value)
                    {
                        _toolBarGroup.Remove(this);
                    }
                    _toolBarGroup = value;
                    if (_toolBarGroup != null)
                        _toolBarGroup.Add(this);
                }
            }
            public string Name { get; set; }
            public string ContextName { get; set; }
            public string ToolTipText { get; set; }
            public string StatusTip { get; set; }
            public Role Role { get; set; }
            public string ImageFileName { get; set; }
            public bool Enabled { get; set; }
            public bool Visible { get; set; }
            public Func<bool> BindVisible { get; set; }
            public bool Checked { get; set; }
            public bool CloseActiveControllers { get; set; }
            public Keys ShortcutKeys { get; set; }
            public bool ImageOnlyOnToolbar { get; set; }
            public int ImageIndex { get; set; }

            List<IMenuEntry> _items = new List<IMenuEntry>();
            public void Add(params IMenuEntry[] item)
            {
                _items.AddRange(item);
            }
            Action _what;

            public MenuEntry(string caption,
                Action what = null,
                string ContextName = null,
                bool CloseActiveControllers = false,
                Role Role = null,
                string Name = null,
                string ToolTipText = null,
                Image Image = null,
                string ImageFileName = null,
                int ImageIndex = 0,
                bool ImageOnlyOnToolbar = false,
                ToolBarGroup ToolBarGroup = null,
                Keys ShortcutKeys = Keys.None,
            bool Visible = true,
                Func<bool> BindVisible = null,
                bool Enabled = true,
                bool Checked = false,
                string StatusTip = null
                )
            {
                Caption = caption;
                _what = what;
                this.ShortcutKeys = ShortcutKeys;
                this.ContextName = ContextName;
                this.Image = Image;
                this.ToolBarGroup = ToolBarGroup;
                this.Name = Name;
                this.ToolTipText = ToolTipText;
                this.Role = Role;
                this.ImageFileName = ImageFileName;
                this.ImageIndex = ImageIndex;
                this.Visible = Visible;
                this.BindVisible = BindVisible;
                this.Enabled = Enabled;
                this.Checked = Checked;
                this.CloseActiveControllers = CloseActiveControllers;
                this.ImageOnlyOnToolbar = ImageOnlyOnToolbar;
                this.StatusTip = StatusTip;
            }
            protected virtual bool LetMenuManagerManageEnabled()
            {
                return true;
            }
            SuspendedToolStripMenuItem _pulldownMenuItem;



            public void AddToPulldownMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                AddToMenu(mm, add, false);

            }

            private void AddToMenu(MenuManager mm, Action<ToolStripItem> add, bool contextMenu)
            {
                _pulldownMenuItem = new SuspendedToolStripMenuItem()
                {
                    Text = Caption
                };
                if (ShortcutKeys != Keys.None && !contextMenu)
                    _pulldownMenuItem.ShortcutKeys = ShortcutKeys;
                DecoratePullDownMenu(_pulldownMenuItem, mm, contextMenu);
                foreach (var childitem in _items)
                {
                    childitem.AddToPulldownMenu(mm, x => _pulldownMenuItem.DropDownItems.Add(x));
                }
                AddToToolboxCommon(_pulldownMenuItem, mm);
                add(_pulldownMenuItem);
                _pulldownMenuItem.Checked = Checked;
                if (StatusTip != null)
                {

                    _pulldownMenuItem.MouseEnter += delegate
                    {
                        Common.PushStatusText(Languages.Translate(StatusTip));
                    };
                    _pulldownMenuItem.MouseLeave += delegate
                    {
                        Common.PopStatusText();
                    };
                }
            }

            public void AddToContextMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                this.AddToMenu(mm, add, true);
            }
            void AddToToolboxCommon(ToolStripItem item, MenuManager mm)
            {

                if (ToolTipText != null)
                    item.ToolTipText = ToolTipText;
                item.Visible = Visible;
                if (LetMenuManagerManageEnabled())
                    item.Enabled = Enabled;
                DecorateMenuItem(item, mm);
                mm.Map(Name ?? "", item, () => (Role != null ? Role.Allowed : true) && (BindVisible == null || BindVisible()), LetMenuManagerManageEnabled());
                if (!ImageOnlyOnToolbar || !(item is SuspendedToolStripMenuItem))
                {
                    if (ImageIndex > 0)
                        mm.SetImageByIndex(item, ImageIndex);
                    if (Image != null)
                    {
                        item.Image = Image;
                        item.ImageTransparentColor = Color.Black;
                    }
                    if (!string.IsNullOrEmpty(ImageFileName))
                        MenuManager.BindImage(item, ImageFileName);
                }

            }
            public void AddToToolbar(MenuManager mm, Action<ToolStripItem> add)
            {
                var item = new ToolStripButton();
                item.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
                item.ToolTipText = Caption.Replace("&", "");
                AddToToolboxCommon(item, mm);
                add(item);
                DecorateToolBox(item, mm);
                mm.MapAsChild(_pulldownMenuItem, item);


            }

            protected virtual void DecoratePullDownMenu(SuspendedToolStripMenuItem item, MenuManager mm, bool contextMenu)
            { }
            protected virtual void DecorateToolBox(ToolStripButton item, MenuManager mm)
            { }
            protected virtual void DecorateMenuItem(ToolStripItem item, MenuManager mm)
            {
                if (_what != null)
                {
                    item.Click += (s, e) =>
                    {
                        if (CloseActiveControllers)
                            mm.CloseActiveControllersAndRun(item, () => _what());
                        else
                            mm.Run(item, _what);
                    };
                }
            }

            public IEnumerator<IMenuEntry> GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }



        }
        public class ToolBarGroup
        {
            List<IMenuEntry> _items = new List<IMenuEntry>();
            public IMenuEntry Add(IMenuEntry x)
            {
                _items.Add(x);
                return x;
            }

            internal void AddToToolbox(MenuManager menuManager, Action<ToolStripItem> add)
            {
                foreach (var item in _items)
                {
                    item.AddToToolbar(menuManager, add);
                }
            }

            internal void Remove(MenuEntry menuEntry)
            {
                _items.Remove(menuEntry);
            }
        }
        public class DeveloperToolsMenu : GroupOfMenus
        {
            static List<IMenuEntry> _menus = new List<IMenuEntry>();
            public static void AddMenus(params IMenuEntry[] menus)
            {
                _menus.AddRange(menus);
            }
            public DeveloperToolsMenu(ApplicationControllerBase application, Type typeOfDataSources, Role userManagementRole = null)
            {
                var devTools = new MenuEntry("Developer Tools", BindVisible: () => Common.CanShowDeveloperTools())
                    {
                        new RaiseCommand("Break Into Code", Commands.BreakAll),
                        new MenuEntry("Test Error Log", () => ErrorLog.Test()),
                        new MenuEntry("About Firefly", () => new ENV.UI.AboutFirefly().ShowDialog()),
                        new MenuEntry("Call Stack", () => ErrorLog.ShowCurrentLocation()),
                        new MenuEntry("Application Call Stack", () => ENV.Utilities.CallStackBrowser.Run()),
                        new MenuEntry("Entities (Tables & Views)", () => ENV.Utilities.EntityBrowser.ShowEntityBrowser(application)),
                        new MenuEntry("Controllers (Programs)", () => ENV.Utilities.ProgramRunner.ShowAllPrograms(application)),
                        new MenuEntry("Generate Entity C#", () => ENV.Utilities.GetDefinition.Run(typeOfDataSources)),
                        new MenuEntry("Execute SQL Query", () => ENV.Utilities.SQLQuery.Run(typeOfDataSources)),
                        new MenuEntry("User Settings (ini)", () => UserSettings.Display()),
                        new MenuEntry("Test Logical Name", () => PathDecoder.ContextPathDecoder.PathDecoderTester()),
                        new MenuEntry("Parameters In Memory", () => ParametersInMemory.Instance.Display()),
                        new MenuEntry("Shared Parameters In Memory", () => ParametersInMemory.SharedInstance.Display()),


                    };
                devTools.Add(_menus.ToArray());
                devTools.Add(new ProfilerMenuEntry(),
                        new MenuEntry("Demo - Expand Button", () => ENV.UI.TextBox.ToggleExpandButton()),
                        new MenuEntry("Demo - Scaling", () => ENV.Labs.ScreenScale.Show()),
                        new MenuEntry("Demo - Enable Enhanced Grid Features", () => ENV.UI.Grid.AlwaysEnableGridEnhancements = !ENV.UI.Grid.AlwaysEnableGridEnhancements),
                        new MenuEntry("Demo - Enhanced Grid UI", () => ENV.UI.Grid.AlwaysUseUnderConstructionNewGridLook = !ENV.UI.Grid.AlwaysUseUnderConstructionNewGridLook),
                        new MenuEntry("Demo - FaceLift", () => ENV.Labs.FaceLiftDemo.Enabled = !ENV.Labs.FaceLiftDemo.Enabled));


                Add(new MenuEntry("Users", () => ENV.Security.UserManager.ManageUsers(), Role: userManagementRole),
                    new MenuEntry("Groups", () => ENV.Security.UserManager.ManageGroups(), Role: userManagementRole),
                    new MenuEntry("Secured Values", () => ENV.Security.UserManager.ManageSecuredValues(), Role: userManagementRole),
                    new MenuEntry("Change Password", () => ENV.Security.UserManager.ChangePassword()),
                   devTools);
            }
            class ProfilerMenuEntry : MenuEntry
            {
                public ProfilerMenuEntry() : base("Profiler")
                {
                }
                protected override void DecoratePullDownMenu(SuspendedToolStripMenuItem item, MenuManager mm, bool contextMenu)
                {
                    ENV.Utilities.Profiler.InitMenu(item);
                }

                protected override void DecorateToolBox(ToolStripButton item, MenuManager mm)
                {
                    base.DecorateToolBox(item, mm);
                }
            }
        }
        public class GroupOfMenus : IMenuEntry
        {
            List<IMenuEntry> _menus = new List<IMenuEntry>();
            public void Add(params IMenuEntry[] menus)
            {
                _menus.AddRange(menus);
            }


            public void AddToPulldownMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                foreach (var item in _menus)
                {
                    item.AddToPulldownMenu(mm, add);
                }
            }
            public void AddToContextMenu(MenuManager mm, ContextMenuStrip contextMenu)
            {
                AddToContextMenu(mm, x => contextMenu.Items.Add(x));
            }
            public void AddToContextMenu(MenuManager mm, Action<ToolStripItem> add)
            {
                foreach (var item in _menus)
                {
                    item.AddToContextMenu(mm, add);
                }
            }

            public void AddToToolbar(MenuManager mm, Action<ToolStripItem> add)
            {
                foreach (var item in _menus)
                {
                    item.AddToToolbar(mm, add);
                }
            }
        }
    }
}
