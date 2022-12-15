using System.ComponentModel;

namespace NuJournalPro.Enums
{
    public enum ImageMimeType
    {
        [Description("image/jpeg")]
        Jpeg,
        [Description("image/png")]
        Png,
        [Description("image/gif")]
        Gif,
        [Description("image/bmp")]
        Bmp,
        [Description("image/tiff")]
        Tiff,
        [Description("image/webp")]
        Webp,
        [Description("image/svg+xml")]
        Svg
    }
}
