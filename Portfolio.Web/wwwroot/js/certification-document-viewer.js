const dialogs = new WeakMap();

export function initialize(dialog, dotNetReference) {
    const handleClose = () => {
        void dotNetReference.invokeMethodAsync("OnDocumentDialogClosed").catch(() => {});
    };

    dialog.addEventListener("close", handleClose);
    dialogs.set(dialog, handleClose);
}

export function show(dialog) {
    if (!dialog.open) {
        dialog.showModal();
    }
}

export function close(dialog) {
    if (dialog.open) {
        dialog.close();
    }
}

export function dispose(dialog) {
    const handleClose = dialogs.get(dialog);
    if (handleClose) {
        dialog.removeEventListener("close", handleClose);
        dialogs.delete(dialog);
    }
}
