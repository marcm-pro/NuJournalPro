using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services
{
    public class ImageService : IImageService
    {
        public string? DecodeImage(byte[] fileData, string mimeType)
        {
            if (fileData is null || mimeType is null)
            {
                return null;
            }
            else
            {
                return $"data:{mimeType};base64,{Convert.ToBase64String(fileData)}";
            }
        }

        public async Task<byte[]?> EncodeImageAsync(IFormFile file)
        {
            if (file is null) return null;
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }

        public async Task<byte[]> EncodeImageAsync(string fileName)
        {
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/resources/images/{fileName}";
            return await File.ReadAllBytesAsync(file);
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
            var file = $"{Directory.GetCurrentDirectory()}/wwwroot/resources/images/{fileName}";
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