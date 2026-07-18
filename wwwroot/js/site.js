// Notespack site animations
// All the little touches that make the page feel alive
// Runs once per page load, called from Home.razor via JS interop

window.notespack = window.notespack || {};

window.notespack.initHome = function () {
    initSplitText();
    initTilt();
    initReveals();
    initCounters();
    initCursorHalo();
};

// Split the hero heading into words so we can stagger them in
function initSplitText() {
    const targets = document.querySelectorAll('[data-split]');
    targets.forEach(el => {
        if (el.dataset.splitDone) return;
        const words = el.textContent.trim().split(/\s+/);
        el.textContent = '';
        words.forEach((word, i) => {
            const span = document.createElement('span');
            span.className = 'word';
            span.style.transitionDelay = (i * 60) + 'ms';
            span.textContent = word;
            el.appendChild(span);
            el.appendChild(document.createTextNode(' '));
        });
        el.dataset.splitDone = 'true';
    });
}

// 3D card tilt that follows the mouse
// Uses CSS transforms, no library needed
function initTilt() {
    const cards = document.querySelectorAll('[data-tilt]');
    cards.forEach(card => {
        card.addEventListener('mousemove', e => {
            const rect = card.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;
            const rotateX = ((y / rect.height) - 0.5) * -8;
            const rotateY = ((x / rect.width) - 0.5) * 8;
            card.style.transform = `perspective(900px) rotateX(${rotateX}deg) rotateY(${rotateY}deg) translateZ(0)`;
            card.style.setProperty('--mx', (x / rect.width * 100) + '%');
            card.style.setProperty('--my', (y / rect.height * 100) + '%');
        });
        card.addEventListener('mouseleave', () => {
            card.style.transform = '';
        });
    });
}

// Fade sections in as they scroll into view
function initReveals() {
    const revealEls = document.querySelectorAll('.reveal');
    if (!('IntersectionObserver' in window)) {
        revealEls.forEach(el => el.classList.add('is-visible'));
        return;
    }
    const io = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('is-visible');
                io.unobserve(entry.target);
            }
        });
    }, { threshold: 0.15, rootMargin: '0px 0px -60px 0px' });
    revealEls.forEach(el => io.observe(el));
}

// Count numbers up from zero when the stat strip scrolls into view
function initCounters() {
    const nums = document.querySelectorAll('[data-count]');
    if (!nums.length) return;

    const animate = (el) => {
        const target = parseInt(el.dataset.count, 10);
        const suffix = el.dataset.suffix || '';
        const duration = 1400;
        const start = performance.now();
        const step = (now) => {
            const p = Math.min((now - start) / duration, 1);
            const eased = 1 - Math.pow(1 - p, 3);
            el.textContent = Math.floor(eased * target).toLocaleString() + suffix;
            if (p < 1) requestAnimationFrame(step);
        };
        requestAnimationFrame(step);
    };

    const io = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                animate(entry.target);
                io.unobserve(entry.target);
            }
        });
    }, { threshold: 0.4 });

    nums.forEach(n => io.observe(n));
}

// Soft cyan dot that follows the cursor across the page
function initCursorHalo() {
    if (window.matchMedia('(hover: none)').matches) return;

    let halo = document.getElementById('np-cursor-halo');
    if (!halo) {
        halo = document.createElement('div');
        halo.id = 'np-cursor-halo';
        halo.setAttribute('aria-hidden', 'true');
        halo.style.cssText = 'position:fixed;top:0;left:0;width:340px;height:340px;pointer-events:none;border-radius:50%;background:radial-gradient(circle,rgba(0,212,255,0.18),rgba(0,212,255,0) 60%);transform:translate(-50%,-50%);z-index:1;transition:opacity .3s ease;opacity:0;mix-blend-mode:screen;';
        document.body.appendChild(halo);
    }

    document.addEventListener('mousemove', e => {
        halo.style.left = e.clientX + 'px';
        halo.style.top = e.clientY + 'px';
        halo.style.opacity = '1';
    });

    document.addEventListener('mouseleave', () => {
        halo.style.opacity = '0';
    });
}

// Respect users who prefer reduced motion
// If the OS setting says "reduce motion", skip everything above
if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) {
    window.notespack.initHome = function () {
        document.querySelectorAll('.reveal').forEach(el => el.classList.add('is-visible'));
        document.querySelectorAll('[data-count]').forEach(el => {
            el.textContent = el.dataset.count + (el.dataset.suffix || '');
        });
    };
}