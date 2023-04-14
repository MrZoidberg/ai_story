export const DEFAULT_LANG = 'en';
export const SUPPORTED_LANGS = ["en", "ru", "ua"];


export const setLanguage = (lang) => {
    console.log(`setLanguage: ${lang}`);
    if (typeof window !== 'undefined') {
        localStorage.setItem('lang', lang);
    }
}

export const getLanguage = () => {
    if (typeof window !== 'undefined') {
        return localStorage.getItem('lang') ?? DEFAULT_LANG;
    }

    return DEFAULT_LANG;
}