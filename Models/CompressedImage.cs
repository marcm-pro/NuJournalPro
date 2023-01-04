using System.IO.Compression;

namespace NuJournalPro.Models
{
    public class CompressedImage
    {
        public byte[]? CompressedImageData { get; set; }
        public string? MimeType { get; set; }
        public byte[]? ImageData
        {
            get
            {
                if (CompressedImageData is not null && MimeType is not null)
                {
                    return DecompressImageData(CompressedImageData);
                }
                else
                {
                    return null;
                }
            }
        }
        public string? ImageBase64
        {
            get
            {
                if (ImageData is not null && MimeType is not null)
                {
                    return $"data:{MimeType};base64,{Convert.ToBase64String(ImageData)}";
                }
                else
                {
                    return null;
                }
            }
        }
        private byte[] DecompressImageData(byte[] compressedImageData)
        {
            using var compressedMemoryStream = new MemoryStream(compressedImageData);
            using var gZipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
            using var decompressedMemoryStream = new MemoryStream();
            gZipStream.CopyTo(decompressedMemoryStream);
            return decompressedMemoryStream.ToArray();
        }
    }
}
