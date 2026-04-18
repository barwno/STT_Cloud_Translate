using Google.Cloud.Speech.V1;
using Google.Cloud.Translation.V2;
using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services
{
    // Główny moduł wykonawczy – to "mózg" aplikacji, który łączy usługi Google w jeden proces przetwarzania danych.
    public class GoogleTranscriptionService : ITranscriptionService
    {
        private readonly SpeechClient _speech;
        private readonly TranslationClient _translator;
        private readonly FirestoreDb _firestore;

        public GoogleTranscriptionService(SpeechClient speech, TranslationClient translator, FirestoreDb firestore)
        {
            _speech = speech;
            _translator = translator;
            _firestore = firestore;
        }

        // Główny proces: zamiana mowy na tekst, opcjonalne tłumaczenie i archiwizacja wyników w bazie danych.
        public async Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode)
        {
            // 1. Zamiana przesłanego nagrania audio na tekst przy użyciu zaawansowanych algorytmów rozpoznawania mowy Google.
            var response = await _speech.RecognizeAsync(new RecognitionConfig {
                Encoding = RecognitionConfig.Types.AudioEncoding.WebmOpus,
                SampleRateHertz = 48000,
                LanguageCode = languageCode
            }, RecognitionAudio.FromBytes(Convert.FromBase64String(base64Audio)));

            string text = response.Results.FirstOrDefault()?.Alternatives.FirstOrDefault()?.Transcript ?? "";

            if (string.IsNullOrEmpty(text)) return "[Nie wykryto mowy]";

            // 2. Jeśli język nagrania jest inny niż polski, automatycznie tłumaczymy uzyskany tekst na język polski.
            if (languageCode != "pl-PL") {
                var translation = await _translator.TranslateTextAsync(text, Google.Cloud.Translation.V2.LanguageCodes.Polish);
                text = translation.TranslatedText;
            }

            // 3. Archiwizacja przetworzonego tekstu w bazie danych chmurowej, aby można było do niego wrócić w przyszłości.
            await _firestore.Collection("conversations").AddAsync(new { 
                originalLang = languageCode, 
                content = text, 
                timestamp = DateTime.UtcNow 
            });

            return text;
        }
    }
}