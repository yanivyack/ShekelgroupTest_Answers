namespace Northwind.Shared.Theme.IO.Html
{
    public class HtmlSection : ENV.IO.Html.HtmlSection 
    {
        /// <summary>HtmlSection</summary>
        public HtmlSection()
        {
            InitializeComponent();
        }
        void InitializeComponent()
        {
            ColorScheme = new Colors.DefaultWindow();
            DefaultFontScheme = new Fonts.HTML_Default();
            H1FontScheme = new Fonts.Header1();
            H1FontScheme = new Fonts.Header2();
            H1FontScheme = new Fonts.Header3();
            H1FontScheme = new Fonts.Header4();
            H1FontScheme = new Fonts.Header5();
            H1FontScheme = new Fonts.Header6();
            BoldFontScheme = new Fonts.HTML_DefaultBold();
            H1BoldFontScheme = new Fonts.Header1Bold();
            H2BoldFontScheme = new Fonts.Header2Bold();
            H3BoldFontScheme = new Fonts.Header3Bold();
            H4BoldFontScheme = new Fonts.Header4Bold();
            H5BoldFontScheme = new Fonts.Header5Bold();
            H6BoldFontScheme = new Fonts.Header6Bold();
            ItalicFontScheme = new Fonts.HTML_DefaultItalic();
            H1ItalicFontScheme = new Fonts.Header1Italic();
            H2ItalicFontScheme = new Fonts.Header2Italic();
            H3ItalicFontScheme = new Fonts.Header3Italic();
            H4ItalicFontScheme = new Fonts.Header4Italic();
            H5ItalicFontScheme = new Fonts.Header5Italic();
            H6ItalicFontScheme = new Fonts.Header6Italic();
            BoldItalicFontScheme = new Fonts.HTML_DefaultBoldItalic();
            H1BoldItalicFontScheme = new Fonts.Header1BoldItalic();
            H2BoldItalicFontScheme = new Fonts.Header2BoldItalic();
            H3BoldItalicFontScheme = new Fonts.Header3BoldItalic();
            H4BoldItalicFontScheme = new Fonts.Header4BoldItalic();
            H5BoldItalicFontScheme = new Fonts.Header5BoldItalic();
            H6BoldItalicFontScheme = new Fonts.Header6BoldItalic();
        }
        public HtmlSection(ENV.BusinessProcessBase controller) : base(controller)
        {
            InitializeComponent();
        }
        public HtmlSection(ENV.AbstractUIController controller) : base(controller)
        {
            InitializeComponent();
        }
        public HtmlSection(ENV.ApplicationControllerBase controller) : base(controller)
        {
            InitializeComponent();
        }
    }
}
