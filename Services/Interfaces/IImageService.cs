namespace NuJournalPro.Services.Interfaces
{
    public interface IImageService
    {
        Task<byte[]?> EncodeImageAsync(IFormFile file);
        Task<byte[]> EncodeImageAsync(string fileName);
        string? DecodeImage(byte[] fileData, string mimeType);
        string? MimeType(IFormFile file);
        string? MimeType(string fileName);
        int ImageSize(IFormFile file);
    }
}
