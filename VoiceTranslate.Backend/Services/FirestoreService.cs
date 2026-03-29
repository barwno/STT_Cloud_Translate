using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services;

public class FirestoreService : IFirestoreService
{
    private readonly FirestoreDb _db;

    public FirestoreService(string projectId, string keyPath)
    {
        // Łączymy ścieżkę do klucza JSON
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), keyPath);
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
        _db = FirestoreDb.Create(projectId);
    }

    public async Task SaveTranslationAsync(string sessionId, string person, string original, string translated)
    {
        var doc = _db.Collection("history").Document();
        await doc.SetAsync(new { 
            SessionId = sessionId, 
            Person = person, 
            Original = original, 
            Translated = translated, 
            Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) 
        });
    }

    public async Task<List<Dictionary<string, object>>> GetHistoryAsync(string sessionId)
    {
        var snapshot = await _db.Collection("history")
            .WhereEqualTo("SessionId", sessionId)
            .OrderByDescending("Timestamp")
            .GetSnapshotAsync();

        return snapshot.Documents.Select(d => d.ToDictionary()).ToList();
    }
}