using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Models;
using System.Linq;
using Google.Cloud.Translation.V2;

namespace VoiceTranslate.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _service;
        private readonly TranslationClient _translationClient;

        public TranscriptionController(ITranscriptionService service, TranslationClient translationClient)
        {
            _service = service;
            _translationClient = translationClient;
        }
        [HttpGet("supported-languages")]
        public async Task<IActionResult> GetSupportedLanguages()
        {
            try
            {
                // Operacja sieciowa, która może rzucić wyjątek
                var languages = await _translationClient.ListLanguagesAsync(target: "pl");

                var result = languages.Select(l => new
                {
                    code = l.Code,
                    // Upiększamy nazwę: pierwsza litera duża, reszta bez zmian
                    name = !string.IsNullOrEmpty(l.Name)
                ? char.ToUpper(l.Name[0]) + l.Name.Substring(1)
                : l.Name
                });
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Logujemy błąd do konsoli
                Console.WriteLine($"BŁĄD POBIERANIA JĘZYKÓW: {ex.Message}");

                // Zwracamy kod 500 z komunikatem, żeby frontend wiedział, że coś poszło nie tak
                return StatusCode(500, new { message = "Nie udało się pobrać listy języków.", error = ex.Message });
            }
        }
        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] TranscriptionRequest request)
        {
            if (string.IsNullOrEmpty(request.AudioContent))
                return BadRequest("Puste audio");

            try
            {
                // Próba przetworzenia transkrypcji
                var result = await _service.ProcessTranscriptionAsync(request.AudioContent, request.LanguageCode);
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                // 1. Zapisujemy pełny stack trace do logów Google Cloud
                // Dzięki temu w zakładce "Dzienniki" zobaczysz dokładnie, w której linii jest błąd
                Console.WriteLine($"PEŁNY BŁĄD APLIKACJI: {ex.ToString()}");

                // 2. Zwracamy szczegóły do przeglądarki, abyś widział je w narzędziach F12
                // To pozwoli nam zidentyfikować błąd bez ciągłego zaglądania do logów w konsoli
                return StatusCode(500, new
                {
                    message = "Wystąpił błąd podczas przetwarzania transkrypcji.",
                    exception = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}