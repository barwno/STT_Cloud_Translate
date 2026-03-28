const state = {
    'wrapper-a': { activeIdx: 0, open: false, touchStart: 0 },
    'wrapper-b': { activeIdx: 1, open: false, touchStart: 0 }
};

let animationId;

// --- INICJALIZACJA (Wywoływana z backend-com.js) ---
function initAllDrums() {
    initDrum('wrapper-a');
    initDrum('wrapper-b');
}

// --- WIZUALIZACJA GŁOŚNOŚCI ---
function startVolumeVisualizer(stream, btnId) {
    const btn = document.getElementById(btnId);
    if (!audioContext || audioContext.state === 'closed') {
        audioContext = new (window.AudioContext || window.webkitAudioContext)();
    }

    const source = audioContext.createMediaStreamSource(stream);
    analyser = audioContext.createAnalyser();
    analyser.fftSize = 64;
    source.connect(analyser);
    dataArray = new Uint8Array(analyser.frequencyBinCount);

    function frame() {
        if (!btn.classList.contains('recording')) return;
        analyser.getByteFrequencyData(dataArray);

        let sum = dataArray.reduce((a, b) => a + b, 0);
        let average = sum / dataArray.length;

        let intensity = 0.3 + (average / 128) * 0.6;
        let scaleBonus = (average / 255) * 0.25;
        let totalScale = 1.05 + scaleBonus;

        const baseColor = btnId === 'mic-a' ? '37, 99, 235' : '5, 150, 104';
        btn.style.backgroundColor = `rgba(${baseColor}, ${Math.min(intensity, 0.9)})`;

        const transformBase = window.innerWidth >= 768 ? 'translate(-50%, -50%)' : '';
        btn.style.transform = `${transformBase} scale(${totalScale})`;

        animationId = requestAnimationFrame(frame);
    }
    frame();
}

// --- OBSŁUGA KLIKNIĘCIA MIKROFONU ---
async function handleMicAction(id, person) {
    const btn = document.getElementById(id);
    if (!btn.classList.contains('recording')) {
        btn.classList.add('recording');
        audioChunks = [];

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            startVolumeVisualizer(stream, id);

            mediaRecorder = new MediaRecorder(stream);
            mediaRecorder.ondataavailable = e => audioChunks.push(e.data);
            mediaRecorder.onstop = async () => {
                const blob = new Blob(audioChunks, { type: 'audio/wav' });
                const base64 = await blobToBase64(blob);
                sendToBackend(base64, person); // Wywołanie z backend-com.js
                stream.getTracks().forEach(t => t.stop());
            };
            mediaRecorder.start();
        } catch (err) {
            console.error("Błąd mikrofonu:", err);
            btn.classList.remove('recording');
        }
    } else {
        btn.classList.remove('recording');
        cancelAnimationFrame(animationId);

        btn.style.backgroundColor = (id === 'mic-a') ? 'var(--color-a-soft)' : 'var(--color-b-soft)';
        btn.style.transform = window.innerWidth >= 768 ? 'translate(-50%, -50%) scale(1)' : 'scale(1)';

        if (mediaRecorder) mediaRecorder.stop();
        if (audioContext) audioContext.close();
    }
}

// --- (WHEEL PICKER) ---
function initDrum(id) {
    const wrapper = document.getElementById(id);
    const wheel = wrapper.querySelector('.wheel');

    wheel.innerHTML = '';
    languages.forEach(l => {
        const div = document.createElement('div');
        div.className = 'lang-opt';
        div.innerHTML = `<span class="fi fi-${l.flag}"></span> <span>${l.name}</span>`;
        wheel.appendChild(div);
    });

    // Przewijanie kółkiem myszy
    wrapper.onwheel = (e) => {
        if (!state[id].open) return;
        e.preventDefault();
        if (e.deltaY > 0 && state[id].activeIdx < languages.length - 1) state[id].activeIdx++;
        else if (e.deltaY < 0 && state[id].activeIdx > 0) state[id].activeIdx--;
        updateDrum(id);
    };

    // Obsługa dotyku (początek)
    wrapper.ontouchstart = (e) => {
        if (state[id].open) state[id].touchStart = e.touches[0].clientY;
    };

    // Obsługa dotyku (przesunięcie)
    wrapper.ontouchmove = (e) => {
        if (!state[id].open) return;
        const dist = e.touches[0].clientY - state[id].touchStart;
        if (Math.abs(dist) > 30) {
            if (dist < 0 && state[id].activeIdx < languages.length - 1) state[id].activeIdx++;
            if (dist > 0 && state[id].activeIdx > 0) state[id].activeIdx--;
            state[id].touchStart = e.touches[0].clientY;
            updateDrum(id);
        }
    };

    wrapper.onclick = (e) => {
        e.stopPropagation();
        if (!state[id].open) {
            Object.keys(state).forEach(k => { state[k].open = false; updateDrum(k); });
            state[id].open = true;
        } else {
            const rect = wrapper.getBoundingClientRect();
            const clickY = e.clientY - rect.top;
            const itemsAbove = Math.min(state[id].activeIdx, 2);
            const clickedInView = Math.floor(clickY / 55);
            const actualIdx = state[id].activeIdx - itemsAbove + clickedInView;

            if (actualIdx >= 0 && actualIdx < languages.length) {
                state[id].activeIdx = actualIdx;
                const flagId = id === 'wrapper-a' ? 'flag-display-a' : 'flag-display-b';
                const flagEl = document.getElementById(flagId);
                if (flagEl) flagEl.className = `fi fi-${languages[actualIdx].flag}`;
            }
            state[id].open = false;
        }
        updateDrum(id);
    };

    updateDrum(id);
}

function updateDrum(id) {
    const wrapper = document.getElementById(id);
    const wheel = wrapper.querySelector('.wheel');
    const lens = wrapper.querySelector('.lens');
    const s = state[id];
    const itemH = 55;

    const itemsAbove = Math.min(s.activeIdx, 2);
    const itemsBelow = Math.min(languages.length - 1 - s.activeIdx, 2);
    const totalVisible = itemsAbove + itemsBelow + 1;

    if (s.open) {
        wrapper.style.height = `${totalVisible * itemH}px`;
        wrapper.style.top = `-${itemsAbove * itemH}px`;
        lens.style.top = `${itemsAbove * itemH}px`;
        wheel.style.transform = `translateY(-${(s.activeIdx - itemsAbove) * itemH}px)`;
    } else {
        wrapper.style.height = `${itemH}px`;
        wrapper.style.top = `0px`;
        lens.style.top = `0px`;
        wheel.style.transform = `translateY(-${s.activeIdx * itemH}px)`;
    }

    wheel.querySelectorAll('.lang-opt').forEach((opt, i) => {
        opt.classList.toggle('active', i === s.activeIdx);
    });
}

// --- RENDEROWANIE HISTORII ---
function renderBubble(orig, trans, person) {
    const list = document.getElementById('history-list');
    const empty = list.querySelector('.empty-state');
    if (empty) empty.remove();

    const item = document.createElement('div');
    item.className = `history-item type-${person}`;
    item.innerHTML = `<div class="original">${orig}</div><div class="translated">${trans}</div>`;
    list.prepend(item);
}

// --- URUCHOMIENIE SYSTEMU ---
window.onload = async () => {
    //Pobierz języki (funkcja z backend-com.js)
    await fetchLanguages();

    document.addEventListener('click', () => {
        Object.keys(state).forEach(id => { state[id].open = false; updateDrum(id); });
    });
    document.getElementById('mic-a').onclick = (e) => { e.stopPropagation(); handleMicAction('mic-a', 'a'); };
    document.getElementById('mic-b').onclick = (e) => { e.stopPropagation(); handleMicAction('mic-b', 'b'); };
};