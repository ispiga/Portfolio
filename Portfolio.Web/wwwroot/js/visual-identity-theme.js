export function updateStylesheet(href, mainLogoUrl, footerLogoUrl) {
    const stylesheet = document.getElementById("visual-identity-theme-stylesheet");
    if (!stylesheet) {
        return false;
    }

    const resolvedHref = new URL(href, document.baseURI).href;
    if (stylesheet.href !== resolvedHref) {
        stylesheet.href = resolvedHref;
    }

    updateLogo("mainlogo", mainLogoUrl);
    updateLogo("footerlogo", footerLogoUrl);

    return true;
}

function updateLogo(kind, url) {
    if (!url) {
        return;
    }

    for (const image of document.querySelectorAll(`[data-visual-identity-asset="${kind}"]`)) {
        const resolvedUrl = new URL(url, document.baseURI).href;
        if (image.src !== resolvedUrl) {
            image.src = resolvedUrl;
        }
    }
}
