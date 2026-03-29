using Microsoft.AspNetCore.Mvc;
using VoiceTranslate.Backend.Models;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Controllers;

[ApiController]
[Route("api/[controller]")] // Adres: /api/translation
public class TranslationController : ControllerBase
{
    private readonly IFirestoreService _firestore;

    public TranslationController(IFirestoreService firestore)
    {
        _firestore = firestore;
    }

    [HttpPost("process")]
    public async Task<IActionResult> Process([FromBody] TranslationRequest request)
    {
        // 1. Obsługa ciasteczka sesji
        string? sessionId = Request.Cookies["SessionId"] ?? Guid.NewGuid().ToString();
        if (!Request.Cookies.ContainsKey("SessionId"))
        {
            Response.Cookies.Append("SessionId", sessionId, new CookieOptions { 
                HttpOnly = true, 
                Expires = DateTime.Now.AddDays(7) 
            });
        }

        // 2. Tutaj docelowo wywołasz STT i Tłumacza (na razie mocki)
        string mockOriginal = "Dzień dobry";
        string mockTranslated = "Good morning";

        // 3. Zapis do Firebase przez Interfejs
        await _firestore.SaveTranslationAsync(sessionId, request.Person, mockOriginal, mockTranslated);

        return Ok(new TranslationResponse { 
            OriginalText = mockOriginal, 
            TranslatedText = mockTranslated 
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
        var langs = new[] {
            new { code = "pl", name = "Polski", flag = "pl" },
            new { code = "en", name = "English", flag = "gb" },
            new { code = "de", name = "Deutsch", flag = "de" }
        };
        return Ok(langs);
    }
}
