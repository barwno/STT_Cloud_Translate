namespace VoiceTranslate.Backend.Models;

public class TranslationRequest
{
    public string AudioBase64 { get; set; } = string.Empty;
    public string TargetLang { get; set; } = string.Empty;
    public string Person { get; set; } = string.Empty; // "a" lub "b"
}