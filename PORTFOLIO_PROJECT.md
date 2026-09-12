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
- documentos PDF
- otros archivos multimedia

La aplicación y la base de datos deben permanecer desacopladas del almacenamiento físico de estos archivos.

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
Español
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

Se utilizará el sistema de recursos/localización.

Desde el principio, incluso aunque solo exista español, los textos de interfaz deben utilizar recursos.

Ejemplo conceptual:

```text
About = "Sobre mí"
Projects = "Proyectos"
Certifications = "Certificaciones"
Blog = "Blog"
Contact = "Contacto"
```

Cuando se añada inglés se incorporarán los recursos correspondientes sin duplicar las pantallas.

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

Se utilizará preferentemente una timeline:

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

Esto evita saturar el menú y permite que "Sobre mí" funcione como una página/perfil profesional completo.

---

# 18. Proyectos

En Home se mostrará una selección de proyectos mediante un:

**Carrusel de proyectos**

Cada tarjeta podrá mostrar:

- imagen
- nombre
- breve descripción
- tecnologías
- GitHub
- demo si existe
- enlace para ver el proyecto

El usuario podrá acceder a una vista más completa del proyecto.

Ruta pública prevista:

```text
/projects/{slug}
```

Se prefiere `slug` para URLs públicas legibles.

El esquema definitivo de `Project` se decidirá cuando se diseñe esta sección.

No crear la entidad completa antes de conocer las necesidades reales del diseño.

---

# 19. Certificaciones

Las certificaciones no utilizarán el mismo patrón visual que los proyectos.

Se utilizará preferentemente:

- grid
- tarjetas
- posible timeline
- presentación visual alternativa al carrusel

Esto permite demostrar variedad de diseño UI.

Cada certificación podrá tener una vista de detalle:

```text
/certifications/{slug}
```

El detalle podrá mostrar:

- nombre
- organización
- fecha
- duración/horas
- descripción
- imagen
- documento si corresponde

El modelo definitivo se diseñará cuando se construya la sección.

---

# 20. Blog

En Home se mostrará:

**el último artículo publicado**

No el último artículo creado.

Esto evita que borradores o contenidos no publicados aparezcan públicamente.

La tarjeta podrá incluir:

- imagen destacada
- título
- fecha
- extracto
- botón "Leer artículo"
- botón/enlace para acceder al blog completo

Rutas previstas:

```text
/blog
/blog/{slug}
```

El contenido será administrado desde el panel mediante TinyMCE.

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
POST
       ↓
Backend .NET
       ↓
Servicio de correo
       ↓
Email
```

El formulario deberá contemplar:

- validación
- mensajes de éxito/error
- protección antispam
- logging apropiado

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

---

## Commit 4

```text
feat: create home hero section
```

Se empezará a construir visualmente el Hero.

---

## Commits posteriores

Se seguirá una estructura similar:

```text
feat: add about section
feat: add experience timeline
feat: add project carousel
feat: add certifications section
feat: add blog preview
feat: add contact section
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
└── Navegación
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
├── Contenido dinámico
├── Proyectos
├── Certificaciones
├── Experiencia
├── Blog
└── Contacto
│
▼
FASE 7 — Administración
│
├── Identity
├── Login
├── Autorización
├── Dashboard
├── CRUD
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
- [x] Español inicialmente
- [x] Inglés preparado para una fase posterior
- [x] Tema Sistema/Claro/Oscuro
- [x] localStorage para preferencia de tema
- [x] `/admin` como punto de acceso administrativo
- [x] Rueda de administración visible solo tras login
- [x] Logo propio pendiente de diseño
- [x] Home definido conceptualmente
- [x] Formulario de contacto
- [x] Desarrollo incremental mediante Git

---

# 30. Próximo paso

El primer objetivo es crear la **solución base funcional** y realizar el primer commit:

```text
chore: initialize portfolio solution
```

La aplicación debe compilar y ejecutarse correctamente, pero la Home solo tendrá una presentación temporal:

```text
Portfolio
Aplicación en construcción
```

A partir del siguiente commit se comenzará a construir el sistema visual y posteriormente el **Navbar + Hero**, avanzando desde ahí por las distintas secciones del Home.

> **No comenzar creando todas las entidades de base de datos.**
>
> **No implementar toda la administración antes de necesitarla.**
>
> **No diseñar todo el Home de una sola vez.**
>
> El proyecto debe evolucionar mediante módulos y commits pequeños siguiendo el flujo definido anteriormente.
