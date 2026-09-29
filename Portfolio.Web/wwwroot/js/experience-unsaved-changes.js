let enabled = false;
let form;
let dialog;
let pendingAction;
let dialogText;

const logoutFormIds = new Set(["admin-header-logout-form", "public-navbar-logout-form"]);

function beforeUnload(event) {
    if (!enabled) {
        return;
    }

    event.preventDefault();
    event.returnValue = "";
}

function trackFormChange() {
    if (form) {
        setEnabled(true);
    }
}

function closeDialog() {
    pendingAction = undefined;
    dialog?.remove();
    dialog = undefined;
}

function showDialog(action, title, message, stayText, discardText) {
    if (dialog) {
        return;
    }

    pendingAction = action;
    dialog = document.createElement("div");
    dialog.className = "session-expired-backdrop";
    dialog.innerHTML = '<section class="session-expired-dialog" role="alertdialog" aria-modal="true"><h2></h2><p></p><div class="experience-unsaved-dialog__actions"><button class="button button--secondary" type="button"></button><button class="button button--primary" type="button"></button></div></section>';
    dialog.querySelector("h2").textContent = title;
    dialog.querySelector("p").textContent = message;

    const [stayButton, discardButton] = dialog.querySelectorAll("button");
    stayButton.textContent = stayText;
    discardButton.textContent = discardText;
    stayButton.addEventListener("click", closeDialog);
    discardButton.addEventListener("click", () => {
        const actionToRun = pendingAction;
        closeDialog();
        setEnabled(false);
        if (actionToRun?.type === "navigate") {
            actionToRun.link.click();
        } else if (actionToRun?.type === "logout") {
            document.getElementById(actionToRun.formId)?.requestSubmit();
        }
    });
    dialog.addEventListener("keydown", event => {
        if (event.key === "Escape") {
            event.preventDefault();
            closeDialog();
            return;
        }

        if (event.key === "Tab") {
            event.preventDefault();
            (document.activeElement === stayButton ? discardButton : stayButton).focus();
        }
    });
    document.body.append(dialog);
    stayButton.focus();
}

function confirmInternalAction(event) {
    if (!enabled || event.button !== undefined && event.button !== 0 || event.metaKey || event.ctrlKey || event.shiftKey || event.altKey) {
        return;
    }

    const target = event.target instanceof Element ? event.target : null;
    const link = target?.closest("a[data-unsaved-navigation][href]");
    if (!link || link.target && link.target !== "_self" || link.hasAttribute("download")) {
        return;
    }

    const destination = new URL(link.href, window.location.href);
    if (destination.origin !== window.location.origin || destination.href === window.location.href) {
        return;
    }

    event.preventDefault();
    event.stopImmediatePropagation();
    showDialog({ type: "navigate", link }, ...dialogText);
}

function confirmLogout(event) {
    const form = event.target;
    if (!enabled || !(form instanceof HTMLFormElement) || !logoutFormIds.has(form.id)) {
        return;
    }

    event.preventDefault();
    event.stopImmediatePropagation();
    showDialog({ type: "logout", formId: form.id }, ...dialogText);
}

export function initialize(formId, title, message, stayText, discardText) {
    form = document.getElementById(formId);
    dialogText = [title, message, stayText, discardText];
    form?.addEventListener("input", trackFormChange);
    form?.addEventListener("change", trackFormChange);
    document.addEventListener("click", confirmInternalAction, true);
    document.addEventListener("submit", confirmLogout, true);
}

export function setEnabled(value) {
    const shouldEnable = Boolean(value);
    if (enabled === shouldEnable) {
        return;
    }

    enabled = shouldEnable;
    if (enabled) {
        window.addEventListener("beforeunload", beforeUnload);
    } else {
        window.removeEventListener("beforeunload", beforeUnload);
    }
}

export function dispose() {
    form?.removeEventListener("input", trackFormChange);
    form?.removeEventListener("change", trackFormChange);
    document.removeEventListener("click", confirmInternalAction, true);
    document.removeEventListener("submit", confirmLogout, true);
    dialog?.remove();
    form = undefined;
    dialog = undefined;
    pendingAction = undefined;
    dialogText = undefined;
    setEnabled(false);
}
