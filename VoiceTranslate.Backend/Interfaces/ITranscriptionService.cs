namespace VoiceTranslate.Backend.Interfaces
{
    // Definicja głównego kontraktu dla systemu transkrypcji – określa, co konkretnie musi umieć każda usługa przetwarzająca mowę na tekst.
    public interface ITranscriptionService
    {
        // Główna funkcja zlecająca przetworzenie nagrania audio (w formie zakodowanego tekstu) na zapis tekstowy w wybranym języku.
        Task<string> ProcessTranscriptionAsync(string base64Audio, string languageCode);
    }
}