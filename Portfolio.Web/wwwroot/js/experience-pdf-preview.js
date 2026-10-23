const previews = new WeakMap();
const pdfDocuments = new Map();
const observedCanvases = new Set();
let pdfjsPromise;
let previewObserver;

const previewRootMargin = "320px 0px";

async function getPdfJs() {
    pdfjsPromise ??= import("./vendor/pdfjs/pdf.min.mjs")
        .then(pdfjs => {
            pdfjs.GlobalWorkerOptions.workerSrc = new URL("./vendor/pdfjs/pdf.worker.min.mjs", import.meta.url).toString();
            return pdfjs;
        })
        .catch(error => {
            pdfjsPromise = undefined;
            throw error;
        });
    return pdfjsPromise;
}

export async function renderFirstPage(url, canvas) {
    disposePreview(canvas);
    const state = createPreviewState(url, canvas, null);
    previews.set(canvas, state);
    await renderPreview(state);
}

export function observeFirstPage(url, canvas, dotNetReference) {
    const current = previews.get(canvas);
    if (current?.url === url && !current.controller.signal.aborted) {
        return;
    }

    disposePreview(canvas);
    const state = createPreviewState(url, canvas, dotNetReference);
    previews.set(canvas, state);

    try {
        const observer = getPreviewObserver();
        if (observer) {
            observedCanvases.add(canvas);
            observer.observe(canvas);
        } else {
            void renderPreview(state);
        }
    } catch (error) {
        disposePreview(canvas);
        throw error;
    }
}

export function disposePreview(canvas) {
    const state = previews.get(canvas);
    if (!state) {
        return;
    }

    previews.delete(canvas);
    state.controller.abort();
    stopObserving(canvas);

    try {
        state.renderTask?.cancel();
    } catch (error) {
        console.debug("PDF preview render cancellation was already complete.", error);
    }

    releasePdfDocument(state);
}

function createPreviewState(url, canvas, dotNetReference) {
    return {
        url,
        canvas,
        dotNetReference,
        controller: new AbortController(),
        documentEntry: null,
        renderTask: null,
        started: false
    };
}

function getPreviewObserver() {
    if (previewObserver || typeof IntersectionObserver === "undefined") {
        return previewObserver;
    }

    previewObserver = new IntersectionObserver(entries => {
        for (const entry of entries) {
            if (!entry.isIntersecting) {
                continue;
            }

            const state = previews.get(entry.target);
            stopObserving(entry.target);
            if (state) {
                void renderPreview(state);
            }
        }
    }, { rootMargin: previewRootMargin });

    return previewObserver;
}

function stopObserving(canvas) {
    previewObserver?.unobserve(canvas);
    observedCanvases.delete(canvas);
    if (observedCanvases.size === 0 && previewObserver) {
        previewObserver.disconnect();
        previewObserver = null;
    }
}

function isCurrent(state) {
    return previews.get(state.canvas) === state && !state.controller.signal.aborted;
}

async function notifyDotNet(state, method, ...args) {
    if (!state.dotNetReference || !isCurrent(state)) {
        return;
    }

    try {
        await state.dotNetReference.invokeMethodAsync(method, ...args);
    } catch (error) {
        if (isCurrent(state)) {
            console.error(`PDF preview callback ${method} failed while its component was active.`, error);
        } else if (!state.controller.signal.aborted) {
            console.debug(`PDF preview callback ${method} could not reach its component.`, error);
        }
    }
}

function acquirePdfDocument(state) {
    let entry = pdfDocuments.get(state.url);
    if (!entry) {
        const controller = new AbortController();
        entry = {
            controller,
            consumers: new Set(),
            loadingTask: null,
            document: null,
            promise: null
        };
        pdfDocuments.set(state.url, entry);
        entry.promise = loadPdfDocument(state.url, entry).catch(error => {
            if (pdfDocuments.get(state.url) === entry) {
                pdfDocuments.delete(state.url);
            }
            throw error;
        });
    }

    entry.consumers.add(state);
    state.documentEntry = entry;
    return entry.promise;
}

async function loadPdfDocument(url, entry) {
    const pdfjs = await getPdfJs();
    if (entry.consumers.size === 0) {
        throw createAbortError();
    }

    const response = await fetch(url, {
        credentials: "same-origin",
        signal: entry.controller.signal
    });
    if (!response.ok) {
        throw new Error(`Unable to load PDF preview: ${response.status}`);
    }

    const buffer = await response.arrayBuffer();
    if (entry.consumers.size === 0 || entry.controller.signal.aborted) {
        throw createAbortError();
    }

    entry.loadingTask = pdfjs.getDocument({
        data: new Uint8Array(buffer),
        cMapUrl: new URL("./vendor/pdfjs/cmaps/", import.meta.url).toString(),
        cMapPacked: true,
        standardFontDataUrl: new URL("./vendor/pdfjs/standard_fonts/", import.meta.url).toString()
    });
    entry.document = await entry.loadingTask.promise;
    return entry.document;
}

function releasePdfDocument(state) {
    const entry = state.documentEntry;
    if (!entry) {
        return;
    }

    state.documentEntry = null;
    entry.consumers.delete(state);
    if (entry.consumers.size === 0) {
        entry.controller.abort();
        if (pdfDocuments.get(state.url) === entry) {
            pdfDocuments.delete(state.url);
        }
        destroyPdfDocument(entry);
    }
}

function destroyPdfDocument(entry) {
    try {
        entry.document?.cleanup();
    } catch (error) {
        console.debug("PDF document cleanup failed.", error);
    }

    const loadingTask = entry.loadingTask;
    entry.loadingTask = null;
    entry.document = null;
    if (!loadingTask) {
        return;
    }

    try {
        void Promise.resolve(loadingTask.destroy()).catch(error => {
            if (error?.name !== "AbortException" && error?.name !== "RenderingCancelledException") {
                console.debug("PDF preview loading task cleanup failed.", error);
            }
        });
    } catch (error) {
        console.debug("PDF preview loading task cleanup failed.", error);
    }
}

function createAbortError() {
    const error = new Error("The PDF preview operation was cancelled.");
    error.name = "AbortError";
    return error;
}

async function renderPreview(state) {
    if (!isCurrent(state) || state.started) {
        return;
    }
    state.started = true;

    let document;
    let page;
    try {
        await notifyDotNet(state, "OnPdfPreviewLoading");
        if (!isCurrent(state)) {
            return;
        }

        document = await acquirePdfDocument(state);
        if (!isCurrent(state)) {
            return;
        }

        page = await document.getPage(1);
        if (!isCurrent(state)) {
            return;
        }

        const initialViewport = page.getViewport({ scale: 1 });
        const scale = Math.min(1.5, 360 / initialViewport.width);
        const viewport = page.getViewport({ scale });
        const outputScale = window.devicePixelRatio || 1;
        const context = state.canvas.getContext("2d", { alpha: false });

        state.canvas.width = Math.floor(viewport.width * outputScale);
        state.canvas.height = Math.floor(viewport.height * outputScale);
        state.canvas.style.aspectRatio = `${viewport.width} / ${viewport.height}`;

        const renderTask = page.render({
            canvasContext: context,
            viewport,
            transform: outputScale !== 1 ? [outputScale, 0, 0, outputScale, 0, 0] : null
        });
        state.renderTask = renderTask;
        await renderTask.promise;

        if (isCurrent(state)) {
            await notifyDotNet(state, "OnPdfPreviewLoaded");
        }
    } catch (error) {
        if (!isCurrent(state) || error?.name === "AbortError") {
            return;
        }

        await notifyDotNet(state, "OnPdfPreviewFailed", error?.name ?? "Error");
    } finally {
        state.renderTask = null;
        try {
            page?.cleanup();
        } catch (error) {
            if (isCurrent(state)) {
                console.debug("PDF preview page cleanup failed.", error);
            }
        }
        releasePdfDocument(state);
    }
}
