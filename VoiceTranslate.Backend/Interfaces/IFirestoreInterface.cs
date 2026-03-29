using VoiceTranslate.Backend.Models;

namespace VoiceTranslate.Backend.Interfaces;

public interface IFirestoreService
{
    Task SaveTranslationAsync(string sessionId, string person, string original, string translated);
    Task<List<Dictionary<string, object>>> GetHistoryAsync(string sessionId);
}