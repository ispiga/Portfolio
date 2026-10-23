document.addEventListener("submit", async event => {
    const form = event.target;
    if (!(form instanceof HTMLFormElement) || !form.matches("[data-admin-login]")) {
        return;
    }

    event.preventDefault();

    const submitButton = form.querySelector('button[type="submit"]');
    const rateLimitMessage = document.getElementById("admin-login-rate-limit");
    const requestErrorMessage = document.getElementById("admin-login-request-error");

    const showRequestError = () => {
        if (requestErrorMessage) {
            requestErrorMessage.hidden = false;
            requestErrorMessage.focus();
        }
        if (submitButton) {
            submitButton.disabled = false;
        }
    };

    const formData = new FormData(form);
    if (!formData.has("__RequestVerificationToken")) {
        window.location.assign("/admin/login?error=antiforgery");
        return;
    }

    if (submitButton) {
        submitButton.disabled = true;
    }

    try {
        const response = await fetch(form.action, {
            method: form.method,
            body: formData,
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

        if (response.status === 400) {
            window.location.assign("/admin/login?error=antiforgery");
            return;
        }

        if (response.status === 401) {
            window.location.assign("/admin/login?error=session-expired");
            return;
        }

        showRequestError();
    } catch {
        showRequestError();
    }
});
