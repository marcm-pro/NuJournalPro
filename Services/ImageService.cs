using NuJournalPro.Models;
using NuJournalPro.Services.Interfaces;
using System.IO.Compression;

namespace NuJournalPro.Services
{
    public class ImageService : IImageService
    {
        public string? DecodeImage(byte[]? imageData, string? mimeType, bool? decompress = null)
        {
            if (imageData is null || mimeType is null)
            {
                return null;
            }
            else
            {
                if (decompress is not null && decompress is true)
                {
                    using var compressedMemoryStream = new MemoryStream(imageData);
                    using var gZipStream = new GZipStream(compressedMemoryStream, CompressionMode.Decompress);
                    using var decompressedMemoryStream = new MemoryStream();
                    gZipStream.CopyTo(decompressedMemoryStream);
                    return $"data:{mimeType};base64,{Convert.ToBase64String(decompressedMemoryStream.ToArray())}";
                }
                else
                {
                    return $"data:{mimeType};base64,{Convert.ToBase64String(imageData)}";
                }
            }
        }

        public string? DecodeImage(CompressedImage compressedImage)
        {
            if (compressedImage is null) return null;
            else return DecodeImage(compressedImage.CompressedImageData, compressedImage.MimeType, true);
        }

        public async Task<byte[]?> EncodeImageAsync(IFormFile file, bool? compress = null)
        {
            if (file is null) return null;
            else
            {
                using var fileContentsMemoryStream = new MemoryStream();
                await file.CopyToAsync(fileContentsMemoryStream);

                if (compress is not null && compress is true)
                {
                    using var gZipStream = new GZipStream(fileContentsMemoryStream, CompressionLevel.Optimal);
                    using var compressedMemoryStream = new MemoryStream();
                    gZipStream.CopyTo(compressedMemoryStream);
                    return compressedMemoryStream.ToArray();
                }
                else
                {
                    return fileContentsMemoryStream.ToArray();
                }
            }
        }

        public async Task<byte[]?> EncodeImageAsync(string fileName, bool? compress = null)
        {
            if (fileName is null) return null;
            else
            {
                var file = $"{Directory.GetCurrentDirectory()}/wwwresources/images/{fileName}";
                var fileContents = await File.ReadAllBytesAsync(file);
                if (compress is not null && compress is true)
                {
                    using var decompressedMemoryStream = new MemoryStream(fileContents);
                    using var gZipStream = new GZipStream(decompressedMemoryStream, CompressionLevel.Optimal);
                    using var compressedMemoryStream = new MemoryStream();
                    gZipStream.CopyTo(compressedMemoryStream);
                    return compressedMemoryStream.ToArray();
                }
                else
                {
                    return fileContents;
                }
            }
        }

        public async Task<CompressedImage?> EncodeImageAsync(IFormFile file)
        {
            if (file is null) return null;
            else
            {
                return new CompressedImage()
                {
                    CompressedImageData = await EncodeImageAsync(file, true),
                    MimeType = MimeType(file)
                };
            }
        }

        public async Task<CompressedImage?> EncodeImageAsync(string fileName)
        {
            if (fileName is null) return null;
            else
            {
                return new CompressedImage()
                {
                    CompressedImageData = await EncodeImageAsync(fileName, true),
                    MimeType = MimeType(fileName)
                };
            }
        }

        public int ImageSize(IFormFile file)
        {
            return Convert.ToInt32(file?.Length);
        }

        public string? MimeType(IFormFile file)
        {
            return file?.ContentType;
        }

        public string? MimeType(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwresources/images/{fileName}";
            var fileExtension = Path.GetExtension(file);
            if (fileExtension == string.Empty)
            {
                return null;
            }
            else
            {
                if (fileExtension == ".svg")
                {
                    return "image/svg+xml";
                }
                else if (fileExtension == ".png")
                {
                    return "image/png";
                }
                else if (fileExtension == ".jpg" || fileExtension == ".jpeg")
                {
                    return "image/jpeg";
                }
                else if (fileExtension == ".gif")
                {
                    return "image/gif";
                }
                else if (fileExtension == ".bmp")
                {
                    return "image/bmp";
                }
                else return null;
            }
        }
    }
}