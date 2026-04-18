// Funkcja zamieniająca nagranie dźwiękowe z przeglądarki na format tekstowy zrozumiały dla serwera.
async function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result.split(',')[1]);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

// Funkcja wysyłająca nagranie do backendu i oczekująca na gotowy tekst (transkrypcję).
async function sendToBackend(base64Audio) {
    const lang = document.getElementById("language").value;
    console.log("Wysyłanie danych do backendu... Język:", lang);

    try {
        const response = await fetch('/api/transcription/process', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                audioContent: base64Audio,
                languageCode: lang
            })
        });

        // Obsługa błędów, jeśli serwer nie może przetworzyć nagrania.
        if (!response.ok) {
            const errText = await response.text();
            console.error("Serwer zwrócił błąd:", response.status, errText);
            return `Błąd serwera: ${response.status}`;
        }

        const data = await response.json();
        return data.text || "Brak tekstu w odpowiedzi.";
    } catch (error) {
        console.error("Błąd sieci/fetch:", error);
        return "Błąd połączenia: " + error.message;
    }
}

// Pobiera listę języków z serwera i dynamicznie uzupełnia menu wyboru na stronie.
async function initializeLanguageSelector() {
    const selectElement = document.getElementById('language');
    
    try {
        const response = await fetch('/api/transcription/supported-languages');
        if (!response.ok) throw new Error('Błąd API');
        
        const languages = await response.json();
        
        selectElement.innerHTML = ''; // Usuwa dotychczasowe opcje.

        // Dodaje nowe opcje języków pobrane z Google.
        languages.forEach(lang => {
            const option = document.createElement('option');
            option.value = lang.code;
            option.textContent = lang.name;
            selectElement.appendChild(option);
        });

        selectElement.value = 'pl-PL'; // Ustawia polski jako domyślny język.
    } catch (error) {
        console.error('Nie udało się załadować języków:', error);
    }
}
window.addEventListener('DOMContentLoaded', initializeLanguageSelector);