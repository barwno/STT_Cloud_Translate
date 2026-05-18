const startBtn = document.getElementById("startBtn");
const stopBtn = document.getElementById("stopBtn");
const clearBtn = document.getElementById("clearBtn");
const copyBtn = document.getElementById("copyBtn");
const output = document.getElementById("output");
const statusText = document.getElementById("statusText");
const statusDot = document.getElementById("statusDot");
const charCount = document.getElementById("charCount");

let mediaRecorder;
let audioChunks = [];
let activeStream = null;

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

// Pobiera historię sesji i odświeża pole tekstowe czatu
async function refreshChatFromSession() {
    const history = await fetchSessionHistory();
    output.value = history
        .map(item => `[${item.timestamp}] ${item.content}`)
        .join("\n\n");
        
    output.scrollTop = output.scrollHeight;
    updateCount();
}

startBtn.addEventListener("click", async () => {
    try {
        const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
        activeStream = stream;
        mediaRecorder = new MediaRecorder(stream);
        audioChunks = [];

        mediaRecorder.ondataavailable = (e) => {
            if (e.data.size > 0) audioChunks.push(e.data);
        };
        
        mediaRecorder.onstop = async () => {
            setStatus("Przetwarzanie...", true);
            
            try {
                const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
                const base64 = await blobToBase64(audioBlob);
                
                const resultText = await sendToBackend(base64);
                
                if (resultText && resultText.startsWith("Błąd")) {
                    output.value += (output.value ? "\n\n" : "") + `[System] ${resultText}`;
                    setStatus("Błąd przetwarzania");
                    return;
                }

                if (!resultText || resultText.trim() === "") {
                    setStatus("Nie rozpoznano mowy");
                } else {
                    setStatus("Gotowe");
                }

                await refreshChatFromSession();

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
    updateCount();
    setStatus("Wyczyszczono podgląd sesji");
});

copyBtn.addEventListener("click", () => {
    if (output.value) {
        navigator.clipboard.writeText(output.value);
        setStatus("Skopiowano do schowka");
    }
});

output.addEventListener("input", updateCount);

// INICJALIZACJA SYSTEMU SESYJNEGO
window.addEventListener('DOMContentLoaded', async () => {
    startBtn.disabled = true; 
    setStatus("Inicjalizacja sesji...");

    await startNewSession(); 
    
    if (currentSessionId) {
        startBtn.disabled = false;
        setStatus("System gotowy");
    } else {
        setStatus("Błąd sesji - odśwież stronę");
    }
    
    updateCount();
});