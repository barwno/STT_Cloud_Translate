// --- DANE I KONFIGURACJA AUDIO ---
let languages = [];
let audioContext, analyser, dataArray, mediaRecorder, audioChunks = [];
const isLocal = window.location.hostname === "localhost" || window.location.hostname === "127.0.0.1";
const API_BASE_URL = isLocal 
    ? "http://localhost:5289/api" 
    : "adres clouda";
// Pobieranie dostępnych języków z API C#
async function fetchLanguages() {
    try {
        const response = await fetch(`${API_BASE_URL}/translation/languages`);
        if (!response.ok) throw new Error("Błąd pobierania");
        languages = await response.json();
    } catch (error) {
        console.error("C# Offline - ładuję dane awaryjne");
        languages = [
            { code: 'pl', name: 'Polski', flag: 'pl' },
            { code: 'en', name: 'English', flag: 'gb' },
            { code: 'de', name: 'Deutsch', flag: 'de' }
        ];
    }
    if (typeof initAllDrums === "function") {
        initAllDrums();
    }
}

// Wysyłanie nagrania do tłumaczenia
async function sendToBackend(base64, person) {
    const targetIdx = person === 'a' ? state['wrapper-b'].activeIdx : state['wrapper-a'].activeIdx;
    const targetLang = languages[targetIdx].code;

    try {
        const response = await fetch(`${API_BASE_URL}/translation/languages`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                audioBase64: base64,
                targetLang: targetLang,
                person: person
            })
        });

        const data = await response.json();
        renderBubble(data.original, data.translated, person);

    } catch (error) {
        console.error("Błąd komunikacji z C#:", error);
        renderBubble("Błąd", "Serwer nie odpowiada", person);
    }
}

// Konwersja Bloba na format tekstowy Base64
function blobToBase64(blob) {
    return new Promise(resolve => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result.split(',')[1]);
        reader.readAsDataURL(blob);
    });
}