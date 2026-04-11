namespace VoiceTranslate.Backend.Models
{
    public class TranscriptionRequest
    {
        public string AudioContent { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "pl-PL";
    }
}