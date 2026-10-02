const editors = new Map();

function isDarkTheme() {
    const theme = window.portfolioTheme?.get() || "system";
    return theme === "dark" || theme === "system" && window.matchMedia("(prefers-color-scheme: dark)").matches;
}

function loadTinyMce() {
    if (window.tinymce) {
        return Promise.resolve();
    }

    return new Promise((resolve, reject) => {
        const script = document.createElement("script");
        script.src = "/js/vendor/tinymce/tinymce.min.js";
        script.onload = resolve;
        script.onerror = () => reject(new Error("TinyMCE failed to load."));
        document.head.append(script);
    });
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
        toolbar: "undo redo | blocks | bold italic underline | bullist numlist | link image table | alignleft aligncenter alignright | removeformat | code",
        height: 440,
        branding: false,
        promotion: false,
        convert_urls: false,
        automatic_uploads: true,
        images_upload_handler: async (blobInfo, progress) => {
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
            await entry.dotNetReference.invokeMethodAsync("ImageUploaded");
            return result.location;
        },
        setup: instance => {
            instance.on("init", () => instance.setContent(content || ""));
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

export async function initialize(id, initialContent, language, blogPostId, dotNetReference, saveArticleFirstMessage) {
    const element = document.getElementById(id);
    if (!element) {
        return;
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
        generation: 0,
        editor: null,
        disposed: false
    };
    editors.set(id, entry);

    try {
        await loadTinyMce();
        if (entry.disposed || editors.get(id) !== entry) {
            return;
        }

        const editor = await createEditor(entry, entry.content, entry.darkTheme);
        if (entry.disposed || editors.get(id) !== entry) {
            editor.remove();
            return;
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

export function getContent(id) {
    const entry = editors.get(id);
    return entry?.editor?.getContent() ?? entry?.content ?? document.getElementById(id)?.value ?? "";
}

export async function dispose(id) {
    const entry = editors.get(id);
    if (entry) {
        entry.disposed = true;
        entry.generation++;
        entry.themeObserver?.disconnect();
        entry.colorScheme?.removeEventListener("change", entry.colorSchemeListener);
        entry.editor?.remove();
        editors.delete(id);
    }
}
