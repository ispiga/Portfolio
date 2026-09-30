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
- Keep public and admin layouts aligned with the shared semantic theme tokens, including MudBlazor surfaces and typography.

## Administration and Identity

- Do not add public user registration. Provision the initial administrator through `Portfolio.AdminProvisioning`; keep web Identity services for sign-in, sign-out and authorization only.
- Preserve the 15-minute administrator inactivity timeout, renew it only on authenticated activity, and keep HTTP cookie expiry validation and Blazor circuit revalidation aligned.
- Keep logout as an antiforgery-protected POST and expose it in both public and admin navigation when the administrator is authenticated; do not display the admin email in the public navbar.
- Keep theme and language selectors available in both layouts and accessible when their visible labels are omitted.
- In the Experience editor, preserve unsaved-change protection: use the localized application dialog for marked internal navigation and logout, and the browser-native `beforeunload`/navigation confirmation for document exits such as refresh, close and back. Do not call .NET through JS interop to show that dialog when the Interactive Server circuit may be disconnected; keep logout as an antiforgery-protected POST after confirmation.
- Preserve the Experience editor's existing save flow and unsaved-change behavior. Show its localized success message only after the experience and all pending attachment changes have saved successfully; do not report success when validation or persistence fails. Keep attachment display-name/visibility edits within unsaved-change tracking, ask for confirmation before deleting an attachment or publishing it, and do not trigger the document-exit warning as part of a successful save. Avoid `@bind-Value:event="oninput"` on the editor's `InputText`/`InputTextArea` controls; the existing binding pattern avoids a Blazor event-argument type mismatch in Interactive Server.
- Preserve the Projects editor's unsaved-change protection: use the localized application dialog for marked internal navigation and logout, and browser-native `beforeunload`/navigation confirmation for document exits such as refresh, close and back. Keep successful saves from triggering the exit warning and leave protection active after validation or persistence failures. Project preview image uploads/deletions persist immediately and must not activate unsaved-form warnings by themselves; cache-bust the preview after replacement.
- In the Projects editor, require a complete Spanish title/summary for public fallback; English may be omitted and its absence must be clearly localized. Keep localized project slugs optional and disabled in the editor until project detail pages are implemented; preserve any existing values and do not generate placeholder slugs. Preserve the public order and translation fallback. `IsFeatured` visually emphasizes the card without reordering projects; the card can expand its localized long description. Preview images are public JPG/JPEG/PNG files up to 10 MiB, stored outside `wwwroot` and SQL Server using the configurable `Portfolio:ProjectPreviewImages` options. The public preview URL must include a version based on its opaque stored filename so replacing an image updates the rendered card without a full page refresh. Do not add categories, tags, technologies or relationships unless the specification changes.

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