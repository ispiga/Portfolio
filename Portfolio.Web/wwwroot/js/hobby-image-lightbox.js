let activeDialog;
let previousFocus;
let keydownHandler;

export function rememberFocus() {
    previousFocus = document.activeElement instanceof HTMLElement ? document.activeElement : null;
}

export function activate(dialog) {
    activeDialog = dialog;
    keydownHandler = event => {
        if (event.key !== "Tab") {
            return;
        }

        const focusable = dialog.querySelectorAll(
            'a[href], button:not([disabled]), input:not([disabled]), select:not([disabled]), textarea:not([disabled]), [tabindex]:not([tabindex="-1"])');
        if (focusable.length === 0) {
            event.preventDefault();
            dialog.focus();
            return;
        }

        const first = focusable[0];
        const last = focusable[focusable.length - 1];
        if (event.shiftKey && document.activeElement === first) {
            event.preventDefault();
            last.focus();
        } else if (!event.shiftKey && document.activeElement === last) {
            event.preventDefault();
            first.focus();
        }
    };

    dialog.addEventListener("keydown", keydownHandler);
}

export function deactivate() {
    if (activeDialog && keydownHandler) {
        activeDialog.removeEventListener("keydown", keydownHandler);
    }

    activeDialog = null;
    keydownHandler = null;
    if (previousFocus?.isConnected) {
        previousFocus.focus();
    }

    previousFocus = null;
}
