namespace VoiceTranslate.Backend.Models
{
    // Definicja "formularza", który przechowuje dane przesyłane przez użytkownika podczas zlecenia transkrypcji.
    public class TranscriptionRequest
    {
        // Nagranie audio w formacie tekstowym, gotowe do przetworzenia przez system.
        public string AudioContent { get; set; } = string.Empty;

        // Kod języka, w którym ma zostać wykonana transkrypcja (domyślnie ustawiony na język polski).
        public string LanguageCode { get; set; } = "pl-PL";
    }
}