using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Models;

namespace VoiceTranslate.Backend.Services;

public class LanguageService : ILanguageService
{
    public IEnumerable<LanguageModel> GetAvailableLanguages()
    {
        return new List<LanguageModel>
        {
            new() { Code = "pl", Name = "Polski", Flag = "pl" },
            new() { Code = "en", Name = "English", Flag = "gb" },
            new() { Code = "de", Name = "Deutsch", Flag = "de" },
            new() { Code = "fr", Name = "Français", Flag = "fr" },
            new() { Code = "kr", Name = "한국인", Flag = "kr" }
        };
    }
}