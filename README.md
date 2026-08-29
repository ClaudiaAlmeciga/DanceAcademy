# Dance Academy

Plataforma de e-learning para una academia de baile: catálogo de cursos por niveles, seguimiento de progreso y certificados para estudiantes, y un panel de administración completo — construida con .NET 8 (Clean Architecture) y dos front-ends en Blazor WebAssembly.

## Contenido

- [Funcionalidades](#funcionalidades)
- [Arquitectura](#arquitectura)
- [Tecnologías](#tecnologías)
- [Cómo correr el proyecto](#cómo-correr-el-proyecto)
- [Estructura del repositorio](#estructura-del-repositorio)
- [Tests](#tests)

## Funcionalidades

### Sitio público (estudiantes)

- Registro / inicio de sesión con JWT, recuperación de contraseña por email.
- Catálogo de cursos filtrable por nivel, con planes de suscripción y compra individual.
- Reproductor de lecciones (video/texto) con navegación entre lecciones, auto-completado al terminar el video y control de acceso (contenido bloqueado si no estás inscrito).
- Seguimiento de progreso por lección y por curso, dashboard personal ("Mis Cursos") con avance general.
- Certificados generables al completar el 100% de un curso.
- Aviso de continuidad de niveles: si te inscribes en un curso intermedio/avanzado sin haber completado el nivel anterior, se muestra una recomendación (no bloquea la inscripción).
- Testimonios: cualquier estudiante puede dejar un comentario/calificación tras completar una lección; quedan pendientes de aprobación del Admin antes de publicarse.
- Instructores, preguntas frecuentes, planes de suscripción.
- **Eventos**: catálogo de eventos con cupo limitado, precio propio (distinto al de los cursos) e inscripción; el pago queda pendiente hasta que el Admin lo confirma manualmente (la pasarela de pago no está conectada todavía).
- **Noticias**: publicaciones de actividades realizadas por la academia.
- Migas de pan en todas las páginas de navegación profunda.

### Panel de administración

- CRUD completo de cursos (con módulos y lecciones), niveles, instructores, planes de suscripción, FAQ, eventos y noticias.
- Publicar/despublicar contenido de forma independiente por curso/módulo/lección.
- Moderación de testimonios (aprobar/rechazar — el Admin nunca redacta ni edita el contenido, solo lo enviado por el estudiante).
- Gestión de inscritos a eventos, con botón para marcar el pago como confirmado.
- Listado de estudiantes con su progreso por curso.
- Dashboard con métricas: estudiantes totales, cursos publicados, inscripciones, tasa de finalización promedio.

## Arquitectura

Clean / Layered Architecture en el backend, con dos SPAs Blazor WebAssembly como clientes:

```
┌─────────────────────┐     ┌─────────────────────┐
│  DanceAcademy.Public │     │  DanceAcademy.Admin  │   Blazor WebAssembly
│  (estudiantes)       │     │  (staff)              │   (JWT en localStorage)
└──────────┬───────────┘     └──────────┬───────────┘
           │                            │
           └────────────┬───────────────┘
                         │ HTTP / JSON
                ┌────────▼────────┐
                │ DanceAcademy.Api │  Minimal APIs, JWT Bearer, CORS
                └────────┬────────┘
                         │
         ┌───────────────┼───────────────┐
         │               │               │
┌────────▼────────┐ ┌────▼─────────┐ ┌───▼──────────────────┐
│   Application    │ │    Domain     │ │    Infrastructure     │
│  DTOs, contratos │ │  Entidades +  │ │  EF Core, Npgsql,     │
│  de servicios     │ │  reglas de    │ │  configuraciones,      │
│                   │ │  negocio      │ │  migraciones, SendGrid │
└──────────────────┘ └───────────────┘ └───────────┬───────────┘
                                                     │
                                              ┌──────▼──────┐
                                              │  PostgreSQL  │
                                              │  (Docker)    │
                                              └─────────────┘
```

- **Domain**: entidades con setters privados y métodos de comportamiento (nunca objetos anémicos) — validan sus propias invariantes en el constructor y en cada método público.
- **Application**: DTOs de request/response e interfaces de servicios externos (`IEmailService`, `IPasswordHasher`).
- **Infrastructure**: `AppDbContext`, `IEntityTypeConfiguration<T>` por entidad, migraciones EF Core, implementación de envío de email (SendGrid).
- **Api**: endpoints Minimal API agrupados por área (`/admin/*`, `/public/*`, `/me/*`), autenticación JWT Bearer, autorización por rol (`Admin` / `Student`).

## Tecnologías

| Capa | Tecnología |
|---|---|
| Backend | .NET 8, ASP.NET Core Minimal APIs |
| Base de datos | PostgreSQL 16 (Docker), EF Core 8 + Npgsql |
| Autenticación | JWT Bearer |
| Frontend | Blazor WebAssembly (.NET 8) × 2 (Admin y Public) |
| Email | SendGrid |
| Tests | xUnit (dominio, sin mocks — se prueban invariantes reales) |
| CI | GitHub Actions (build + test en cada push/PR) |

## Cómo correr el proyecto

### Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (para PostgreSQL)

### 1. Configurar secretos locales

La app no arranca sin estos valores (a propósito — evita defaults inseguros committeados). Corre esto una vez, dentro de `DanceAcademy.Api/`:

```bash
dotnet user-secrets set "ConnectionStrings:Default" "Host=127.0.0.1;Port=55432;Database=danceacademy;Username=postgres;Password=postgres"
dotnet user-secrets set "Jwt:Key" "una-clave-larga-y-aleatoria"
dotnet user-secrets set "Seed:AdminEmail" "admin@danceacademy.local"
dotnet user-secrets set "Seed:AdminPassword" "una-contraseña-segura"
```

(`SendGrid:ApiKey` es opcional — sin ella, la app funciona igual y solo se omite el envío de emails de recuperación de contraseña).

### 2. Levantar la base de datos

```bash
docker-compose up -d
```

### 3. Correr los tres proyectos

**Con Visual Studio**: abre `DanceAcademy.sln`, configura "Múltiples proyectos de inicio" (Api + Admin + Public) y presiona F5. El build de `DanceAcademy.Api` ya incluye un paso que levanta Docker/la base de datos automáticamente si no están corriendo.

**Manual** (3 terminales):

```bash
dotnet run --project DanceAcademy.Api --urls http://localhost:5178
dotnet run --project DanceAcademy.Admin --urls http://localhost:5241
dotnet run --project DanceAcademy.Public --urls http://localhost:5182
```

Luego abre `http://localhost:5182` (sitio público) o `http://localhost:5241` (admin, con las credenciales de `Seed:AdminEmail`/`Seed:AdminPassword`).

## Estructura del repositorio

```
DanceAcademy.Domain/          Entidades y reglas de negocio
DanceAcademy.Application/     DTOs e interfaces de servicios
DanceAcademy.Infrastructure/  EF Core, migraciones, SendGrid
DanceAcademy.Api/             Endpoints Minimal API
DanceAcademy.Admin/           Blazor WASM — panel de administración
DanceAcademy.Public/          Blazor WASM — sitio público
DanceAcademy.Tests/           Tests de dominio (xUnit)
scripts/                      Scripts de utilidad (levantar Docker, crear admin, seed)
docker-compose.yml            PostgreSQL local
```

## Despliegue

El repositorio incluye un blueprint de [Render](https://render.com) (`render.yaml`) que despliega los 3 servicios (Api, Admin, Public, cada uno como imagen Docker) más una base de datos PostgreSQL gestionada.

1. En el dashboard de Render: **New → Blueprint** → selecciona este repositorio.
2. Render detecta `render.yaml` y crea automáticamente `danceacademy-db`, `danceacademy-api`, `danceacademy-admin` y `danceacademy-public`.
3. Te pedirá los valores de las variables marcadas como secretas (nunca están en el archivo, que es público): `Jwt:Key` (una clave larga y aleatoria, distinta a la de desarrollo), `Seed:AdminEmail`/`Seed:AdminPassword` (credenciales del primer admin) y `SendGrid:ApiKey` (opcional).
4. Si alguno de los nombres de servicio (`danceacademy-api`, `danceacademy-admin`, `danceacademy-public`) ya está tomado por otra cuenta de Render, tendrás que renombrarlo y actualizar las referencias cruzadas — ver los comentarios al inicio de `render.yaml`.

## Tests

```bash
dotnet test DanceAcademy.Tests/DanceAcademy.Tests.csproj
```

Los tests cubren el dominio (constructores, validaciones y métodos de comportamiento de cada entidad), sin mocks — se instancian los objetos reales y se verifican sus invariantes.
