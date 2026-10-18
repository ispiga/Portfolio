const headerStates = new WeakMap();

export function setNavbarScrollTracking(header, isEnabled) {
    if (!header) {
        return;
    }

    let state = headerStates.get(header);
    if (!state) {
        state = {
            isActive: false,
            update: () => {
                const isScrolled = window.scrollY > 20;
                if (header.classList.contains("is-scrolled") !== isScrolled) {
                    header.classList.toggle("is-scrolled", isScrolled);
                }
            }
        };
        headerStates.set(header, state);
    }

    if (isEnabled && !state.isActive) {
        window.addEventListener("scroll", state.update, { passive: true });
        state.isActive = true;
        state.update();
    } else if (!isEnabled && state.isActive) {
        window.removeEventListener("scroll", state.update);
        state.isActive = false;
        header.classList.remove("is-scrolled");
    }
}

export function dispose(header) {
    const state = headerStates.get(header);
    if (state?.isActive) {
        window.removeEventListener("scroll", state.update);
    }

    header?.classList.remove("is-scrolled");
    headerStates.delete(header);
}

export function scrollToSection(id) {
    let attempts = 0;
    const scrollWhenAvailable = () => {
        const target = document.getElementById(id);
        if (target) {
            target.scrollIntoView({ behavior: "auto", block: "start" });
            return;
        }

        if (++attempts < 120) {
            requestAnimationFrame(scrollWhenAvailable);
        }
    };

    requestAnimationFrame(scrollWhenAvailable);
}

export function scrollToTop() {
    window.scrollTo({ top: 0, behavior: "auto" });
}
