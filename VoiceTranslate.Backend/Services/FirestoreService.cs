using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services;

public class FirestoreService : IFirestoreService
{
    private readonly FirestoreDb? _db;
    private readonly bool _isInitialized = false;

    public FirestoreService(string projectId, string keyPath)
    {
        try 
        {
            var fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, keyPath);
            
            if (File.Exists(fullPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", fullPath);
                _db = FirestoreDb.Create(projectId);
                _isInitialized = true;
                Console.WriteLine("FirestoreService zainicjalizowany pomyślnie.");
            }
            else
            {
                Console.WriteLine($"OSTRZEŻENIE: Brak pliku klucza pod: {fullPath}. Baza danych nie będzie dostępna.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BŁĄD inicjalizacji Firestore: {ex.Message}");
        }
    }

    public async Task SaveTranslationAsync(string sessionId, string person, string original, string translated)
    {
        if (!_isInitialized || _db == null) return;

        try {
            var doc = _db.Collection("history").Document();
            await doc.SetAsync(new { 
                SessionId = sessionId, 
                Person = person, 
                Original = original, 
                Translated = translated, 
                Timestamp = Timestamp.FromDateTime(DateTime.UtcNow) 
            });
        } catch (Exception ex) {
            Console.WriteLine($"Błąd zapisu do Firestore: {ex.Message}");
        }
    }

    public async Task<List<Dictionary<string, object>>> GetHistoryAsync(string sessionId)
    {
        if (!_isInitialized || _db == null) return new List<Dictionary<string, object>>();

        try {
            var snapshot = await _db.Collection("history")
                .WhereEqualTo("SessionId", sessionId)
                .OrderByDescending("Timestamp")
                .GetSnapshotAsync();

            return snapshot.Documents.Select(d => d.ToDictionary()).ToList();
        } catch (Exception ex) {
            Console.WriteLine($"Błąd pobierania z Firestore: {ex.Message}");
            return new List<Dictionary<string, object>>();
        }
    }
}