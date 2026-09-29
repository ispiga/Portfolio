let form;
let lastActivity = 0;
let submitTimer;
const minimumInterval = 30000;
const activityEvents = ["pointerdown", "keydown", "input", "scroll", "touchstart"];

function reportActivity() {
    const now = Date.now();
    if (now - lastActivity < minimumInterval || submitTimer) {
        return;
    }

    submitTimer = window.setTimeout(() => {
        submitTimer = undefined;
        if (!form) {
            return;
        }

        lastActivity = Date.now();
        fetch(form.action, {
            method: form.method,
            body: new FormData(form),
            credentials: "same-origin"
        }).catch(() => {});
    }, 500);
}

export function startMonitoring(formId) {
    form = document.getElementById(formId);
    if (!form) {
        return;
    }

    activityEvents.forEach(eventName => document.addEventListener(eventName, reportActivity, { passive: true }));
}

export function stopMonitoring() {
    activityEvents.forEach(eventName => document.removeEventListener(eventName, reportActivity));
    window.clearTimeout(submitTimer);
    submitTimer = undefined;
    form = undefined;
}
