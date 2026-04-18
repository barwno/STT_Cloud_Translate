using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services
{
    // Usługa symulowana (tzw. Mock), używana do testowania aplikacji bez konieczności łączenia się z płatnymi usługami Google.
    public class MockTranscriptionService : ITranscriptionService
    {
        // Udaje pracę serwera przez sekundę i zwraca przykładowy tekst informujący o braku połączenia z chmurą.
        public async Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode)
        {
            await Task.Delay(1000); 
            return $"[MOCKUP - {languageCode}] To jest symulowana odpowiedź. Podłącz klucze Google Cloud, aby uzyskać prawdziwą transkrypcję.";
        }
    }
}