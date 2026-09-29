const previews = new WeakMap();
let pdfjsPromise;

async function getPdfJs() {
    pdfjsPromise ??= import("./vendor/pdfjs/pdf.min.mjs").then(pdfjs => {
        pdfjs.GlobalWorkerOptions.workerSrc = new URL("./vendor/pdfjs/pdf.worker.min.mjs", import.meta.url).toString();
        return pdfjs;
    });
    return pdfjsPromise;
}

export async function renderFirstPage(url, canvas) {
    disposePreview(canvas);
    const pdfjs = await getPdfJs();
    const response = await fetch(url, { credentials: "same-origin" });
    if (!response.ok) {
        throw new Error(`Unable to load PDF preview: ${response.status}`);
    }

    const data = new Uint8Array(await response.arrayBuffer());
    const loadingTask = pdfjs.getDocument({
        data,
        cMapUrl: new URL("./vendor/pdfjs/cmaps/", import.meta.url).toString(),
        cMapPacked: true,
        standardFontDataUrl: new URL("./vendor/pdfjs/standard_fonts/", import.meta.url).toString()
    });
    previews.set(canvas, loadingTask);
    const document = await loadingTask.promise;
    const page = await document.getPage(1);
    const initialViewport = page.getViewport({ scale: 1 });
    const scale = Math.min(1.5, 360 / initialViewport.width);
    const viewport = page.getViewport({ scale });
    const outputScale = window.devicePixelRatio || 1;
    const context = canvas.getContext("2d", { alpha: false });

    canvas.width = Math.floor(viewport.width * outputScale);
    canvas.height = Math.floor(viewport.height * outputScale);
    canvas.style.aspectRatio = `${viewport.width} / ${viewport.height}`;

    await page.render({
        canvasContext: context,
        viewport,
        transform: outputScale !== 1 ? [outputScale, 0, 0, outputScale, 0, 0] : null
    }).promise;

    page.cleanup();
    document.cleanup();
}

export function disposePreview(canvas) {
    const loadingTask = previews.get(canvas);
    if (loadingTask) {
        loadingTask.destroy();
        previews.delete(canvas);
    }
}
