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
