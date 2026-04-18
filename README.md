# VoiceTranslate - Dokumentacja Backend API

Niniejszy dokument zawiera specyfikację techniczną i instrukcje integracji dla zespołu (Frontend, UX, QA). Backend obsługuje transkrypcję mowy oraz automatyczne tłumaczenie na język polski przy użyciu architektury SOLID.

---

## 1. Architektura i Tryby Pracy
Backend działa w modelu hybrydowym. System automatycznie wybiera implementację serwisu podczas startu aplikacji na podstawie dostępności poświadczeń Google Cloud:

* **Tryb Mockup (Lokalny)**: Aktywuje się automatycznie, gdy serwer nie wykryje kluczy Google Cloud. API zwraca wtedy tekst symulowany: `[MOCKUP - język]: Treść...`. Pozwala to na rozwój UI/UX bez dostępu do chmury i bez ponoszenia kosztów API.
* **Tryb Cloud (Produkcyjny)**: Wykorzystuje realne usługi Google Speech-to-Text, Translation API oraz bazę danych Firestore (kolekcja `conversations`).

---

## 2. Specyfikacja Funkcji i Endpointów

### Pobieranie dostępnych języków
* **URL:** `/api/transcription/supported-languages`
* **Metoda:** `GET`
* **Opis:** Zwraca dynamiczną listę języków obsługiwanych przez Google Translation API, gotową do wyświetlenia w interfejsie.

### Główny Endpoint: Procesowanie Audio
* **URL:** `/api/transcription/process`
* **Metoda:** `POST`
* **Content-Type:** `application/json`

#### Obiekt żądania (Request Body)
| Pole | Typ | Opis | Przykładowa wartość |
| :--- | :--- | :--- | :--- |
| `audioContent` | `string` | Nagranie audio zakodowane w **Base64** | `"UklGRi..."` |
| `languageCode` | `string` | Kod języka źródłowego (standard BCP-47) | `"en-US"`, `"pl-PL"` |

**Obsługiwane kody języków:** Zgodne z listą dostarczaną przez endpoint `supported-languages`.

#### Obiekt odpowiedzi (Response Body)
```json
{
  "text": "Rozpoznany i przetłumaczony tekst"
}
Jeśli język źródłowy jest inny niż polski, pole text zawiera wynik tłumaczenia na język polski. W przypadku braku wykrycia mowy, serwer zwraca komunikat: [Nie wykryto mowy].

3. Instrukcje Implementacyjne dla Frontendu
Konwersja Bloba na Base64
Backend przyjmuje czysty ciąg Base64. Przed wysyłką należy wyciąć metadane formatu (tzw. Data URL prefix):

const reader = new FileReader();
reader.readAsDataURL(audioBlob); 
reader.onloadend = () => {
    // split(',') usuwa prefix np. "data:audio/webm;base64,"
    const base64String = reader.result.split(',')[1]; 
    // base64String należy przypisać do pola audioContent w JSONie
};

Obsługa statusów HTTP
200 OK: Sukces. Wynik znajduje się w polu text.

400 Bad Request: Błędny format JSON, pusty string audio lub nieobsługiwany kod języka.

500 Internal Error: Błąd logiki serwera – szczegóły błędu (exception + stackTrace) są zwracane w ciele odpowiedzi JSON dla ułatwienia debugowania.

4. Narzędzia i Debugowanie
Swagger UI
Specyfikacja techniczna wszystkich endpointów oraz możliwość ręcznego testowania zapytań:
http://localhost:PORT/swagger

Logi Startowe
Podczas startu aplikacji w konsoli serwera pojawia się informacja o aktualnym trybie pracy:

>>> SYSTEM: Tryb Mockup

>>> SYSTEM: Tryb Google Cloud

5. Uruchomienie Projektu
Skopiuj pliki statyczne frontendu (HTML/JS/CSS) do folderu /wwwroot.

Uruchom backend w Visual Studio (F5) lub komendą dotnet run z poziomu folderu projektu.

Aplikacja będzie dostępna pod adresem głównym localhosta.