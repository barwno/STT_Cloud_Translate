using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Models;
using System.Linq;
using Google.Cloud.Translation.V2;

namespace VoiceTranslate.Backend.Controllers
{
    // Główny moduł zarządzający komunikacją między stroną internetową a usługami przetwarzania dźwięku i tłumaczeń.
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

        // Pobiera z Google aktualną listę języków, które system może obsłużyć, aby użytkownik mógł je wybrać w menu.
        [HttpGet("supported-languages")]
        public async Task<IActionResult> GetSupportedLanguages()
        {
            try
            {
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
                // W przypadku awarii połączenia z Google, system rejestruje błąd i informuje aplikację o problemie.
                Console.WriteLine($"BŁĄD POBIERANIA JĘZYKÓW: {ex.Message}");

                return StatusCode(500, new { message = "Nie udało się pobrać listy języków.", error = ex.Message });
            }
        }

        // Przyjmuje nagranie audio od użytkownika, wysyła je do analizy i zwraca przetworzony tekst.
        [HttpPost("process")]
        public async Task<IActionResult> Process([FromBody] TranscriptionRequest request)
        {
            if (string.IsNullOrEmpty(request.AudioContent))
                return BadRequest("Puste audio");

            try
            {
                // Uruchomienie głównego procesu zamiany mowy na tekst za pomocą serwisu transkrypcji.
                var result = await _service.ProcessTranscriptionAsync(request.AudioContent, request.LanguageCode);
                return Ok(new { text = result });
            }
            catch (Exception ex)
            {
                // Rejestrowanie szczegółów błędu oraz zwracanie raportu, aby można było szybko naprawić problem w kodzie.
                Console.WriteLine($"PEŁNY BŁĄD APLIKACJI: {ex.ToString()}");

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