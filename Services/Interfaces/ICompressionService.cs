namespace NuJournalPro.Services.Interfaces
{
    public interface ICompressionService
    {
        string CompressText(string text);
        string DecompressText(string compressedText);
        byte[] CompressBytes(byte[] bytes);
        byte[] DecompressBytes(byte[] bytes);
    }
}
