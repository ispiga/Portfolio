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

## Contact email

- Keep the public contact flow layered: `Portfolio.Web` ? application `ContactService` ? `IEmailService` in `Portfolio.Application` ? `MailKitEmailService` in `Portfolio.Infrastructure` ? SMTP.
- Do not reference MailKit, SMTP APIs or infrastructure services directly from a Blazor component.
- Use asynchronous email sending and await the result in the application flow; do not use fire-and-forget delivery for contact submissions.
- The current transport is Gmail SMTP through MailKit with OAuth 2.0. Keep the email provider and authentication strategy replaceable so the application can later use the SMTP server hosted on QNAP without changing the form or `IEmailService` contract.
- Keep sender, recipient, host, port, security mode, OAuth client values and refresh tokens in validated options supplied through User Secrets or environment variables; never use SMTP passwords, commit credentials or hard-code secrets in components or services.
- Use Gmail's OAuth 2.0 SMTP scope `https://mail.google.com/`; cache access tokens only in memory and never log client secrets, refresh tokens or access tokens.
- Never open, read, print or copy the contents of the user's `secrets.json` or User Secrets store. When documenting configuration, show only setting names and clearly fictitious placeholder values.
- Keep OAuth `ClientSecret` and `RefreshToken` out of tracked files, logs, generated examples containing real values, and chat responses.
- The current intended recipient is `mi-cuenta-personal@gmail.com`. The future QNAP configuration may use `contacto@midominio.com` as the sender; both addresses must remain configuration values.
- Do not add contact persistence, inbox history or an administration panel unless the project specification explicitly changes.

## Implementation discipline

- Prefer simple solutions over unnecessary abstractions or patterns.
- Reuse existing components, services, styles and utilities before creating new ones.
- Before introducing a new dependency, library or architectural pattern, verify that it is consistent with `../PORTFOLIO_PROJECT.md`.
- Do not change established project decisions unless the task explicitly requires it.