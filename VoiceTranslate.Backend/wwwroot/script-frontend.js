const startBtn = document.getElementById("startBtn");
const stopBtn = document.getElementById("stopBtn");
const clearBtn = document.getElementById("clearBtn");
const copyBtn = document.getElementById("copyBtn");
const output = document.getElementById("output");
const statusText = document.getElementById("statusText");
const statusDot = document.getElementById("statusDot");
const language = document.getElementById("language");
const charCount = document.getElementById("charCount");

let mediaRecorder;
let audioChunks = [];
let activeStream = null;
let lastDisplayedTimestamp = null; // Przechowuje timestamp ostatnio dodanej wiadomości
let recordingStartTime = null;     // Zapamięta moment kliknięcia "Start"

function updateCount() {
    charCount.textContent = `${output.value.length} znaków`;
}

function setStatus(text, listening = false) {
    statusText.textContent = text;
    statusDot.classList.toggle("active", listening);
}

function setButtons(listening) {
    startBtn.disabled = listening;
    stopBtn.disabled = !listening;
}

startBtn.addEventListener("click", async () => {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        activeStream = stream; // Zapisujemy strumień, by go później zamknąć
        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.ondataavailable = (e) => {
            if (e.data.size > 0) audioChunks.push(e.data);
        };
        
        mediaRecorder.onstop = async () => {
            const recordingDuration = Date.now() - recordingStartTime;
            if (recordingDuration < 1000) { 
                setStatus("Nagranie zbyt krótkie");
                setButtons(false);
                if (activeStream) {
                    activeStream.getTracks().forEach(track => track.stop());
                }
                return;
            }

            setStatus("Przetwarzanie...", true);
            
            try {
                const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
                const base64 = await blobToBase64(audioBlob);
                
                const resultText = await sendToBackend(base64);
                
                if (resultText && resultText.startsWith("Błąd")) {
                    output.value += (output.value ? "\n\n" : "") + `[System] ${resultText}`;
                    setStatus("Błąd przetwarzania");
                    return; // Wychodzimy, blokując odpytanie Firestore o stary rekord
                }
                
                const latestRecord = await fetchLatestFromRecord();
                
                if (latestRecord) {
                    const newMessage = `[${latestRecord.timestamp}] ${latestRecord.content}`;
                    
                    if (latestRecord.timestamp !== lastDisplayedTimestamp) {
                        output.value += (output.value ? "\n\n" : "") + newMessage;
                        lastDisplayedTimestamp = latestRecord.timestamp;
                    }
                } else {
                    output.value += (output.value ? "\n\n" : "") + "[Błąd: Nie udało się zweryfikować zapisu w bazie danych Firestore]";
                }
                
                output.scrollTop = output.scrollHeight;
                
                updateCount();
                setStatus("Gotowe");
            } catch (err) {
                console.error(err);
                setStatus("Błąd przetwarzania");
            } finally {
                setButtons(false);
                if (activeStream) {
                    activeStream.getTracks().forEach(track => track.stop());
                }
            }
        };

        // Zapisujemy dokładny czas kliknięcia START i odpalamy nagrywanie
        recordingStartTime = Date.now();
        mediaRecorder.start();
        setButtons(true);
        setStatus("Nagrywanie...", true);

    } catch (err) {
        console.error(err);
        setStatus("Błąd: Brak dostępu do mikrofonu");
        setButtons(false);
    }
});

stopBtn.addEventListener("click", () => {
    if (mediaRecorder && mediaRecorder.state !== "inactive") {
        mediaRecorder.stop();
        setStatus("Zatrzymywanie...");
    }
});

clearBtn.addEventListener("click", () => {
    output.value = "";
    lastDisplayedTimestamp = null;
    updateCount();
    setStatus("Wyczyszczono pole");
});

copyBtn.addEventListener("click", () => {
    if (output.value) {
        navigator.clipboard.writeText(output.value);
        setStatus("Skopiowano do schowka");
    }
});

// Autonomiczna funkcja pobierająca najnowszy rekord bezpośrednio z endpointu API bazy danych
async function fetchLatestFromRecord() {
    try {
        const response = await fetch('/api/transcription/latest');
        if (!response.ok) return null;
        const data = await response.json();
        return data; // Zwraca obiekt { content: string, timestamp: string }
    } catch (error) {
        console.error("Błąd pobierania najnowszego rekordu:", error);
        return null;
    }
}

output.addEventListener("input", updateCount);
updateCount();