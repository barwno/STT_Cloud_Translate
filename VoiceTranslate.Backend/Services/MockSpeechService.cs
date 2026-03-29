using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services;

public class MockSpeechService : ISpeechToTextService
{
    public Task<string> ConvertSpeechToTextAsync(string audioBase64, string languageCode)
    {
        // Symulacja: zwracamy stały tekst, żeby sprawdzić czy flow działa
        return Task.FromResult("Dzień dobry, to jest tekst z mowy (Mock)");
    }
}