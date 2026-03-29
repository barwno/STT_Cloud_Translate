using Microsoft.AspNetCore.Mvc;
using VoiceTranslate.Backend.Models;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TranslationController : ControllerBase
{
    private readonly IFirestoreService _firestore;
    private readonly ILanguageService _languages;
    private readonly ISpeechToTextService _stt;
    private readonly ITranslationService _translator;

    public TranslationController(
        IFirestoreService firestore, 
        ILanguageService languages,
        ISpeechToTextService stt,
        ITranslationService translator)
    {
        _firestore = firestore;
        _languages = languages;
        _stt = stt;
        _translator = translator;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] TranslationRequest request)
    {
        // 1. Obsługa sesji (Czysty kod: kontroler dba o ciasteczka)
        string sessionId = Request.Cookies["SessionId"] ?? Guid.NewGuid().ToString();
        if (!Request.Cookies.ContainsKey("SessionId"))
        {
            Response.Cookies.Append("SessionId", sessionId, new CookieOptions 
            { 
                HttpOnly = true, 
                Expires = DateTimeOffset.UtcNow.AddDays(7),
                SameSite = SameSiteMode.Lax,
                Secure = true // Ważne dla HTTPS
            });
        }

        // 2. Wykorzystanie interfejsów (SOLID: DIP)
        // Zamieniamy mowę na tekst (STT)
        string originalText = await _stt.ConvertSpeechToTextAsync(request.AudioBase64, "pl");
        
        // Tłumaczymy uzyskany tekst
        string translatedText = await _translator.TranslateTextAsync(originalText, request.TargetLang);

        // 3. Zapis do bazy danych
        await _firestore.SaveTranslationAsync(sessionId, request.Person, originalText, translatedText);

        // 4. Powrót do JS (CamelCase zostanie obsłużony przez Program.cs)
        return Ok(new TranslationResponse 
        { 
            OriginalText = originalText, 
            TranslatedText = translatedText 
        });
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory()
    {
        string? sessionId = Request.Cookies["SessionId"];
        if (string.IsNullOrEmpty(sessionId)) return Ok(new List<object>());

        var history = await _firestore.GetHistoryAsync(sessionId);
        return Ok(history);
    }

    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        // SOLID: Dane o językach pochodzą z serwisu, nie są wpisane na sztywno
        return Ok(_languages.GetAvailableLanguages());
    }
}