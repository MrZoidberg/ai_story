export function transformLocale(locale) {
    switch (locale.toLowerCase()) {
        case "en":
            return "en";
        case "ru":
            return "ru";
        case "ua":
            return "uk";
        default:
            return "en";
    }
}