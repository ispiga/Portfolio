document.addEventListener("submit", async event => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.matches("[data-admin-login]")) {
        return;
    }

    event.preventDefault();

    const submitButton = form.querySelector('button[type="submit"]');
    const rateLimitMessage = document.getElementById("admin-login-rate-limit");
    if (submitButton) {
        submitButton.disabled = true;
    }

    try {
        const response = await fetch(form.action, {
            method: form.method,
            body: new FormData(form),
            credentials: "same-origin"
        });

        if (response.status === 429) {
            if (rateLimitMessage) {
                rateLimitMessage.hidden = false;
                rateLimitMessage.focus();
            }
            if (submitButton) {
                submitButton.disabled = false;
            }
            return;
        }

        if (response.redirected) {
            window.location.assign(response.url);
            return;
        }

        const contentType = response.headers.get("Content-Type") ?? "text/plain;charset=utf-8";
        const responseDocument = new Blob([await response.blob()], { type: contentType });
        window.location.assign(URL.createObjectURL(responseDocument));
    } catch {
        if (submitButton) {
            submitButton.disabled = false;
        }
    }
});
