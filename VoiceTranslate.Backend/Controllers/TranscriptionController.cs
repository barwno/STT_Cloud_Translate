using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Models;
using System.Linq;
using System.Collections.Generic;
using Google.Cloud.Translation.V2;
using Google.Cloud.Firestore;

namespace VoiceTranslate.Backend.Controllers
{
    [ApiController]
    [Route("api/transcription")]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _service;
        private readonly TranslationClient _translationClient;
        private readonly FirestoreDb _firestore;

        public TranscriptionController(ITranscriptionService service, TranslationClient translationClient, FirestoreDb firestore)
        {
            _service = service;
            _translationClient = translationClient;
            _firestore = firestore;
        }

        [HttpGet("supported-languages")]
        public async Task<IActionResult> GetSupportedLanguages()
        {
            try
            {
                var languages = await _translationClient.ListLanguagesAsync(target: "pl");
                var result = languages.Select(l => new
                {
                    code = l.Code,
                    name = !string.IsNullOrEmpty(l.Name)
                        ? char.ToUpper(l.Name[0]) + l.Name.Substring(1)
                        : l.Name
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD POBIERANIA JĘZYKÓW: {ex.Message}");
                return StatusCode(500, new { message = "Nie udało się pobrać listy języków.", error = ex.Message });
            }
        }

        // 1. Inicjalizacja dokumentu nowej sesji w Firestore
        [HttpGet("start-session")]
        public async Task<IActionResult> StartSession()
        {
            try
            {
                string sessionId = Guid.NewGuid().ToString();
                DocumentReference sessionRef = _firestore.Collection("transcription-sessions").Document(sessionId);

                var sessionData = new Dictionary<string, object>
                {
                    { "sessionId", sessionId },
                    { "createdAt", FieldValue.ServerTimestamp },
                    { "transcriptions", new List<object>() }
                };

                await sessionRef.SetAsync(sessionData);
                return Ok(new { sessionId = sessionId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD TWORZENIA SESJI: {ex.ToString()}");
                return StatusCode(500, new { message = "Nie udało się zainicjalizować sesji.", error = ex.Message });
            }
        }

        // 2. Przetwarzanie audio powiązane z konkretną sesją
        [HttpPost("process")]
public async Task<IActionResult> Process([FromBody] TranscriptionRequest request)
{
    if (string.IsNullOrEmpty(request.SessionId))
        return BadRequest("Brak identyfikatora sesji.");

    if (string.IsNullOrEmpty(request.AudioContent))
        return BadRequest("Puste audio");

    try
    {
        var recognizedText = await _service.ProcessTranscriptionAsync(request.AudioContent, request.LanguageCode);

        if (string.IsNullOrWhiteSpace(recognizedText))
        {
            return Ok(new { text = "" });
        }

        DocumentReference sessionRef = _firestore.Collection("transcription-sessions").Document(request.SessionId);

        // DYNAMICZNE OBLICZANIE CZASU LOKALNEGO NA PODSTAWIE OFFSETU Z FRONTENDU
        DateTime localTime = DateTime.UtcNow.AddMinutes(-request.TimezoneOffset);

        var newItem = new Dictionary<string, object>
        {
            { "timestamp", localTime.ToString("HH:mm:ss") },
            { "content", recognizedText.Trim() }
        };

        await sessionRef.UpdateAsync("transcriptions", FieldValue.ArrayUnion(newItem));
        return Ok(new { text = recognizedText });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"PEŁNY BŁĄD PROCESU TRANSKRYPCJI: {ex.ToString()}");
        return StatusCode(500, new { message = "Wystąpił błąd podczas przetwarzania transkrypcji.", exception = ex.Message });
    }
}

        // 3. Bezpieczne pobieranie historii całej sesji
        [HttpGet("session-history/{sessionId}")]
        public async Task<IActionResult> GetSessionHistory(string sessionId)
        {
            try
            {
                DocumentReference sessionRef = _firestore.Collection("transcription-sessions").Document(sessionId);
                DocumentSnapshot snapshot = await sessionRef.GetSnapshotAsync();

                if (!snapshot.Exists)
                    return NotFound(new { message = "Podana sesja nie istnieje." });

                var resultList = new List<TranscriptionItem>();

                if (snapshot.ContainsField("transcriptions"))
                {
                    var rawTranscriptions = snapshot.GetValue<List<object>>("transcriptions");
                    if (rawTranscriptions != null)
                    {
                        foreach (var item in rawTranscriptions)
                        {
                            if (item is Dictionary<string, object> dict)
                            {
                                resultList.Add(new TranscriptionItem
                                {
                                    Timestamp = dict.ContainsKey("timestamp") ? dict["timestamp"]?.ToString() ?? "" : "",
                                    Content = dict.ContainsKey("content") ? dict["content"]?.ToString() ?? "" : ""
                                });
                            }
                        }
                    }
                }
                return Ok(resultList);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"BŁĄD POBIERANIA HISTORII SESJI: {ex.ToString()}");
                return StatusCode(500, new { message = "Błąd pobierania historii z bazy danych.", error = ex.Message });
            }
        }
    }
}