// Zmienna globalna przechowująca unikalny identyfikator sesji dla karty przeglądarki
let currentSessionId = null;

// Wywoływane automatycznie przy starcie strony – tworzy izolowany dokument sesji w Firestore
async function startNewSession() {
    try {
        const response = await fetch('/api/transcription/start-session');
        if (!response.ok) throw new Error('Nie udało się utworzyć sesji na serwerze.');
        
        const data = await response.json();
        currentSessionId = data.sessionId;
        console.log("Inicjalizacja sesji udana. ID sesji:", currentSessionId);
    } catch (error) {
        console.error("Krytyczny błąd podczas tworzenia sesji:", error);
    }
}

// Funkcja zamieniająca nagranie dźwiękowe z przeglądarki na format tekstowy
async function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result.split(',')[1]);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

// Funkcja wysyłająca nagranie do backendu
async function sendToBackend(base64Audio) {
    const lang = document.getElementById("language").value;
    console.log("Wysyłanie danych do backendu... Język:", lang);
    const offset = new Date().getTimezoneOffset();

    try {
        const response = await fetch('/api/transcription/process', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                AudioContent: base64Audio,
                LanguageCode: lang,
                SessionId: currentSessionId,
                TimezoneOffset: offset // <-- WYSYŁAMY CZAS LOKALNY DO BACKENDU
            })
        });

        if (!response.ok) {
            const errText = await response.text();
            console.error("Serwer zwrócił błąd:", response.status, errText);
            return `Błąd serwera: ${response.status}`;
        }

        const data = await response.json();
        return data.text; 
    } catch (error) {
        console.error("Błąd sieci/fetch:", error);
        return "Błąd połączenia: " + error.message;
    }
}

// Pobiera tablicę wszystkich transkrypcji należących wyłącznie do tej sesji
async function fetchSessionHistory() {
    if (!currentSessionId) return [];
    try {
        const response = await fetch(`/api/transcription/session-history/${currentSessionId}`);
        if (!response.ok) return [];
        return await response.json();
    } catch (error) {
        console.error("Nie udało się pobrać historii sesji:", error);
        return [];
    }
}

// Pobiera listę języków z serwera i uzupełnia selektor
async function initializeLanguageSelector() {
    const selectElement = document.getElementById('language');
    try {
        const response = await fetch('/api/transcription/supported-languages');
        if (!response.ok) throw new Error('Błąd API');
        
        const languages = await response.json();
        selectElement.innerHTML = ''; 

        languages.forEach(lang => {
            const option = document.createElement('option');
            option.value = lang.code;
            option.textContent = lang.name;
            selectElement.appendChild(option);
        });

        selectElement.value = 'pl-PL'; 
    } catch (error) {
        console.error('Nie udało się załadować języków:', error);
    }
}

window.addEventListener('DOMContentLoaded', initializeLanguageSelector);