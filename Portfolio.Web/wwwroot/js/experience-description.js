const observers = new WeakMap();

export function observeDescriptions(root, dotNetReference) {
    stopObservingDescriptions(root);

    const descriptions = [...root.querySelectorAll(".experience-card__description")];
    let previousIds = "";
    const notify = () => {
        const truncatedIds = descriptions
            .filter(description => {
                const wasExpanded = description.classList.contains("is-expanded");
                if (wasExpanded) {
                    description.classList.remove("is-expanded");
                }

                const isTruncated = description.scrollHeight > description.clientHeight + 1;

                if (wasExpanded) {
                    description.classList.add("is-expanded");
                }

                return isTruncated;
            })
            .map(description => description.dataset.experienceId);
        const currentIds = truncatedIds.join(",");

        if (currentIds === previousIds) {
            return;
        }

        previousIds = currentIds;
        dotNetReference.invokeMethodAsync("UpdateTruncatedDescriptions", truncatedIds).catch(() => {});
    };

    const observer = new ResizeObserver(notify);
    descriptions.forEach(description => observer.observe(description));
    observers.set(root, observer);
    notify();
}

export function stopObservingDescriptions(root) {
    observers.get(root)?.disconnect();
    observers.delete(root);
}
