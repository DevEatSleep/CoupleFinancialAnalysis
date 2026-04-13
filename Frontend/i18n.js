// Initialize i18next
async function initI18n() {
    // Always detect browser language first
    const browserLang = navigator.language.split('-')[0];
    let defaultLang = ['en', 'fr', 'es'].includes(browserLang) ? browserLang : 'en';
    
    // Update saved preference to match browser language
    localStorage.setItem('language', defaultLang);

    // Load translation files
    const translations = {
        en: await fetch('locales/en.json').then(r => r.json()),
        fr: await fetch('locales/fr.json').then(r => r.json()),
        es: await fetch('locales/es.json').then(r => r.json())
    };

    // Initialize i18next
    i18next.init({
        lng: defaultLang,
        fallbackLng: 'en',
        resources: {
            en: { translation: translations.en },
            fr: { translation: translations.fr },
            es: { translation: translations.es }
        },
        interpolation: {
            escapeValue: false // React already escapes values
        }
    }, (err, t) => {
        if (err) {
            console.error('i18next initialization error:', err);
        }
        // Update UI with current language
        updateUILanguage();
        
        // Notify backend about initial language
        fetch(`http://localhost:5000/api/bot/language/${defaultLang}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' }
        })
        .then(res => res.json())
        .then(data => {
            console.log('Initial backend language set:', data);
        })
        .catch(err => {
            console.warn('Failed to set initial backend language:', err);
        });
    });

    return i18next;
}

// Simple translate function
function t(key, options = {}) {
    return i18next.t(key, options);
}

// Update all UI elements with current language
function updateUILanguage() {
    // Update all elements with data-i18n attribute
    const elements = document.querySelectorAll('[data-i18n]');
    elements.forEach(el => {
        const key = el.getAttribute('data-i18n');
        const text = t(key);
        el.textContent = text;
    });
    
    // Update placeholders
    const inputs = document.querySelectorAll('[data-i18n-placeholder]');
    inputs.forEach(el => {
        const key = el.getAttribute('data-i18n-placeholder');
        el.placeholder = t(key);
    });
}

// Change language
function changeLanguage(lang) {
    if (!['en', 'fr', 'es'].includes(lang)) return;
    
    localStorage.setItem('language', lang);
    i18next.changeLanguage(lang, (err, t) => {
        if (err) {
            console.error('Language change error:', err);
        } else {
            updateUILanguage();
            // Reload questions for new language
            if (typeof loadQuestions === 'function') {
                loadQuestions(lang);
            }
        }
    });
    
    // Notify backend about language change
    fetch(`http://localhost:5000/api/bot/language/${lang}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' }
    })
    .then(res => res.json())
    .then(data => {
        console.log('Backend language updated:', data);
    })
    .catch(err => {
        console.warn('Failed to update backend language:', err);
    });
    
    // Update active language button - ensure buttons exist first
    setTimeout(() => {
        document.querySelectorAll('.lang-btn').forEach(btn => btn.classList.remove('active'));
        const activeBtn = document.getElementById(`lang-${lang}`);
        if (activeBtn) activeBtn.classList.add('active');
    }, 0);
}

// Setup language button delegation (survives DOM updates)
function setupLanguageDelegation() {
    document.addEventListener('click', (e) => {
        const langBtn = e.target.closest('.lang-btn');
        if (langBtn) {
            const lang = langBtn.id.replace('lang-', '');
            changeLanguage(lang);
        }
    });
}

// Ensure language preference is maintained (auto-restore if it gets reset)
function ensureLanguagePersistence() {
    setInterval(() => {
        const savedLang = localStorage.getItem('language') || 'en';
        const currentLang = i18next.language;
        
        if (currentLang !== savedLang) {
            console.warn(`Language mismatch detected: saved=${savedLang}, current=${currentLang}. Restoring...`);
            i18next.changeLanguage(savedLang);
            updateUILanguage();
        }
        
        // Ensure correct button has active class
        const activeBtn = document.querySelector('.lang-btn.active');
        if (activeBtn) {
            const btnLang = activeBtn.id.replace('lang-', '');
            if (btnLang !== currentLang) {
                document.querySelectorAll('.lang-btn').forEach(btn => btn.classList.remove('active'));
                const correctBtn = document.getElementById(`lang-${currentLang}`);
                if (correctBtn) correctBtn.classList.add('active');
            }
        }
    }, 1000); // Check every 1 second
}

// Export for use in other files
if (typeof module !== 'undefined' && module.exports) {
    module.exports = { initI18n, t, updateUILanguage, changeLanguage };
}
