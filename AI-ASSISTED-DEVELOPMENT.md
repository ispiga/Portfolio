# ***************************************************************
# PROMPT INICIAL PARA GENERAR LA ESTRUCTURA DEL PROYECTO (FASE 1)
# ***************************************************************

He adjuntado PORTFOLIO_PROJECT.md. Léelo antes de realizar cambios. Es la especificación global del proyecto y debe utilizarse como contexto. Sin embargo, esta petición corresponde exclusivamente a la Fase 1 y no debes implementar ninguna funcionalidad perteneciente a fases posteriores.

Quiero iniciar desde cero un proyecto profesional de portfolio personal utilizando .NET 10 y Blazor Web App.

En esta primera fase NO quiero implementar todavía funcionalidades de negocio, base de datos, Entity Framework Core, Identity, Tailwind CSS, MudBlazor, TinyMCE ni el diseño visual definitivo. El objetivo es únicamente crear correctamente la estructura base de la solución para poder desarrollarla posteriormente por fases.

## Objetivo de esta fase

Crear una solución llamada:

Portfolio.sln

Con los siguientes proyectos:

* Portfolio.Web
* Portfolio.Application
* Portfolio.Domain
* Portfolio.Infrastructure
* Portfolio.Tests

La arquitectura debe seguir una separación clara de responsabilidades y una dirección de dependencias coherente.

## Arquitectura

La dependencia conceptual debe ser:

Portfolio.Web
↓
Portfolio.Application
↓
Portfolio.Domain
↑
Portfolio.Infrastructure

Es decir:

* Portfolio.Web puede depender de Portfolio.Application.
* Portfolio.Application puede depender de Portfolio.Domain.
* Portfolio.Infrastructure puede depender de Portfolio.Application y Portfolio.Domain.
* Portfolio.Domain NO debe depender de ningún otro proyecto de la solución.
* Portfolio.Application NO debe depender de Infrastructure ni de Web.
* Portfolio.Tests podrá referenciar los proyectos que sean necesarios para realizar pruebas posteriormente.

Evita crear patrones o abstracciones innecesarias. No quiero repositories, unit of work ni interfaces genéricas simplemente por seguir patrones. La arquitectura debe mantenerse sencilla y preparada para crecer.

## Responsabilidad de cada proyecto

### Portfolio.Domain

Debe contener únicamente elementos propios del dominio de la aplicación:

* Entities
* Value Objects si fueran necesarios
* Enums
* Domain Rules si fueran necesarias

Por ahora no crear entidades reales del portfolio porque todavía no hemos definido completamente el modelo de datos.

Crear únicamente la estructura de carpetas necesaria, sin inventar modelos de negocio.

No debe contener referencias a:

* Entity Framework
* ASP.NET Core
* Blazor
* SQL Server
* Infrastructure
* Web

### Portfolio.Application

Será la capa de aplicación.

Debe quedar preparada para contener posteriormente:

* Application Services / Use Cases
* DTOs
* Validators
* Interfaces/abstracciones necesarias
* Lógica de aplicación

Por ahora no crear casos de uso ficticios ni servicios de ejemplo.

Puede depender de Portfolio.Domain.

### Portfolio.Infrastructure

Será responsable posteriormente de:

* Entity Framework Core
* SQL Server
* DbContext
* EF Core Migrations
* ASP.NET Core Identity
* almacenamiento de archivos
* servicios externos
* email
* otras integraciones externas

En esta primera fase NO implementar todavía ninguno de ellos.

Puede depender de Portfolio.Application y Portfolio.Domain.

### Portfolio.Web

Será la aplicación principal Blazor Web App.

Debe quedar preparada para contener:

* páginas públicas
* componentes Blazor
* layouts
* navegación
* administración
* autenticación/autorización posteriormente
* localización
* estilos

En esta fase solamente debe existir una Home temporal y funcional.

### Portfolio.Tests

Proyecto destinado a pruebas automatizadas.

No es necesario crear todavía una batería de tests ficticios. Únicamente dejar el proyecto correctamente configurado para poder añadirlos posteriormente.

## Home temporal

Crear una página Home temporal muy sencilla que demuestre que la aplicación funciona correctamente.

Debe mostrar únicamente algo similar a:

Portfolio

Aplicación en construcción

No quiero todavía el diseño definitivo, Hero, navbar profesional, proyectos, certificados, blog, contacto, animaciones, Tailwind ni estilos personalizados.

La finalidad es comprobar que:

1. La solución compila.
2. La aplicación Blazor arranca correctamente.
3. Las referencias entre proyectos son correctas.
4. La estructura está preparada para las siguientes fases.

## Reglas importantes

* Utilizar .NET 10.
* Utilizar Blazor Web App.
* Utilizar C#.
* Mantener nullable reference types habilitado.
* Mantener implicit usings habilitado cuando corresponda.
* Utilizar nombres de proyectos, namespaces y carpetas coherentes con los nombres anteriores.
* No instalar paquetes NuGet que todavía no sean necesarios.
* No configurar SQL Server todavía.
* No configurar EF Core todavía.
* No configurar Identity todavía.
* No configurar Tailwind todavía.
* No configurar MudBlazor todavía.
* No configurar TinyMCE todavía.
* No crear Docker todavía.
* No crear entidades de Projects, Certifications, BlogPosts, Experience, Contact, etc.
* No generar datos ficticios.
* No implementar funcionalidades que pertenezcan a fases posteriores.

## Resultado esperado

Al finalizar esta fase quiero tener una solución limpia y compilable con esta estructura aproximada:

Portfolio.sln
│
├── Portfolio.Web
│
├── Portfolio.Application
│
├── Portfolio.Domain
│
├── Portfolio.Infrastructure
│
└── Portfolio.Tests

Y las referencias de proyectos correctamente configuradas según la arquitectura indicada.

Antes de realizar cambios adicionales, revisa la solución completa y comprueba que:

* todas las referencias tienen sentido;
* no existen dependencias circulares;
* Domain permanece independiente;
* la aplicación Blazor funciona;
* la solución compila correctamente.

Si alguna decisión técnica necesaria para crear la estructura entra en conflicto con estas instrucciones, detente y explícame el conflicto antes de introducir una arquitectura diferente.

No avances a la siguiente fase. Esta tarea termina cuando la estructura base esté creada, compilando y funcionando.


# ******************************
# PROMPT PARA REALIZAR LA FASE 2
# ******************************

Quiero implementar la FASE 2 — Sistema visual del proyecto Portfolio.

Lee primero el documento PORTFOLIO_PROJECT.md y respeta todas las decisiones arquitectónicas y visuales definidas en él. El proyecto utiliza .NET 10, ASP.NET Core y Blazor Web App.

Objetivo de esta fase:

Configurar el sistema visual base de la aplicación sin implementar todavía el Navbar, el Footer ni el diseño completo del Home. La Home temporal existente debe seguir funcionando.

Alcance obligatorio:

1. Configurar Tailwind CSS para Portfolio.Web

   - Comprueba primero cómo está estructurado actualmente el proyecto.
   - No reemplaces Tailwind por Bootstrap.
   - Mantén CSS propio junto con Tailwind.
   - Configura correctamente la compilación o generación del CSS para el flujo actual del proyecto.
   - Evita introducir herramientas o dependencias innecesarias.
   - Comprueba que el CSS generado se carga correctamente en la aplicación.

2. Crear la estructura base de estilos

   - Organiza los archivos CSS de forma clara y mantenible.
   - Define estilos base para `html`, `body`, enlaces, selección de texto, focus visible y elementos comunes.
   - Respeta accesibilidad y diseño responsive.
   - No añadas estilos específicos de secciones que todavía pertenecen a fases posteriores.
   - No introduzcas estilos dependientes de Bootstrap.

3. Definir la tipografía

   - Utiliza únicamente fuentes instaladas en el sistema o fuentes incluidas localmente dentro del proyecto.
   - No utilices Google Fonts, Adobe Fonts, CDNs ni ningún otro proveedor externo de fuentes.
   - No dependas de una conexión a Internet para cargar la tipografía.
   - Define una pila de fuentes del sistema con alternativas razonables para distintos sistemas operativos.
   - Establece una tipografía principal coherente con un portfolio profesional y técnico.
   - Define tamaños, pesos, alturas de línea y espaciados mediante tokens o clases reutilizables.
   - Si fuera necesario incluir una fuente adicional, deberá almacenarse y servirse localmente desde la aplicación, aunque se debe priorizar el uso de fuentes del sistema.
   - No dependas de la tipografía predeterminada de Bootstrap.

4. Definir la paleta y los tokens de diseño

   - Define una paleta inicial para el proyecto, entendiendo que podrá modificarse posteriormente.
   - Incluye como mínimo tokens semánticos para:
     - color primario;
     - color secundario;
     - color de acento;
     - fondo principal;
     - superficies o tarjetas;
     - texto principal;
     - texto secundario;
     - bordes y separadores;
     - success;
     - warning;
     - error;
     - info.
   - Define también, cuando sea adecuado, tokens para:
     - radios;
     - sombras;
     - espaciados;
     - tamaños de contenido;
     - transiciones;
     - focus y estados interactivos.
   - Centraliza los valores mediante variables CSS, configuración de Tailwind o una combinación coherente de ambas.
   - Utiliza nombres semánticos como `--color-primary` o `bg-surface`, evitando acoplar los componentes a nombres como `blue-500` o `gray-900`.
   - No repitas colores directamente en los componentes.
   - La paleta debe funcionar tanto en tema claro como en tema oscuro.
   - La elección de colores debe priorizar contraste, legibilidad, accesibilidad y una estética profesional.
   - Documenta brevemente, solo si encaja con el estilo existente, que la paleta es inicial y puede evolucionar.
   - No es necesario crear todavía el diseño definitivo del logo.

5. Implementar el sistema de temas

   Deben existir tres estados:

   - Sistema
   - Claro
   - Oscuro

   Requisitos:

   - El estado inicial debe ser Sistema.
   - El tema Sistema debe respetar `prefers-color-scheme`.
   - El usuario debe poder cambiar manualmente entre Sistema, Claro y Oscuro.
   - La preferencia debe almacenarse en `localStorage`.
   - La preferencia debe persistir entre visitas.
   - La implementación debe evitar parpadeos visuales innecesarios al cargar la página.
   - Los tokens semánticos deben cambiar sus valores según el tema.
   - El tema claro y el tema oscuro deben mantener contraste suficiente.
   - No crees todavía necesariamente el control visual definitivo del selector si pertenece al Navbar; deja preparada una API o mecanismo reutilizable para la Fase 3.
   - Si se necesita JavaScript para acceder a `localStorage`, detectar el tema o aplicarlo antes del renderizado, úsalo únicamente para esa integración.
   - No dupliques estilos completos para cada componente si puede resolverse mediante tokens de tema.

6. Preparar la localización

   - La aplicación se desarrollará inicialmente en español.
   - Utiliza el sistema de recursos/localización de .NET.
   - No dupliques páginas para cada idioma.
   - Configura la cultura por defecto en español.
   - Deja preparada la estructura para añadir inglés posteriormente.
   - Los textos visibles de la Home temporal también deben utilizar recursos.
   - No hardcodees textos de interfaz que ya puedan formar parte del sistema de localización.

7. Mantener el alcance de la fase

   - No implementes todavía Navbar, Footer, navegación completa, Hero ni las secciones definitivas del Home.
   - No crees entidades de base de datos.
   - No implementes Identity, administración, TinyMCE ni funcionalidades de contenido.
   - No realices cambios relacionados con Docker o despliegue.
   - No añadas MudBlazor salvo que ya exista una configuración imprescindible y directamente relacionada con esta fase.
   - No modifiques la arquitectura de proyectos salvo que sea estrictamente necesario.

Antes de editar:

1. Inspecciona la solución y los archivos existentes.
2. Comprueba cómo se carga actualmente el CSS y cómo está configurado el layout de Blazor.
3. Comprueba si Tailwind, localización o gestión de temas ya están parcialmente implementados.
4. Reutiliza la estructura existente y evita duplicar configuraciones.
5. Comprueba si existe alguna configuración previa de fuentes, colores o estilos.
6. Si encuentras una decisión ambigua que afecte a la arquitectura o a la experiencia visual, detente y pregunta antes de implementarla.

Implementación:

- Realiza cambios mínimos, coherentes y mantenibles.
- Respeta el estilo de código existente.
- Usa componentes Blazor y servicios únicamente cuando aporten una separación clara de responsabilidades.
- Si creas un servicio para el tema, define una abstracción sencilla y preparada para ser utilizada por el futuro Navbar.
- Si utilizas JavaScript, limita su responsabilidad a la persistencia y aplicación del tema.
- No introduzcas comentarios innecesarios.
- No muestres bloques de código con los cambios: aplica directamente los cambios en los archivos del workspace.

Validación obligatoria:

1. Ejecuta `dotnet build` sobre la solución.
2. Comprueba que no hay errores de compilación ni errores nuevos relacionados con los cambios.
3. Verifica que la Home temporal sigue mostrando su contenido.
4. Verifica que la aplicación inicia correctamente.
5. Comprueba, en la medida posible, que:
   - el tema Sistema respeta la preferencia del sistema;
   - Claro y Oscuro se aplican correctamente;
   - la preferencia se conserva en `localStorage`;
   - la cultura por defecto es español;
   - los textos de la Home temporal proceden de recursos;
   - los colores se aplican mediante tokens semánticos;
   - la aplicación no intenta cargar fuentes desde servicios externos;
   - la interfaz mantiene un contraste y una legibilidad adecuados.
6. Revisa que no se haya implementado accidentalmente trabajo perteneciente a las fases 3 o posteriores.

Al finalizar:

- Resume los archivos modificados.
- Explica brevemente cómo se configura Tailwind.
- Indica qué paleta inicial se ha elegido y cómo se han definido los tokens.
- Explica cómo funciona el sistema de temas.
- Explica qué fuentes se utilizan y confirma que no dependen de servicios externos.
- Explica cómo queda preparada la localización.
- Indica el resultado de la compilación y de las comprobaciones realizadas.
- No crees un commit automáticamente, pero indica como commit recomendado:


# ******************************
# PROMPT PARA REALIZAR LA FASE 3
# ******************************

Quiero implementar la FASE 3 — Layout, Navbar, Footer y navegación del proyecto Portfolio.

Lee primero PORTFOLIO_PROJECT.md y respeta todas las decisiones arquitectónicas y visuales definidas en él.

Contexto actual:
- Proyecto .NET 10.
- ASP.NET Core con Blazor Web App.
- Renderizado basado en Razor Components.
- Portfolio.Web es el proyecto frontend.
- Tailwind CSS ya está configurado y se utiliza junto con CSS propio.
- Los estilos propios están en Portfolio.Web/wwwroot/css/app.css.
- La entrada de Tailwind está en Portfolio.Web/Styles/tailwind.css.
- El CSS compilado está en Portfolio.Web/wwwroot/css/tailwind.css.
- La tipografía utiliza únicamente fuentes del sistema o locales.
- Los tokens semánticos de color están definidos mediante variables CSS.
- El sistema de temas ya dispone de los estados Sistema, Claro y Oscuro.
- La API JavaScript del tema está en Portfolio.Web/wwwroot/js/theme.js.
- La preferencia de tema se almacena en localStorage.
- La localización está configurada con español como cultura predeterminada y recursos preparados para inglés.
- La Home temporal sigue mostrando:
  - Portfolio
  - Aplicación en construcción

Objetivo de esta fase:

Implementar únicamente la estructura visual general de la parte pública:

1. Navbar
2. Footer
3. Navegación principal
4. Comportamiento responsive del layout
5. Control visual reutilizable para cambiar entre Sistema, Claro y Oscuro

No implementes todavía el Hero ni las secciones definitivas de la Home.

Antes de editar:

1. Inspecciona la solución y los archivos existentes.
2. Lee PORTFOLIO_PROJECT.md, especialmente las secciones de:
   - sistema visual;
   - internacionalización;
   - tema visual;
   - estructura pública;
   - orden de implementación;
   - reglas de accesibilidad.
3. Comprueba cómo están implementados actualmente:
   - MainLayout.razor;
   - NavMenu.razor;
   - App.razor;
   - Routes.razor;
   - Home.razor;
   - app.css;
   - theme.js;
   - recursos de localización.
4. Comprueba si existe ya algún layout, componente o estilo reutilizable que deba conservarse.
5. No dupliques configuraciones ni crees una arquitectura paralela.
6. Si encuentras una decisión arquitectónica ambigua que afecte al layout o a la navegación, detente y pregunta antes de implementarla.

Alcance obligatorio:

1. Implementar Navbar

Crea una Navbar propia para la parte pública utilizando Blazor, Tailwind CSS y CSS propio cuando sea necesario.

La Navbar debe incluir:

- Espacio reservado para el logo del portfolio.
- El logo debe enlazar a `/`.
- Enlaces localizados para:
  - Sobre mí
  - Proyectos
  - Certificados
  - Blog
  - Contacto
- Estado visual del enlace activo.
- Navegación accesible mediante teclado.
- Uso correcto de elementos semánticos HTML.
- Atributos ARIA únicamente cuando sean necesarios.
- Diseño responsive.
- En escritorio, navegación visible.
- En móvil, navegación adaptada mediante un menú desplegable o hamburguesa accesible.
- El menú móvil debe poder abrirse y cerrarse mediante teclado.
- El foco no debe perderse de forma inesperada.
- El botón del menú debe indicar correctamente su estado mediante `aria-expanded`.
- El menú debe cerrarse cuando corresponda al navegar.

No implementes todavía páginas reales para las secciones si no existen. Los enlaces pueden apuntar a anchors o rutas preparadas, pero no crees contenido definitivo de fases posteriores.

2. Implementar Footer

Crea un Footer propio para la parte pública.

Debe incluir únicamente una estructura inicial y preparada para crecer, por ejemplo:

- Nombre o identidad textual del portfolio.
- Año actual.
- Texto breve de derechos.
- Enlaces externos solo si ya existen en el proyecto.
- Enlaces básicos accesibles.

No añadas todavía contenido definitivo de redes sociales, proyectos, blog ni datos personales que no existan.

3. Integrar el layout

Actualiza MainLayout.razor para que la estructura general sea:

- Navbar
- Contenido principal
- Footer

Usa elementos semánticos como:

- `<header>`
- `<nav>`
- `<main>`
- `<footer>`

El contenido de las páginas debe seguir funcionando mediante `@Body`.

No rompas la Home temporal.

No conviertas toda la aplicación en una SPA ni implementes navegación innecesaria en JavaScript.

4. Integrar el selector de tema

Aprovecha la API existente de `theme.js`.

Crea un componente reutilizable para el selector de tema, por ejemplo:

- ThemeSelector.razor

Debe permitir elegir:

- Sistema
- Claro
- Oscuro

Requisitos:

- Utilizar la API existente de JavaScript mediante JS interop.
- No duplicar la lógica de localStorage en C#.
- Mantener la preferencia entre visitas.
- Reflejar visualmente el estado actual.
- Ser accesible mediante teclado.
- Utilizar textos localizados.
- No cargar librerías externas.
- No implementar una segunda API de temas.
- No modificar los tokens CSS salvo que sea estrictamente necesario.

El selector puede formar parte de la Navbar, pero no crees todavía un diseño definitivo de identidad visual o logo.

5. Localización

Todos los textos visibles nuevos deben proceder de recursos.

Añade recursos en español para:

- Sobre mí
- Proyectos
- Certificados
- Blog
- Contacto
- Abrir menú
- Cerrar menú
- Tema
- Sistema
- Claro
- Oscuro
- Derechos reservados
- Nombre o descripción básica del portfolio si se necesita

Prepara también las claves equivalentes en inglés.

No dupliques páginas por idioma.

No hardcodees textos visibles que deban formar parte de la interfaz.

6. Responsive y accesibilidad

El layout debe funcionar correctamente en:

- móvil;
- tablet;
- escritorio;
- anchos reducidos;
- zoom del navegador.

Respeta:

- contraste suficiente;
- focus-visible;
- navegación completa mediante teclado;
- `prefers-reduced-motion`;
- tamaños táctiles razonables;
- textos legibles;
- no depender únicamente del color para comunicar estados;
- no ocultar contenido importante en móvil;
- no utilizar fuentes externas.

7. Estilos

Utiliza las variables semánticas ya existentes:

- colores;
- superficies;
- bordes;
- focus;
- radios;
- sombras;
- transiciones;
- espaciados.

No introduzcas colores acoplados a nombres como `blue-500` o `gray-900`.

No utilices Bootstrap.

No dependas de la tipografía predeterminada de Bootstrap.

No añadas estilos del Hero, proyectos, experiencia, certificaciones, blog ni contacto.

8. Mantener el alcance

No implementes:

- Hero;
- diseño definitivo del Home;
- secciones de Sobre mí;
- experiencia;
- proyectos;
- certificaciones;
- blog;
- contacto;
- base de datos;
- Identity;
- administración;
- TinyMCE;
- Docker;
- despliegue;
- MudBlazor;
- logo definitivo.

Validación obligatoria:

1. Ejecuta `npm run css:build`.
2. Ejecuta `dotnet build` sobre la solución.
3. Comprueba que no hay errores ni warnings nuevos relacionados con los cambios.
4. Inicia la aplicación.
5. Comprueba que la Home temporal sigue mostrando:
   - Portfolio
   - Aplicación en construcción
6. Comprueba que:
   - la Navbar aparece;
   - el Footer aparece;
   - el logo enlaza a `/`;
   - los enlaces se muestran correctamente;
   - el menú móvil se abre y cierra;
   - el menú es usable con teclado;
   - el selector de tema cambia entre Sistema, Claro y Oscuro;
   - la preferencia se conserva en localStorage;
   - los textos visibles nuevos proceden de recursos;
   - no se cargan fuentes externas;
   - el layout no introduce trabajo de la Fase 4.
7. Revisa que `node_modules` no aparezca como archivo pendiente de Git.
8. No crees un commit automáticamente.

Al finalizar:

- Resume los archivos modificados.
- Explica cómo se ha integrado Navbar, Footer y navegación.
- Explica cómo funciona el menú responsive.
- Explica cómo se ha integrado el selector de tema existente.
- Indica qué recursos de localización se han añadido.
- Indica las comprobaciones realizadas.
- Indica el resultado de `npm run css:build`.
- Indica el resultado de `dotnet build`.
- Confirma que la Home temporal sigue funcionando.
- Confirma que no se han implementado elementos de la Fase 4.


# ******************************
# PROMPT PARA REALIZAR LA FASE 4
# ******************************

Quiero implementar la FASE 4 — Home del proyecto Portfolio.

Lee primero y respeta obligatoriamente:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md

Ten en cuenta todo el desarrollo ya realizado en las fases 1, 2 y 3. No sustituyas decisiones existentes ni crees una arquitectura paralela.

## Contexto actual

- Proyecto .NET 10.
- Solución Portfolio.sln.
- Aplicación frontend en Portfolio.Web.
- ASP.NET Core Blazor Web App.
- Razor Components.
- Renderizado Interactive Server únicamente donde aporta interactividad.
- No es una SPA pura.
- Tailwind CSS local.
- Estilos propios en:
  - Portfolio.Web/wwwroot/css/app.css
- Entrada de Tailwind:
  - Portfolio.Web/Styles/tailwind.css
- CSS compilado:
  - Portfolio.Web/wwwroot/css/tailwind.css
- Tema visual existente:
  - Portfolio.Web/wwwroot/js/theme.js
- API global existente:
  - portfolioTheme
- Estados de tema:
  - Sistema
  - Claro
  - Oscuro
- La preferencia de tema se guarda mediante la API JavaScript existente en localStorage.
- Localización configurada con:
  - es-ES como cultura predeterminada.
  - en-US como cultura soportada.
- Recursos existentes:
  - Portfolio.Web/Resources/SharedResource.resx
  - Portfolio.Web/Resources/SharedResource.es.resx
  - Portfolio.Web/Resources/SharedResource.en.resx
- Layout público actual:
  - Portfolio.Web/Components/Layout/MainLayout.razor
- Navbar actual:
  - Portfolio.Web/Components/Layout/NavMenu.razor
- Footer actual:
  - Portfolio.Web/Components/Layout/PortfolioFooter.razor
- Selector de tema actual:
  - Portfolio.Web/Components/Layout/ThemeSelector.razor
- Rutas actuales:
  - Portfolio.Web/Components/Routes.razor
- Home temporal actual:
  - Portfolio.Web/Components/Pages/Home.razor

La Home temporal actualmente muestra:

- Portfolio
- Aplicación en construcción

La Fase 3 ya implementó:

- Navbar pública.
- Footer.
- Menú móvil responsive.
- Enlaces provisionales a anchors.
- Selector de tema.
- Layout con header, main y footer.
- Interactividad mediante Blazor Interactive Server.
- Recursos localizados para la navegación y el tema.

Debes conservar todo ese funcionamiento.

## Objetivo de la Fase 4

Sustituir la Home temporal por la estructura inicial de la Home pública del portfolio.

La Home debe seguir esta estructura:

1. Navbar
2. Hero
3. Sobre mí
4. Experiencia
5. Proyectos
6. Certificaciones
7. Último artículo publicado
8. Contacto
9. Footer

La Navbar y el Footer ya existen y deben conservarse. No los reemplaces por una arquitectura distinta salvo que sea estrictamente necesario.

## Alcance funcional de esta fase

Implementa únicamente la parte visual y estructural inicial de la Home:

- Hero.
- Sección Sobre mí.
- Sección Experiencia.
- Sección Proyectos.
- Sección Certificaciones.
- Sección de último artículo publicado.
- Sección Contacto.

En esta fase no implementes todavía:

- Base de datos.
- Entity Framework.
- Entidades definitivas.
- Repositorios.
- Servicios de persistencia.
- CRUD.
- Administración.
- ASP.NET Core Identity.
- Login.
- Autorización.
- TinyMCE.
- Gestión real de proyectos.
- Gestión real de certificaciones.
- Gestión real del blog.
- Envío real de formularios.
- Docker.
- Despliegue.
- Integraciones externas.
- Redes sociales si no existen actualmente en el proyecto.
- Hero definitivo basado en información personal no proporcionada.
- Logo definitivo.

## Regla sobre contenido personal

Antes de inventar datos personales, experiencia profesional, empresas, proyectos, certificaciones, enlaces, fechas o artículos:

1. Comprueba si esos datos ya existen en el repositorio.
2. Si no existen y son necesarios para construir una sección real, detente y pregunta.
3. No inventes información personal para rellenar la interfaz.
4. Mientras no existan datos definitivos, utiliza una estructura visual inicial y textos provisionales localizados claramente identificables como placeholders, o solicita la información necesaria antes de continuar.

No conviertas la Home en una biografía inventada.

## Hero

Crea un componente reutilizable propio para el Hero, por ejemplo:

- Portfolio.Web/Components/Sections/HeroSection.razor

El Hero debe respetar las decisiones de PORTFOLIO_PROJECT.md:

- Fotografía personal solo si existe una imagen local válida.
- Nombre solo si existe en el proyecto o se proporciona explícitamente.
- Titular profesional.
- Descripción breve.
- CTA.
- Elemento visual relacionado con tecnologías.

El texto debe ser breve:

- Una frase o titular principal.
- Dos o tres líneas como máximo para la descripción.
- No crear una biografía extensa en el Hero.

El elemento visual de tecnologías debe:

- Ser propio mediante HTML, CSS o Blazor.
- Ser sutil y profesional.
- Ser responsive.
- Funcionar con tema claro y oscuro.
- Respetar prefers-reduced-motion.
- No depender de GIFs pesados.
- No cargar imágenes, iconos, fuentes o recursos desde CDN.
- No saturar visualmente el Hero.

Si no existen datos o imágenes personales, crea una composición visual provisional sin inventar identidad personal.

## Sección Sobre mí

Crea una sección reutilizable, por ejemplo:

- AboutSection.razor

Debe prever:

- Perfil profesional.
- Especialización.
- Áreas de interés.
- Enfoque tecnológico.
- Áreas principales de trabajo.

La estructura puede contemplar inicialmente:

- Backend.
- Frontend.
- Datos.
- DevOps.

No añadas una biografía personal inventada. Utiliza recursos localizados y contenido provisional solo cuando sea necesario.

## Sección Experiencia

Crea un componente reutilizable, por ejemplo:

- ExperienceTimeline.razor

Debe crear únicamente la estructura visual inicial de una timeline:

- Periodo.
- Puesto.
- Organización.
- Descripción.
- Tecnologías.

No inventes empleos ni fechas. Si no hay datos reales, utiliza un estado vacío o contenido provisional localizado.

## Sección Proyectos

Crea componentes reutilizables iniciales, por ejemplo:

- ProjectSection.razor
- ProjectCard.razor

La tarjeta de proyecto puede prever:

- Imagen.
- Nombre.
- Descripción.
- Tecnologías.
- Enlace a GitHub.
- Demo.
- Enlace de detalle.

No crees todavía entidades ni consultas a base de datos.

No inventes proyectos reales ni URLs. Si no hay proyectos disponibles, muestra una estructura inicial o un estado vacío localizado.

La ruta futura prevista para detalles es:

- /projects/{slug}

Pero no es necesario crear todavía páginas de detalle ni navegación funcional a ellas.

## Sección Certificaciones

Crea un componente reutilizable, por ejemplo:

- CertificationsSection.razor
- CertificationCard.razor

Debe prever:

- Nombre.
- Organización.
- Fecha.
- Duración u horas.
- Descripción.
- Imagen.
- Documento si corresponde.

No inventes certificaciones ni documentos.

No crees todavía entidades ni persistencia.

## Sección de último artículo publicado

Crea un componente inicial, por ejemplo:

- FeaturedPost.razor

Debe representar visualmente el último artículo publicado, no el último artículo creado.

Puede prever:

- Imagen destacada.
- Título.
- Fecha.
- Extracto.
- Enlace de lectura.

No implementes todavía:

- Blog dinámico.
- Editor TinyMCE.
- Consultas a base de datos.
- Publicación real.
- Gestión administrativa.

Si no hay artículos, utiliza un estado vacío o placeholder localizado.

## Sección Contacto

Crea un componente inicial, por ejemplo:

- ContactSection.razor

Debe incluir únicamente la estructura visual inicial.

Puede prever:

- Título.
- Texto introductorio.
- Nombre.
- Email.
- Mensaje.
- Botón de envío.

No implementes todavía:

- Envío real de emails.
- Persistencia.
- Validación de backend.
- Antispam.
- Base de datos.
- Integraciones externas.

Si se muestra un formulario, debe quedar claramente como estructura visual inicial y todos los textos deben estar localizados.

## Arquitectura de componentes

Mantén los componentes organizados dentro de Portfolio.Web/Components.

Puedes crear una estructura como:

- Components/Sections/HeroSection.razor
- Components/Sections/AboutSection.razor
- Components/Sections/ExperienceTimeline.razor
- Components/Sections/ProjectSection.razor
- Components/Sections/ProjectCard.razor
- Components/Sections/CertificationsSection.razor
- Components/Sections/CertificationCard.razor
- Components/Sections/FeaturedPost.razor
- Components/Sections/ContactSection.razor

La estructura final debe seguir las convenciones reales del repositorio. No crees capas innecesarias ni servicios que todavía no sean necesarios.

## Localización

Todos los textos visibles nuevos deben proceder de recursos.

Añade las claves necesarias a:

- SharedResource.es.resx
- SharedResource.en.resx

Incluye, como mínimo, los textos de:

- Hero.
- CTA.
- Sobre mí.
- Experiencia.
- Proyectos.
- Certificaciones.
- Blog o último artículo.
- Contacto.
- Campos del formulario.
- Botón de contacto.
- Estados vacíos.
- Textos provisionales si se utilizan.

No hardcodees textos visibles nuevos en los componentes.

No crees:

- HomeSpanish.razor
- HomeEnglish.razor

No implementes todavía un selector de idioma visible si no está contemplado expresamente por la fase actual.

## Estilos

Utiliza:

- Tailwind CSS local.
- Portfolio.Web/wwwroot/css/app.css.
- Variables y tokens semánticos existentes.

No utilices:

- Bootstrap.
- Fuentes externas.
- CDN.
- Colores acoplados a nombres como blue-500 o gray-900.
- Una segunda paleta paralela.
- Estilos aislados que contradigan los tokens existentes.

Los estilos deben funcionar correctamente con:

- Tema claro.
- Tema oscuro.
- Tema sistema.
- Anchos móviles.
- Tablet.
- Escritorio.
- Zoom del navegador.
- Anchos reducidos.

No modifiques la API de tema existente salvo que sea estrictamente necesario.

## Accesibilidad

La Home debe respetar:

- HTML semántico.
- Jerarquía correcta de encabezados.
- Navegación mediante teclado.
- `focus-visible`.
- Contraste suficiente.
- Tamaños táctiles razonables.
- No depender únicamente del color.
- `prefers-reduced-motion`.
- Atributos ARIA solo cuando sean necesarios.
- Imágenes con texto alternativo cuando existan.
- Formularios con labels correctamente asociados.
- Estados vacíos comprensibles.
- Enlaces y botones con nombres accesibles.

No añadas ARIA redundante.

## Navegación desde la Navbar

Conserva los anchors existentes:

- `/#about`
- `/#projects`
- `/#certificates`
- `/#blog`
- `/#contact`

Añade los `id` correspondientes a las secciones de la Home para que la navegación actual funcione.

Si se añade un anchor para Experiencia, actualiza la navegación únicamente si existe una decisión explícita y se mantienen todos los textos localizados.

No dupliques la navegación mediante JavaScript.

## Animaciones

Las animaciones deben ser:

- Sutiles.
- Profesionales.
- CSS o Blazor.
- Compatibles con tema claro y oscuro.
- Desactivables o reducidas mediante `prefers-reduced-motion`.

No añadas animaciones decorativas excesivas.

## Revisión previa obligatoria

Antes de editar:

1. Inspecciona la solución.
2. Lee `PORTFOLIO_PROJECT.md`.
3. Lee `.github/copilot-instructions.md`.
4. Revisa:
   - Home.razor.
   - MainLayout.razor.
   - NavMenu.razor.
   - PortfolioFooter.razor.
   - ThemeSelector.razor.
   - Routes.razor.
   - app.css.
   - theme.js.
   - recursos de localización.
5. Comprueba si ya existen componentes reutilizables.
6. Comprueba si existen imágenes o contenidos personales locales.
7. Si falta información personal necesaria y no puede resolverse con un estado vacío o placeholder localizado, detente y pregunta antes de implementar contenido inventado.

## Validación obligatoria

Ejecuta:

1. `npm run css:build` desde `Portfolio.Web`.
2. `dotnet build Portfolio.sln`.
3. Comprueba que no hay errores ni warnings nuevos relacionados con los cambios.
4. Inicia la aplicación.
5. Comprueba que:
   - La Navbar sigue funcionando.
   - El Footer sigue funcionando.
   - El selector de tema sigue funcionando.
   - La Home ya no muestra únicamente la presentación temporal.
   - Los anchors navegan a las secciones correspondientes.
   - La Home funciona en móvil, tablet y escritorio.
   - No se cargan fuentes externas.
   - Los textos nuevos proceden de recursos.
   - El tema claro, oscuro y sistema siguen funcionando.
   - `prefers-reduced-motion` se respeta.
   - No se ha introducido trabajo de las fases 5, 6 o 7.
6. Comprueba que `node_modules` no aparece como archivo pendiente de Git.
7. No crees un commit automáticamente.

## Documentación

Si durante la Fase 4 se toma una decisión nueva y permanente:

- Actualiza `PORTFOLIO_PROJECT.md` si afecta a la arquitectura, alcance, diseño o planificación.
- Actualiza `.github/copilot-instructions.md` solo si se trata de una directriz reutilizable para futuras modificaciones.
- No conviertas decisiones puntuales de una sección en reglas globales sin justificación.

## Resultado final esperado

Al finalizar:

1. Resume los archivos creados y modificados.
2. Explica cómo se ha estructurado la Home.
3. Explica cómo se han reutilizado Navbar, Footer y selector de tema.
4. Indica qué recursos de localización se han añadido.
5. Indica si se ha utilizado contenido provisional o contenido real.
6. Explica las decisiones de responsive y accesibilidad.
7. Indica el resultado de `npm run css:build`.
8. Indica el resultado de `dotnet build`.
9. Confirma que la navegación y el tema siguen funcionando.
10. Confirma que no se han implementado base de datos, administración, Identity, TinyMCE ni funcionalidades dinámicas de fases posteriores.
11. Confirma que no se ha creado ningún commit.