# Copilot Instructions

## Project context

- Consult `../PORTFOLIO_PROJECT.md` when the task depends on project architecture, technology choices, conventions, routes, UX decisions or other project-level requirements.
- Treat `../PORTFOLIO_PROJECT.md` as the source of truth for project decisions.
- Do not introduce alternatives that conflict with the project specification without identifying the conflict first.
- Keep changes focused on the requested task. Do not modify unrelated projects, files or architecture.

## Architecture

- Use .NET 10, Blazor Web App and Razor Components.
- Use Interactive Server only where interactivity is required.
- Do not turn the public site into a SPA or duplicate navigation with JavaScript.
- Preserve the existing project dependency direction and separation of responsibilities.
- Keep technical identifiers in English. User-facing content is localized.

## Localization

- All user-visible UI text must use shared localization resources.
- `es-ES` is the default culture; prepare `en-US` through resources.
- Do not create duplicated pages or components per language.

## Public UI

- Use local Tailwind CSS and `wwwroot/css/app.css`.
- Reuse existing semantic design tokens.
- Do not use Bootstrap.
- Do not hard-code colors using palette-specific names.
- Use local/system fonts only. Do not load fonts from CDNs or remote services.
- Keep the UI responsive, semantic and accessible.
- Support keyboard navigation, `:focus-visible`, adequate touch targets, sufficient contrast and `prefers-reduced-motion`.
- Use ARIA only when semantically necessary.
- Mobile menus must expose their state with `aria-expanded`, support keyboard interaction and avoid unexpected focus loss.

## Theme

- Use only the existing `portfolioTheme` API in `wwwroot/js/theme.js` through JS interop.
- Do not duplicate theme or `localStorage` logic in C#.
- Do not create a second theme API.

## Implementation discipline

- Prefer simple solutions over unnecessary abstractions or patterns.
- Reuse existing components, services, styles and utilities before creating new ones.
- Before introducing a new dependency, library or architectural pattern, verify that it is consistent with `../PORTFOLIO_PROJECT.md`.
- Do not change established project decisions unless the task explicitly requires it.