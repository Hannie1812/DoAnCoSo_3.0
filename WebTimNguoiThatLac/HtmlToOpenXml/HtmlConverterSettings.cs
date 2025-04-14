using DocumentFormat.OpenXml.Wordprocessing;

namespace HtmlToOpenXml
{
    internal class HtmlConverterSettings
    {
        public string DefaultFont { get; set; }
        public int DefaultFontSize { get; set; }
        public string DefaultColor { get; set; }
        public int ParagraphSpacingAfter { get; set; }
        public JustificationValues DefaultAlignment { get; set; }
    }
}