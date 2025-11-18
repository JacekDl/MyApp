(function () {
    const THEME_KEY = 'theme';
    const mq = window.matchMedia('(prefers-color-scheme: dark)');

    function systemTheme() {
        return mq.matches ? 'dark' : 'light';
    }

    function getSavedTheme() {
        return localStorage.getItem(THEME_KEY);
    }

    function setTheme(theme, persist = true) {
        document.documentElement.setAttribute('data-bs-theme', theme);
        if (persist) localStorage.setItem(THEME_KEY, theme);
        updateToggleUI(theme);
    }

    function currentTheme() {
        return document.documentElement.getAttribute('data-bs-theme') || systemTheme();
    }

    function nextTheme(t) {
        if (t === 'light') return 'dark';
        if (t === 'dark') return 'contrast';
        return 'light';
    }

    function updateToggleUI(theme) {
        const btn = document.getElementById('theme-toggle');
        const label = document.getElementById('theme-toggle-label');
        if (!btn || !label) return;

        const map = {
            light: { text: 'Light', icon: '☀️' },
            dark: { text: 'Dark', icon: '🌙' },
            contrast: { text: 'Contrast', icon: '⚡' },
        };
        const { text, icon } = map[theme] || map.light;

        label.textContent = text;
        btn.setAttribute('aria-label', `Switch theme (current: ${text})`);

        const iconSpan = btn.querySelector('[data-icon]');
        if (iconSpan) iconSpan.textContent = icon;
    }

    function initTheme() {
        const saved = getSavedTheme();
        setTheme(saved || systemTheme(), /*persist*/ !!saved);

        if (!saved) {
            mq.addEventListener?.('change', () => setTheme(systemTheme(), /*persist*/ false));
        }
    }

    function initToggle() {
        const btn = document.getElementById('theme-toggle');
        if (!btn) return;
        btn.addEventListener('click', () => {
            const next = nextTheme(currentTheme());
            setTheme(next, /*persist*/ true);
        });
    }

    initTheme();
    document.addEventListener('DOMContentLoaded', initToggle);

    document.addEventListener('DOMContentLoaded', function () {
        if (!window.bootstrap || !bootstrap.Toast) return;
        document.querySelectorAll('.toast').forEach(function (el) {
            new bootstrap.Toast(el, { autohide: true, delay: 5000 }).show();
        });
    });

    document.addEventListener('DOMContentLoaded', function () {
        const html = document.documentElement;
        const btn = document.getElementById('themeToggle');
        const icons = {
            light: document.getElementById('icon-sun'),
            dark: document.getElementById('icon-dark'),
            contrast: document.getElementById('icon-contrast')
        };

        const savedTheme = localStorage.getItem('theme') || 'light';
        setTheme(savedTheme);

        btn.addEventListener('click', () => {
            const current = html.getAttribute('data-bs-theme');
            const next = current === 'light' ? 'dark'
                : current === 'dark' ? 'contrast'
                    : 'light';
            setTheme(next);
            localStorage.setItem('theme', next);
        });

        function setTheme(theme) {
            html.setAttribute('data-bs-theme', theme);
            Object.values(icons).forEach(i => i.classList.add('d-none'));
            icons[theme]?.classList.remove('d-none');
        }
    });
})();
