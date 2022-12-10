using NuJournalPro.Services.Interfaces;

namespace NuJournalPro.Services
{
    public class ImageService : IImageService
    {
        private readonly string[] suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };
        private readonly string defaultImage = "~/img/DefaultUserImage.svg";
        public string ConvertByteArrayToFile(byte[] fileData, string extension)
        {
            if (fileData != null)
            {
                try
                {
                    string imageBase64Data = Convert.ToBase64String(fileData);
                    return string.Format($"data:{extension};base64,{imageBase64Data}");
                }
                catch (Exception)
                {
                    throw;
                }
            }
            else
            {
                return defaultImage;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
        {
            try
            {
                using MemoryStream memoryStream = new();
                await file.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                return byteFile;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}