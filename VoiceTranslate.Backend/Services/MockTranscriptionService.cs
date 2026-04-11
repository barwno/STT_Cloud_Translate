using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services
{
    public class MockTranscriptionService : ITranscriptionService
    {
        public async Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode)
        {
            // Symulacja pracy serwera
            await Task.Delay(1000); 
            return $"[MOCKUP - {languageCode}] To jest symulowana odpowiedź. Podłącz klucze Google Cloud, aby uzyskać prawdziwą transkrypcję.";
        }
    }
}