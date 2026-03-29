using VoiceTranslate.Backend.Models;

namespace VoiceTranslate.Backend.Interfaces;

public interface ILanguageService
{
    IEnumerable<LanguageModel> GetAvailableLanguages();
}