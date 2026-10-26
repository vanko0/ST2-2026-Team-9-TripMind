(function () {
    const widget = document.getElementById('chat-widget');
    if (!widget) return;

    const toggleBtn = document.getElementById('chat-toggle');
    const panel = document.getElementById('chat-panel');
    const clearBtn = document.getElementById('chat-clear');
    const messagesEl = document.getElementById('chat-messages');
    const suggestionsEl = document.getElementById('chat-suggestions');
    const input = document.getElementById('chat-input');
    const sendBtn = document.getElementById('chat-send');

    const STORAGE_KEY = 'tripmind-chat';
    const WELCOME = "Hi! I'm the TripMind Assistant. Ask me about destinations, budgets or your saved trips.";

    let history = [];   // [{ role: 'user' | 'model', text: '...' }]
    let busy = false;

    // ---------- Save / load (so the chat survives page changes) ----------
    function save() {
        try {
            sessionStorage.setItem(STORAGE_KEY, JSON.stringify({
                history: history,
                open: widget.classList.contains('open')
            }));
        } catch (e) { /* storage not available, ignore */ }
    }

    function load() {
        try {
            const saved = JSON.parse(sessionStorage.getItem(STORAGE_KEY));
            if (saved) {
                history = saved.history || [];
                if (saved.open) setOpen(true, false);
            }
        } catch (e) { /* ignore */ }
    }

    // ---------- Showing messages ----------
    function addMessage(text, kind) {
        const div = document.createElement('div');
        div.className = 'chat-msg ' + kind;      // kind: user | bot | error
        div.textContent = text;                  // textContent is safe (no HTML injection)
        messagesEl.appendChild(div);
        scrollDown();
        return div;
    }

    function addTyping() {
        const div = document.createElement('div');
        div.className = 'chat-msg bot chat-typing';
        div.innerHTML = '<span></span><span></span><span></span>';
        messagesEl.appendChild(div);
        scrollDown();
        return div;
    }

    function scrollDown() {
        messagesEl.scrollTop = messagesEl.scrollHeight;
    }

    function renderAll() {
        messagesEl.innerHTML = '';
        addMessage(WELCOME, 'bot');
        history.forEach(t => addMessage(t.text, t.role === 'user' ? 'user' : 'bot'));
        suggestionsEl.style.display = history.length === 0 ? 'flex' : 'none';
    }

    // ---------- Open / close ----------
    function setOpen(open, shouldSave) {
        widget.classList.toggle('open', open);
        panel.classList.toggle('chat-hidden', !open);
        if (open) {
            scrollDown();
            input.focus();
        }
        if (shouldSave !== false) save();
    }

    // ---------- Sending ----------
    async function send(text) {
        text = (text || '').trim();
        if (!text || busy) return;

        busy = true;
        sendBtn.disabled = true;
        input.value = '';
        suggestionsEl.style.display = 'none';

        addMessage(text, 'user');
        const typing = addTyping();

        try {
            const res = await fetch('/api/chat', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({
                    message: text,
                    history: history.slice(-10)   // the server adds the new message itself
                })
            });

            const data = await res.json().catch(() => ({}));
            if (!res.ok) throw new Error(data.error || 'Something went wrong. Please try again.');

            typing.remove();
            addMessage(data.reply, 'bot');

            history.push({ role: 'user', text: text });
            history.push({ role: 'model', text: data.reply });
            save();
        } catch (err) {
            typing.remove();
            addMessage(err.message || 'Could not reach the assistant.', 'error');
        } finally {
            busy = false;
            sendBtn.disabled = false;
            input.focus();
        }
    }

    // ---------- Events ----------
    toggleBtn.addEventListener('click', () => setOpen(!widget.classList.contains('open')));

    sendBtn.addEventListener('click', () => send(input.value));

    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            send(input.value);
        }
    });

    document.querySelectorAll('.chat-chip').forEach(chip =>
        chip.addEventListener('click', () => send(chip.textContent)));

    clearBtn.addEventListener('click', () => {
        history = [];
        renderAll();
        save();
    });

    // ---------- Start ----------
    load();
    renderAll();
})();