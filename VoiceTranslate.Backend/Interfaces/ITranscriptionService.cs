namespace VoiceTranslate.Backend.Interfaces
{
    public interface ITranscriptionService
    {
        Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode);
    }
}