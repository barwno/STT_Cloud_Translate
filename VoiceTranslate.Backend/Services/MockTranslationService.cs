using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services;

public class MockTranslationService : ITranslationService
{
    public Task<string> TranslateTextAsync(string text, string targetLanguage)
    {
        return Task.FromResult($"[MOCK {targetLanguage.ToUpper()}]: {text}");
    }
}