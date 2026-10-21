window.portfolioContact = {
    submit: async request => {
        const token = document.querySelector('input[name="__RequestVerificationToken"]')?.value;
        if (!token) {
            return { statusCode: 0, result: null };
        }

        try {
            const response = await fetch("/contact/submit", {
                method: "POST",
                credentials: "same-origin",
                headers: {
                    "Content-Type": "application/json",
                    "RequestVerificationToken": token
                },
                body: JSON.stringify(request)
            });
            const payload = await response.json().catch(() => null);
            return { statusCode: response.status, result: payload?.result ?? null };
        } catch {
            return { statusCode: 0, result: null };
        }
    }
};
