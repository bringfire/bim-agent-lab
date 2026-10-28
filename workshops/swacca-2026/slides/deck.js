// Small slide engine shared by the SWACCA 2026 decks. No dependencies; works from file://.
// Slides live in agenda blocks: <div class="block" data-label data-start data-end data-tone>.
// Keys: arrows/Page/Space navigate, N notes, S presenter window, L light theme, F fullscreen, ? help.
(() => {
  const body = document.body;
  const stage = document.querySelector('.stage');
  const slides = [...stage.querySelectorAll('.slide')];
  const total = slides.length;
  const deck = body.dataset.deck || location.pathname;
  const session = body.dataset.session || '';
  const params = new URLSearchParams(location.search);
  const NS = 'bal-deck';
  const store = {
    get(key) { try { return localStorage.getItem(key); } catch { return null; } },
    set(key, value) { try { localStorage.setItem(key, value); } catch { /* storage unavailable */ } },
  };

  // Decorate each slide with its block's tone, a kicker and a footer.
  slides.forEach((slide, i) => {
    const block = slide.closest('.block');
    const info = block ? block.dataset : {};
    slide.dataset.tone ||= info.tone || 'frame';
    slide.dataset.label = info.label || '';
    slide.dataset.start = info.start || '';
    slide.dataset.end = info.end || '';
    const heading = slide.querySelector('h1, h2');
    slide.dataset.title ||= heading ? heading.textContent.trim() : `Slide ${i + 1}`;
    slide.setAttribute('role', 'group');
    slide.setAttribute('aria-roledescription', 'slide');
    slide.setAttribute('aria-label', `${i + 1} of ${total}: ${slide.dataset.title}`);
    if (!slide.classList.contains('title') && info.label) {
      const kicker = document.createElement('div');
      kicker.className = 'kicker';
      kicker.textContent = `${info.label} · ${info.start}–${info.end} min`;
      slide.prepend(kicker);
    }
    const foot = document.createElement('div');
    foot.className = 'foot';
    const left = document.createElement('span');
    left.textContent = `BIM Agent Lab · SWACCA BIM Summit 2026 · ${session}`;
    const right = document.createElement('span');
    right.className = 'page';
    right.textContent = `${slide.dataset.page ? `Handout p. ${slide.dataset.page} · ` : ''}${i + 1} / ${total}`;
    foot.append(left, right);
    slide.append(foot);
  });

  const notesOf = (i) => {
    const notes = slides[i] && slides[i].querySelector('.notes');
    return notes ? notes.innerHTML : '<p>No notes for this slide.</p>';
  };
  const indexFromHash = () => {
    const n = parseInt(location.hash.slice(1), 10);
    return Number.isFinite(n) ? Math.min(Math.max(n - 1, 0), total - 1) : 0;
  };
  const clamp = (i) => Math.min(Math.max(i, 0), total - 1);

  if (store.get('bal-deck-theme') === 'light' || params.has('light')) body.classList.add('light');
  const toggleTheme = () => {
    body.classList.toggle('light');
    store.set('bal-deck-theme', body.classList.contains('light') ? 'light' : 'dark');
  };

  const help = document.createElement('div');
  help.className = 'help';
  help.innerHTML = [
    ['→ Space', 'next slide'], ['←', 'previous slide'], ['Home End', 'first / last'],
    ['12 Enter', 'go to slide 12'], ['N', 'speaker notes'], ['S', 'presenter window'],
    ['L', 'light / dark'], ['F', 'fullscreen'], ['?', 'this help'],
  ].map(([k, v]) => `<div><kbd>${k}</kbd> ${v}</div>`).join('');
  body.append(help);

  // Shared key handling; `go` differs between the audience and presenter windows.
  let typed = '';
  const bindKeys = (go, current, extra) => {
    document.addEventListener('keydown', (e) => {
      if (e.metaKey || e.ctrlKey || e.altKey) return;
      const k = e.key;
      if (/^[0-9]$/.test(k)) { typed += k; return; }
      if (k === 'Enter' && typed) { go(parseInt(typed, 10) - 1); typed = ''; e.preventDefault(); return; }
      typed = '';
      if (['ArrowRight', 'ArrowDown', 'PageDown', ' ', 'Enter'].includes(k)) go(current() + 1);
      else if (['ArrowLeft', 'ArrowUp', 'PageUp', 'Backspace'].includes(k)) go(current() - 1);
      else if (k === 'Home') go(0);
      else if (k === 'End') go(total - 1);
      else if (k === 'l' || k === 'L') toggleTheme();
      else if (k === '?' || k === 'h' || k === 'H') body.classList.toggle('show-help');
      else if (k === 'f' || k === 'F') {
        if (document.fullscreenElement) document.exitFullscreen();
        else if (document.documentElement.requestFullscreen) document.documentElement.requestFullscreen();
      } else if (extra && extra(k)) { /* handled */ } else return;
      e.preventDefault();
    });
  };

  if (params.has('presenter')) presenter();
  else audience();

  function audience() {
    let cur = 0;
    const panel = document.createElement('div');
    panel.className = 'notes-panel';
    body.append(panel);

    const fit = () => stage.style.setProperty('--scale', Math.min(innerWidth / 1280, innerHeight / 720));
    const show = (i) => {
      cur = clamp(i);
      slides.forEach((s, j) => { s.hidden = j !== cur; });
      history.replaceState(null, '', `${location.pathname}${location.search}#${cur + 1}`);
      panel.innerHTML = `<h4>Notes · ${cur + 1} / ${total} · ${slides[cur].dataset.title}</h4>${notesOf(cur)}`;
    };

    // The presenter window asks where we are and requests moves; this window stays authoritative.
    window.addEventListener('message', (e) => {
      const d = e.data;
      if (!d || d.ns !== NS || d.deck !== deck) return;
      if (d.type === 'go') show(d.i);
      if (e.source) e.source.postMessage({ ns: NS, deck, type: 'at', i: cur }, '*');
    });
    const openPresenter = () => {
      const url = `${location.href.split(/[?#]/)[0]}?presenter#${cur + 1}`;
      window.open(url, `bal-presenter-${deck}`, 'width=1280,height=800');
    };

    bindKeys(show, () => cur, (k) => {
      if (k === 'n' || k === 'N') { body.classList.toggle('show-notes'); return true; }
      if (k === 's' || k === 'S') { openPresenter(); return true; }
      return false;
    });
    let touchX = null;
    stage.addEventListener('touchstart', (e) => { touchX = e.touches[0].clientX; }, { passive: true });
    stage.addEventListener('touchend', (e) => {
      if (touchX === null) return;
      const dx = e.changedTouches[0].clientX - touchX;
      if (Math.abs(dx) > 50) show(cur + (dx < 0 ? 1 : -1));
      touchX = null;
    });
    window.addEventListener('hashchange', () => { if (indexFromHash() !== cur) show(indexFromHash()); });
    window.addEventListener('resize', fit);
    fit();
    show(indexFromHash());
  }

  function presenter() {
    body.classList.add('presenter');
    document.title = `Presenter · ${document.title}`;
    const startClock = (body.dataset.start || '00:00').split(':').map(Number);
    const view = document.createElement('div');
    view.className = 'pv';
    view.innerHTML = `
      <div class="pv-frame pv-current"><div class="stage"></div></div>
      <div class="pv-notes"></div>
      <div class="pv-side">
        <div class="pv-clock">
          <div><span class="label">Clock</span><strong class="pv-now">--:--</strong></div>
          <div><span class="label">Elapsed · T starts</span><strong class="pv-elapsed">--:--</strong></div>
        </div>
        <div class="pv-block"></div>
        <h4>Next</h4>
        <div class="pv-frame pv-next"><div class="stage"></div></div>
        <div class="pv-tools"><button data-act="prev">◀ Prev</button><button data-act="next">Next ▶</button><button data-act="timer">Timer</button></div>
        <div class="pv-list"></div>
      </div>`;
    body.append(view);
    const $ = (sel) => view.querySelector(sel);
    const frames = [$('.pv-current'), $('.pv-next')];
    frames.forEach((f) => { f.style.aspectRatio = '16 / 9'; });
    const list = $('.pv-list');
    slides.forEach((s, i) => {
      const b = document.createElement('button');
      b.innerHTML = `<b>${i + 1}</b>`;
      b.append(s.dataset.title);
      b.addEventListener('click', () => request(i));
      list.append(b);
    });

    let cur = indexFromHash();
    let lastLocal = 0;
    const linked = () => window.opener && !window.opener.closed;
    const fit = () => frames.forEach((f) => f.firstElementChild.style.setProperty('--scale', f.clientWidth / 1280));
    const place = (frame, i) => {
      const target = frame.firstElementChild;
      target.replaceChildren();
      if (i >= total) {
        const end = document.createElement('section');
        end.className = 'slide';
        end.innerHTML = '<h2 class="muted">End of deck</h2>';
        target.append(end);
        return;
      }
      const copy = slides[i].cloneNode(true);
      copy.hidden = false;
      target.append(copy);
    };
    const render = (i) => {
      cur = clamp(i);
      place(frames[0], cur);
      place(frames[1], cur + 1);
      $('.pv-notes').innerHTML = `<h4>Notes · ${cur + 1} / ${total}</h4>${notesOf(cur)}`;
      [...list.children].forEach((b, j) => b.classList.toggle('on', j === cur));
      list.children[cur].scrollIntoView({ block: 'nearest' });
      history.replaceState(null, '', `${location.pathname}${location.search}#${cur + 1}`);
      tick();
    };
    const request = (i) => {
      if (linked()) window.opener.postMessage({ ns: NS, deck, type: 'go', i: clamp(i) }, '*');
      else { lastLocal = Date.now(); render(i); }
    };
    window.addEventListener('message', (e) => {
      const d = e.data;
      if (!d || d.ns !== NS || d.deck !== deck || d.type !== 'at') return;
      if (d.i !== cur && Date.now() - lastLocal > 500) render(d.i);
    });
    setInterval(() => { if (linked()) window.opener.postMessage({ ns: NS, deck, type: 'where' }, '*'); }, 300);

    // Timer: compare elapsed time with the agenda block of the current slide.
    let started = null;
    try { started = Number(sessionStorage.getItem(`bal-timer-${deck}`)) || null; } catch { /* unavailable */ }
    const toggleTimer = () => {
      started = started ? null : Date.now();
      try { sessionStorage.setItem(`bal-timer-${deck}`, started ? String(started) : ''); } catch { /* unavailable */ }
      tick();
    };
    const two = (n) => String(n).padStart(2, '0');
    const clockAt = (minutes) => {
      const m = startClock[0] * 60 + (startClock[1] || 0) + minutes;
      const h = Math.floor(m / 60) % 24;
      return `${(h + 11) % 12 + 1}:${two(m % 60)}`;
    };
    function tick() {
      const now = new Date();
      $('.pv-now').textContent = `${(now.getHours() + 11) % 12 + 1}:${two(now.getMinutes())}`;
      const s = slides[cur].dataset;
      const start = Number(s.start), end = Number(s.end);
      const blockEl = $('.pv-block');
      blockEl.dataset.tone = s.tone;
      let status = '<span class="pv-status">Timer not started</span>';
      if (started) {
        const sec = Math.floor((Date.now() - started) / 1000);
        $('.pv-elapsed').textContent = `${two(Math.floor(sec / 60))}:${two(sec % 60)}`;
        const min = sec / 60;
        if (min > end) status = `<span class="pv-status behind">Over this block by ${Math.ceil(min - end)} min</span>`;
        else if (min < start) status = `<span class="pv-status ahead">Ahead: block starts at ${start} min</span>`;
        else status = `<span class="pv-status ok">On time · ${Math.ceil(end - min)} min left in block</span>`;
      } else $('.pv-elapsed').textContent = '--:--';
      blockEl.innerHTML = s.label
        ? `<span class="label">Agenda block</span><br><strong>${s.label}</strong> · ${start}–${end} min · ${clockAt(start)}–${clockAt(end)}<br>${status}`
        : `<span class="label">Agenda block</span><br>${status}`;
    }
    setInterval(tick, 500);

    view.addEventListener('click', (e) => {
      const act = e.target.closest('button') && e.target.closest('button').dataset.act;
      if (act === 'prev') request(cur - 1);
      if (act === 'next') request(cur + 1);
      if (act === 'timer') toggleTimer();
    });
    bindKeys(request, () => cur, (k) => {
      if (k === 't' || k === 'T') { toggleTimer(); return true; }
      return false;
    });
    window.addEventListener('resize', fit);
    fit();
    render(cur);
  }
})();
