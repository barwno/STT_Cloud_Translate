namespace VoiceTranslate.Backend.Interfaces;

public interface ISpeechToTextService
{
    // Przyjmuje base64, zwraca rozpoznany tekst
    Task<string> ConvertSpeechToTextAsync(string audioBase64, string languageCode);
}