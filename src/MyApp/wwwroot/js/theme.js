// wwwroot/js/theme.js

(function () {
    const THEME_KEY = 'theme';            // persisted choice: 'light' | 'dark' | 'contrast'
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

    // Cycle: light -> dark -> contrast -> light
    function nextTheme(t) {
        if (t === 'light') return 'dark';
        if (t === 'dark') return 'contrast';
        return 'light';
    }

    // Keep button label/icon in sync
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
        // If you show an icon span:
        const iconSpan = btn.querySelector('[data-icon]');
        if (iconSpan) iconSpan.textContent = icon;
    }

    // Initialize theme on load
    function initTheme() {
        const saved = getSavedTheme();
        setTheme(saved || systemTheme(), /*persist*/ !!saved);

        // If user did NOT choose manually, follow system changes live
        if (!saved) {
            mq.addEventListener?.('change', () => setTheme(systemTheme(), /*persist*/ false));
        }
    }

    // Wire up button
    function initToggle() {
        const btn = document.getElementById('theme-toggle');
        if (!btn) return;
        btn.addEventListener('click', () => {
            const next = nextTheme(currentTheme());
            setTheme(next, /*persist*/ true);
        });
    }

    // Run
    initTheme();
    document.addEventListener('DOMContentLoaded', initToggle);
})();
