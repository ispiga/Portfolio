# Copilot Instructions

## Project context

- Consult `Portfolio\PORTFOLIO_PROJECT.md` when the task depends on project architecture, technology choices, conventions, routes, UX decisions or other project-level requirements. Treat `Portfolio\PORTFOLIO_PROJECT.md` as the source of truth for project decisions.
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
- Keep the public UI responsive, semantic and accessible.
- Support keyboard navigation, `:focus-visible`, adequate touch targets, sufficient contrast and `prefers-reduced-motion`.
- Use ARIA only when semantically necessary.
- Mobile menus must expose their state with `aria-expanded`, support keyboard interaction and avoid unexpected focus loss.
- The sticky/translucent navbar treatment is currently Home-only: `MainLayout` applies it to the outer `header` only when the Home hero is present. `home-navbar-scroll.js` attaches one passive scroll listener only on Home, toggles `is-scrolled` when `window.scrollY > 20`, and removes the listener when leaving Home or disposing the layout. CSS uses `--color-surface` at 85% alpha with a 4px backdrop blur; an expanded mobile menu stays opaque, and transitions respect `prefers-reduced-motion`. When applying this pattern to another screen, opt in that layout explicitly, target its full header surface, preserve opaque expanded menus and anchor offsets, and clean up listeners on route changes/disposal. Do not globalize the treatment to shared navbars or use JavaScript to replace Blazor navigation.
- The shared wide `.portfolio-shell` on `site-main` and navbar content is scoped to Home, the public certifications archive, and public Blog archive/detail. Preserve its `120rem` maximum width, fluid side inset (`clamp(1rem, 4vw, 4rem)`, increasing from `1280px` to `clamp(4.8rem, 6vw, 7.5rem)`); do not extend it to other public routes or change the scope unless explicitly requested. In “Sobre mí”, preserve the separate profile/skills zones and the current CSS Columns layout for dynamic, naturally sized skill cards; use one column below `28rem` and two columns from `28rem` through `47.999rem`, retaining the existing tablet/desktop behavior. Do not reintroduce JavaScript masonry or change card spacing, width, corner cuts, or desktop distribution unless explicitly requested. Skill groups must remain in the administration-defined DOM order and independent of category names/count. Optional group background colors tint the card gradient and decorative border; skill chips keep the semantic theme surface. Preserve the default theme gradient when no color is assigned.
- The public certifications Home section shows the first six records in manual query order and links to `/certifications` when more exist; the archive shows all records ordered by issue date descending, null dates last, then manual order and ID. Both views reuse `CertificationCard` and the same responsive 3/2/1-column grid breakpoints and attachment-preview grid; do not add archive-only horizontal scrolling or duplicate card markup. The archive return link targets `/#certificates` using the existing Blazor enhanced navigation and smooth-scroll behavior; do not disable enhanced navigation or globally change scroll behavior to work around the observed transition unless explicitly requested.
- In the public Experience timeline, show the localized expand/collapse control only when the four-line clamp actually hides description content. Detect real DOM overflow and keep the result responsive to viewport/element resizing; do not approximate it from text length.
- In Hero Technology Orbit, preserve the accepted composition of two SVG arc layers around the photo and the current 3D depth of logos. Keep shared track styling in `.technology-orbit__track` (`fill: none`, semantic stroke color, `0.5px`, `butt`) and do not change orbit geometry or animation parameters. The occasional line-over-badge appearance is accepted; do not add dynamic masks or further z-index changes to suppress it unless explicitly requested.

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
- Keep the administration shell and all admin routes responsive at mobile widths without horizontal overflow. Reuse `wwwroot/css/app.css`, retain usable touch targets, and let MudBlazor tables, form controls, attachment previews and action buttons adapt to narrow screens.
- File-storage services must create missing configured directories and parent folders when preparing an upload; do not require operators to pre-create them. Log filesystem deletion failures. After successful entity deletion, remove only that entity's GUID directories and only when empty, without recursive deletion or escaping the configured storage root. Certification cleanup removes empty `attachments`, `card` and certification GUID directories; Experience and Blog cleanup removes only the corresponding empty GUID directory. Project previews live directly in shared `ProjectPreviewImages`: never try to remove that shared root when deleting a project. Keep every `App_Data` directory ignored in Git (`**/App_Data/`).
- Certification attachments must be private when uploaded; require explicit confirmation before publishing, include visibility changes in unsaved-change tracking, serve public reads only for published attachments and protect administrative previews with `AdministratorOnly`. Keep the certification card image public. Preserve legacy certification attachment access in the visibility migration and do not apply generated migrations automatically.
- In the Experience editor, preserve unsaved-change protection: use the localized application dialog for marked internal navigation and logout, and the browser-native `beforeunload`/navigation confirmation for document exits such as refresh, close and back. Do not call .NET through JS interop to show that dialog when the Interactive Server circuit may be disconnected; keep logout as an antiforgery-protected POST after confirmation.
- In the Hero editor, preserve unsaved-change protection using the shared localized dialog and native document-exit confirmation. The snapshot must include headlines, orbit count, profile image metadata and every logo field; keep protection active during pending uploads and after failed saves, disable it only after success, and clean up temporary uploads on discard. Keep the profile-image form responsive, with the file selector beside its preview and a compact delete action. New logos start at display order `0`, stay first until both orbit and display order are assigned, then sort by orbit and display order.
- In the About editor, preserve unsaved-change protection using the shared localized dialog and native document-exit confirmation. Include the profile, all groups and skills, each group's optional background color, hobbies, orders and image metadata in change tracking; protect pending hobby uploads and clean them up on discard. The group color picker and `#RRGGBB` text field stay synchronized and validate before save; Reset clears the optional override and restores the theme gradient. Keep the `#20314A` value only as the unassigned selector preview, not as a value silently saved to existing groups. Apply custom colors to the group card only; skill chips retain their semantic surface background. Disable protection only after successful save and keep it active after failures. New groups, skills and hobbies start at display order `0`, stay first until their order is edited, then sort by `DisplayOrder`; when multiple entries are pending in one list, show the newest first. Keep generated migration `20261013202053_AddSkillGroupBackgroundColor` unapplied automatically.
- Preserve the Experience editor's existing save flow and unsaved-change behavior. Show its localized success message only after the experience and all pending attachment changes have saved successfully; do not report success when validation or persistence fails. Keep attachment display-name/visibility edits within unsaved-change tracking, ask for confirmation before deleting an attachment or publishing it, and do not trigger the document-exit warning as part of a successful save. Avoid `@bind-Value:event="oninput"` on the editor's `InputText`/`InputTextArea` controls; the existing binding pattern avoids a Blazor event-argument type mismatch in Interactive Server.
- Preserve the Projects editor's unsaved-change protection: use the localized application dialog for marked internal navigation and logout, and browser-native `beforeunload`/navigation confirmation for document exits such as refresh, close and back. Keep successful saves from triggering the exit warning and leave protection active after validation or persistence failures. Project preview image uploads/deletions persist immediately and must not activate unsaved-form warnings by themselves; cache-bust the preview after replacement.
- In the Projects editor, require a complete Spanish title/summary for public fallback; English may be omitted and its absence must be clearly localized. Keep localized project slugs optional and disabled in the editor until project detail pages are implemented; preserve any existing values and do not generate placeholder slugs. Preserve the public order and translation fallback. `IsFeatured` visually emphasizes the card without reordering projects; the card can expand its localized long description. Preview images are public JPG/JPEG/PNG files up to 10 MiB, stored outside `wwwroot` and SQL Server using the configurable `Portfolio:ProjectPreviewImages` options. The public preview URL must include a version based on its opaque stored filename so replacing an image updates the rendered card without a full page refresh. Do not add categories, tags, technologies or relationships unless the specification changes.
- In Blog administration, keep the editorial state explicit (`Draft`, `ReadyToPublish`, `Published`) and separate from translation completeness; preserve the existing published-post Home selection and Spanish fallback. Use local TinyMCE 8 only for `BlogPostTranslation.Content`. Blog images are JPG/JPEG/PNG/WebP/SVG up to 10 MiB, stored outside `wwwroot` and SQL Server per article with opaque keys; validate content and serve administrative previews only to administrators. Upload through authenticated multipart HTTP with antiforgery, not base64 JS interop. Public image reads must require a published post with a non-future publication date. Keep the generated Blog administration migration unapplied automatically.
  - Preserve the public blog routes and selection: Home shows up to three unique eligible posts, featured first then recent; `/blog` lists all eligible posts newest first while only visually marking featured posts; `/blog/{slug}` uses one reusable detail page. Resolve the active-culture slug first and fall back to the Spanish slug/content only when that translation is missing. Exclude drafts and future publications from Home, archive, and detail.
  - El listado admin muestra �Art�culo destacado� despu�s del estado editorial, y sus valores deben usar recursos propios del blog (`BlogPostFeatured` / `BlogPostNotFeatured`), no claves de proyectos.
  - Mantener el espaciado vertical del bloque de subida/lista de im�genes del blog entre selector y tarjetas y entre cada tarjeta.
  - La inicializaci�n TinyMCE debe evitar llamadas duplicadas y manejar disposal concurrente; filtros de diagn�stico SignalR/circuitos solo en Development.

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
- Before introducing a new dependency, library or architectural pattern, verify that it is consistent with `Portfolio\PORTFOLIO_PROJECT.md`.
- Do not change established project decisions unless the task explicitly requires it.
