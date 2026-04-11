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
      setStatus("Przetwarzanie...", true);
      
      try {
        const audioBlob = new Blob(audioChunks, { type: 'audio/webm' });
        const base64 = await blobToBase64(audioBlob);
        
        // Wywołanie funkcji z backend-com.js
        const resultText = await sendToBackend(base64);
        
        output.value = resultText;
        updateCount();
        setStatus("Gotowe");
      } catch (err) {
        console.error(err);
        setStatus("Błąd przetwarzania");
      } finally {
        setButtons(false);
        // Wyłączenie mikrofonu (zniknie ikonka w przeglądarce)
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
  setStatus("Wyczyszczono pole");
});

copyBtn.addEventListener("click", () => {
  if (output.value) {
    navigator.clipboard.writeText(output.value);
    setStatus("Skopiowano do schowka");
  }
});

output.addEventListener("input", updateCount);
updateCount();