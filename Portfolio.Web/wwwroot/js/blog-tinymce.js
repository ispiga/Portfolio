const editors = new Map();
let tinyMceLoadPromise;
const blogImageMimeType = "application/x-portfolio-blog-image";

function isDarkTheme() {
    const theme = window.portfolioTheme?.get() || "system";
    return theme === "dark" || theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches;
}

function loadTinyMce() {
    if (window.tinymce) {
        return Promise.resolve();
    }

    if (!tinyMceLoadPromise) {
        tinyMceLoadPromise = new Promise((resolve, reject) => {
            const script = document.createElement("script");
            script.src = "/js/vendor/tinymce/tinymce.min.js";
            script.onload = resolve;
            script.onerror = () => {
                script.remove();
                tinyMceLoadPromise = null;
                reject(new Error("TinyMCE failed to load."));
            };
            document.head.append(script);
        });
    }

    return tinyMceLoadPromise;
}

function containsImageFile(event) {
    const transfer = event.dataTransfer ?? event.clipboardData;
    return Array.from(transfer?.files ?? []).some(file =>
        file.type.startsWith("image/") || /\.(jpe?g|png|webp|svg)$/i.test(file.name));
}

async function uploadImage(entry, blobInfo, progress) {
    if (entry.disposed) {
        throw new Error("The editor has been disposed.");
    }

    if (!entry.blogPostId) {
        throw new Error(entry.saveArticleFirstMessage);
    }

    const token = document.querySelector('input[name="__RequestVerificationToken"]');
    if (!token) {
        throw new Error("The antiforgery token is unavailable.");
    }

    progress(10);
    const formData = new FormData();
    formData.append(token.name, token.value);
    formData.append("file", blobInfo.blob(), blobInfo.filename());
    const response = await fetch(`/admin/blog-post-images/${entry.blogPostId}`, {
        method: "POST",
        body: formData,
        credentials: "same-origin"
    });
    const result = await response.json().catch(() => null);
    if (!response.ok || !result?.location) {
        throw new Error(result?.error || "Image upload failed.");
    }

    progress(100);
    if (entry.disposed) {
        throw new Error("The editor has been disposed.");
    }

    setTimeout(() => {
        if (entry.disposed) {
            return;
        }

        entry.dotNetReference.invokeMethodAsync("ImageUploaded").catch(error => {
            console.error("The uploaded image could not be added to the article image list.", error);
        });
    }, 0);

    return result.location;
}

async function createEditor(entry, content, darkTheme) {
    const editor = await window.tinymce.init({
        target: entry.element,
        base_url: "/js/vendor/tinymce",
        suffix: ".min",
        license_key: "gpl",
        skin: darkTheme ? "oxide-dark" : "oxide",
        content_css: darkTheme ? "dark" : "default",
        language: entry.language === "es" ? "es" : "en",
        language_url: entry.language === "es" ? "/js/vendor/tinymce/langs8/es.js" : undefined,
        menubar: false,
        plugins: "image link lists table code",
        toolbar: `undo redo | blocks | bold italic underline | bullist numlist | link ${entry.blogPostId ? "image " : ""}table | alignleft aligncenter alignright | removeformat | code`,
        toolbar_mode: "sliding",
        width: "100%",
        height: 440,
        branding: false,
        promotion: false,
        convert_urls: false,
        automatic_uploads: true,
        images_upload_handler: (blobInfo, progress) => {
            const upload = uploadImage(entry, blobInfo, progress);
            entry.pendingUploads.add(upload);
            upload.then(
                () => entry.pendingUploads.delete(upload),
                () => entry.pendingUploads.delete(upload));
            return upload;
        },
        setup: instance => {
            entry.galleryDragStart ??= event => {
                const image = event.target.closest?.("img[data-blog-post-image-id]");
                const imageId = image?.dataset.blogPostImageId;
                if (!imageId || !/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(imageId)) {
                    return;
                }

                event.dataTransfer?.setData(blogImageMimeType, imageId);
                event.dataTransfer?.setData("text/uri-list", `${location.origin}/admin/blog-post-images/${imageId}`);
                event.dataTransfer?.setData("text/html", `<img src="/admin/blog-post-images/${imageId}">`);
                if (event.dataTransfer) {
                    event.dataTransfer.effectAllowed = "copy";
                }
            };
            document.addEventListener("dragstart", entry.galleryDragStart);

            if (!entry.blogPostId) {
                instance.on("drop paste", event => {
                    if (!containsImageFile(event)) {
                        return;
                    }

                    event.preventDefault();
                    event.stopPropagation();
                    instance.notificationManager.open({
                        text: entry.saveArticleFirstMessage,
                        type: "warning"
                    });
                });
            }

            instance.on("init", () => {
                instance.setContent(content || "");
                instance.getBody().addEventListener("drop", event => {
                    const imageId = event.dataTransfer?.getData(blogImageMimeType);
                    if (!imageId || !/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(imageId)) {
                        return;
                    }

                    event.preventDefault();
                    event.stopImmediatePropagation();
                    instance.insertContent(`<p><img src="/admin/blog-post-images/${imageId}"></p>`);
                }, true);
            });
            instance.on("input change undo redo", async () => {
                if (entry.disposed) {
                    return;
                }

                const content = instance.getContent();
                entry.content = content;
                await entry.dotNetReference.invokeMethodAsync("ContentChanged", content);
                entry.element.dispatchEvent(new Event("input", { bubbles: true }));
            });
        }
    });
    return editor[0];
}

async function restartEditor(entry) {
    const darkTheme = isDarkTheme();
    if (entry.darkTheme === darkTheme || entry.disposed) {
        return;
    }

    entry.darkTheme = darkTheme;
    const generation = ++entry.generation;
    if (entry.editor) {
        entry.content = entry.editor.getContent();
        entry.editor.remove();
        entry.editor = null;
    }

    const editor = await createEditor(entry, entry.content, darkTheme);
    if (entry.disposed || generation !== entry.generation) {
        editor.remove();
        return;
    }

    entry.editor = editor;
}

async function initializeEditor(entry) {
    const { id } = entry;

    try {
        await loadTinyMce();
        if (entry.disposed || editors.get(id) !== entry) {
            return;
        }

        const editor = await createEditor(entry, entry.content, entry.darkTheme);
        if (entry.disposed || editors.get(id) !== entry) {
            editor?.remove();
            return;
        }
        if (!editor) {
            throw new Error(`TinyMCE did not create an editor for ${id}.`);
        }

        entry.editor = editor;
        entry.themeObserver = new MutationObserver(() => restartEditor(entry));
        entry.themeObserver.observe(document.documentElement, { attributes: true, attributeFilter: ["data-theme"] });
        entry.colorScheme = window.matchMedia("(prefers-color-scheme: dark)");
        entry.colorSchemeListener = () => {
            if ((window.portfolioTheme?.get() || "system") === "system") {
                restartEditor(entry);
            }
        };
        entry.colorScheme.addEventListener("change", entry.colorSchemeListener);
    } catch (error) {
        if (editors.get(id) === entry) {
            editors.delete(id);
        }
        throw error;
    }
}

export function initialize(id, initialContent, language, blogPostId, dotNetReference, saveArticleFirstMessage) {
    const element = document.getElementById(id);
    if (!element) {
        return Promise.resolve();
    }

    const currentEntry = editors.get(id);
    if (currentEntry && !currentEntry.disposed && currentEntry.element === element) {
        return currentEntry.initializationPromise ?? Promise.resolve();
    }
    if (currentEntry) {
        removeEditor(currentEntry);
    }

    const entry = {
        id,
        element,
        language,
        blogPostId,
        dotNetReference,
        saveArticleFirstMessage,
        content: initialContent || "",
        darkTheme: isDarkTheme(),
        pendingUploads: new Set(),
        generation: 0,
        editor: null,
        disposed: false
    };
    editors.set(id, entry);
    entry.initializationPromise = initializeEditor(entry);
    return entry.initializationPromise;
}

function removeEditor(entry) {
    entry.disposed = true;
    entry.generation++;
    entry.themeObserver?.disconnect();
    entry.colorScheme?.removeEventListener("change", entry.colorSchemeListener);
    if (entry.galleryDragStart) {
        document.removeEventListener("dragstart", entry.galleryDragStart);
    }
    entry.editor?.remove();
    if (editors.get(entry.id) === entry) {
        editors.delete(entry.id);
    }
}

export async function getContent(id) {
    const entry = editors.get(id);
    if (!entry?.editor) {
        return entry?.content ?? document.getElementById(id)?.value ?? "";
    }

    await Promise.all(entry.pendingUploads);
    await entry.editor.uploadImages();
    await Promise.all(entry.pendingUploads);
    return entry.editor.getContent();
}

export async function dispose(id) {
    const entry = editors.get(id);
    if (entry) {
        removeEditor(entry);
    }
}
