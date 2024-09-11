using System.Drawing;
using Firefly.Box.UI;
using Firefly.Box;
using System.Collections.Generic;
namespace Northwind.Shared.Theme
{
    /// <summary>Map of Fonts used to resolve value by index</summary>
    public class FontSchemes
    {
        static FontSchemes()
        {
            _map.Add(1, new Fonts.DefaultTableEditField());
            _map.Add(2, new Fonts.DefaultTableTitle());
            _map.Add(3, new Fonts.DefaultDialogEditFields());
            _map.Add(4, new Fonts.DefaultDialogText());
            _map.Add(5, new Fonts.DefaultTableNameTitle());
            _map.Add(6, new Fonts.DefaultHelp());
            _map.Add(7, new Fonts.DefaultFixedSizeFont());
            _map.Add(8, new Fonts.DefaultImportForm());
            _map.Add(9, new Fonts.PushButtonDefaultFont());
            _map.Add(10, new Fonts.RadioButtonDefaultFont());
            _map.Add(11, new Fonts.Reserved());
            _map.Add(12, new Fonts.Reserved_());
            _map.Add(13, new Fonts.Reserved_1());
            _map.Add(14, new Fonts.Reserved_2());
            _map.Add(15, new Fonts.Reserved_3());
            _map.Add(16, new Fonts.Reserved_4());
            _map.Add(17, new Fonts.Reserved_5());
            _map.Add(18, new Fonts.Reserved_6());
            _map.Add(19, new Fonts.Reserved_7());
            _map.Add(20, new Fonts.Reserved_8());
            _map.Add(21, new Fonts.TabText());
            _map.Add(22, new Fonts.Tooltips());
            _map.Add(23, new Fonts.TableFields());
            _map.Add(24, new Fonts.TableTitle());
            _map.Add(25, new Fonts.DialogFields());
            _map.Add(26, new Fonts.DialogText());
            _map.Add(27, new Fonts.PushButtonText());
            _map.Add(28, new Fonts.StatusLineText());
            _map.Add(29, new Fonts.ConfirmMessagesFont());
            _map.Add(30, new Fonts.ToolkitWindowsFont());
            _map.Add(31, new Fonts.SmallFont());
            _map.Add(32, new Fonts.LargeFont());
            _map.Add(33, new Fonts.FixedSizeFont());
            _map.Add(34, new Fonts.InheritedProperty());
            _map.Add(35, new Fonts.BrokenProperty());
            _map.Add(36, new Fonts.AsDataProperty());
            _map.Add(37, new Fonts.Reserved_9());
            _map.Add(38, new Fonts.Reserved_10());
            _map.Add(39, new Fonts.Reserved_11());
            _map.Add(40, new Fonts.Reserved_12());
            _map.Add(41, new Fonts.Reserved_13());
            _map.Add(42, new Fonts.Reserved_14());
            _map.Add(43, new Fonts.Reserved_15());
            _map.Add(44, new Fonts.Reserved_16());
            _map.Add(45, new Fonts.Reserved_17());
            _map.Add(46, new Fonts.Reserved_18());
            _map.Add(47, new Fonts.Reserved_19());
            _map.Add(48, new Fonts.Reserved_20());
            _map.Add(49, new Fonts.Reserved_21());
            _map.Add(50, new Fonts.HTML_Default());
            _map.Add(51, new Fonts.Header1());
            _map.Add(52, new Fonts.Header2());
            _map.Add(53, new Fonts.Header3());
            _map.Add(54, new Fonts.Header4());
            _map.Add(55, new Fonts.Header5());
            _map.Add(56, new Fonts.Header6());
            _map.Add(57, new Fonts.HTML_DefaultBold());
            _map.Add(58, new Fonts.Header1Bold());
            _map.Add(59, new Fonts.Header2Bold());
            _map.Add(60, new Fonts.Header3Bold());
            _map.Add(61, new Fonts.Header4Bold());
            _map.Add(62, new Fonts.Header5Bold());
            _map.Add(63, new Fonts.Header6Bold());
            _map.Add(64, new Fonts.HTML_DefaultItalic());
            _map.Add(65, new Fonts.Header1Italic());
            _map.Add(66, new Fonts.Header2Italic());
            _map.Add(67, new Fonts.Header3Italic());
            _map.Add(68, new Fonts.Header4Italic());
            _map.Add(69, new Fonts.Header5Italic());
            _map.Add(70, new Fonts.Header6Italic());
            _map.Add(71, new Fonts.HTML_DefaultBoldItalic());
            _map.Add(72, new Fonts.Header1BoldItalic());
            _map.Add(73, new Fonts.Header2BoldItalic());
            _map.Add(74, new Fonts.Header3BoldItalic());
            _map.Add(75, new Fonts.Header4BoldItalic());
            _map.Add(76, new Fonts.Header5BoldItalic());
            _map.Add(77, new Fonts.Header6BoldItalic());
            _map.Add(78, new Fonts.Reserved_22());
            _map.Add(79, new Fonts.Reserved_23());
            _map.Add(80, new Fonts.Reserved_24());
            _map.Add(81, new Fonts.WizardLargeTitle());
            _map.Add(82, new Fonts.WizardSmallTitle());
            _map.Add(83, new Fonts.WizardText());
            _map.Add(84, new Fonts.Reserved_25());
            _map.Add(85, new Fonts.Reserved_26());
            _map.Add(86, new Fonts.Reserved_27());
            _map.Add(87, new Fonts.Reserved_28());
            _map.Add(88, new Fonts.Reserved_29());
            _map.Add(89, new Fonts.Reserved_30());
            _map.Add(90, new Fonts.Reserved_31());
            _map.Add(91, new Fonts.Reserved_32());
            _map.Add(92, new Fonts.Reserved_33());
            _map.Add(93, new Fonts.Reserved_34());
            _map.Add(94, new Fonts.Reserved_35());
            _map.Add(95, new Fonts.Reserved_36());
            _map.Add(96, new Fonts.Reserved_37());
            _map.Add(97, new Fonts.Reserved_38());
            _map.Add(98, new Fonts.Reserved_39());
            _map.Add(99, new Fonts.Reserved_40());
            _map.Add(100, new Fonts.Reserved_41());
            _map.Add(101, new Fonts.SampleFontRotation45Deg());
            _map.Add(102, new Fonts.SampleFontRotation90Deg());
            _map.Add(103, new Fonts.SampleFontRotation135Deg());
            _map.Add(104, new Fonts.SampleFontRotation180Deg());
            _map.Add(105, new Fonts.SampleFontRotation225Deg());
            _map.Add(106, new Fonts.SampleFontRotation270Deg());
            _map.Add(107, new Fonts.SampleFontRotation315Deg());
            _map.Add(108, new Fonts.SampleFontRotation0Deg());
            _map.Add(109, new Fonts.UserDefinedFont());
            _map.Add(110, new Fonts.UserDefinedFont_());
        }
        /// <summary>Used to find Fonts by index</summary>
        public static FontScheme Find(Number index)
        {
            if (index==null||!_map.ContainsKey(index))
                return null;
            return _map[index];
        }
        static Dictionary<Number,FontScheme> _map = new Dictionary<Number,FontScheme>();
    }
}
