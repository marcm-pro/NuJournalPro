using NuJournalPro.Models;

namespace NuJournalPro.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]?> EncodeImageAsync(IFormFile file, bool? compress = null);
        Task<byte[]?> EncodeImageAsync(string fileName, bool? compress = null);
        Task<CompressedImage?> EncodeImageAsync(IFormFile file);
        Task<CompressedImage?> EncodeImageAsync(string fileName);
        string? DecodeImage(byte[] imageData, string mimeType, bool? decompress = null);
        string? DecodeImage(CompressedImage compressedImage);
        string? MimeType(IFormFile file);
        string? MimeType(string fileName);
        int ImageSize(IFormFile file);
    }
}
