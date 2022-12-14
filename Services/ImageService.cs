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
    }
}