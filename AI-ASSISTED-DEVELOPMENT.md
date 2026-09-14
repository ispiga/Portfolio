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


