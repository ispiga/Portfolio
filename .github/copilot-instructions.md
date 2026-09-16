# Copilot Instructions

## Directrices del proyecto
- Para este proyecto, la tipografía debe utilizar siempre fuentes instaladas/locales del sistema o incluidas localmente; no se deben utilizar fuentes externas cargadas desde CDN o servicios remotos.
- La parte pública debe implementarse con Blazor Web App, Razor Components e Interactive Server únicamente cuando aporte interactividad. No se debe convertir la aplicación en una SPA pura ni duplicar la navegación mediante JavaScript.
- Todos los textos visibles de la interfaz deben utilizar los recursos compartidos mediante localización. Se debe mantener es-ES como cultura predeterminada, preparar en-US mediante recursos y evitar páginas duplicadas por idioma.
- El sistema de tema debe reutilizar exclusivamente la API portfolioTheme existente en wwwroot/js/theme.js mediante JS interop. No se debe duplicar la lógica de localStorage en C# ni crear una segunda API de temas.
- La parte pública debe utilizar Tailwind CSS local y wwwroot/css/app.css, aprovechando los tokens semánticos existentes. No se debe utilizar Bootstrap ni introducir colores acoplados a nombres concretos de una paleta.
- Los componentes públicos deben conservar HTML semántico, navegación completa por teclado, focus-visible, tamaños táctiles razonables, contraste suficiente y soporte para prefers-reduced-motion. Los atributos ARIA solo deben añadirse cuando sean necesarios.
- La interfaz pública debe mantenerse responsive desde el principio. Los menús móviles deben reflejar su estado con aria-expanded, poder abrirse y cerrarse mediante teclado y no perder el foco de forma inesperada.
