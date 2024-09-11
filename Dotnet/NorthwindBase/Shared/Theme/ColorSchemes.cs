using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
using System.Collections.Generic;
namespace Northwind.Shared.Theme
{
    /// <summary>Map of Colors used to resolve value by index</summary>
    public class ColorSchemes
    {
        static ColorSchemes()
        {
            _map.Add(1, new Colors.DefaultWindow());
            _map.Add(2, new Colors.DefaultEditField());
            _map.Add(3, new Colors.DefaultFreeText());
            _map.Add(4, new Colors.DefaultHelpWindow());
            _map.Add(5, new Colors.Default3D_Effect());
            _map.Add(6, new Colors.DefaultPrintFormColor());
            _map.Add(7, new Colors.DefaultHyperlink());
            _map.Add(8, new Colors.Reserved());
            _map.Add(9, new Colors.Reserved_());
            _map.Add(10, new Colors.Reserved_1());
            _map.Add(11, new Colors.Reserved_2());
            _map.Add(12, new Colors.Reserved_3());
            _map.Add(13, new Colors.Reserved_4());
            _map.Add(14, new Colors.Reserved_5());
            _map.Add(15, new Colors.Reserved_6());
            _map.Add(16, new Colors.Reserved_7());
            _map.Add(17, new Colors.Reserved_8());
            _map.Add(18, new Colors.Window());
            _map.Add(19, new Colors.EditField());
            _map.Add(20, new Colors._3D_Effect());
            _map.Add(21, new Colors.DialogWindows());
            _map.Add(22, new Colors.DialogFields());
            _map.Add(23, new Colors.DialogTitles());
            _map.Add(24, new Colors.ListWindows());
            _map.Add(25, new Colors.ListFields());
            _map.Add(26, new Colors.ListTitles());
            _map.Add(27, new Colors.PopupWindows());
            _map.Add(28, new Colors.PopupWindowFields());
            _map.Add(29, new Colors.PopupWindowTitles());
            _map.Add(30, new Colors.DescriptionText());
            _map.Add(31, new Colors.GroupText());
            _map.Add(32, new Colors.TitleInFocus());
            _map.Add(33, new Colors.TitleOutOfFocus());
            _map.Add(34, new Colors._3D_Effect_());
            _map.Add(35, new Colors.ProgramTree());
            _map.Add(36, new Colors.UnparkedItems());
            _map.Add(37, new Colors.BreakPoint());
            _map.Add(38, new Colors.HotSpot());
            _map.Add(39, new Colors.HyperlinkPushbuttonText());
            _map.Add(40, new Colors.HyperlinkPushbuttonVisited());
            _map.Add(41, new Colors.HyperlinkPushB_Walkthrough());
            _map.Add(42, new Colors.CommentedEntry());
            _map.Add(43, new Colors.ComponentObject());
            _map.Add(44, new Colors.InheritedProperty());
            _map.Add(45, new Colors.BrokenProperty());
            _map.Add(46, new Colors.AsDataProperty());
            _map.Add(47, new Colors.MarkingColumnControls());
            _map.Add(48, new Colors.ComponentBuilderWarning());
            _map.Add(49, new Colors.ApplicationWorkspace());
            _map.Add(50, new Colors.Windows3D_TableTitle());
            _map.Add(51, new Colors.MenuBar());
            _map.Add(52, new Colors.Toolbar());
            _map.Add(53, new Colors.StatusBar());
            _map.Add(54, new Colors.Reserved_9());
            _map.Add(55, new Colors.Reserved_10());
            _map.Add(56, new Colors.Reserved_11());
            _map.Add(57, new Colors.Reserved_12());
            _map.Add(58, new Colors.Reserved_13());
            _map.Add(59, new Colors.Reserved_14());
            _map.Add(60, new Colors.RemarkOperation());
            _map.Add(61, new Colors.SelectOperation());
            _map.Add(62, new Colors.VerifyOperation());
            _map.Add(63, new Colors.Link_EndLinkOperation());
            _map.Add(64, new Colors.Block_EndBlockOperation());
            _map.Add(65, new Colors.CallOperation());
            _map.Add(66, new Colors.EvaluateOperation());
            _map.Add(67, new Colors.UpdateOperation());
            _map.Add(68, new Colors.InputOperation());
            _map.Add(69, new Colors.FlowOutput());
            _map.Add(70, new Colors.BrowseOperation());
            _map.Add(71, new Colors.ExitOSOperation());
            _map.Add(72, new Colors.RaiseEventOperation());
            _map.Add(73, new Colors.HandlerLevel());
            _map.Add(74, new Colors.Reserved_15());
            _map.Add(75, new Colors.Reserved_16());
            _map.Add(76, new Colors.Reserved_17());
            _map.Add(77, new Colors.Reserved_18());
            _map.Add(78, new Colors.Reserved_19());
            _map.Add(79, new Colors.Reserved_20());
            _map.Add(80, new Colors.Reserved_21());
            _map.Add(81, new Colors.Reserved_22());
            _map.Add(82, new Colors.Reserved_23());
            _map.Add(83, new Colors.Reserved_24());
            _map.Add(84, new Colors.Reserved_25());
            _map.Add(85, new Colors.Reserved_26());
            _map.Add(86, new Colors.Reserved_27());
            _map.Add(87, new Colors.Reserved_28());
            _map.Add(88, new Colors.Reserved_29());
            _map.Add(89, new Colors.Reserved_30());
            _map.Add(90, new Colors.Reserved_31());
            _map.Add(91, new Colors.Reserved_32());
            _map.Add(92, new Colors.Reserved_33());
            _map.Add(93, new Colors.Reserved_34());
            _map.Add(94, new Colors.Reserved_35());
            _map.Add(95, new Colors.Reserved_36());
            _map.Add(96, new Colors.Reserved_37());
            _map.Add(97, new Colors.Reserved_38());
            _map.Add(98, new Colors.Reserved_39());
            _map.Add(99, new Colors.Reserved_40());
            _map.Add(100, new Colors.Reserved_41());
        }
        /// <summary>Used to find Colors by index</summary>
        public static ColorScheme Find(Number index)
        {
            if (index==null||!_map.ContainsKey(index))
                return new ENV.UI.MissingColor();
            return _map[index];
        }
        static Dictionary<Number,ColorScheme> _map = new Dictionary<Number,ColorScheme>();
    }
}
