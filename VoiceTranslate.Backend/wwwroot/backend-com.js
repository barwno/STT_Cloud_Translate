async function blobToBase64(blob) {
    return new Promise((resolve, reject) => {
        const reader = new FileReader();
        reader.onloadend = () => resolve(reader.result.split(',')[1]);
        reader.onerror = reject;
        reader.readAsDataURL(blob);
    });
}

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

        if (!response.ok) {
            const errText = await response.text();
            console.error("Serwer zwrócił błąd:", response.status, errText);
            return `Błąd serwera: ${response.status}`;
        }

        const data = await response.json();
        console.log("Odebrano dane:", data);
        return data.text || "Brak tekstu w odpowiedzi.";
    } catch (error) {
        console.error("Błąd sieci/fetch:", error);
        return "Błąd połączenia: " + error.message;
    }
}
async function initializeLanguageSelector() {
    const selectElement = document.getElementById('language');
    
    try {
        // Pobieramy listę z Twojego nowego endpointu w TranscriptionController
        const response = await fetch('/api/transcription/supported-languages');
        if (!response.ok) throw new Error('Błąd API');
        
        const languages = await response.json();
        
        // Czyścimy wszystko, co wpisałeś ręcznie w HTML
        selectElement.innerHTML = '';

        // Dodajemy opcje dynamicznie
        languages.forEach(lang => {
            const option = document.createElement('option');
            option.value = lang.code;
            option.textContent = lang.name;
            selectElement.appendChild(option);
        });

        // Opcjonalnie: ustaw domyślny język, np. Polski
        selectElement.value = 'pl-PL'; 
    } catch (error) {
        console.error('Nie udało się załadować języków:', error);
        // Opcjonalnie: zostaw wtedy statyczną listę jako "fallback"
    }
}
window.addEventListener('DOMContentLoaded', initializeLanguageSelector);