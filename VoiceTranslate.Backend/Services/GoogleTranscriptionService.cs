using Google.Cloud.Speech.V1;
using Google.Cloud.Translation.V2;
using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;

namespace VoiceTranslate.Backend.Services
{
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

        public async Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode)
        {
            // 1. Rozpoznawanie mowy
            var response = await _speech.RecognizeAsync(new RecognitionConfig {
                Encoding = RecognitionConfig.Types.AudioEncoding.WebmOpus,
                SampleRateHertz = 48000,
                LanguageCode = languageCode
            }, RecognitionAudio.FromBytes(Convert.FromBase64String(base64Audio)));

            string text = response.Results.FirstOrDefault()?.Alternatives.FirstOrDefault()?.Transcript ?? "";

            if (string.IsNullOrEmpty(text)) return "[Nie wykryto mowy]";

            // 2. Tłumaczenie na PL (jeśli trzeba)
            if (languageCode != "pl-PL") {
                var translation = await _translator.TranslateTextAsync(text, Google.Cloud.Translation.V2.LanguageCodes.Polish);
                text = translation.TranslatedText;
            }

            // 3. Zapis do Firestore
            await _firestore.Collection("conversations").AddAsync(new { 
                originalLang = languageCode, 
                content = text, 
                timestamp = DateTime.UtcNow 
            });

            return text;
        }
    }
}