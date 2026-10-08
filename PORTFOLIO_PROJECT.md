# Portfolio Personal — Especificación y Guía del Proyecto

> Documento de referencia para el desarrollo del portfolio personal.  
> Su objetivo es servir como guía técnica y funcional para mantener las decisiones acordadas, evitar desviaciones arquitectónicas y permitir que cualquier agente/desarrollador pueda continuar el proyecto con el mismo criterio.

---

## 1. Objetivo del proyecto

Construir desde cero un **portfolio profesional personal** que cumpla dos objetivos simultáneos:

1. Servir como web pública para presentar el perfil profesional, experiencia, proyectos, certificaciones, blog y formas de contacto.
2. Servir como **proyecto demostrable de desarrollo profesional**, mostrando buenas prácticas de arquitectura, desarrollo .NET, Blazor, bases de datos, autenticación, gestión de contenidos, responsive design, internacionalización, Docker y despliegue.

El proyecto no debe parecer una plantilla genérica. La parte pública debe tener una identidad visual propia, moderna y profesional, evitando depender de una estética predeterminada de Bootstrap.

---

# 2. Stack tecnológico

## Backend

- **.NET 10**
- **ASP.NET Core**
- **C#**
- **Entity Framework Core**
- **ASP.NET Core Identity**
- **SQL Server**

### Motivos

**.NET 10 / ASP.NET Core**

Es la plataforma principal elegida porque el objetivo profesional del proyecto está muy orientado al ecosistema .NET. Permite demostrar conocimientos actuales de desarrollo backend, arquitectura, seguridad, inyección de dependencias, configuración, validación y despliegue.

**C#**

Será el lenguaje principal del proyecto por ser el lenguaje central del ecosistema .NET y una de las tecnologías principales del perfil profesional.

**Entity Framework Core**

Se utilizará como ORM para evitar tener que gestionar manualmente toda la persistencia mediante ADO.NET y permitir trabajar con entidades, relaciones y migraciones de forma estructurada.

**SQL Server**

Será la base de datos principal. Encaja directamente con el perfil .NET y permite demostrar experiencia real trabajando con bases de datos relacionales.

**ASP.NET Core Identity**

Se utilizará para la autenticación y autorización del área de administración. No se utilizará una URL oculta como mecanismo de seguridad.

---

# 3. Frontend

## Aplicación

- **Blazor Web App**
- Razor Components
- HTML5
- CSS
- C#
- JavaScript únicamente cuando sea necesario

### Motivo de elegir Blazor

Blazor permite construir la interfaz utilizando C# y componentes reutilizables, encajando de forma natural con el backend .NET.

No se utilizará React en este proyecto.

React podrá demostrarse posteriormente mediante un proyecto independiente, evitando forzar varias tecnologías frontend dentro de un mismo portfolio.

## Estrategia de renderizado

No se plantea una SPA pura.

La aplicación debe aprovechar las capacidades de **Blazor Web App**, priorizando:

- SEO
- carga rápida
- accesibilidad
- contenido indexable
- interactividad únicamente donde aporte valor

La parte pública tendrá prioridad por rendimiento e indexación, mientras que el área administrativa podrá utilizar componentes altamente interactivos.

### Implementación inicial

La aplicación utiliza el modelo **Blazor Web App con Razor Components** y renderizado interactivo **Interactive Server**. La interactividad se habilita en `Routes.razor`; no se convierte la aplicación en una SPA pura ni se utiliza JavaScript para sustituir la navegación de Blazor.

La aplicación usa `MapRazorComponents<App>()` y `AddInteractiveServerRenderMode()`.

---

# 4. Sistema visual

## Parte pública

Se utilizará:

- **Tailwind CSS**
- CSS personalizado
- Componentes Blazor propios

No se utilizará Bootstrap como base visual.

### Motivo

Bootstrap es excelente como framework general, pero para este proyecto se busca una identidad visual propia y evitar el aspecto de portfolio genérico basado en componentes predeterminados.

Tailwind permite construir un sistema visual personalizado manteniendo un desarrollo rápido y consistente.

### Implementación actual del sistema visual

Tailwind CSS se mantiene como dependencia local de `Portfolio.Web` mediante `@tailwindcss/cli` y los scripts npm `css:build` y `css:watch`. La entrada es `Styles/tailwind.css`, el análisis incluye `Components` y el resultado se genera en `wwwroot/css/tailwind.css`. Los estilos propios están en `wwwroot/css/app.css` e incluyen tokens semánticos, tipografía local o del sistema, temas, foco visible, layout responsive y `prefers-reduced-motion`. `App.razor` referencia `css/tailwind.css` y `css/app.css`.

Se crearán componentes reutilizables propios, por ejemplo:

- `PortfolioNavbar.razor`
- `HeroSection.razor`
- `AboutSection.razor`
- `ExperienceTimeline.razor`
- `ProjectCard.razor`
- `ProjectCarousel.razor`
- `CertificationCard.razor`
- `FeaturedPost.razor`
- `ContactSection.razor`
- `PortfolioFooter.razor`

También podrán existir componentes visuales base como:

- botones
- cards
- badges de tecnologías
- títulos de sección
- elementos de timeline
- elementos de navegación

## Área administrativa

Se utilizará **MudBlazor**.

### Motivo

En administración interesa disponer rápidamente de componentes maduros para:

- formularios
- tablas
- diálogos
- validación visual
- grids
- selección de archivos
- controles interactivos

No es necesario reinventar estos componentes para el área privada.

Por tanto:

```text
Parte pública
    Tailwind + CSS propio + componentes Blazor

Administración
    MudBlazor
```

---

# 5. Editor de contenidos

Para el blog se utilizará:

- **TinyMCE 8**
- Integración con Blazor

### Motivo

El blog necesita un editor de texto enriquecido que permita administrar contenido sin tener que escribir HTML manualmente.

TinyMCE permite gestionar:

- títulos
- párrafos
- listas
- enlaces
- imágenes
- formato de texto
- contenido enriquecido

El contenido textual estructurado se almacenará en SQL Server.

Las imágenes y archivos no se almacenarán como BLOB en SQL Server salvo que exista una razón concreta para hacerlo.

---

# 6. Almacenamiento de archivos

Los archivos multimedia se almacenarán en almacenamiento persistente externo, con destino previsto en **QNAP**.

Ejemplos:

- imágenes de proyectos
- imágenes de certificaciones
- imágenes destacadas del blog
- imágenes, diplomas y cartas de recomendación asociados a una experiencia laboral, cuando exista autorización para gestionarlos y publicarlos
- documentos PDF
- otros archivos multimedia

La aplicación y la base de datos deben permanecer desacopladas del almacenamiento físico de estos archivos.

Las ubicaciones se configuran por módulo mediante las opciones `Portfolio:ProjectPreviewImages:Directory`, `Portfolio:ExperienceAttachments:Directory`, `Portfolio:CertificationMedia:Directory` y `Portfolio:BlogImages:Directory`; sus valores predeterminados están bajo `App_Data`. Los servicios crean automáticamente el directorio configurado y las carpetas padre que falten al preparar la primera carga de un archivo; no se crean durante el arranque ni al guardar contenido que aún no tiene archivos. Al eliminar archivos o recursos se registran los errores de filesystem. La limpieza de carpetas por elemento es no recursiva y elimina solo directorios vacíos derivados del GUID del elemento después de confirmar el borrado en base de datos; nunca elimina raíces compartidas ni otros datos. Los proyectos guardan previews directamente en la raíz compartida `ProjectPreviewImages`, que no se elimina al borrar un proyecto. El patrón `**/App_Data/` de `.gitignore` evita versionar archivos de datos locales en cualquier ubicación del repositorio.

Los documentos de experiencia pueden contener datos personales o información confidencial. No serán públicos por defecto: su publicación deberá ser explícita y contar con autorización. En la Fase 7 se determinará si una experiencia necesita varios adjuntos y cómo modelar sus metadatos y permisos; la entidad `Experience` actual no tiene campos de media.

Esto facilitará el despliegue mediante Docker y evitará perder archivos al recrear un contenedor.

---

# 7. Arquitectura

La estructura prevista es:

```text
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
```

## Portfolio.Domain

Contendrá el núcleo del dominio:

- entidades
- value objects si son necesarios
- enumeraciones
- reglas de dominio que realmente pertenezcan al dominio

No debe depender de infraestructura ni de la interfaz web.

Ejemplos futuros de entidades:

- Project
- Experience
- Certification
- BlogPost
- Category
- Tag

Estas entidades son orientativas y **no deben crearse todas desde el principio**.

El modelo definitivo se derivará de las necesidades reales de cada módulo.

---

## Portfolio.Application

Contendrá la lógica de aplicación:

- casos de uso
- servicios de aplicación
- DTOs
- validadores
- interfaces necesarias
- orquestación de operaciones

Su responsabilidad será coordinar las operaciones de negocio sin depender directamente de detalles concretos de infraestructura.

---

## Portfolio.Infrastructure

Contendrá las implementaciones técnicas:

- Entity Framework Core
- DbContext
- configuraciones de entidades
- migraciones
- implementación de repositorios/servicios si realmente son necesarios
- SQL Server
- ASP.NET Core Identity
- almacenamiento de archivos
- servicios de correo
- otras integraciones externas

---

## Portfolio.Web

Será la aplicación Blazor:

- páginas públicas
- componentes
- layouts
- navegación
- administración
- autenticación de usuario
- recursos de idioma
- configuración de la interfaz

---

## Portfolio.Tests

Contendrá pruebas automatizadas.

Se añadirán progresivamente a medida que existan funcionalidades que merezca la pena cubrir.

No se pretende crear una cantidad artificial de tests desde el primer día.

---

# 8. Dependencias entre capas

La arquitectura conceptual será:

```text
                 Portfolio.Web
                       │
                       ▼
              Portfolio.Application
                       │
                       ▼
                 Portfolio.Domain
                       ▲
                       │
              Portfolio.Infrastructure
```

Infrastructure implementará las necesidades técnicas de Application/Domain.

El objetivo es evitar que el dominio termine acoplado a:

- SQL Server
- EF Core
- Blazor
- MudBlazor
- almacenamiento físico
- proveedores externos

---

# 9. Base de datos

Se utilizará:

**SQL Server + Entity Framework Core + Code First + Migrations**

## ¿Por qué Code First?

El proyecto se está construyendo desde cero y la aplicación y el modelo de datos evolucionarán conjuntamente.

Code First permite:

1. Diseñar el modelo en código.
2. Generar migraciones.
3. Revisar los cambios.
4. Aplicarlos a la base de datos.

Ejemplo:

```text
Entidad inicial
      ↓
Migration 001
      ↓
Base de datos

Se necesita un nuevo campo
      ↓
Modificar entidad
      ↓
Migration 002
      ↓
Actualizar base de datos
```

## Cambios de esquema

Añadir campos o tablas será normalmente sencillo mediante nuevas migraciones.

Los cambios que puedan afectar a datos existentes deben revisarse cuidadosamente.

Especialmente:

- cambios de tipo de datos
- eliminación de columnas
- renombrado de columnas
- cambios de relaciones

En un renombrado de columna se debe revisar la migración generada para evitar que EF interprete el cambio como:

```text
Eliminar columna antigua
+
Crear columna nueva
```

cuando realmente se pretende conservar los datos mediante un renombrado.

## Entornos

Las migraciones pertenecen a la misma aplicación y pueden existir diferentes estados entre entornos.

Ejemplo:

```text
Development → Migration 007
Production  → Migration 004
```

Al desplegar una nueva versión, Production puede aplicar las migraciones pendientes:

```text
004 → 005 → 006 → 007
```

No significa que diferentes aplicaciones deban compartir y administrar independientemente las mismas tablas.

---

# 10. Internacionalización

La aplicación se diseñará desde el principio preparada para múltiples idiomas.

Inicialmente:

```text
Español (`es-ES`)
```

Posteriormente:

```text
Español
English
```

## Regla importante

No se crearán páginas duplicadas:

```text
HomeSpanish.razor
HomeEnglish.razor
```

Se utilizará el sistema de recursos/localización. La configuración actual admite `es-ES` y `en-US`, con `es-ES` como cultura y UI culture predeterminada. Los recursos se organizan mediante `SharedResource` y los archivos `SharedResource.resx`, `SharedResource.es.resx` y `SharedResource.en.resx`.

Desde el principio, incluso aunque solo exista español, los textos de interfaz deben utilizar recursos.

Ejemplo conceptual:

```text
About = "Sobre mí"
Projects = "Proyectos"
Certifications = "Certificaciones"
Blog = "Blog"
Contact = "Contacto"
```

Los recursos en inglés están preparados para los textos existentes. La Navbar ya dispone de selector de idioma y la cultura se persiste mediante la cookie de localización de ASP.NET Core, manteniendo el sistema de recursos sin duplicar las pantallas.

### Culturas y contenido localizado

Mientras el portfolio mantenga únicamente español e inglés, las culturas soportadas permanecerán definidas en el código (`es-ES` y `en-US`). Los textos estructurales de la interfaz continuarán utilizando recursos RESX (`SharedResource.es.resx` y `SharedResource.en.resx`).

El contenido dinámico del portfolio se almacenará en SQL Server y se localizará mediante una entidad principal y una tabla de traducciones, evitando duplicar tablas por idioma o añadir columnas como `TitleEn` y `DescriptionEn`.

Ejemplo conceptual:

```text
Projects
    Id
    RepositoryUrl
    DemoUrl
    PreviewImagePath
    IsFeatured
    DisplayOrder

ProjectTranslations
    ProjectId
    LanguageCode
    Title
    Slug
    Summary
    Description
```

Se aplicará el mismo patrón a experiencias, certificaciones y artículos. Cada traducción utilizará los códigos culturales `es-ES` o `en-US`, con una restricción única sobre la combinación de la entidad y el código de idioma. Los datos comunes permanecerán en la entidad principal y los campos traducibles en su tabla de traducciones.

Si en el futuro se necesitan más idiomas, se podrá evolucionar la lista de culturas a una entidad `Language` administrable sin duplicar las tablas de contenido. Esta evolución requerirá valorar también un proveedor de localización para los textos estructurales; añadir un idioma a SQL Server no crea por sí solo los recursos RESX ni sus traducciones.

---

# 11. Tema visual

La web tendrá tres estados:

```text
☀ Claro
🌙 Oscuro
🖥 Sistema
```

## Comportamiento inicial

Por defecto se utilizará:

```text
Sistema
```

Esto significa que la web respetará la preferencia del sistema/navegador mediante la configuración correspondiente (`prefers-color-scheme`).

Ejemplo:

```text
Sistema operativo/navegador claro
        ↓
Portfolio claro

Sistema operativo/navegador oscuro
        ↓
Portfolio oscuro
```

## Preferencia manual

El usuario podrá seleccionar:

- Claro
- Oscuro
- Sistema

La preferencia elegida se guardará en **localStorage** para mantenerla entre visitas.

El sistema de colores estará centralizado para poder cambiar la identidad visual posteriormente sin rehacer todos los componentes.

La implementación actual mantiene esta lógica en `wwwroot/js/theme.js`, sin duplicar `localStorage` en C#. La API `portfolioTheme` expone `get`, `set`, `apply` y `options`, y `ThemeSelector.razor` la consume mediante JS interop.

---

# 12. Identidad visual y logo

El portfolio tendrá un logo propio.

La idea conceptual es integrar tres elementos:

- **I** → Ismael
- **π** → referencia a la inicial del primer apellido
- **Q** → referencia a la inicial del segundo apellido

El diseño gráfico definitivo se realizará posteriormente.

Inicialmente se reservará el espacio correspondiente:

```text
[ LOGO ]
```

No se debe bloquear el desarrollo esperando al logo definitivo.

El logo final podrá convertirse posteriormente en parte de la identidad visual completa:

- navbar
- favicon
- footer
- redes profesionales
- CV
- otros elementos del portfolio

## Colores

La paleta inicial se elegirá durante el desarrollo visual.

No se considera definitiva.

Debe estar centralizada mediante variables/tokens de diseño para permitir cambiarla fácilmente si durante el desarrollo se descubre que una combinación funciona mejor.

---

# 13. Estructura pública

La navegación principal será:

```text
Sobre mí
Proyectos
Certificados
Blog
Contacto
```

El logo llevará a:

```text
/
```

No es necesario añadir una opción explícita llamada "Inicio" si el logo ya cumple esa función.

En móvil se utilizará una navegación adaptada, previsiblemente mediante menú desplegable/hamburguesa.

### Implementación de la Fase 3

`MainLayout.razor` usa `header`, `main`, `footer` y `@Body`; `NavMenu.razor` implementa la navbar y el espacio provisional `[ LOGO ]`; `PortfolioFooter.razor` muestra identidad, año y derechos reservados; y `ThemeSelector.razor` se integra en la navbar. Los enlaces apuntan provisionalmente a anchors de la Home. En escritorio se muestran horizontalmente y el selector de tema se alinea a la derecha. En móvil, por debajo de `48rem`, el botón usa `aria-expanded`, el panel permanece oculto hasta abrirlo y se cierra al seleccionar un enlace.

---

# 14. Administración

Ruta principal:

```text
/admin
```

El acceso estará protegido mediante:

- autenticación
- autorización
- ASP.NET Core Identity

No se utilizará ocultar la URL como mecanismo de seguridad.

## Login

Inicialmente no habrá un botón de login visible para el público.

El administrador podrá acceder escribiendo:

```text
/admin
```

Si no está autenticado:

```text
/admin
    ↓
Login
    ↓
Autenticación correcta
    ↓
/admin
```

Si ya está autenticado:

```text
/admin
    ↓
Panel de administración
```

## Icono de administración

La rueda dentada:

```text
⚙
```

estará oculta para usuarios anónimos.

Después de iniciar sesión aparecerá discretamente en la navbar.

Conceptualmente:

```text
Usuario anónimo
→ no ve ⚙

Administrador autenticado
→ ve ⚙
```

La visibilidad del icono es únicamente una decisión de UX. La seguridad real se consigue mediante autenticación y autorización.

Inicialmente solo existirá un usuario administrador.

### Responsive del área administrativa

Las rutas administrativas deben seguir siendo utilizables en móvil sin desbordamiento horizontal. El layout compartido reorganiza cabecera, controles y navegación; por debajo de `48rem`, los enlaces de navegación se disponen en dos columnas. Los listados `MudTable` usan el breakpoint `Sm` para presentar sus filas de forma adaptada y las acciones pueden ajustarse al ancho disponible. Formularios, selectores, botones, alertas, tablas, vistas previas y cargas de archivos también deben reducir o envolver su contenido según el espacio. TinyMCE local se limita al ancho del contenedor y usa `toolbar_mode: "sliding"` para conservar las herramientas en pantallas estrechas.

---

# 15. Home

El Home será la principal carta de presentación del portfolio.

Estructura prevista:

```text
Navbar
   ↓
Hero
   ↓
Sobre mí
   ↓
Experiencia
   ↓
Proyectos
   ↓
Certificaciones
   ↓
Último artículo publicado
   ↓
Contacto
   ↓
Footer
```

---

# 16. Hero

El Hero tendrá:

- fotografía personal
- nombre
- titular profesional
- descripción muy breve
- CTA
- elemento visual relacionado con las tecnologías

El texto debe ser breve, aproximadamente una frase/titular y 2-3 líneas como máximo.

No se debe convertir el Hero en una biografía.

La información más extensa estará en "Sobre mí".

## Tecnologías animadas

Se quiere incluir un elemento visual que represente las tecnologías habituales.

Posibilidades:

- banda horizontal animada de logos
- composición visual alrededor de la presentación
- animaciones CSS/HTML
- componentes interactivos

Se priorizará una implementación propia mediante HTML/CSS/Blazor frente a utilizar simplemente un GIF pesado.

El efecto debe ser:

- profesional
- sutil
- responsive
- compatible con tema claro/oscuro

No debe saturar visualmente el Hero.

---

# 17. Sobre mí

La sección "Sobre mí" incluirá varios bloques relacionados.

## Perfil

Descripción profesional más completa que la del Hero.

Debe explicar:

- perfil profesional
- especialización
- áreas de interés
- enfoque tecnológico

## Áreas principales

Se mostrarán visualmente las principales áreas tecnológicas.

Por ejemplo:

```text
Backend
C# · .NET · ASP.NET Core

Frontend
Blazor · JavaScript · HTML · CSS

Datos
SQL Server · Python · Pandas · Power BI

DevOps
Docker · Git · GitHub · Azure
```

No es necesario mostrar aquí toda la lista de habilidades del CV.

## Experiencia profesional

Se integrará dentro de "Sobre mí", pero como bloque visual independiente.

No se creará inicialmente una sección principal independiente llamada "Experiencia" en la navegación.

Se utiliza una timeline dinámica:

```text
2025 — Actualidad
Desarrollador...
       │
2024 — 2025
...
       │
2023 — 2024
...
```

Los datos se consultan desde SQL Server mediante un servicio de aplicación y se muestran únicamente cuando existe una traducción válida para la cultura actual o para `es-ES` como fallback. Esto evita saturar el menú y permite que "Sobre mí" funcione como una página/perfil profesional completo.

---

# 18. Proyectos

En Home se muestra una selección de proyectos mediante una:

**rejilla responsive de tarjetas**

Cada tarjeta muestra, según los datos disponibles:

- imagen
- nombre
- breve descripción
- GitHub
- demo si existe

La tarjeta muestra actualmente la información traducida disponible, la imagen de vista previa si existe, los enlaces de repositorio o demo cuando están configurados y el resumen. Si existe una descripción larga, se puede expandir y contraer con un control accesible y localizado. `IsFeatured` realza visualmente la tarjeta, pero no altera `DisplayOrder`; el texto del enlace de demo usa el recurso localizado `ProjectDemo` y no depende de un slug. La consulta utiliza `IDbContextFactory<PortfolioDbContext>`, `AsNoTracking()` y selección de traducción por cultura con fallback a `es-ES`.

Actualmente no existe una ruta pública de detalle de proyecto ni se duplica la navegación mediante enlaces ficticios. Proyectos no mantiene un slug; cualquier futura ruta de detalle requerirá una decisión específica de alcance y una consulta propia.

---

# 19. Certificaciones

Las certificaciones no utilizan el mismo patrón visual que los proyectos.

Se utiliza:

- grid
- tarjetas

Esto permite demostrar variedad de diseño UI.

La Home muestra las certificaciones mediante tarjetas en una rejilla responsive. Cada tarjeta puede mostrar el nombre y emisor traducidos, la fecha de obtención en formato mes y año (conservando la fecha completa en los datos), el ID de credencial opcional, detalles localizados opcionales de hasta 1.000 caracteres colapsados tras «Ver detalles», la imagen local si existe, las horas opcionales y el enlace de credencial si está configurado. Las horas aparecen en la columna de la imagen, debajo de ella cuando existe. Si no hay imagen, el signo «+» es decorativo, no interactivo. También puede mostrar adjuntos públicos: miniaturas para imágenes y vista previa de la primera página para documentos PDF, identificados por su nombre para mostrar. En administración se puede cambiar el nombre para mostrar de cada adjunto sin alterar su nombre de archivo original ni su almacenamiento.

Actualmente no existe una ruta pública de detalle ni un `slug` de certificación. Una futura ruta de detalle requerirá una decisión específica de alcance y un modelo ampliado si fuese necesario.

---

# 20. Blog

En Home se muestra:

**el último artículo publicado**

No el último artículo creado.

Esto evita que borradores o contenidos no publicados aparezcan públicamente.

La tarjeta incluye actualmente:

- imagen destacada
- título
- fecha
- extracto
- estado vacío localizado cuando no existe una publicación válida

La consulta pública selecciona primero publicaciones destacadas válidas y después la publicación más reciente por fecha descendente, utilizando el identificador como desempate estable. Excluye artículos cuyo estado no sea `Published` y publicaciones futuras.

La selección de traducción utiliza la cultura actual (`es-ES` o `en-US`) y hace fallback a `es-ES`. La Home muestra hasta tres publicaciones públicas únicas: primero las destacadas y después las más recientes para completar los espacios disponibles.

La ruta `/blog` muestra todas las publicaciones públicas en una lista vertical ordenada por fecha descendente; las destacadas se identifican visualmente sin alterar el orden cronológico. Cada resumen enlaza con `/blog/{slug}`, implementada mediante una única plantilla reutilizable que carga el contenido completo según el slug localizado. Se busca primero el slug del idioma activo y se usa el slug/contenido español únicamente cuando no existe traducción para ese idioma. Ambas rutas excluyen borradores y publicaciones futuras; un slug inexistente o no público devuelve la página 404.

El contenido completo se almacena en `BlogPostTranslation.Content` y se administra desde el panel mediante TinyMCE 8 local. TinyMCE se limita al contenido enriquecido del blog: los demás módulos usan campos estructurados y localizados, no necesitan edición de HTML enriquecido. La carga de imágenes se realiza mediante multipart HTTP autenticado y protegido por antiforgery; los archivos se guardan fuera de `wwwroot` y SQL Server en un directorio configurable, por artículo, con nombre opaco y validación de extensión, MIME, firma y tamaño. Se admiten JPG/JPEG/PNG/WebP/SVG hasta 10 MiB; los SVG pasan validación XML restrictiva. El administrador puede marcar la imagen destacada y borrar imágenes no referenciadas por el contenido. La vista previa administrativa requiere autorización y la ruta pública solo sirve imágenes de artículos publicados con fecha no futura.

---

# 21. Contacto

Se incluirán varias vías de contacto:

- email directo
- formulario de contacto
- GitHub
- LinkedIn

## Formulario

Aunque actualmente muchas personas puedan preferir utilizar directamente el email o LinkedIn, el formulario se mantendrá por dos motivos:

1. Facilita una alternativa de contacto directa.
2. Demuestra funcionalidad backend real del proyecto.

Flujo conceptual:

```text
Formulario Blazor
       ↓
Validación
       ↓
ContactService
       ↓
IEmailService
       ↓
MailKitEmailService
       ↓
SMTP
       ↓
Email de destino
```

El formulario deberá contemplar:

- validación
- mensajes de éxito/error
- protección antispam
- logging apropiado

### Arquitectura del envío

El componente Blazor no accederá directamente a MailKit, SMTP, `Infrastructure`, `DbContext` ni a ningún proveedor externo. El envío se realizará mediante las capas de aplicación e infraestructura:

```text
Portfolio.Web
       ↓
ContactService
       ↓
IEmailService (Portfolio.Application)
       ↓
MailKitEmailService (Portfolio.Infrastructure)
       ↓
Servidor SMTP
```

`IEmailService` será el contrato que permita sustituir MailKit por otro proveedor en el futuro sin modificar el formulario de contacto. El envío será asíncrono y el componente esperará el resultado para mostrar el estado correcto al usuario; no se utilizarán operaciones «fire-and-forget» para evitar perder errores de entrega.

### Transporte de correo inicial

Durante la primera implementación se utilizará **Gmail SMTP mediante MailKit y autenticación OAuth 2.0**. MailKit deberá referenciarse únicamente desde `Portfolio.Infrastructure`; `Portfolio.Application` contendrá los contratos y el caso de uso, y `Portfolio.Web` solo consumirá el servicio de aplicación.

La autenticación utilizará un OAuth Client ID, Client Secret y Refresh Token de Google para obtener access tokens temporales. La configuración del transporte se realizará mediante opciones validadas y valores externos a los archivos versionados. No se utilizarán contraseñas SMTP ni se almacenarán en el repositorio client secrets, refresh tokens o access tokens; todos deberán proporcionarse mediante User Secrets o variables de entorno.

El scope SMTP de Gmail será `https://mail.google.com/` y el refresh token se conservará únicamente como secreto de configuración. El proveedor renovará el access token cuando sea necesario y no registrará sus valores.

La configuración local se almacenará con .NET User Secrets y la configuración de despliegue mediante variables de entorno o un almacén de secretos del entorno. No se debe abrir, leer ni copiar el contenido real de `secrets.json` al documentar o revisar esta configuración. Para referencia, estas son únicamente las claves esperadas y valores ficticios; no se deben usar literalmente:

```json
{
  "ContactEmail": {
    "UserName": "sender@example.invalid",
    "FromAddress": "sender@example.invalid",
    "OAuth": {
      "ClientId": "example-client-id.apps.googleusercontent.com",
      "ClientSecret": "EXAMPLE_CLIENT_SECRET",
      "RefreshToken": "EXAMPLE_REFRESH_TOKEN"
    }
  }
}
```

`Host`, `Port`, `ToAddress`, `UseStartTls`, `OAuth:TokenEndpoint` y `OAuth:Scope` pueden usar los valores no sensibles definidos en `appsettings.json`. `ClientSecret` y `RefreshToken` deben permanecer en User Secrets o en el almacén seguro del entorno; nunca en archivos versionados, logs o documentación.

Como configuración funcional prevista:

- el destinatario será configurable y actualmente se prevé `mi-cuenta-personal@gmail.com`;
- el remitente no se fijará en el componente ni en el código del servicio;
- el client ID, client secret, refresh token, host, puerto, seguridad y direcciones se suministrarán mediante configuración segura.

### Migración futura al correo del QNAP

Cuando el portfolio se aloje en el QNAP, se podrá sustituir Gmail SMTP y su autenticación OAuth 2.0 por el servidor de correo del propio QNAP sin modificar `ContactSection.razor`, `ContactService` ni el contrato `IEmailService`. La implementación futura podrá utilizar autenticación propia del QNAP o un adaptador SMTP específico. La configuración futura podrá utilizar `contacto@midominio.com` como remitente y mantener `mi-cuenta-personal@gmail.com` como destinatario, siempre que el dominio, DNS y servidor SMTP estén configurados correctamente.

Esta migración deberá limitarse al transporte y a su configuración. No se creará un buzón de mensajes, historial, panel de administración ni persistencia de contactos como parte de esta funcionalidad.

---

# 22. Responsive design

El diseño debe realizarse pensando desde el principio en:

- escritorio
- tablet
- móvil

No se desarrollará primero una versión de escritorio para intentar adaptarla posteriormente.

Cada sección deberá plantearse considerando cómo cambia su composición en pantallas pequeñas.

Ejemplo:

```text
Desktop
Foto       Texto
           Tecnologías

Mobile
Foto
Texto
Tecnologías
```

---

# 23. Flujo de trabajo del desarrollo

El proyecto no seguirá este modelo:

```text
Diseñar toda la UI
        ↓
Diseñar toda la BD
        ↓
Programar todo el backend
        ↓
Conectar todo
```

En su lugar se utilizará un desarrollo **iterativo por módulos/vertical slices**.

## Flujo

```text
Diseñar visualmente una sección
          ↓
Identificar información necesaria
          ↓
Diseñar modelo conceptual
          ↓
Crear/ajustar entidades
          ↓
Implementar lógica de aplicación
          ↓
Implementar infraestructura
          ↓
Crear migración EF
          ↓
Aplicar migración
          ↓
Conectar Blazor
          ↓
Probar
          ↓
Refinar diseño
          ↓
Siguiente módulo
```

### Motivo

El diseño visual puede revelar necesidades que no eran evidentes al diseñar la base de datos.

Por ejemplo, al diseñar proyectos podría descubrirse que se necesitan:

```text
Project
Technology
ProjectTechnology
```

porque un proyecto utiliza varias tecnologías y una tecnología aparece en varios proyectos.

De la misma manera, al diseñar el blog pueden aparecer necesidades como:

- categorías
- tags
- estado de publicación
- fecha de publicación
- imagen destacada
- autor
- artículos relacionados
- SEO

Por eso no se debe intentar adivinar todo el modelo definitivo al principio.

---

# 24. Regla importante de arquitectura

No sobreingenierizar.

Se debe utilizar una arquitectura profesional, pero cada abstracción debe tener una razón.

No crear automáticamente:

- repositorios innecesarios
- interfaces para todo
- servicios sin lógica
- patrones únicamente por seguir una moda

La arquitectura debe facilitar:

- mantenimiento
- pruebas
- evolución
- separación de responsabilidades
- despliegue

pero sin introducir complejidad artificial.

---

# 25. Git y estrategia de commits

El historial de Git debe reflejar la evolución real del proyecto.

No se debe realizar un primer commit gigantesco con toda la aplicación.

## Commit 1

```text
chore: initialize portfolio solution
```

Debe contener:

- solución
- proyectos
- referencias
- estructura inicial
- configuración básica
- Home funcional temporal

La Home inicialmente mostrará:

```text
Portfolio
Aplicación en construcción
```

Debe poder:

```text
dotnet build
```

y ejecutarse correctamente.

Todavía no debe existir el diseño final del Home.

---

## Commit 2

```text
feat: configure frontend design system
```

Incluirá:

- Tailwind
- estructura CSS
- tipografía
- variables de diseño
- colores iniciales
- preparación de temas
- recursos/localización

La implementación incorpora además `theme.js` como API JavaScript mínima para aplicar el tema y conservarlo en `localStorage`, recursos compartidos en español e inglés y configuración de `es-ES`/`en-US` en el pipeline de ASP.NET Core.

---

## Commit 3

```text
feat: create portfolio layout and navigation
```

Incluirá:

- Layout
- Navbar
- Footer
- navegación
- responsive inicial
- espacio reservado para logo
- menú móvil accesible con estado `aria-expanded`
- selector reutilizable de tema Sistema/Claro/Oscuro
- navegación interactiva mediante Blazor Server
- enlaces provisionales a anchors de la Home

---

## Commit 4

```text
feat: build public home structure
```

Incluye la estructura visual inicial de la Home pública:

- Hero con composición tecnológica provisional
- Sobre mí
- timeline visual de experiencia
- tarjetas iniciales de proyectos
- tarjetas iniciales de certificaciones
- estado inicial del último artículo publicado
- formulario visual de contacto sin envío real
- anchors compatibles con la navegación pública existente
- componentes Razor reutilizables en `Components/Sections`
- estilos responsive en `wwwroot/css/app.css`
- textos localizados en español e inglés

Mientras no existan datos personales definitivos, la Home utiliza placeholders y estados vacíos localizados. No se han creado entidades, persistencia, consultas dinámicas ni URLs ficticias.

---

## Fase 5 — Persistencia inicial

La persistencia se ha preparado de forma incremental a partir de las secciones que ya existen visualmente en la Home. Se utiliza **Entity Framework Core 10 + SQL Server + Code First + Migrations**.

### Decisiones de arquitectura

- Las entidades iniciales `Project`, `Experience`, `Certification` y `BlogPost` están en `Portfolio.Domain/Entities`.
- Las entidades no contienen referencias a EF Core, SQL Server, Blazor ni Infrastructure.
- `Portfolio.Infrastructure/PortfolioDbContext.cs` es el único `DbContext` y contiene un `DbSet` para cada entidad inicial.
- Los mapeos están separados en `IEntityTypeConfiguration<T>` dentro de `Portfolio.Infrastructure/Configurations`.
- No se han creado todavía `Category`, `Tag`, tecnologías de proyectos, relaciones, repositorios, casos de uso ni servicios de contenido.
- Los identificadores son `Guid`; los slugs de artículos tienen un índice único y los slugs de proyectos son opcionales con índice único filtrado por idioma; las fechas de experiencia y certificación se almacenan como `date`. El slug de proyecto se conserva como base para futuras páginas públicas de detalle, pero no es editable ni requerido hasta implementar esa función.

### Configuración y migraciones

La conexión se obtiene de `ConnectionStrings:Portfolio`. `Portfolio.Web/appsettings.json` y `appsettings.Development.json` no contienen nombres de servidores ni credenciales específicas de una máquina. El proyecto Web utiliza `UserSecretsId` para que cada entorno de desarrollo configure su propia instancia SQL Server mediante User Secrets. En otros entornos, especialmente producción, la cadena debe suministrarse mediante variables de entorno, secretos de Docker u otro mecanismo seguro de configuración.

La primera migración es `InitialPortfolioContent`. Crea las tablas iniciales sin datos de contenido. Posteriormente, `AddProjectTranslations` separó los campos traducibles de proyectos en `ProjectTranslations`, conservando `Slug` como campo localizado con índice único por idioma; `AddProjectPreviewImagePath` añadió la ruta opcional de la imagen de vista previa. La migración `20260930074714_MakeProjectTranslationSlugOptional` permite valores nulos y conserva unicidad para los slugs existentes. Se aplicó manualmente a la base de datos de desarrollo y su esquema está actualizado. También `AddCertificationTranslations` separó los campos traducibles de certificaciones, `AddCertificationImagePath` añadió su imagen opcional, `AddExperienceTranslations` separó los campos traducibles de experiencia, `AddBlogPostTranslations` separó los campos traducibles de artículos, `AddBlogPostFeaturedImage` añadió la imagen destacada común y su texto alternativo localizado, `AddExperienceAttachments` añade metadatos de los adjuntos, `ExpandExperienceDescription` amplía la descripción de experiencia a 2.000 caracteres, y `AddExperienceAttachmentDisplayName` añade el nombre de presentación editable de cada adjunto, inicializado con el nombre original existente. `20261001081903_AddCertificationTranslationDetails` añade el detalle opcional de certificados (`nvarchar(1000)`); no se ha aplicado. `20261001084559_LimitProjectDescriptionTo1000` trunca previamente las descripciones existentes de proyectos a sus primeros 1.000 caracteres y reduce la columna a `nvarchar(1000)`; tampoco se ha aplicado. La migración posterior `20260929192624_RemoveProjectTranslationSlug` no se aplicó a ninguna base y se ha retirado del código; no debe aplicarse. Cada base destino debe actualizarse manualmente con `dotnet ef database update` después de verificar que la configuración apunta al destino correcto. Las migraciones permanecen en `Portfolio.Infrastructure/Migrations` y `dotnet ef` utiliza la configuración del proyecto Web; no se deben incluir valores de User Secrets en documentación ni en archivos versionados.

La configuración local no se versiona con valores específicos. El procedimiento para inicializar, consultar o modificar `ConnectionStrings:Portfolio` mediante `dotnet user-secrets` está documentado en el `README.md` raíz.

La migración `20260930163422_AddCertificationAttachments` añade la tabla de metadatos de adjuntos de certificación, una FK restrictiva y un índice por certificación/fecha. Se generó en la entrega 4 y **no se ha aplicado a ninguna base de datos**; debe aplicarse explícitamente en cada destino antes de utilizar estos adjuntos.

### Catálogo inicial de entidades y columnas

El modelo actual de persistencia se documenta a continuación. Estos campos son la base actual y podrán evolucionar mediante nuevas migraciones cuando una sección concreta revele necesidades adicionales.

#### `Projects`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno único del proyecto. |
| `RepositoryUrl` | URL del repositorio del proyecto, si existe. |
| `DemoUrl` | URL de una demo pública, si existe. |
| `PreviewImagePath` | Ruta de la imagen de vista previa del proyecto, si existe. Se almacena la ruta del archivo, no la imagen ni una carpeta. |
| `IsFeatured` | Indica si el proyecto debe mostrarse como destacado. |
| `DisplayOrder` | Orden manual de presentación. |

#### `Experiences`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno único de la experiencia. |
| `StartDate` | Fecha de inicio de la experiencia. |
| `EndDate` | Fecha de finalización; puede quedar vacía si continúa activa. |
| `DisplayOrder` | Orden manual dentro de la línea temporal. |

Los campos `RoleTitle`, `CompanyName` y `Summary` se almacenan en `ExperienceTranslations`, junto con `LanguageCode`.

#### `ExperienceAttachments`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno del adjunto. |
| `ExperienceId` | Experiencia asociada; la FK restringe su borrado mientras conserve adjuntos. |
| `OriginalFileName` | Nombre saneado del archivo original, conservado como metadato. |
| `DisplayName` | Nombre editable mostrado en las vistas administrativas y públicas. Se inicializa con `OriginalFileName`. |
| `StorageKey` | Clave relativa opaca generada por el sistema; no es una ruta pública. |
| `ContentType` | Tipo MIME validado (`application/pdf`, `image/jpeg` o `image/png`). |
| `SizeBytes` | Tamaño verificado del archivo. |
| `CreatedAt` | Fecha de recepción UTC. |
| `IsPublic` | Privado por defecto (`false`); la publicación o revocación es explícita por adjunto. |

Los binarios se guardan fuera de SQL Server en un directorio persistente configurable y fuera de `wwwroot`; la ruta pública anónima solo sirve adjuntos publicados y la ruta administrativa protegida por `AdministratorOnly` permite al administrador autenticado ver adjuntos privados y públicos. Ambas respuestas son en línea y usan `no-store` y `nosniff`. Límites iniciales: cinco archivos por experiencia, 10 MiB por archivo y PDF/JPG/JPEG/PNG. El tamaño, cantidad y directorio se configuran bajo `Portfolio:ExperienceAttachments` (`Directory`, `MaximumFileCount`, `MaximumFileSizeBytes` y `AllowedExtensions`); los valores predeterminados son `App_Data/ExperienceAttachments`, `5` y `10485760` bytes. Los tipos permitidos también se comprueban por MIME y firma; añadir formatos requiere ampliar explícitamente su validación, sin cambiar el esquema. Nunca guardar documentos potencialmente sensibles en `wwwroot` ni proporcionarles acceso público implícito. Al borrar una experiencia, tras confirmar el borrado de la entidad, se elimina únicamente su carpeta GUID si está vacía; los errores de eliminación física se registran.

#### `Certifications`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno único de la certificación. |
| `IssuedOn` | Fecha de obtención, si se conoce. |
| `CredentialUrl` | URL de verificación o credencial, si existe. |
| `CredentialId` | Identificador opcional asignado por el emisor a la credencial. |
| `Hours` | Duración opcional en horas enteras, no negativa. |
| `ImagePath` | Ruta de la imagen local de la certificación, si existe. |
| `DisplayOrder` | Orden manual de presentación. |

Los campos `Name`, `Issuer` y `Details` se almacenan en `CertificationTranslations`, junto con `LanguageCode`. `Details` es opcional y se localiza por idioma; la documentación de límites se pospone hasta revisar la sección exacta del esquema y sus restricciones de traducción.

#### `CertificationAttachments`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno único del adjunto. |
| `CertificationId` | Certificación asociada; la FK restrictiva requiere retirar los adjuntos antes de borrar la certificación. |
| `OriginalFileName` | Nombre saneado del archivo original, conservado como metadato. |
| `DisplayName` | Nombre visible editable; se inicializa con `OriginalFileName` y no renombra el archivo almacenado. |
| `StorageKey` | Clave relativa opaca generada por el sistema; no es una ruta pública. |
| `ContentType` | Tipo MIME validado (`application/pdf`, `image/jpeg` o `image/png`). |
| `SizeBytes` | Tamaño verificado del archivo. |
| `CreatedAt` | Fecha de recepción UTC. |

Los binarios y la imagen de tarjeta se almacenan fuera de SQL Server y `wwwroot`, en el directorio persistente configurable `Portfolio:CertificationMedia:Directory` (predeterminado `App_Data/CertificationMedia`). La imagen de tarjeta admite JPG/JPEG/PNG y cada adjunto JPG/JPEG/PNG/PDF, con un máximo de 10 MiB por archivo y sin máximo de cantidad. Se validan extensión, MIME, tamaño real y firma. Los adjuntos nuevos son privados al cargarse; el administrador puede publicarlos explícitamente tras confirmación. `/certification-attachments/{attachmentId}` sirve únicamente adjuntos publicados en línea con `no-store` y `nosniff`; `/admin/certifications/attachments/{attachmentId}` permite al administrador autenticado previsualizar archivos públicos y privados. `/certification-card-images/{certificationId}` sirve la imagen de tarjeta con versión opaca y se mantiene pública. Las imágenes adjuntas se muestran como miniaturas y los PDF mediante PDF.js local con su primera página. No se almacenan binarios en SQL Server. Al borrar una certificación se eliminan sus archivos; una vez confirmado el borrado del registro se retiran, solo si están vacías, sus carpetas `attachments`, `card` y la carpeta GUID del certificado. La migración `20261008113506_AddCertificationAttachmentVisibility` marca como públicos los adjuntos ya existentes para conservar su acceso y no se aplica automáticamente.

#### `BlogPosts`

| Columna | Explicación |
| --- | --- |
| `Id` | Identificador interno único del artículo. |
| `FeaturedImagePath` | Ruta de la imagen destacada local del artículo, si existe. No se almacena la imagen como BLOB. |
| `PublishedOn` | Fecha y hora de publicación, si se ha publicado. |
| `IsPublished` | Indica si el artículo está publicado. |
| `IsFeatured` | Indica si el artículo debe mostrarse como destacado. |

Los campos `Title`, `Slug`, `Excerpt` y `Content` se almacenan en `BlogPostTranslations`, junto con `LanguageCode`. `FeaturedImageAlt` también se almacena en esa tabla porque puede localizarse por idioma; si está vacío, la capa de aplicación utiliza el título como fallback accesible.

Los campos traducibles de proyectos se almacenan en `ProjectTranslations`, con la clave compuesta `ProjectId` + `LanguageCode` y los campos `Title`, `Slug`, `Summary` y `Description`. El slug se conserva para una futura ruta pública de detalle, pero dicha página y la gestión de contenido detallado (por ejemplo, galería de imágenes) no forman parte de la entrega actual. `PreviewImagePath`, `ImagePath` y `FeaturedImagePath` son datos comunes de sus entidades respectivas y no se duplican por idioma. Las rutas deben apuntar a archivos concretos gestionados fuera de SQL Server; no se almacenan imágenes binarias ni rutas de carpetas. No se crearán columnas como `TitleEn` ni tablas duplicadas por idioma.

La Fase 6 está completada. Conecta con SQL Server las secciones públicas de experiencia, proyectos, certificaciones y blog mediante servicios de consulta, DTOs, `AsNoTracking()`, consultas asíncronas y selección localizada con fallback a `es-ES`. También implementa el formulario de contacto con validación server-side, honeypot, localización, logging seguro y envío asíncrono por Gmail SMTP con MailKit y OAuth 2.0; el envío real se probó correctamente. No incluye CRUD, Identity, administración ni gestión administrativa de archivos, que corresponden a la Fase 7. El modelo se ampliará mediante nuevas migraciones solo cuando una sección concreta revele necesidades concretas.

---

## Commits posteriores

Se seguirá una estructura similar:

```text
feat: add identity authentication
feat: add admin dashboard
feat: add project management
...
```

Los commits deben ser relativamente pequeños y representar cambios coherentes.

---

# 26. Despliegue

El objetivo de despliegue es:

**QNAP + Docker**

La aplicación debe estar preparada para ejecutarse en contenedores.

Conceptualmente:

```text
QNAP
│
├── Portfolio Web Container
│
├── SQL Server
│
└── Persistent Storage
       │
       ├── Images
       ├── PDFs
       └── Files
```

La información persistente no debe depender de que el contenedor concreto siga existiendo.

Los volúmenes y configuración de Docker deberán diseñarse posteriormente cuando la aplicación funcional esté suficientemente avanzada.

---

# 27. Orden general de implementación

El proyecto se abordará aproximadamente en este orden:

```text
FASE 1 — Base
│
├── Crear solución
├── Crear proyectos
├── Referencias
├── Configuración
└── Home temporal
│
▼
FASE 2 — Sistema visual
│
├── Tailwind
├── CSS
├── Tipografía
├── Colores
├── Tema
└── Localización
│
▼
FASE 3 — Layout
│
├── Navbar
├── Footer
├── Responsive
├── Navegación
└── Selector de tema integrado
│
▼
FASE 4 — Home
│
├── Hero
├── Sobre mí
├── Experiencia
├── Proyectos
├── Certificaciones
├── Blog
└── Contacto
│
▼
FASE 5 — Datos
│
├── Diseñar entidades según necesidades reales
├── EF Core
├── DbContext
├── Migrations
└── SQL Server
│
▼
FASE 6 — Funcionalidades
│
├── Selector de idioma en la Navbar
├── Cambio de cultura y persistencia de preferencia
├── Modelo de traducciones del contenido
├── Contenido dinámico
├── Proyectos
├── Certificaciones
├── Experiencia
├── Blog
└── Contacto

La Fase 6 se desarrolló de forma incremental por secciones, no como una implementación monolítica de todos los módulos. El ciclo seguido fue:

```text
Seleccionar una sección
        ↓
Revisar su diseño y necesidades de contenido
        ↓
Revisar o ajustar sus entidades y traducciones
        ↓
Crear la migración necesaria
        ↓
Implementar consulta y caso de uso
        ↓
Conectar la sección de la Home
        ↓
Probar la sección
        ↓
Detener el desarrollo para revisión
        ↓
Incorporar las observaciones y cambios acordados
        ↓
Validar de nuevo la sección
        ↓
Continuar con la siguiente sección
```

Después de cada sección se realizó una pausa explícita para revisar el resultado visual y funcionalmente. La Fase 6 queda cerrada; la misma práctica de implementación incremental y revisión se mantendrá en la Fase 7.
│
▼
FASE 7 — Administración
│
├── Identity
├── Login
├── Autorización
├── Dashboard
├── CRUD
├── Edición localizada por idioma
├── Estado de traducciones
├── Media
└── TinyMCE
│
▼
FASE 8 — Calidad
│
├── Tests
├── Validación
├── Seguridad
├── SEO
├── Accesibilidad
└── Responsive
│
▼
FASE 9 — Docker/QNAP
│
├── Dockerfile
├── Configuración
├── Persistencia
├── SQL Server
└── Despliegue
```

El orden podrá modificarse cuando una decisión técnica lo justifique, pero no se debe adelantar trabajo innecesariamente.

### Entregas incrementales de la Fase 7

La Fase 7 se implementará por entregas funcionales, con pruebas y revisión antes de continuar:

1. **Fundamentos administrativos:** ASP.NET Core Identity, login/logout, autorización de `/admin`, creación segura del único administrador y estructura del Dashboard. No habrá registro público. TinyMCE no forma parte de esta entrega.
2. **Experiencia:** CRUD, edición de `es-ES` y `en-US` desde un único formulario, aviso visible de la traducción ausente y orden manual. Completada en la segunda entrega. Los adjuntos usan `ExperienceAttachments` y un directorio persistente configurable, con nombres internos generados, metadatos mínimos y acceso privado por defecto. Decisiones acordadas: hasta cinco archivos por experiencia, 10 MiB cada uno, PDF/JPG/JPEG/PNG; el administrador puede publicar o revocar cada archivo explícitamente. Los adjuntos publicados se muestran en la web pública y los privados solo en administración autorizada. El límite, tamaño y ubicación pueden cambiar mediante configuración; incorporar otros formatos requiere ampliar el validador MIME/firma.
3. **Proyectos:** CRUD, edición localizada, aviso de traducciones ausentes, orden/destacado y gestión de las imágenes asociadas. Completada en la tercera entrega; la implementación y las pruebas se detallan más adelante. El ajuste posterior de slug, destacado y descripción se registra como una corrección de esta entrega, no como una nueva entrega de la Fase 7.
4. **Certificaciones:** CRUD, edición localizada, aviso de traducciones ausentes, fechas, credenciales, orden e imágenes/documentos acordados. Completada en la cuarta entrega; la implementación, el almacenamiento y las pruebas se detallan más adelante.
5. **Blog:** listado de artículos con acción para crear, editar o eliminar. La acción «Editar» seleccionará el artículo por su identificador y cargará ese contenido en el editor; no hace falta un desplegable separado para escogerlo. La edición localizada se realizará desde el mismo formulario mediante pestañas o controles de idioma. TinyMCE 8 se usará únicamente para `BlogPostTranslation.Content`, por ser el campo de texto enriquecido; experiencia, proyectos y certificaciones usarán campos estructurados y controles de texto normales. El formulario incluirá un estado editorial explícito y permitirá guardar cambios aunque el artículo tenga todos sus campos rellenos pero todavía se encuentre en redacción. **Completada en la quinta entrega;** implementación y validación descritas en «Entrega 5 implementada — Blog».

El almacenamiento de archivos se implementa como infraestructura reutilizable mediante un directorio persistente configurable (local para desarrollo y montable desde QNAP), mantiene los binarios fuera de SQL Server y valida tipo, firma, tamaño y nombres seguros. Cada módulo define sus reglas de publicación y acceso: los adjuntos de experiencia son privados por defecto y se publican explícitamente; los previews de proyectos y los archivos de certificación son públicos según las reglas específicas de esos módulos.

### Entrega 1 implementada — Fundamentos administrativos

- `PortfolioDbContext` deriva de `IdentityDbContext<PortfolioUser>` y conserva la persistencia en `Portfolio.Infrastructure`.
- ASP.NET Core Identity autentica mediante cookie `Portfolio.Admin.Authentication`, `HttpOnly`, `SameSite=Lax` y `Secure`, con expiración tras 15 minutos de inactividad. La actividad autenticada de administración renueva la expiración con un intervalo mínimo entre renovaciones; el tracker del layout detecta escritura, teclado, puntero, scroll y tacto mediante una solicitud POST protegida por antiforgery. El servidor valida la expiración y los circuitos Blazor revalidan cada 30 segundos la actividad y el security stamp. La cookie no usa renovación deslizante por solicitudes ajenas a la administración.
- La política `AdministratorOnly` exige el rol `Administrator`. No existe registro público.
- Rutas implementadas: `GET /admin/login`, `POST /admin/login/submit`, `POST /admin/logout`, `POST /admin/session/activity`, `GET /admin/access-denied` y `GET /admin` (Dashboard). El POST de login usa una ruta separada para evitar colisiones con el endpoint Razor Component; login, actividad y logout validan antiforgery, y actividad/logout requieren la política administrativa. El login permite mostrar u ocultar la contraseña.
- En esta primera entrega, el Dashboard mostraba la identidad autenticada, navegación separada, resumen inicial de contenido y accesos deshabilitados para experiencias, proyectos, certificaciones y blog. No incluía CRUD; las siguientes entregas habilitaron Experiencias y Proyectos.
- El login y el Dashboard utilizan recursos compartidos para `es-ES` y `en-US`. Los navbars público y administrativo comparten selectores compactos de tema e idioma; las opciones incluyen iconos, con etiquetas accesibles no visibles. El tema administrativo utiliza los tokens semánticos compartidos también en los paneles MudBlazor.
- Si hay una sesión administrativa, el navbar público muestra únicamente la acción para cerrar sesión, sin mostrar el correo. El cierre de sesión está al final y alineado a la derecha en ambos navbars.
- `Portfolio.AdminProvisioning` aprovisiona de forma puntual el único administrador usando `UserManager`; pide y confirma la contraseña de forma oculta, y la almacena Identity como hash. Exige que la migración esté aplicada. En desarrollo comparte el `UserSecretsId` de Web; en despliegue acepta la cadena de conexión mediante configuración de entorno.
- Migración generada: `20260925120731_AddAdministrativeIdentity` (`AddAdministrativeIdentity`), aplicada en la base de datos de desarrollo; debe aplicarse en cada nueva base de datos destino antes del aprovisionamiento.
- Quedan para futuras entregas la gestión de usuarios, recuperación de contraseña, MFA y los módulos de certificaciones y blog.

### Entrega 2 implementada — Experiencias

- CRUD administrativo en `/admin/experiences` y `/admin/experiences/edit`, protegido con `AdministratorOnly`; Experiencias es el único módulo de contenido habilitado. No se ha adelantado la gestión de proyectos, certificaciones ni blog.
- Un único formulario permite editar fechas, orden y traducciones `es-ES`/`en-US`. El contenido español completo es obligatorio para conservar el fallback público; la traducción inglesa puede omitirse y el listado/editor muestran un aviso visible. Una traducción parcialmente introducida debe completarse antes de guardar.
- La validación de servidor comprueba campos requeridos y longitudes, orden no negativo y fechas coherentes. Los mensajes se resuelven mediante los recursos compartidos `es-ES` y `en-US`.
- La descripción se mantiene como texto plano con saltos de línea y un máximo de 2.000 caracteres por idioma. En la web pública se limita inicialmente a cuatro líneas con acción accesible para expandir y contraer.
- El editor detecta cambios pendientes comparando el formulario con su estado inicial e incluye los cambios sin guardar del nombre y la visibilidad de adjuntos. Al pulsar enlaces de navegación internos marcados para protección o cerrar sesión desde cualquiera de los navbars, presenta el diálogo propio con opciones localizadas para seguir editando o salir sin guardar. El diálogo se ejecuta en el cliente y reutiliza el diseño visual del aviso de expiración de sesión; no depende de una invocación .NET sobre el circuito Interactive Server.
- Al confirmar la salida se continúa con el destino del enlace original. En el caso de cerrar sesión, se reenvía el formulario como `POST` con antiforgery; cancelar el diálogo no navega ni envía el formulario. El listener de `beforeunload` conserva el aviso nativo para recarga y cierre de pestaña/ventana; `NavigationLock` confirma la navegación atrás cuando Blazor la procesa como externa. No se sustituye el diálogo nativo del navegador por uno personalizado.
- La persistencia actualiza experiencia y traducciones en una única operación de EF Core. Al completar correctamente el guardado de la experiencia y sus cambios pendientes de adjuntos, el editor muestra un aviso de éxito localizado; los errores de validación o persistencia no muestran ese aviso. La eliminación de una experiencia se rechaza hasta retirar primero los adjuntos asociados.
- La consulta pública conserva `DisplayOrder` (con desempate por identificador), la selección del idioma solicitado y el fallback a `es-ES`; no se introduce un estado de publicación que el modelo existente no tenía. Solo se muestra una traducción completa. Incluye metadatos únicamente de los adjuntos publicados.
- Los adjuntos aceptados son PDF/JPG/JPEG/PNG, hasta cinco por experiencia y 10 MiB cada uno. Se guardan en un directorio persistente configurable fuera de `wwwroot`, con metadatos en SQL Server, nombres de almacenamiento opacos, validación de MIME y firma, y `IsPublic=false` al subir. Cada adjunto conserva su nombre original saneado y dispone de un nombre de presentación editable. La interfaz permite guardar los cambios de nombre y visibilidad, confirma el borrado antes de ejecutarlo y requiere confirmación antes de publicar; los cambios pendientes de nombre o visibilidad participan en la protección contra pérdida de cambios. La ruta anónima `/experience-attachments/{attachmentId}` sirve solo adjuntos publicados; `/admin/experiences/attachments/{attachmentId}` requiere `AdministratorOnly` y permite al administrador previsualizar adjuntos privados y públicos. Ambas son respuestas inline con `no-store`/`nosniff`.
- Las imágenes se muestran como miniaturas y los PDF presentan la primera página mediante PDF.js vendorizado localmente, con apertura del documento completo en nueva pestaña. El paquete `pdfjs-dist` está fijado en npm y `assets:copy` copia el módulo, worker, CMaps, fuentes estándar y licencia a `wwwroot/js/vendor/pdfjs`.
- Migración generada: `20260927175613_AddExperienceAttachments`. Añade exclusivamente la tabla de metadatos, el índice por experiencia/fecha y una FK restrictiva. Debe aplicarse explícitamente en cada base de datos destino antes de usar adjuntos; no fue aplicada durante esta entrega.
- Migración generada: `20260928091703_ExpandExperienceDescription`. Amplía `ExperienceTranslations.Summary` de `nvarchar(1000)` a `nvarchar(2000)`. Debe aplicarse manualmente en las bases de datos destino antes de guardar descripciones por encima de 1.000 caracteres; no se aplicó a ninguna base en esta modificación.
- Migración generada: `20260928130000_AddExperienceAttachmentDisplayName`. Añade el nombre de presentación editable y copia inicialmente el nombre original para los adjuntos ya existentes. Debe aplicarse explícitamente en cada base de datos destino antes de usar esta funcionalidad.
- Las pruebas cubren autorización de página/descarga, validación, CRUD y persistencia de traducciones, traducción ausente y fallback, orden, límites/tipos/firmas de adjuntos, privacidad, publicación, rutas HTTP, nombre de presentación editable, descripción ampliada y expiración/renovación de sesión. La interacción visual de los diálogos de cambios pendientes, borrado y publicación no cuenta con automatización de navegador en la suite actual.

### Entrega 3 implementada — Proyectos

- CRUD administrativo en `/admin/projects` y `/admin/projects/edit[/{id}]`, protegido por `AdministratorOnly`, habilitado en la navegación y el Dashboard. Un formulario gestiona los campos comunes existentes y las traducciones `es-ES`/`en-US`.
- La traducción española completa (nombre y descripción breve) es obligatoria para conservar el fallback público; la inglesa puede omitirse. El listado y editor advierten de forma localizada si falta una traducción. Las traducciones parciales se rechazan; el slug localizado es opcional y aparece deshabilitado con el placeholder «Deshabilitado» hasta implementar las páginas de detalle; los valores existentes se preservan al guardar. Las URL opcionales aceptan únicamente `http`/`https` y el orden no puede ser negativo.
- Se administran `DisplayOrder`, `IsFeatured`, `RepositoryUrl` y `DemoUrl`. La web pública conserva el orden, la selección por cultura y el fallback a español; `IsFeatured` resalta la tarjeta sin moverla en el orden. La tarjeta presenta el resumen y ofrece expandir/contraer la descripción larga. No se añadieron categorías, etiquetas, tecnologías ni relaciones.
- La imagen de vista previa existente se gestiona desde el editor una vez creado el proyecto. La carga y el borrado se persisten inmediatamente y no activan por sí solos la confirmación de cambios del formulario. Se aceptan JPG/JPEG y PNG, hasta 10 MiB; se comprueban extensión, MIME, tamaño real y firma. Los archivos usan nombres opacos y se almacenan fuera de `wwwroot` y SQL Server en el directorio configurable `Portfolio:ProjectPreviewImages:Directory` (predeterminado `App_Data/ProjectPreviewImages`). `/project-preview-images/{projectId}` sirve únicamente la imagen asociada al proyecto y responde con `no-store`/`nosniff`. Las URL de vista previa del editor y de las tarjetas públicas incluyen como versión el nombre opaco del archivo almacenado, por lo que su `src` cambia tras una sustitución y el navegador solicita la nueva imagen sin recargar toda la página.
- El editor protege los campos modificados sin guardar: muestra una confirmación localizada propia para navegación interna marcada y la confirmación nativa del navegador para salidas del documento. Un guardado correcto desactiva el aviso; los errores no se presentan como guardado satisfactorio.
- El borrado desde la administración elimina la imagen y las traducciones del proyecto. Los archivos de preview se guardan directamente en la raíz compartida `ProjectPreviewImages`; no hay una carpeta individual por proyecto que limpiar y nunca se elimina esa raíz compartida. Los fallos al eliminar archivos se registran. La migración `20260930074714_MakeProjectTranslationSlugOptional` permite `NULL` y crea un índice único filtrado para mantener los slugs existentes sin obligar a asignar valores a proyectos nuevos. Está aplicada manualmente en la base de datos de desarrollo.
- Las pruebas cubren CRUD, traducciones y fallback, orden/destacado, validación de URL, borrado, políticas de autorización y validación/almacenamiento y sustitución de imágenes. La descripción extensa se conserva en el modelo de lectura y se presenta en la tarjeta pública. La compilación completa es correcta y las 78 pruebas de `Portfolio.Tests` pasan.

### Entrega 4 implementada — Certificaciones

- CRUD administrativo en `/admin/certifications` y `/admin/certifications/edit[/{id}]`, protegido con `AdministratorOnly` e integrado en la navegación y el Dashboard. Un formulario gestiona los campos comunes y las traducciones `es-ES`/`en-US`.
- El nombre y emisor en español son obligatorios para el fallback público; la traducción inglesa puede omitirse y el listado/editor avisan cuando falta. Las traducciones parciales se rechazan. `Details` es opcional y localizado. La fecha de obtención, `CredentialUrl`, `CredentialId` y `Hours` son opcionales; el ID admite hasta 200 caracteres, las horas son enteras no negativas, la URL solo admite `http`/`https` y `DisplayOrder` no puede ser negativo. Las consultas conservan orden manual con desempate por identificador y fallback a español. La tarjeta presenta la fecha como mes y año, muestra el ID seguido de «Ver detalles» cuando hay descripción y antes del enlace, y coloca las horas en la columna visual de la imagen.
- `Certifications.ImagePath` se conserva como imagen independiente de la tarjeta. Se permiten varios adjuntos por certificación: JPG/JPEG/PNG/PDF, hasta 10 MiB por archivo, sin máximo de cantidad. Los binarios se guardan fuera de SQL Server y `wwwroot`; el directorio se configura mediante `Portfolio:CertificationMedia:Directory` (predeterminado `App_Data/CertificationMedia`). Se generan claves opacas y se validan nombre, extensión, MIME, tamaño real y firma. La carga y eliminación se persisten inmediatamente y no activan la protección de cambios del formulario.
- Los adjuntos son privados al cargarse y el formulario solicita confirmación antes de publicarlos; los cambios de visibilidad forman parte del seguimiento de cambios pendientes. `/certification-attachments/{attachmentId}` sirve únicamente adjuntos publicados en línea con `no-store` y `nosniff`; `/admin/certifications/attachments/{attachmentId}` permite al administrador autenticado previsualizar adjuntos privados y públicos. `/certification-card-images/{certificationId}` sirve la imagen de tarjeta con versión opaca en la URL y se mantiene pública. Las imágenes adjuntas aparecen como miniaturas y los PDF como vista previa de la primera página usando PDF.js local. El nombre visible y la visibilidad pueden guardarse junto con el formulario; al crear una certificación, las secciones de imagen y adjuntos aparecen una vez guardada. El borrado de una certificación elimina también sus adjuntos e imagen y, tras confirmar el borrado de la entidad, intenta retirar sus directorios GUID `attachments`, `card` y raíz cuando están vacíos; los errores de borrado se registran.
- El formulario compara el contenido con la instantánea inicial: los enlaces internos marcados y el logout muestran el diálogo localizado; las salidas del documento conservan el aviso nativo del navegador. Un guardado correcto desactiva el aviso; los errores no se anuncian como éxito. Los nombres para mostrar pendientes de adjuntos se incluyen en el guardado general y la lista administrativa muestra vista previa, tamaño en KB y controles de cada archivo en una fila vertical responsiva. El guardado individual del nombre usa el mismo feedback localizado que Experiencias.
- Migraciones generadas: `20260930163422_AddCertificationAttachments`, `20260930222211_AddCertificationMetadataAndAttachmentDisplayName`, `20261001081903_AddCertificationTranslationDetails` y `20261008113506_AddCertificationAttachmentVisibility`. La última añade `IsPublic` y conserva el acceso de adjuntos ya existentes marcándolos como públicos; las cargas nuevas son privadas por defecto. `20261001081903_AddCertificationTranslationDetails` añade `Details` nullable (`nvarchar(1000)`) a `CertificationTranslations`. `20261001084559_LimitProjectDescriptionTo1000` trunca datos previos excedentes y reduce `ProjectTranslations.Description` a `nvarchar(1000)`. **Ninguna de estas migraciones se aplicó durante esta modificación**; deben aplicarse explícitamente en el destino antes de usar los cambios.
- Las pruebas cubren CRUD, validación, traducciones/fallback, orden, autorización, carga/lectura/borrado, MIME/tamaño/firmas, almacenamiento externo, reemplazo de imagen, ID/horas, detalles y nombre para mostrar de adjuntos y respuesta pública segura. La validación más reciente de esta entrega ejecutó 92 pruebas de `Portfolio.Tests`; se añadieron después límites de 1.000 caracteres para Details y Project.Description, y la suite volvió a pasar con 92/92. La compilación completa es correcta.

### Entrega 5 implementada — Blog

- CRUD administrativo en `/admin/blog` y `/admin/blog/edit[/{id}]`, protegido con `AdministratorOnly` e integrado en la navegación y el Dashboard. El formulario único administra los campos comunes, las traducciones `es-ES`/`en-US` y el contenido HTML mediante TinyMCE 8 local, limitado al ancho disponible y con toolbar deslizante (`toolbar_mode: "sliding"`) en pantallas estrechas. Los estados editoriales explícitos son `Draft`, `ReadyToPublish` y `Published`; la completitud de traducciones no determina el estado y `PublishedOn` se establece al publicar por primera vez.
- El español completo es necesario para preparar contenido publicable; pueden guardarse borradores incompletos. El listado informa del estado editorial, traducciones ausentes e imágenes. El editor protege cambios no guardados y separa el guardado del artículo de la gestión inmediata de imágenes.
- Las imágenes admitidas son JPG/JPEG/PNG/WebP/SVG de hasta 10 MiB; se validan extensión, MIME, tamaño real, firma y seguridad XML de SVG. Se almacenan con identificador opaco en directorio configurable, fuera de `wwwroot` y SQL Server, en una carpeta por artículo. TinyMCE envía archivos con multipart HTTP al endpoint administrativo autenticado con antiforgery. El administrador puede seleccionar la imagen destacada y borrar las no referenciadas por el contenido; las vistas administrativas no se almacenan en caché. Las rutas públicas solo sirven imágenes de artículos publicados y con fecha no futura. Al borrar un post, después de confirmar el borrado de su registro, se intenta eliminar únicamente su carpeta GUID si está vacía; los fallos de eliminación se registran.
- La Home muestra hasta tres artículos públicos únicos, priorizando los destacados y completando con los más recientes. `/blog` lista todas las publicaciones públicas en orden cronológico descendente y marca las destacadas sin reordenarlas. `/blog/{slug}` utiliza una plantilla de detalle reutilizable que muestra `BlogPostTranslation.Content`; resuelve el slug del idioma actual y recurre al slug/contenido español solo cuando falta esa traducción. Las tres vistas excluyen borradores y publicaciones futuras.
- Migración generada: `20261001164735_AddBlogPostAdministration`, que convierte `IsPublished` en `EditorialStatus` conservando el estado publicado existente y crea `BlogPostImages`. **No se ha aplicado automáticamente**; debe aplicarse explícitamente en el destino antes de utilizar la nueva persistencia.
- La compilación completa es correcta y las 103 pruebas de `Portfolio.Tests` pasan.

### Avisos de traducción y estado editorial del blog

El estado de traducción indica si existe y está completa la traducción de cada idioma. Si falta, el panel mostrará un aviso específico, por ejemplo, «Falta la traducción en English». Es un aviso de edición y no el estado editorial del contenido.

El estado editorial corresponde al artículo completo del blog: **Borrador** (`Draft`), **Listo para publicar** (`ReadyToPublish`) o **Publicado** (`Published`). No se deriva de que los campos estén completos; la advertencia de idioma ausente permanece separada. `PublishedOn` conserva la fecha de publicación efectiva. La migración `20261001164735_AddBlogPostAdministration` sustituye `IsPublished` por `EditorialStatus`, conserva la equivalencia de los artículos ya publicados y crea la tabla de imágenes; está pendiente de aplicación explícita.

---

# 28. Principios del proyecto

Durante el desarrollo se deben respetar estos principios:

### 1. Diseño antes de modelar completamente

No crear toda la base de datos por anticipado.

### 2. Desarrollo incremental

Construir por módulos y funcionalidades completas.

### 3. Seguridad real

Ocultar elementos de interfaz no sustituye la autorización.

### 4. Reutilización

Crear componentes reutilizables cuando exista una necesidad real.

### 5. No sobreingeniería

Utilizar la arquitectura necesaria, no patrones por obligación.

### 6. Responsive desde el principio

No tratar móvil como una adaptación posterior.

### 7. Identidad visual propia

Evitar que la web parezca una plantilla Bootstrap genérica.

### 8. Preparación para evolución

El proyecto debe poder incorporar:

- inglés
- nuevos tipos de contenido
- nuevos proyectos
- nuevas funcionalidades administrativas

sin necesidad de rehacer la arquitectura.

### 9. Git limpio

Cada commit debe representar un cambio coherente y explicable.

### 10. El portfolio también es un proyecto demostrable

El código debe ser suficientemente profesional como para que el repositorio pueda utilizarse como muestra de las capacidades técnicas del desarrollador.

---

# 29. Estado inicial del proyecto

Antes de empezar la implementación visual, las decisiones principales están cerradas:

- [x] .NET 10
- [x] ASP.NET Core
- [x] Blazor Web App
- [x] C#
- [x] SQL Server
- [x] EF Core
- [x] Code First + Migrations
- [x] ASP.NET Core Identity
- [x] Tailwind CSS
- [x] CSS propio
- [x] MudBlazor para administración
- [x] TinyMCE 8 para blog
- [x] QNAP/Docker como destino de despliegue
- [x] Recursos de idioma desde el inicio
- [x] Español (`es-ES`) como cultura predeterminada
- [x] Inglés (`en-US`) preparado mediante recursos
- [x] Tema Sistema/Claro/Oscuro
- [x] localStorage para preferencia de tema
- [x] API JavaScript de tema reutilizada mediante JS interop
- [x] Layout público responsive con navbar y footer iniciales
- [x] Menú móvil accesible con teclado y `aria-expanded`
- [x] Pipeline local de Tailwind mediante npm
- [x] `/admin` como punto de acceso administrativo
- [x] Rueda de administración visible solo tras login
- [x] Logo propio pendiente de diseño
- [x] Home pública con estructura visual inicial
- [x] Componentes reutilizables para las secciones de la Home
- [x] Formulario de contacto funcional con envío real probado mediante Gmail SMTP y OAuth 2.0
- [x] Estados vacíos localizados para contenido aún no disponible
- [x] Composición tecnológica provisional sin recursos externos
- [x] Desarrollo incremental mediante Git
- [x] Entidades iniciales de persistencia para proyectos, experiencia, certificaciones y artículos
- [x] `PortfolioDbContext` y configuraciones EF Core en Infrastructure
- [x] Migración inicial aplicada en la base de datos local de desarrollo
- [x] Fase 6 — Funcionalidades completada, incluido el envío real del formulario de contacto
- [x] Fase 7, entrega 1 — Identity, autorización administrativa, aprovisionamiento y Dashboard inicial
- [x] Fase 7, entrega 2 — CRUD de experiencias, traducciones y adjuntos privados, nombre de presentación editable, confirmaciones de acciones, feedback de guardado y protección de navegación
- [x] Fase 7, entrega 3 — CRUD administrativo de proyectos, traducciones es/en, orden/destacado e imágenes de vista previa
- [x] Ajuste posterior de Proyectos — Conservación del slug para futuras páginas de detalle, realce visual del destacado y descripción larga expandible en Home
- [x] Fase 7, entrega 4 — CRUD de certificaciones, traducciones, adjuntos públicos y metadatos opcionales
- [x] Fase 7, entrega 5 — CRUD de Blog, estados editoriales explícitos, TinyMCE 8 local e imágenes externas

---

# 30. Próximo paso

La **Fase 6 — Funcionalidades** está finalizada, incluida la prueba real satisfactoria del formulario de contacto mediante Gmail SMTP y OAuth 2.0. No quedan tareas de código de esa fase pendientes.

Las entregas 1–5 de la **Fase 7 — Administración** (fundamentos, Experiencias, Proyectos, Certificaciones y Blog) están implementadas y validadas. La solución compila y la suite `Portfolio.Tests` pasa con 103/103 pruebas. La migración `20260930074714_MakeProjectTranslationSlugOptional` se aplicó manualmente a la base de datos de desarrollo; `20261001081903_AddCertificationTranslationDetails`, `20261001084559_LimitProjectDescriptionTo1000` y `20261001164735_AddBlogPostAdministration` están pendientes de aplicación. La última también reemplaza `IsPublished` por el estado editorial explícito y crea `BlogPostImages`. La Fase 7 continúa abierta hasta completar las tareas restantes.

La administración del Blog permite editar `Español` y `English` desde el mismo formulario; la cultura seleccionada en la Navbar pública no determina el idioma de edición del panel. El panel muestra una advertencia cuando falta una traducción, sin confundir ese aviso con el estado editorial del blog.

Al guardar un contenido se actualizan la entidad principal y sus traducciones dentro de una única transacción. La eliminación de la entidad principal elimina sus traducciones relacionadas de forma controlada. Se mantienen la validación de campos y el fallback público a `es-ES`; el estado editorial del blog se elige explícitamente en el formulario y no se infiere de campos rellenos. El contenido se selecciona desde el listado administrativo y la edición abre el registro seleccionado, con sus traducciones en el mismo formulario.

> **No comenzar creando todas las entidades de base de datos.**
>
> **No implementar toda la administración antes de necesitarla.**
>
> **La Fase 4 establece la estructura visual, pero no implementa contenido dinámico.**
>
> El proyecto debe evolucionar mediante módulos y commits pequeños siguiendo el flujo definido anteriormente.
