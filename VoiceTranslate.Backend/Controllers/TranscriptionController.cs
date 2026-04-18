using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Models;

namespace VoiceTranslate.Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TranscriptionController : ControllerBase
    {
        private readonly ITranscriptionService _service;

        public TranscriptionController(ITranscriptionService service)
        {
            _service = service;
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
                return StatusCode(500, new { 
                    message = "Wystąpił błąd podczas przetwarzania transkrypcji.",
                    exception = ex.Message,
                    stackTrace = ex.StackTrace 
                });
            }
        }
    }
}