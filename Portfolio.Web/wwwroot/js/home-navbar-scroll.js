const headerStates = new WeakMap();

export function setHomeScrollTracking(header, isHome) {
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

    if (isHome && !state.isActive) {
        window.addEventListener("scroll", state.update, { passive: true });
        state.isActive = true;
        state.update();
    } else if (!isHome && state.isActive) {
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
