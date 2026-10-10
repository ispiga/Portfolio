export function createPreview(inputId) {
    const input = document.getElementById(inputId);
    const file = input.files?.[0];
    if (!file) {
        throw new Error("No image selected.");
    }

    return URL.createObjectURL(file);
}

export function revokePreview(url) {
    if (url) {
        URL.revokeObjectURL(url);
    }
}
