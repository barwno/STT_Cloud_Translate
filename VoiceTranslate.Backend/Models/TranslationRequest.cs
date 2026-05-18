namespace VoiceTranslate.Backend.Models
{
    public class TranscriptionRequest
    {
        public string AudioContent { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = "pl-PL";
        public string SessionId { get; set; } = string.Empty;

        // NOWE POLE: Przesunięcie czasowe w minutach względem UTC (np. dla Polski to -120 lub -60)
        public int TimezoneOffset { get; set; }
    }
}