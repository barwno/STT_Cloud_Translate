using Microsoft.AspNetCore.Mvc;
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

            try {
                var result = await _service.ProcessTranscriptionAsync(request.AudioContent, request.LanguageCode);
                return Ok(new { text = result });
            }
            catch (Exception ex) {
                return StatusCode(500, $"Błąd serwera: {ex.Message}");
            }
        }
    }
}