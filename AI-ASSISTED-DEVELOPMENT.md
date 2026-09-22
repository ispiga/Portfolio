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


# ******************************
# PROMPT PARA REALIZAR LA FASE 5
# ******************************

Quiero implementar la FASE 5 — Datos del proyecto Portfolio.

Lee primero y respeta obligatoriamente:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md

Ten en cuenta todo el desarrollo realizado en las fases 1, 2, 3 y 4. No sustituyas decisiones existentes, no crees una arquitectura paralela y no reviertas los cambios actuales.

## Contexto del proyecto

- Solución: Portfolio.sln
- Framework: .NET 10
- Aplicación pública: Portfolio.Web
- Frontend: Blazor Web App con Razor Components
- Renderizado: Interactive Server únicamente donde aporte interactividad
- Capas existentes:
  - Portfolio.Web
  - Portfolio.Application
  - Portfolio.Domain
  - Portfolio.Infrastructure
  - Portfolio.Tests
- Base de datos prevista:
  - SQL Server
  - Entity Framework Core
  - Code First
  - Migrations
- Arquitectura prevista:

  Portfolio.Web
        ↓
  Portfolio.Application
        ↓
  Portfolio.Domain
        ↑
  Portfolio.Infrastructure

## Estado actual

La Fase 4 ya implementó la estructura visual inicial de la Home pública:

- Hero.
- Sobre mí.
- Experiencia.
- Proyectos.
- Certificaciones.
- Último artículo publicado.
- Contacto.
- Componentes Razor reutilizables.
- Estados vacíos y placeholders localizados.
- Estilos responsive en `Portfolio.Web/wwwroot/css/app.css`.
- Localización es-ES y en-US.
- Navbar, Footer y selector de tema existentes.
- API de tema existente en `Portfolio.Web/wwwroot/js/theme.js`.

La Home todavía no utiliza contenido dinámico ni base de datos.

## Objetivo de la Fase 5

Preparar la base de datos y la estructura de persistencia del proyecto utilizando:

- Entity Framework Core.
- SQL Server.
- Code First.
- Migrations.

La implementación debe ser incremental y derivarse de las necesidades reales de la Home y de las fases futuras. No se debe intentar diseñar todo el sistema definitivo de una sola vez.

## Alcance obligatorio

Antes de editar:

1. Inspecciona la solución completa.
2. Revisa los archivos `.csproj`.
3. Revisa las referencias entre proyectos.
4. Comprueba los paquetes NuGet existentes.
5. Revisa el contenido actual de:
   - Portfolio.Domain
   - Portfolio.Application
   - Portfolio.Infrastructure
   - Portfolio.Web/Program.cs
   - Portfolio.Web/appsettings*.json
   - Portfolio.Tests
6. Comprueba si ya existen entidades, DbContext, configuraciones, migraciones o servicios de persistencia.
7. Comprueba si existe alguna configuración de conexión a SQL Server.
8. Comprueba si existe algún contenido personal o modelo previo que deba conservarse.

Después de la inspección, implementa únicamente la base de datos mínima necesaria para esta fase.

## Modelo de dominio

No crees automáticamente todas las entidades posibles.

Las entidades futuras orientativas son:

- Project.
- Experience.
- Certification.
- BlogPost.
- Category.
- Tag.

El modelo debe derivarse de las necesidades reales del proyecto y mantenerse mínimo.

Si no existe información suficiente para decidir una entidad, sus propiedades, relaciones o reglas de negocio:

- No inventes datos personales.
- No inventes proyectos, empresas, fechas, certificaciones ni artículos.
- No inventes relaciones innecesarias.
- Utiliza nombres técnicos provisionales únicamente cuando sean imprescindibles.
- Documenta la decisión.
- Si la decisión afecta significativamente a la arquitectura, detente y pregunta antes de continuar.

Como mínimo, analiza qué estructura de persistencia será necesaria para soportar posteriormente:

- Proyectos.
- Experiencia.
- Certificaciones.
- Artículos publicados.

No es necesario implementar todavía la funcionalidad pública dinámica de estas secciones.

## Portfolio.Domain

Las entidades y reglas de dominio deben permanecer en `Portfolio.Domain`.

Respeta estas reglas:

- El dominio no debe depender de Entity Framework Core.
- El dominio no debe depender de SQL Server.
- El dominio no debe depender de Blazor.
- El dominio no debe depender de `Portfolio.Web`.
- No añadas atributos de persistencia al dominio salvo que exista una razón justificada.
- Utiliza tipos y reglas de dominio coherentes con el proyecto.
- Evita crear value objects o abstracciones innecesarias.

## Portfolio.Application

No implementes todavía casos de uso completos ni servicios dinámicos de la Home.

Solo crea interfaces, contratos o DTOs si son necesarios para mantener la separación entre Application e Infrastructure.

No añadas repositorios genéricos por defecto.

No crees servicios vacíos sin una necesidad real.

## Portfolio.Infrastructure

Implementa aquí la integración con Entity Framework Core:

- DbContext.
- Configuraciones de entidades.
- Registro de persistencia.
- Migraciones.
- Configuración de SQL Server.

El DbContext debe permanecer en `Portfolio.Infrastructure`.

Utiliza configuraciones separadas mediante `IEntityTypeConfiguration<T>` cuando mejore la claridad y sea coherente con el tamaño real del modelo.

No acoples la capa pública directamente a detalles internos de EF Core.

## SQL Server y configuración

Añade la configuración necesaria para SQL Server siguiendo las convenciones actuales del proyecto.

Considera:

- `appsettings.json`.
- `appsettings.Development.json`.
- Variables de configuración.
- No incluir contraseñas reales.
- No incluir secretos en Git.
- No sobrescribir configuraciones existentes sin revisarlas.
- No crear una base de datos remota ni conectarte a servicios externos sin autorización explícita.

Si se necesita una cadena de conexión de ejemplo, utiliza un placeholder seguro y documentado.

La aplicación debe poder compilar aunque SQL Server no esté disponible localmente, salvo que la configuración actual del proyecto establezca otra cosa.

## Migraciones

Crea la primera migración únicamente después de revisar el modelo.

La migración debe:

- Ser reproducible.
- Corresponder exactamente al modelo implementado.
- Mantener nombres claros.
- No incluir datos personales inventados.
- No insertar proyectos, certificaciones, artículos ni experiencias ficticias.
- No crear tablas que no estén justificadas.

Si no es posible generar una migración correctamente por falta de configuración de SQL Server, documenta el bloqueo y no lo ocultes mediante soluciones improvisadas.

## Lo que no se debe implementar en esta fase

No implementes:

- Contenido dinámico de la Home.
- CRUD.
- Panel de administración.
- ASP.NET Core Identity.
- Login.
- Autorización.
- Roles.
- Dashboard.
- TinyMCE.
- Gestión de proyectos.
- Gestión de certificaciones.
- Gestión de experiencia.
- Gestión de artículos.
- Envío real de formularios.
- Servicios de correo.
- Almacenamiento de imágenes.
- Docker.
- Despliegue en QNAP.
- Integraciones externas.
- Redes sociales.
- Datos personales inventados.
- Seeders con información ficticia.

Identity, administración, CRUD y TinyMCE pertenecen a fases posteriores.

## Pruebas

Añade únicamente pruebas útiles para el modelo o la configuración creada.

No generes una cantidad artificial de tests.

Si el proyecto de pruebas no tiene todavía infraestructura suficiente, crea solo las pruebas que puedan ejecutarse de forma fiable sin depender de un SQL Server externo.

Puedes utilizar una base de datos aislada para pruebas únicamente si encaja con los paquetes y convenciones existentes. No añadas dependencias innecesarias.

## Documentación

Si tomas una decisión permanente sobre:

- ubicación del DbContext;
- estrategia de configuraciones;
- convenciones de nombres;
- conexión a SQL Server;
- migraciones;
- entidades iniciales;
- separación entre Domain, Application e Infrastructure;

actualiza `PORTFOLIO_PROJECT.md`.

No modifiques `.github/copilot-instructions.md` salvo que aparezca una directriz reutilizable para futuras tareas.

## Validación obligatoria

Ejecuta y documenta:

1. `dotnet restore` si es necesario.
2. `dotnet build Portfolio.sln`.
3. Las pruebas disponibles.
4. La generación de la migración, si procede.
5. `dotnet ef migrations list`, si la herramienta está disponible.
6. Comprobación de que no existen secretos en los archivos modificados.
7. Comprobación de que no se ha modificado la Home pública de forma innecesaria.
8. Comprobación de que la Navbar, Footer, selector de tema y localización siguen intactos.
9. Comprobación de que no se han implementado funcionalidades de las fases 6, 7, 8 o 9.
10. Comprobación del estado de Git.

No crees ningún commit automáticamente.

## Resultado final esperado

Al finalizar:

1. Resume las entidades creadas y justifica por qué son necesarias.
2. Resume los cambios realizados por proyecto.
3. Explica dónde está el DbContext.
4. Explica cómo se configura SQL Server.
5. Indica qué migraciones se han creado.
6. Indica si se han añadido configuraciones de entidades.
7. Indica las pruebas ejecutadas.
8. Indica el resultado de `dotnet build Portfolio.sln`.
9. Indica si existe alguna limitación por no disponer de SQL Server local.
10. Confirma que no se han creado datos ficticios.
11. Confirma que no se ha implementado CRUD, Identity, administración ni contenido dinámico.
12. Confirma que no se ha creado ningún commit.


# ****************************************
# PROMPT PARA REALIZAR LA FASE 6 - PARTE 1
# ****************************************

Quiero comenzar la FASE 6 — Funcionalidades del proyecto Portfolio.

Lee primero y respeta obligatoriamente:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md
3. README.md

Ten en cuenta todo el desarrollo realizado en las fases 1, 2, 3, 4 y 5. No sustituyas decisiones existentes, no crees una arquitectura paralela y no reviertas cambios actuales.

## Contexto actual

- Solución: Portfolio.sln
- Framework: .NET 10
- Aplicación: Portfolio.Web
- Frontend: Blazor Web App con Razor Components
- Renderizado: Interactive Server únicamente donde aporte interactividad
- Persistencia: EF Core 10 + SQL Server + Code First + Migrations
- DbContext: Portfolio.Infrastructure/PortfolioDbContext.cs
- Base de datos de desarrollo: PortfolioDb
- User Secrets configurado para la conexión local
- Culturas actuales:
  - es-ES
  - en-US
- Cultura predeterminada: es-ES
- Recursos existentes:
  - SharedResource.resx
  - SharedResource.es.resx
  - SharedResource.en.resx
- La Home pública todavía utiliza contenido estático, placeholders y estados vacíos.
- La Fase 5 no conectó todavía la Home con contenido dinámico.

## Forma de trabajo obligatoria

La Fase 6 debe desarrollarse de forma incremental por secciones.

En esta primera intervención implementa únicamente:

# Selector de idioma en la Navbar

No implementes todavía:

- Contenido dinámico de proyectos.
- Experiencia dinámica.
- Certificaciones dinámicas.
- Blog dinámico.
- CRUD.
- Panel de administración.
- Identity.
- Traducciones almacenadas en SQL Server.
- Tablas de traducciones.
- Nuevas entidades de dominio.
- Funcionalidades de fases posteriores.

Después de implementar el selector de idioma, detén el desarrollo para que pueda revisar visual y funcionalmente el resultado antes de continuar con la siguiente sección.

## Objetivo funcional

Añadir un selector de idioma en la Navbar:

- Debe aparecer a la derecha del selector de tema.
- Debe permitir seleccionar:
  - Español
  - English
- Debe utilizar las culturas existentes:
  - es-ES
  - en-US
- Debe cambiar la cultura actual de ASP.NET Core.
- Debe utilizar la localización existente y los recursos RESX actuales.
- Debe persistir la cultura seleccionada mediante el mecanismo adecuado de localización de ASP.NET Core.
- Debe mantener es-ES como valor predeterminado.
- No debe duplicar páginas por idioma.
- No debe crear una segunda infraestructura de localización.

## Requisitos técnicos

Antes de editar:

1. Inspecciona la Navbar actual.
2. Inspecciona el selector de tema actual.
3. Revisa cómo está configurada la localización en Program.cs.
4. Revisa Routes.razor, App.razor y los componentes relacionados.
5. Revisa los recursos actuales es-ES y en-US.
6. Comprueba si existe ya algún mecanismo de cultura o persistencia de idioma.
7. Revisa los estilos existentes en wwwroot/css/app.css.

Implementa el selector reutilizando la arquitectura actual.

El selector debe:

- Ser accesible mediante teclado.
- Tener focus-visible.
- Mantener buen contraste.
- Ser responsive.
- Integrarse correctamente en el menú móvil.
- Reflejar el idioma actual.
- Utilizar textos localizados.
- No introducir Bootstrap.
- No cargar fuentes externas.
- No duplicar la lógica del selector de tema.
- No utilizar localStorage para la cultura si ASP.NET Core puede resolverlo mediante cookie.
- No cambiar la API portfolioTheme existente.

## Localización

Todos los textos visibles del selector deben utilizar los recursos compartidos.

Si faltan recursos para el selector:

- Añádelos en español e inglés.
- No escribas textos visibles directamente en el componente.
- Mantén es-ES como cultura predeterminada.

## Pruebas y validación

Añade únicamente las pruebas útiles y posibles para esta funcionalidad.

Valida:

1. `dotnet build Portfolio.sln`.
2. Las pruebas disponibles.
3. Cambio entre es-ES y en-US.
4. Persistencia de la cultura tras recargar.
5. Funcionamiento en escritorio.
6. Funcionamiento en móvil.
7. Accesibilidad básica mediante teclado.
8. Integración con el selector de tema.
9. Que Navbar, Footer, Home y tema no se rompen.
10. Que no se han implementado funcionalidades posteriores.
11. Estado final de Git.

No crees ningún commit automáticamente.

Al finalizar:

- Resume los archivos modificados.
- Explica cómo funciona el cambio de cultura.
- Indica cómo se persiste el idioma.
- Indica las pruebas ejecutadas.
- Indica el resultado de la compilación.
- Confirma que no se ha implementado contenido dinámico ni administración.
- Detén el trabajo para revisión antes de continuar con otra sección.


# ****************************************
# PROMPT PARA REALIZAR LA FASE 6 - PARTE 2
# ****************************************

Quiero continuar la FASE 6 — Funcionalidades del proyecto Portfolio.

Lee y respeta obligatoriamente antes de modificar nada:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md
3. README.md

Ten en cuenta todo el desarrollo realizado en las fases 1, 2, 3, 4, 5 y la primera sección de la Fase 6. No sustituyas decisiones existentes, no crees una arquitectura paralela y no reviertas cambios actuales.

## Contexto actual

- Solución: Portfolio.sln
- Framework: .NET 10
- Aplicación: Portfolio.Web
- Arquitectura:
  - Portfolio.Web
  - Portfolio.Application
  - Portfolio.Domain
  - Portfolio.Infrastructure
  - Portfolio.Tests
- Frontend: Blazor Web App con Razor Components
- Renderizado: Interactive Server únicamente cuando aporte interactividad
- Persistencia: EF Core 10 + SQL Server + Code First + Migrations
- DbContext: Portfolio.Infrastructure/PortfolioDbContext.cs
- Base de datos de desarrollo: PortfolioDb
- User Secrets configurado para la conexión local
- Culturas:
  - es-ES
  - en-US
- Cultura predeterminada: es-ES
- Recursos RESX existentes:
  - SharedResource.resx
  - SharedResource.es.resx
  - SharedResource.en.resx
- Selector de idioma de la Navbar implementado y revisado
- La cookie de cultura de ASP.NET Core ya está funcionando
- La Home continúa utilizando contenido estático, placeholders y estados vacíos

## Objetivo de esta intervención

Implementar únicamente el modelo base para contenido traducible en SQL Server.

El objetivo es preparar la persistencia para que las futuras secciones de proyectos, experiencia, certificaciones y blog puedan tener traducciones mediante entidades principales y tablas de traducciones relacionadas.

## No implementes todavía

- Contenido dinámico en la Home.
- Consultas públicas.
- Servicios de aplicación para proyectos.
- CRUD.
- Panel de administración.
- Identity.
- Formularios de administración.
- Blog dinámico.
- Experiencia dinámica.
- Certificaciones dinámicas.
- Nuevas páginas públicas.
- Selector de idioma adicional.
- Traducciones de textos estructurales que ya utilizan RESX.
- Campos duplicados como TitleEn, DescriptionEn, SummaryEn o SlugEn.
- Tablas duplicadas por idioma.
- Funcionalidades de fases posteriores.

## Requisitos técnicos

Antes de editar:

1. Lee completamente las entidades actuales del dominio.
2. Revisa PortfolioDbContext.cs.
3. Revisa todas las configuraciones EF Core existentes.
4. Revisa la migración inicial aplicada.
5. Revisa los proyectos y referencias entre Domain, Application, Infrastructure y Web.
6. Revisa las pruebas existentes.
7. Comprueba si ya existe alguna entidad, propiedad o configuración relacionada con idiomas o traducciones.
8. Comprueba el estado actual de Git y no modifiques archivos ajenos a esta funcionalidad.

## Diseño esperado

Define un modelo preparado para contenido localizado sin duplicar tablas por idioma.

Como mínimo, analiza y decide cómo representar:

- Código de idioma.
- Entidad principal de contenido.
- Entidad de traducción relacionada.
- Relación entre entidad principal y traducciones.
- Restricción única por entidad y código de idioma.
- Campos traducibles.
- Campos comunes que no deben duplicarse por idioma.
- Longitud máxima de códigos y textos.
- Reglas de borrado.
- Índices necesarios.
- Compatibilidad con las culturas existentes es-ES y en-US.

Utiliza las decisiones documentadas en PORTFOLIO_PROJECT.md. No introduzcas una entidad administrable de idiomas si todavía no es necesaria para las culturas actuales.

Si el diseño requiere concretar el primer contenido traducible, utiliza únicamente el dominio de proyectos y deja preparados los patrones necesarios para reutilizarlos posteriormente en experiencia, certificaciones y blog. No conectes todavía el contenido a la Home.

## Persistencia

- Añade únicamente las entidades y configuraciones necesarias para este modelo.
- Actualiza PortfolioDbContext.cs siguiendo el patrón existente.
- Crea una migración EF Core coherente con el modelo.
- No apliques cambios destructivos.
- No almacenes traducciones estructurales de la interfaz en SQL Server.
- Mantén la base de datos de desarrollo y la configuración mediante User Secrets.
- No añadas credenciales ni cadenas de conexión al repositorio.

## Pruebas

Añade únicamente pruebas útiles para esta funcionalidad.

Como mínimo, valida:

1. Que el modelo contiene la entidad principal y su entidad de traducciones.
2. Que existe la relación esperada.
3. Que la combinación de entidad y código de idioma es única.
4. Que las restricciones principales del modelo están configuradas.
5. Que no se han creado columnas duplicadas por idioma.
6. Que las entidades existentes de las fases anteriores siguen presentes.
7. Que la migración se genera correctamente.

## Forma de trabajo obligatoria

Trabaja de forma incremental:

1. Inspecciona el repositorio.
2. Presenta un plan breve si el cambio afecta a varias capas.
3. Implementa solo el modelo de traducciones.
4. Ejecuta las pruebas y la compilación.
5. Revisa el estado final de Git.
6. Detén el desarrollo para revisión.

No continúes con contenido dinámico, consultas públicas ni administración después de completar esta sección.

## Validación final

Ejecuta:

- dotnet build Portfolio.sln
- Las pruebas disponibles de Portfolio.Tests
- La generación o validación de la migración correspondiente
- git status
- git diff --check

Al finalizar:

- Resume los archivos modificados.
- Explica el modelo de entidades y traducciones.
- Explica las restricciones e índices añadidos.
- Indica el nombre de la migración.
- Indica las pruebas ejecutadas y sus resultados.
- Confirma que no se ha conectado todavía la Home a contenido dinámico.
- Confirma que no se ha implementado CRUD, Identity ni administración.
- No crees ningún commit automáticamente.
- Detén el trabajo para revisión antes de continuar con la siguiente sección.


# ****************************************
# PROMPT PARA REALIZAR LA FASE 6 - PARTE 3
# ****************************************

Quiero continuar la FASE 6 — Funcionalidades del proyecto Portfolio.

Lee y respeta obligatoriamente antes de modificar nada:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md
3. README.md

Ten en cuenta todo el desarrollo realizado en las fases 1, 2, 3, 4, 5 y en las secciones anteriores de la Fase 6.

No sustituyas decisiones existentes, no crees una arquitectura paralela y no reviertas cambios actuales.

## Contexto actual

- Solución: Portfolio.sln
- Framework: .NET 10
- Aplicación: Portfolio.Web
- Arquitectura:
  - Portfolio.Web
  - Portfolio.Application
  - Portfolio.Domain
  - Portfolio.Infrastructure
  - Portfolio.Tests
- Frontend: Blazor Web App con Razor Components
- Renderizado: Interactive Server únicamente cuando aporte interactividad
- Persistencia: EF Core 10 + SQL Server + Code First + Migrations
- DbContext: Portfolio.Infrastructure/PortfolioDbContext.cs
- Base de datos local: PortfolioDb
- User Secrets configurado
- Culturas:
  - es-ES
  - en-US
- Cultura predeterminada: es-ES
- Selector de idioma implementado mediante cookie de ASP.NET Core
- Modelo de proyectos traducibles implementado
- Migración de traducciones aplicada
- `Project` contiene datos comunes:
  - Id
  - RepositoryUrl
  - DemoUrl
  - PreviewImagePath
  - IsFeatured
  - DisplayOrder
- `ProjectTranslation` contiene:
  - ProjectId
  - LanguageCode
  - Title
  - Slug
  - Summary
  - Description
- La migración `AddProjectPreviewImagePath` ya está aplicada a `PortfolioDb`.
- `PreviewImagePath` es opcional y está configurado como `nvarchar(500) NULL`.
- La Home todavía muestra proyectos estáticos, placeholders o estados vacíos.
- `ProjectCard.razor` contiene actualmente un bloque visual provisional para la vista previa.

## Objetivo de esta intervención

Conectar únicamente la sección pública de proyectos de la Home con los datos almacenados en SQL Server.

La implementación debe:

- Consultar proyectos desde SQL Server.
- Utilizar el modelo existente `Project` y `ProjectTranslation`.
- Respetar la cultura actual `es-ES` o `en-US`.
- Utilizar fallback a `es-ES` si no existe la traducción de la cultura actual.
- Mantener únicamente proyectos con una traducción válida.
- Ordenar los proyectos por `DisplayOrder`.
- Utilizar `IsFeatured` según el comportamiento esperado de la sección actual.
- Mostrar `PreviewImagePath` cuando tenga valor.
- Mantener el placeholder visual actual cuando `PreviewImagePath` sea nulo o vacío.
- Utilizar `DemoUrl` únicamente como enlace a la demo pública.
- Utilizar `RepositoryUrl` únicamente como enlace al repositorio.
- No inventar proyectos ni insertar datos de ejemplo.
- Mantener el estado vacío localizado cuando no existan proyectos.

## Uso de PreviewImagePath

`PreviewImagePath` representa la ruta concreta de un archivo de imagen, por ejemplo:

- `/images/projects/portfolio.webp`
- `/images/projects/project-management.png`

No representa:

- Una carpeta.
- La URL de la demo.
- El repositorio.
- El contenido binario de la imagen.

Cuando exista una ruta válida:

- Renderiza una imagen accesible en la tarjeta.
- Utiliza `ProjectTranslation.Title` como texto alternativo, salvo que el diseño actual requiera otra solución localizada.
- Conserva el diseño visual, proporciones y comportamiento responsive existentes.
- Evita imágenes rotas si la ruta está vacía.

Cuando no exista una ruta:

- Mantén el bloque visual placeholder actual.
- No muestres una imagen ficticia.
- No generes una URL por defecto.

No es necesario modificar nuevamente la entidad ni crear otra migración. El campo ya existe y la migración ya está aplicada.

## No implementes todavía

- CRUD.
- Panel de administración.
- Identity.
- Formularios de administración.
- Carga de imágenes.
- Subida de archivos.
- Gestión multimedia.
- Blog dinámico.
- Experiencia dinámica.
- Certificaciones dinámicas.
- Contacto persistente.
- Nuevas entidades.
- Nuevas tablas de traducciones.
- Nuevas páginas públicas.
- Sistema avanzado de publicación.
- Edición de proyectos.
- Categorías o tecnologías persistidas.
- Campos adicionales no necesarios para esta sección.

## Requisitos técnicos

Antes de editar:

1. Revisa `Project` y `ProjectTranslation`.
2. Revisa las configuraciones EF Core actuales.
3. Revisa `PortfolioDbContext`.
4. Revisa `ProjectSection.razor`.
5. Revisa `ProjectCard.razor`.
6. Revisa `Home.razor`.
7. Revisa los estilos de las tarjetas de proyectos en `wwwroot/css/app.css`.
8. Revisa los patrones actuales de `Portfolio.Application`.
9. Revisa las pruebas existentes.
10. Comprueba el estado actual de Git.
11. No modifiques archivos ajenos a esta funcionalidad.

## Capa de aplicación

Implementa la consulta siguiendo la arquitectura existente.

Si todavía no existe un patrón de consultas públicas:

- Crea únicamente el servicio necesario para proyectos.
- Colócalo en la capa que corresponda según la arquitectura actual.
- No introduzcas CQRS completo si el proyecto no lo utiliza.
- No accedas directamente a `PortfolioDbContext` desde los componentes Blazor.
- Utiliza un DTO o modelo de lectura.
- No expongas entidades EF Core directamente a la interfaz.
- Utiliza consultas asíncronas.
- Evita mantener innecesariamente un DbContext vivo dentro del componente.

La consulta debe obtener como mínimo:

- Id del proyecto.
- Título localizado.
- Slug localizado.
- Resumen localizado.
- Descripción localizada si la tarjeta la utiliza.
- RepositoryUrl.
- DemoUrl.
- PreviewImagePath.
- IsFeatured.
- DisplayOrder.

## Selección de idioma

La selección de la traducción debe seguir este orden:

1. Leer `CultureInfo.CurrentUICulture.Name`.
2. Utilizar la traducción de esa cultura si existe.
3. Si no existe, utilizar la traducción `es-ES`.
4. Si tampoco existe `es-ES`, descartar el proyecto de la presentación pública.
5. No mostrar tarjetas incompletas.
6. No crear contenido de fallback directamente en el componente.
7. No duplicar la lógica de localización existente.

Admite únicamente las culturas ya soportadas:

- `es-ES`
- `en-US`

## Componentes y presentación

Conecta la consulta con la sección actual de proyectos.

Mantén:

- La estructura semántica existente.
- El diseño actual.
- Tailwind CSS local.
- `wwwroot/css/app.css`.
- Los tokens semánticos existentes.
- El soporte responsive.
- La navegación mediante teclado.
- El foco visible.
- El contraste.
- `prefers-reduced-motion`.
- Los textos estructurales localizados mediante RESX.

La tarjeta debe utilizar los datos reales del proyecto:

- Título.
- Resumen.
- Imagen de vista previa si existe.
- Tecnologías solo si ya existe una fuente real para ellas.
- Enlace al repositorio si `RepositoryUrl` tiene valor.
- Enlace a la demo si `DemoUrl` tiene valor.

No inventes tecnologías ni estados de publicación.

Si no hay tecnologías persistidas, no añadas badges ficticios. Mantén únicamente los elementos que puedan construirse con datos reales o conserva el diseño de forma neutra y localizada.

Los enlaces externos deben:

- Tener texto o accesibilidad suficiente.
- Ser utilizables mediante teclado.
- Mantener el contraste.
- No romper el layout responsive.

## Estado vacío

Si la consulta no devuelve proyectos válidos:

- Mostrar el estado vacío localizado que ya existe.
- No insertar registros automáticamente.
- No modificar la base de datos con datos de prueba.
- No mostrar tarjetas placeholder como si fueran proyectos reales.

## Imagen de vista previa

Si `PreviewImagePath` tiene valor:

- Renderiza un elemento `img`.
- Usa `PreviewImagePath` como `src`.
- Utiliza el título localizado como `alt`.
- Mantén dimensiones y proporciones coherentes.
- Evita desbordamientos y layout shifts innecesarios.

Si `PreviewImagePath` es nulo o vacío:

- Renderiza el placeholder visual actual.
- Usa el recurso localizado `ProjectVisualPlaceholder`.
- Conserva `aria-hidden="true"` únicamente en el bloque puramente decorativo.

No añadas todavía mecanismos de subida, validación física del archivo ni almacenamiento multimedia.

## Pruebas

Añade únicamente pruebas útiles para esta funcionalidad.

Como mínimo, valida:

1. Que se recuperan proyectos ordenados por `DisplayOrder`.
2. Que se selecciona la traducción de la cultura actual.
3. Que se utiliza fallback a `es-ES`.
4. Que un proyecto sin traducción válida no provoca errores.
5. Que `PreviewImagePath` se devuelve correctamente.
6. Que una ruta de imagen nula no provoca errores.
7. Que `RepositoryUrl` y `DemoUrl` se conservan correctamente.
8. Que una base de datos sin proyectos devuelve una colección vacía.
9. Que el componente no accede directamente al DbContext.
10. Que no se generan tecnologías ni contenido ficticio.
11. Que las entidades y migraciones existentes siguen funcionando.

Utiliza la estrategia de pruebas existente. No introduzcas una infraestructura de testing nueva salvo que sea imprescindible.

## Validación manual

Si el entorno lo permite, comprueba:

- Home sin proyectos.
- Home con un proyecto en `es-ES`.
- Home con un proyecto en `en-US`.
- Fallback a `es-ES` cuando falta la traducción inglesa.
- Proyecto con `PreviewImagePath`.
- Proyecto sin `PreviewImagePath`.
- Proyecto con `DemoUrl`.
- Proyecto con `RepositoryUrl`.
- Proyecto sin enlaces.
- Orden por `DisplayOrder`.
- Cambio de idioma desde la Navbar.
- Integración con el selector de tema.
- Visualización responsive en escritorio y móvil.
- Navegación mediante teclado.
- Ausencia de imágenes rotas.

## Validación final

Ejecuta:

- `dotnet build Portfolio.sln`
- Todas las pruebas disponibles de `Portfolio.Tests`
- `git diff --check`
- `git status`

No crees nuevas migraciones salvo que el modelo actual realmente lo necesite. `PreviewImagePath` ya existe en la entidad y en la base de datos.

## Forma de trabajo obligatoria

Trabaja de forma incremental:

1. Inspecciona los componentes y patrones actuales.
2. Presenta un plan si el cambio afecta a varias capas.
3. Implementa únicamente la consulta pública de proyectos.
4. Conecta la sección de proyectos de la Home.
5. Sustituye el placeholder por la imagen cuando exista `PreviewImagePath`.
6. Conserva el placeholder cuando no exista imagen.
7. Añade las pruebas necesarias.
8. Ejecuta compilación y pruebas.
9. Revisa el estado final de Git.
10. Detén el desarrollo para revisión.

Al finalizar:

- Resume los archivos modificados.
- Explica cómo se consultan los proyectos.
- Explica cómo se selecciona la traducción.
- Explica el fallback a `es-ES`.
- Explica cómo se utiliza `PreviewImagePath`.
- Indica las pruebas ejecutadas y sus resultados.
- Confirma que no se ha implementado CRUD, administración, Identity, carga de imágenes ni otras secciones dinámicas.
- No crees ningún commit automáticamente.
- Detén el trabajo para revisión antes de continuar con la siguiente parte de la Fase 6.


# ****************************************
# PROMPT PARA REALIZAR LA FASE 6 - PARTE 4
# ****************************************

Quiero continuar la FASE 6 — Funcionalidades del proyecto Portfolio.

Lee y respeta obligatoriamente antes de modificar nada:

1. PORTFOLIO_PROJECT.md
2. .github/copilot-instructions.md
3. README.md

Ten en cuenta todo el desarrollo realizado en las fases 1, 2, 3, 4, 5 y en las secciones anteriores de la Fase 6:

- Selector de idioma en la Navbar.
- Persistencia de cultura mediante cookie de ASP.NET Core.
- Modelo de proyectos traducibles.
- Migraciones de proyectos y traducciones aplicadas.
- Campo `PreviewImagePath` para proyectos.
- Servicio de consulta pública de proyectos.
- DTO de lectura de proyectos.
- Fallback de traducciones a `es-ES`.
- Conexión de proyectos dinámicos con la Home.
- Renderizado de imágenes, repositorio, demo y estado vacío localizado.

No sustituyas decisiones existentes, no crees una arquitectura paralela y no reviertas cambios actuales.

## Objetivo de esta intervención

Implementar únicamente la sección pública de certificaciones dinámicas.

La sección debe consultar y mostrar certificaciones desde SQL Server utilizando la arquitectura existente y manteniendo el diseño actual de `CertificationsSection.razor` y `CertificationCard.razor`.

Antes de editar, revisa si el modelo actual de certificaciones es suficiente o si necesita separar datos comunes y datos traducibles siguiendo el patrón utilizado para proyectos.

## Contexto actual

- Solución: Portfolio.sln
- Framework: .NET 10
- Aplicación: Portfolio.Web
- Frontend: Blazor Web App con Razor Components
- Renderizado: Interactive Server únicamente cuando aporte interactividad
- Persistencia: EF Core 10 + SQL Server + Code First + Migrations
- DbContext: Portfolio.Infrastructure/PortfolioDbContext.cs
- Base de datos: PortfolioDb
- Culturas:
  - es-ES
  - en-US
- Cultura predeterminada: es-ES
- Selector de idioma implementado mediante cookie.
- La sección de proyectos ya consulta contenido dinámico.
- Las certificaciones actuales todavía pueden utilizar datos estáticos, placeholders o estado vacío.

## Alcance permitido

Implementa únicamente:

- Revisión del modelo actual de `Certification`.
- Modelo de traducciones de certificaciones si es necesario.
- Configuraciones EF Core necesarias.
- Migración necesaria, solo si el modelo cambia.
- Servicio de consulta pública de certificaciones.
- DTO de lectura.
- Selección de traducción actual.
- Fallback a `es-ES`.
- Conexión con `CertificationsSection.razor`.
- Actualización de `CertificationCard.razor`.
- Recursos RESX necesarios.
- Pruebas útiles para esta sección.

## No implementes todavía

- Proyectos adicionales ni cambios en proyectos ya implementados.
- Experiencia dinámica.
- Blog dinámico.
- Contacto persistente.
- CRUD.
- Panel de administración.
- Identity.
- Formularios administrativos.
- Carga de archivos.
- Gestión multimedia.
- Nuevas páginas públicas.
- Tecnologías persistidas si no son necesarias para certificaciones.
- Funcionalidades de publicación avanzada.
- Funcionalidades de fases posteriores.

## Requisitos técnicos

Antes de modificar:

1. Revisa `Certification`.
2. Revisa `CertificationConfiguration`.
3. Revisa `PortfolioDbContext`.
4. Revisa las migraciones actuales.
5. Revisa `CertificationsSection.razor`.
6. Revisa `CertificationCard.razor`.
7. Revisa los estilos existentes.
8. Revisa el servicio y DTO utilizados para proyectos.
9. Revisa los recursos RESX.
10. Revisa las pruebas existentes.
11. Comprueba el estado de Git.

Reutiliza el patrón empleado para proyectos siempre que sea aplicable. No copies código sin analizar si la certificación requiere campos comunes y campos traducibles diferentes.

## Modelo y traducciones

Determina qué campos son comunes y cuáles traducibles.

Como mínimo, analiza:

- Nombre de la certificación.
- Emisor.
- Fecha de obtención.
- URL de credencial.
- Orden de presentación.
- Slug, si realmente es necesario.
- Descripción, solo si la interfaz actual la necesita.

No añadas campos que no tengan una necesidad real en la interfaz.

Si se necesita una tabla de traducciones:

- No utilices columnas como `NameEn` o `IssuerEn`.
- No dupliques tablas por idioma.
- Mantén `es-ES` y `en-US`.
- Utiliza una restricción única por entidad e idioma.
- Mantén los campos comunes en `Certification`.
- Mantén los campos traducibles en una entidad de traducción.
- Conserva los datos existentes mediante una migración segura.
- No apliques la migración sin revisarla.

Si el modelo actual es suficiente para la sección y no requiere traducciones, no crees entidades nuevas innecesariamente.

## Consulta pública

Implementa la consulta siguiendo el patrón existente de proyectos:

- Contrato en `Portfolio.Application`.
- DTO de lectura.
- Implementación EF Core en `Portfolio.Infrastructure`.
- `IDbContextFactory<PortfolioDbContext>`.
- `AsNoTracking()`.
- Consultas asíncronas.
- Proyección a DTO.
- Ningún acceso directo al DbContext desde componentes Blazor.

La consulta debe:

- Ordenar por `DisplayOrder`.
- Seleccionar la traducción de la cultura actual.
- Aplicar fallback a `es-ES`.
- Descartar certificaciones sin traducción válida.
- Devolver únicamente los campos necesarios para la tarjeta.
- Devolver una colección vacía si no hay datos.

## Componentes públicos

Conecta la consulta con:

- `CertificationsSection.razor`.
- `CertificationCard.razor`.

Mantén:

- HTML semántico.
- Diseño actual.
- Tailwind CSS local.
- `wwwroot/css/app.css`.
- Tokens semánticos.
- Responsive.
- Focus-visible.
- Navegación por teclado.
- Contraste adecuado.
- `prefers-reduced-motion`.
- Textos visibles mediante recursos RESX.

Si no hay certificaciones:

- Muestra un estado vacío localizado.
- No insertes datos ficticios.
- No muestres placeholders como si fueran certificaciones reales.

No añadas imágenes si el modelo actual no las contempla y no son necesarias para esta sección.

## Localización

Utiliza `CultureInfo.CurrentUICulture.Name`.

El orden de selección será:

1. Cultura actual.
2. Fallback a `es-ES`.
3. Descartar la certificación si no existe ninguna traducción válida.

No dupliques la lógica de localización dentro del componente.

## Pruebas

Añade únicamente pruebas útiles.

Como mínimo, valida:

1. Orden por `DisplayOrder`.
2. Selección de traducción actual.
3. Fallback a `es-ES`.
4. Descarte de certificaciones sin traducción válida.
5. Conservación de fecha, URL y demás campos comunes.
6. Colección vacía sin registros.
7. Restricciones del modelo si se crean entidades nuevas.
8. Que el componente no accede directamente al DbContext.
9. Que no se modifica el comportamiento existente de proyectos.

Utiliza la infraestructura de pruebas actual. No añadas una nueva infraestructura salvo que sea imprescindible.

## Validación final

Ejecuta:

- `dotnet build Portfolio.sln`
- Todas las pruebas disponibles de `Portfolio.Tests`
- `git diff --check`
- `git status`

Si el modelo cambia:

- Genera la migración correspondiente.
- Revísala.
- Verifica que no pierde datos.
- Aplícala únicamente a la base de datos local `PortfolioDb`.
- Confirma que no quedan migraciones pendientes.

Valida, si es posible:

- Home sin certificaciones.
- Certificación en `es-ES`.
- Certificación en `en-US`.
- Fallback a español.
- Orden de certificaciones.
- URLs de credencial.
- Cambio de idioma desde la Navbar.
- Selector de tema.
- Escritorio.
- Móvil.
- Navegación mediante teclado.

## Forma de trabajo obligatoria

Trabaja de forma incremental:

1. Inspecciona el modelo y componentes actuales.
2. Presenta un plan si el cambio afecta a varias capas.
3. Decide si el modelo actual necesita traducciones.
4. Implementa únicamente certificaciones dinámicas.
5. Añade migración solo si es necesaria.
6. Conecta la sección pública.
7. Añade las pruebas.
8. Ejecuta compilación y pruebas.
9. Revisa el estado final de Git.
10. Detén el desarrollo para revisión.

Al finalizar:

- Resume los archivos modificados.
- Explica el modelo utilizado.
- Explica la consulta.
- Explica la selección de idioma y el fallback.
- Indica si se creó y aplicó una migración.
- Indica las pruebas ejecutadas y sus resultados.
- Confirma que proyectos, experiencia, blog, CRUD, administración e Identity no se han modificado ni implementado.
- No crees ningún commit automáticamente.
- Detén el trabajo para revisión antes de continuar con la siguiente sección de la Fase 6.


# ****************************************
# PROMPT PARA REALIZAR LA FASE 6 - PARTE 5
# ****************************************

Quiero continuar la FASE 6 — Funcionalidades del proyecto Portfolio.

Implementa únicamente la sección pública de experiencia dinámica.

Ten en cuenta todas las decisiones y funcionalidades ya existentes en el proyecto:

- .NET 10.
- Blazor Web App con Razor Components.
- Interactive Server únicamente cuando aporte interactividad.
- Arquitectura separada en Domain, Application, Infrastructure, Web y Tests.
- EF Core 10 con SQL Server, Code First y Migrations.
- Selector de idioma en la Navbar.
- Persistencia de cultura mediante cookie de ASP.NET Core.
- Culturas `es-ES` y `en-US`.
- Localización mediante recursos RESX.
- Proyectos dinámicos con traducciones, fallback a `es-ES`, imágenes, repositorio, demo y estado vacío localizado.
- Certificaciones dinámicas con traducciones, fallback a `es-ES`, fecha, credencial, imagen opcional y estado vacío localizado.
- Uso de `IDbContextFactory<PortfolioDbContext>` para consultas públicas.
- Ningún acceso directo al DbContext desde componentes Blazor.
- Pruebas existentes en `Portfolio.Tests`.

No reviertas, sustituyas ni dupliques decisiones ya implementadas.

## Objetivo

Convertir la sección pública de experiencia para que consulte y muestre datos desde SQL Server utilizando la arquitectura existente y manteniendo el diseño actual de `ExperienceTimeline.razor`.

## Alcance permitido

Implementa únicamente:

- Revisión del modelo actual `Experience`.
- Separación entre campos comunes y campos traducibles si es necesario.
- Entidad de traducciones de experiencia si el modelo lo requiere.
- Configuraciones EF Core.
- Migración necesaria únicamente si cambia el modelo.
- Contrato de consulta en `Portfolio.Application`.
- DTO de lectura.
- Selector de traducción actual.
- Fallback a `es-ES`.
- Servicio de consulta pública en `Portfolio.Infrastructure`.
- Conexión con `ExperienceTimeline.razor`.
- Actualización de componentes relacionados con experiencia únicamente si es necesario.
- Recursos RESX necesarios.
- Pruebas útiles para experiencia dinámica.

## No implementes todavía

- Blog dinámico.
- Contacto persistente.
- CRUD.
- Panel de administración.
- Identity.
- Formularios administrativos.
- Carga de archivos.
- Gestión multimedia adicional.
- Nuevas páginas públicas.
- Tecnologías persistidas.
- Funcionalidades de fases posteriores.
- Cambios funcionales en proyectos o certificaciones ya implementados.

## Revisión obligatoria antes de editar

Inspecciona:

1. `Experience`.
2. `ExperienceConfiguration`.
3. `PortfolioDbContext`.
4. Migraciones actuales.
5. `ExperienceTimeline.razor`.
6. Componentes relacionados con experiencia.
7. Estilos actuales de la timeline.
8. Servicio y DTO de proyectos.
9. Servicio y DTO de certificaciones.
10. Recursos RESX.
11. Pruebas existentes.
12. Estado actual de Git.

Antes de editar, presenta un plan si el cambio afecta a varias capas.

## Modelo y traducciones

Determina qué campos son comunes y cuáles traducibles siguiendo los patrones utilizados para proyectos y certificaciones.

Analiza como mínimo:

- Rol o puesto.
- Empresa u organización.
- Resumen o descripción.
- Fecha de inicio.
- Fecha de finalización.
- Orden de presentación.
- Indicador de experiencia actual, solo si la interfaz existente lo necesita.

No añadas campos que no tengan una necesidad real en la interfaz actual.

Si se necesita una tabla de traducciones:

- No utilices columnas como `RoleTitleEn`, `CompanyNameEn` o `SummaryEn`.
- No dupliques tablas por idioma.
- Mantén `es-ES` y `en-US`.
- Utiliza una clave o restricción única por entidad e idioma.
- Mantén los campos comunes en `Experience`.
- Mantén los campos traducibles en una entidad de traducción.
- Conserva los datos existentes mediante una migración segura.
- Revisa la migración antes de aplicarla.
- No añadas slug, imagen ni campos multimedia si la interfaz actual no los necesita.

## Consulta pública

Reutiliza el patrón existente:

- Contrato en `Portfolio.Application`.
- DTO de lectura.
- Implementación EF Core en `Portfolio.Infrastructure`.
- `IDbContextFactory<PortfolioDbContext>`.
- `AsNoTracking()`.
- Consultas asíncronas.
- Proyección o selección a DTO.
- Ningún acceso directo al DbContext desde componentes Blazor.

La consulta debe:

- Ordenar por `DisplayOrder`.
- Seleccionar la traducción de la cultura actual.
- Utilizar `CultureInfo.CurrentUICulture.Name`.
- Aplicar fallback a `es-ES`.
- Descartar experiencias sin traducción válida.
- Devolver únicamente los campos necesarios para la timeline.
- Devolver una colección vacía si no existen experiencias.

El orden de selección debe ser:

1. Cultura actual.
2. Fallback a `es-ES`.
3. Descartar la experiencia si no existe ninguna traducción válida.

No dupliques la lógica de localización dentro de los componentes.

## Componentes públicos

Conecta la consulta con:

- `ExperienceTimeline.razor`.
- Componentes relacionados únicamente si es necesario.

Mantén:

- El diseño visual actual de la timeline.
- HTML semántico.
- Navegación mediante teclado.
- `focus-visible`.
- Diseño responsive.
- Contraste suficiente.
- Soporte para `prefers-reduced-motion`.
- Tailwind CSS local.
- `wwwroot/css/app.css`.
- Tokens semánticos existentes.
- Textos visibles mediante recursos RESX.

Si no hay experiencias:

- Muestra un estado vacío localizado.
- No insertes experiencias ficticias.
- No muestres placeholders como si fueran experiencias reales.

## Pruebas

Utiliza la infraestructura de pruebas existente.

Valida como mínimo:

1. Orden por `DisplayOrder`.
2. Selección de la traducción actual.
3. Fallback a `es-ES`.
4. Descarte de experiencias sin traducción válida.
5. Conservación de fechas y demás campos comunes.
6. Colección vacía sin registros.
7. Restricciones del modelo si se crean entidades nuevas.
8. Que los componentes no acceden directamente al DbContext.
9. Que no se modifica el comportamiento existente de proyectos.
10. Que no se modifica el comportamiento existente de certificaciones.

## Migración

Si el modelo cambia:

- Genera la migración correspondiente.
- Revísala manualmente.
- Verifica que no pierda datos existentes.
- Aplícala únicamente a la base de datos local `PortfolioDb`.
- Confirma que no quedan migraciones pendientes.

## Validación final

Ejecuta:
dotnet build Portfolio.sln


Ejecuta todas las pruebas de `Portfolio.Tests`.

Ejecuta:
git diff --check git status


Valida, si es posible:

- Home sin experiencias.
- Experiencia en `es-ES`.
- Experiencia en `en-US`.
- Fallback a español.
- Orden de experiencias.
- Fechas correctas.
- Cambio de idioma desde la Navbar.
- Selector de tema.
- Vista de escritorio.
- Vista móvil.
- Navegación mediante teclado.

## Forma de trabajo obligatoria

Trabaja incrementalmente:

1. Inspecciona el modelo y los componentes actuales.
2. Presenta un plan si procede.
3. Decide si el modelo necesita traducciones.
4. Implementa únicamente experiencia dinámica.
5. Añade migración solo si es necesaria.
6. Conecta la sección pública.
7. Añade las pruebas.
8. Ejecuta compilación y pruebas.
9. Revisa el estado final de Git.
10. Detén el desarrollo para revisión.

Al finalizar:

- Resume los archivos modificados.
- Explica el modelo utilizado.
- Explica la consulta.
- Explica la selección de idioma y el fallback.
- Indica si se creó y aplicó una migración.
- Indica las pruebas ejecutadas y sus resultados.
- Confirma que proyectos y certificaciones no se han modificado funcionalmente.
- Confirma que blog, contacto, CRUD, administración e Identity no se han implementado.
- No crees ningún commit automáticamente.
- Detén el trabajo para revisión antes de continuar con la siguiente sección de la Fase 6.