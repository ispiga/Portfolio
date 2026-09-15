(function () {
    const storageKey = "portfolio-theme";
    const themes = new Set(["system", "light", "dark"]);

    function readTheme() {
        try {
            const storedTheme = window.localStorage.getItem(storageKey);
            return themes.has(storedTheme) ? storedTheme : "system";
        } catch {
            return "system";
        }
    }

    function applyTheme(theme, persist) {
        const selectedTheme = themes.has(theme) ? theme : "system";
        document.documentElement.dataset.theme = selectedTheme;

        if (persist) {
            try {
                window.localStorage.setItem(storageKey, selectedTheme);
            } catch {
            }
        }

        return selectedTheme;
    }

    window.portfolioTheme = {
        get: function () {
            return readTheme();
        },
        set: function (theme) {
            return applyTheme(theme, true);
        },
        apply: function (theme) {
            return applyTheme(theme, false);
        },
        options: ["system", "light", "dark"]
    };

    applyTheme(readTheme(), false);
}());
