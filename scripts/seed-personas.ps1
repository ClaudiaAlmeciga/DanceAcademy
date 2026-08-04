<#
.SYNOPSIS
    Siembra datos realistas de las buyer personas de DanceAcademy (Camila, Andrés, Marcela)
    para las pruebas de M8 (docs/ROADMAP.md), usando únicamente los endpoints públicos de la
    API (mismo camino que seguiría un usuario real) — sin insertar filas a mano por SQL,
    salvo el paso opcional de espaciar fechas de avance de Marcela al final.

.DESCRIPTION
    Crea, si no existen ya:
      - 2 Levels ("Principiante", "Intermedio")
      - 3 Instructor (Ballet, Salsa, Danza contemporánea)
      - 8 FaqItem (2 por categoría: Cuenta, Pagos, Contenido, Técnico)
      - 2 Course publicados con su Module y Lessons publicados:
          "Ballet para Principiantes" (nivel Principiante, 5 lecciones)
          "Salsa Intermedia"          (nivel Intermedio, 4 lecciones)
      - 3 usuarios Student (las buyer personas) con su perfil (FullName) actualizado:
          Camila Rodríguez  — ~40% de avance en Ballet para Principiantes
          Andrés Martínez   — 100% en Ballet para Principiantes (dispara certificado) +
                              50% en Salsa Intermedia (en progreso, sin certificado)
          Marcela Gómez     — 100% en Ballet para Principiantes (dispara certificado),
                              con fechas de avance espaciadas (no consecutivas)
      - 3 Testimonial (uno por persona), enviados por cada estudiante vía POST /me/testimonials
        y aprobados por el Admin vía PATCH /admin/testimonials/{id}/publish

    Julián (buyer persona "investigador") no se siembra como usuario — se simula navegando
    de incógnito por las páginas públicas ya construidas en M4 (Nosotros, Instructores,
    Testimonios, FAQ). No requiere datos adicionales más allá de los sembrados aquí.

    Idempotente: puede volver a ejecutarse sin duplicar Levels/Instructors/FaqItems/Courses
    (se buscan por nombre/título antes de crear) ni Enrollments/testimonios (se ignoran
    conflictos 409 y se revisa /me/testimonials antes de enviar uno nuevo). Los registros de
    usuario ya existentes se detectan por el 409 de /auth/register y simplemente inician
    sesión con la misma contraseña.

    Requiere: API corriendo (dotnet run, con el admin seed por defecto ya creado al arrancar
    — ver Seed:AdminEmail/AdminPassword en appsettings.json) y el contenedor de Postgres
    levantado (docker-compose up -d). El paso 8 (espaciar fechas de Marcela) requiere además
    el cliente psql dentro del contenedor de Postgres (imagen postgres:16 lo trae).

.PARAMETER ApiBaseUrl
    URL base de la API. Por defecto http://localhost:5178 (perfil "http" de launchSettings.json).

.PARAMETER AdminEmail / AdminPassword
    Credenciales del Admin usadas para crear Levels/Instructors/FaqItems/Courses y moderar
    testimonios. Por defecto, el admin seed de appsettings.json (admin@danceacademy.local).

.PARAMETER PersonaPassword
    Contraseña usada para registrar a las 3 personas (mínimo 8 caracteres). Si el script ya
    se corrió antes con otra contraseña, el login de esa persona fallará — usar el mismo valor
    en cada corrida o borrar los usuarios de prueba antes de cambiarla.

.PARAMETER SkipBackdate
    Si se pasa, omite el paso 8 (espaciar por SQL las fechas de avance de Marcela). Útil si no
    se tiene acceso a `docker exec` desde donde se corre el script.

.PARAMETER ContainerName / DatabaseName
    Contenedor y base de datos de Postgres, usados solo en el paso 8. Por defecto
    danceacademy-postgres / danceacademy (docker-compose.yml).

.EXAMPLE
    ./scripts/seed-personas.ps1

.EXAMPLE
    ./scripts/seed-personas.ps1 -SkipBackdate
#>
param(
    [string]$ApiBaseUrl = "http://localhost:5178",

    [string]$AdminEmail = "admin@danceacademy.local",
    [string]$AdminPassword = "Admin12345!",

    [string]$PersonaPassword = "Persona12345!",

    [switch]$SkipBackdate,

    [string]$ContainerName = "danceacademy-postgres",
    [string]$DatabaseName = "danceacademy"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Helpers
# ---------------------------------------------------------------------------

function Invoke-Api {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Path,
        [hashtable]$Body,
        [string]$Token,
        [int[]]$IgnoreStatusCodes = @()
    )

    $uri = "$ApiBaseUrl$Path"
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    try {
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 8
            # Windows PowerShell 5.1 no manda -Body <string> como UTF-8 de forma confiable
            # (usa la codepage por defecto de la consola) — corrompe tildes/eñes al viajar
            # por HTTP. Se codifica a bytes UTF-8 explícitamente antes de enviar.
            $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
            return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers -ContentType "application/json; charset=utf-8" -Body $bytes
        }
        return Invoke-RestMethod -Method $Method -Uri $uri -Headers $headers
    }
    catch {
        $statusCode = $_.Exception.Response.StatusCode.value__
        if ($IgnoreStatusCodes -contains $statusCode) {
            return $null
        }
        Write-Error "Fallo $Method $Path (HTTP $statusCode): $($_.Exception.Message)"
        throw
    }
}

function Get-Token {
    param([string]$Email, [string]$Password)
    $result = Invoke-Api -Method Post -Path "/auth/login" -Body @{ email = $Email; password = $Password }
    return $result.access_token
}

function Ensure-Level {
    param([string]$Name, [int]$Order, [string]$AdminToken)

    $levels = Invoke-Api -Method Get -Path "/admin/levels" -Token $AdminToken
    $existing = $levels | Where-Object { $_.name -eq $Name }
    if ($existing) {
        Write-Host "   Nivel '$Name' ya existe." -ForegroundColor Yellow
        return $existing.id
    }

    $created = Invoke-Api -Method Post -Path "/admin/levels" -Token $AdminToken -Body @{ Name = $Name; Order = $Order }
    Write-Host "   Nivel '$Name' creado." -ForegroundColor Green
    return $created.id
}

function Ensure-Instructor {
    param([string]$FullName, [string]$Specialty, [string]$Bio, [string]$AdminToken)

    $instructors = Invoke-Api -Method Get -Path "/admin/instructors" -Token $AdminToken
    $existing = $instructors | Where-Object { $_.fullName -eq $FullName }
    if ($existing) {
        Write-Host "   Instructor '$FullName' ya existe." -ForegroundColor Yellow
        return
    }

    Invoke-Api -Method Post -Path "/admin/instructors" -Token $AdminToken -Body @{
        FullName = $FullName
        Specialty = $Specialty
        Bio = $Bio
        PhotoUrl = $null
    } | Out-Null
    Write-Host "   Instructor '$FullName' creado." -ForegroundColor Green
}

function Ensure-FaqItem {
    param([string]$Question, [string]$Answer, [string]$Category, [int]$Order, [string]$AdminToken)

    $items = Invoke-Api -Method Get -Path "/admin/faq" -Token $AdminToken
    $existing = $items | Where-Object { $_.question -eq $Question }
    if ($existing) {
        Write-Host "   FAQ '$Question' ya existe." -ForegroundColor Yellow
        return
    }

    Invoke-Api -Method Post -Path "/admin/faq" -Token $AdminToken -Body @{
        Question = $Question
        Answer = $Answer
        Category = $Category
        Order = $Order
    } | Out-Null
    Write-Host "   FAQ '$Question' creada." -ForegroundColor Green
}

# Crea (si no existe) un curso publicado con un único módulo publicado y sus lecciones
# publicadas. $Lessons es un arreglo de @{ Title = "..."; Content = "..." }. Devuelve
# @{ CourseId = ...; LessonIds = @(...) } con los LessonIds en el mismo orden que $Lessons.
function Ensure-CourseWithContent {
    param(
        [string]$Title,
        [string]$LevelId,
        [string]$Description,
        [string]$ModuleTitle,
        [array]$Lessons,
        [string]$AdminToken
    )

    $courses = Invoke-Api -Method Get -Path "/admin/courses" -Token $AdminToken
    $existingCourse = $courses | Where-Object { $_.title -eq $Title }

    if ($existingCourse) {
        Write-Host "   Curso '$Title' ya existe." -ForegroundColor Yellow
        $courseId = $existingCourse.id
    }
    else {
        $created = Invoke-Api -Method Post -Path "/admin/courses" -Token $AdminToken -Body @{
            Title = $Title
            LevelId = $LevelId
            Description = $Description
            PricingType = 1   # Free — únicas inscripciones habilitadas mientras la Fase 5 (Wompi) siga bloqueada
            Price = $null
            SubscriptionPlanIds = @()
        }
        Write-Host "   Curso '$Title' creado." -ForegroundColor Green
        $courseId = $created.id
    }

    $detail = Invoke-Api -Method Get -Path "/admin/courses/$courseId" -Token $AdminToken
    $module = $detail.modules | Where-Object { $_.title -eq $ModuleTitle }

    if (-not $module) {
        $createdModule = Invoke-Api -Method Post -Path "/admin/courses/$courseId/modules" -Token $AdminToken -Body @{ Title = $ModuleTitle; Order = 1 }
        Write-Host "   Módulo '$ModuleTitle' creado." -ForegroundColor Green
        $moduleId = $createdModule.id
        $existingLessons = @()
    }
    else {
        $moduleId = $module.id
        $existingLessons = $module.lessons
    }

    $order = 1
    foreach ($lesson in $Lessons) {
        $existingLesson = $existingLessons | Where-Object { $_.title -eq $lesson.Title }
        if (-not $existingLesson) {
            $createdLesson = Invoke-Api -Method Post -Path "/admin/modules/$moduleId/lessons" -Token $AdminToken -Body @{
                Title = $lesson.Title
                Order = $order
                Content = $lesson.Content
                VideoUrl = $null
            }
            Write-Host "   Lección '$($lesson.Title)' creada." -ForegroundColor Green
        }
        $order++
    }

    # Re-lee el curso completo y publica lo que falte (lecciones -> módulo -> curso, en ese orden,
    # porque /me/progress solo permite completar lecciones cuyo curso/módulo/lección estén publicados).
    $detail = Invoke-Api -Method Get -Path "/admin/courses/$courseId" -Token $AdminToken
    $moduleDetail = $detail.modules | Where-Object { $_.id -eq $moduleId }

    $orderedLessonIds = @()
    foreach ($lesson in $Lessons) {
        $lessonDetail = $moduleDetail.lessons | Where-Object { $_.title -eq $lesson.Title }
        if (-not $lessonDetail.isPublished) {
            Invoke-Api -Method Patch -Path "/admin/lessons/$($lessonDetail.id)/publish" -Token $AdminToken | Out-Null
        }
        $orderedLessonIds += $lessonDetail.id
    }

    if (-not $moduleDetail.isPublished) {
        Invoke-Api -Method Patch -Path "/admin/modules/$moduleId/publish" -Token $AdminToken | Out-Null
    }
    if (-not $detail.isPublished) {
        Invoke-Api -Method Patch -Path "/admin/courses/$courseId/publish" -Token $AdminToken | Out-Null
    }

    return [PSCustomObject]@{
        CourseId = $courseId
        LessonIds = $orderedLessonIds
    }
}

# Registra (o reutiliza, si ya existe) a una buyer persona y le pone FullName real.
function Register-Persona {
    param([string]$Email, [string]$FullName)

    Invoke-Api -Method Post -Path "/auth/register" -Body @{ email = $Email; password = $PersonaPassword } -IgnoreStatusCodes @(409) | Out-Null

    $token = Get-Token -Email $Email -Password $PersonaPassword

    Invoke-Api -Method Put -Path "/me" -Token $token -Body @{ FullName = $FullName; Phone = $null; BirthDate = $null } | Out-Null

    return $token
}

function Enroll-InCourse {
    param([string]$Token, [string]$CourseId)
    Invoke-Api -Method Post -Path "/me/enrollments" -Token $Token -Body @{ CourseId = $CourseId } -IgnoreStatusCodes @(409) | Out-Null
}

function Complete-Lessons {
    param([string]$Token, [array]$LessonIds)
    foreach ($lessonId in $LessonIds) {
        Invoke-Api -Method Post -Path "/me/progress/lessons/$lessonId/complete" -Token $Token | Out-Null
    }
}

function Submit-TestimonialIfMissing {
    param([string]$Token, [string]$Content, [int]$Rating, [string]$CourseId, [string]$PersonaLabel)

    $mine = Invoke-Api -Method Get -Path "/me/testimonials" -Token $Token
    if ($mine -and $mine.Count -gt 0) {
        Write-Host "   $PersonaLabel ya tiene un testimonio enviado, se omite." -ForegroundColor Yellow
        return
    }

    Invoke-Api -Method Post -Path "/me/testimonials" -Token $Token -Body @{ Content = $Content; Rating = $Rating; CourseId = $CourseId } | Out-Null
    Write-Host "   Testimonio de $PersonaLabel enviado (pendiente de moderación)." -ForegroundColor Green
}

function Publish-TestimonialsByStudentName {
    param([string]$AdminToken, [string]$StudentName)

    $testimonials = Invoke-Api -Method Get -Path "/admin/testimonials" -Token $AdminToken
    $pending = $testimonials | Where-Object { $_.studentName -eq $StudentName -and -not $_.isPublished }
    foreach ($t in $pending) {
        Invoke-Api -Method Patch -Path "/admin/testimonials/$($t.id)/publish" -Token $AdminToken | Out-Null
        Write-Host "   Testimonio de '$StudentName' aprobado y publicado." -ForegroundColor Green
    }
}

# ---------------------------------------------------------------------------
# Datos de contenido (español, sin lorem ipsum)
# ---------------------------------------------------------------------------

$balletLessons = @(
    @{ Title = "Postura y alineación básica"; Content = "Introducción a la postura correcta: alineación de columna, hombros y cadera antes de empezar cualquier ejercicio de ballet." },
    @{ Title = "Las cinco posiciones de pies"; Content = "Explicación y práctica guiada de las cinco posiciones básicas de pies del ballet clásico." },
    @{ Title = "Posiciones de brazos (port de bras)"; Content = "Coordinación de brazos con las posiciones de pies ya aprendidas; primeros ejercicios de port de bras." },
    @{ Title = "Ejercicios en la barra"; Content = "Rutina básica de barra: pliés, tendus y elevaciones sencillas para fortalecer piernas y equilibrio." },
    @{ Title = "Primera coreografía: variación sencilla"; Content = "Combinación corta que integra posiciones de pies, brazos y los ejercicios de barra vistos en el curso." }
)

$salsaLessons = @(
    @{ Title = "Repaso de paso básico y timing"; Content = "Repaso del paso básico de salsa y del conteo (timing) sobre la música, base para el resto del curso." },
    @{ Title = "Vueltas sencillas (vuelta derecha e izquierda)"; Content = "Introducción a las vueltas básicas de salsa, individualmente antes de combinarlas con pareja." },
    @{ Title = "Combinaciones de pasos con desplazamiento"; Content = "Encadenar el paso básico con desplazamientos laterales y hacia adelante/atrás en la pista." },
    @{ Title = "Trabajo de pareja: guía y seguimiento"; Content = "Fundamentos de guía (lead) y seguimiento (follow) para bailar en pareja de forma coordinada." }
)

$instructors = @(
    @{ FullName = "Laura Fernández"; Specialty = "Ballet clásico"; Bio = "Bailarina profesional con 12 años de experiencia en compañías de ballet clásico y formación en pedagogía dancística." },
    @{ FullName = "Carlos Ramírez"; Specialty = "Salsa y ritmos latinos"; Bio = "Instructor certificado de salsa con más de 10 años enseñando en academias y eventos de baile social." },
    @{ FullName = "Daniela Torres"; Specialty = "Danza contemporánea"; Bio = "Formada en danza contemporánea, combina técnica clásica con movimiento libre en sus clases." }
)

$faqItems = @(
    @{ Question = "¿Cómo creo una cuenta en DanceAcademy?"; Answer = "Regístrate desde la página de inicio con tu correo electrónico y una contraseña de al menos 8 caracteres. Podrás empezar a explorar el catálogo de inmediato."; Category = "Cuenta"; Order = 1 },
    @{ Question = "¿Puedo cambiar mi contraseña?"; Answer = "Sí, desde 'Mi Cuenta' puedes actualizar tu contraseña en cualquier momento ingresando tu contraseña actual y la nueva."; Category = "Cuenta"; Order = 2 },
    @{ Question = "¿Los cursos tienen costo?"; Answer = "Actualmente todos los cursos publicados son gratuitos. Estamos preparando planes de pago e inscripción individual para próximamente."; Category = "Pagos"; Order = 1 },
    @{ Question = "¿Qué métodos de pago aceptarán?"; Answer = "Estamos integrando pagos en línea y anunciaremos los métodos disponibles antes de habilitar los cursos de pago."; Category = "Pagos"; Order = 2 },
    @{ Question = "¿Los cursos incluyen certificado?"; Answer = "Sí, al completar el 100% de las lecciones publicadas de un curso recibes automáticamente un certificado de finalización descargable."; Category = "Contenido"; Order = 1 },
    @{ Question = "¿Puedo ver las clases a mi propio ritmo?"; Answer = "Sí, todas las lecciones quedan disponibles para que las veas cuantas veces quieras, sin fechas límite."; Category = "Contenido"; Order = 2 },
    @{ Question = "El video de una lección no carga, ¿qué hago?"; Answer = "Verifica tu conexión a internet y recarga la página. Si el problema persiste, escríbenos desde la página de Contacto."; Category = "Técnico"; Order = 1 },
    @{ Question = "¿La plataforma funciona en el celular?"; Answer = "Sí, DanceAcademy funciona desde el navegador de tu celular sin necesidad de instalar una app."; Category = "Técnico"; Order = 2 }
)

$camilaEmail = "camila.persona@danceacademy.local"
$camilaFullName = "Camila Rodríguez"

$andresEmail = "andres.persona@danceacademy.local"
$andresFullName = "Andrés Martínez"

$marcelaEmail = "marcela.persona@danceacademy.local"
$marcelaFullName = "Marcela Gómez"

# ---------------------------------------------------------------------------
# 1) Login como Admin
# ---------------------------------------------------------------------------

Write-Host "1) Iniciando sesión como Admin en $ApiBaseUrl ..." -ForegroundColor Cyan
$adminToken = Get-Token -Email $AdminEmail -Password $AdminPassword
if (-not $adminToken) {
    Write-Error "No se pudo autenticar como Admin. Verifica que la API esté corriendo y que el admin seed exista (Seed:AdminEmail/AdminPassword en appsettings.json)."
    exit 1
}
Write-Host "   Sesión de Admin OK." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2) Levels
# ---------------------------------------------------------------------------

Write-Host "2) Asegurando niveles ..." -ForegroundColor Cyan
$principianteId = Ensure-Level -Name "Principiante" -Order 1 -AdminToken $adminToken
$intermedioId = Ensure-Level -Name "Intermedio" -Order 2 -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 3) Instructores
# ---------------------------------------------------------------------------

Write-Host "3) Asegurando instructores ..." -ForegroundColor Cyan
foreach ($i in $instructors) {
    Ensure-Instructor -FullName $i.FullName -Specialty $i.Specialty -Bio $i.Bio -AdminToken $adminToken
}

# ---------------------------------------------------------------------------
# 4) FAQ
# ---------------------------------------------------------------------------

Write-Host "4) Asegurando preguntas frecuentes ..." -ForegroundColor Cyan
foreach ($f in $faqItems) {
    Ensure-FaqItem -Question $f.Question -Answer $f.Answer -Category $f.Category -Order $f.Order -AdminToken $adminToken
}

# ---------------------------------------------------------------------------
# 5) Cursos con contenido publicado
# ---------------------------------------------------------------------------

Write-Host "5) Asegurando curso 'Ballet para Principiantes' ..." -ForegroundColor Cyan
$balletCourse = Ensure-CourseWithContent `
    -Title "Ballet para Principiantes" `
    -LevelId $principianteId `
    -Description "Curso introductorio de ballet clásico: postura, posiciones básicas de brazos y piernas, y una primera coreografía sencilla." `
    -ModuleTitle "Fundamentos de Ballet" `
    -Lessons $balletLessons `
    -AdminToken $adminToken

Write-Host "6) Asegurando curso 'Salsa Intermedia' ..." -ForegroundColor Cyan
$salsaCourse = Ensure-CourseWithContent `
    -Title "Salsa Intermedia" `
    -LevelId $intermedioId `
    -Description "Perfecciona tu técnica de salsa con pasos combinados, vueltas y trabajo de pareja." `
    -ModuleTitle "Pasos de Salsa" `
    -Lessons $salsaLessons `
    -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 7) Buyer personas — registro, inscripción y avance
# ---------------------------------------------------------------------------

Write-Host "7) Sembrando buyer persona Camila (principiante ansiosa, ~40% de avance) ..." -ForegroundColor Cyan
$camilaToken = Register-Persona -Email $camilaEmail -FullName $camilaFullName
Enroll-InCourse -Token $camilaToken -CourseId $balletCourse.CourseId
Complete-Lessons -Token $camilaToken -LessonIds $balletCourse.LessonIds[0..1]   # 2 de 5 lecciones = 40%
Submit-TestimonialIfMissing -Token $camilaToken -Content "Empecé sin saber nada de ballet y el curso me hizo sentir cómoda desde la primera clase. Las explicaciones son claras y puedo repasar los videos las veces que necesite." -Rating 5 -CourseId $balletCourse.CourseId -PersonaLabel "Camila"

Write-Host "8) Sembrando buyer persona Andrés (paga por estructura y certificados) ..." -ForegroundColor Cyan
$andresToken = Register-Persona -Email $andresEmail -FullName $andresFullName
Enroll-InCourse -Token $andresToken -CourseId $balletCourse.CourseId
Complete-Lessons -Token $andresToken -LessonIds $balletCourse.LessonIds        # 5 de 5 = 100% -> certificado
Enroll-InCourse -Token $andresToken -CourseId $salsaCourse.CourseId
Complete-Lessons -Token $andresToken -LessonIds $salsaCourse.LessonIds[0..1]   # 2 de 4 = 50%, en progreso
Submit-TestimonialIfMissing -Token $andresToken -Content "Ya llevo dos cursos completos y los certificados quedaron perfectos para mi portafolio. La estructura por módulos hace que sea fácil seguir el avance." -Rating 5 -CourseId $balletCourse.CourseId -PersonaLabel "Andrés"

Write-Host "9) Sembrando buyer persona Marcela (retoma el baile, prueba social) ..." -ForegroundColor Cyan
$marcelaToken = Register-Persona -Email $marcelaEmail -FullName $marcelaFullName
Enroll-InCourse -Token $marcelaToken -CourseId $balletCourse.CourseId
Complete-Lessons -Token $marcelaToken -LessonIds $balletCourse.LessonIds       # 5 de 5 = 100% -> certificado
Submit-TestimonialIfMissing -Token $marcelaToken -Content "Volví a bailar después de varios años y este curso me ayudó a retomar la técnica con calma, a mi propio ritmo. Me hubiera gustado tener más ejercicios de repaso, pero en general muy buena experiencia." -Rating 4 -CourseId $balletCourse.CourseId -PersonaLabel "Marcela"

# ---------------------------------------------------------------------------
# 10) Moderación — Admin aprueba los 3 testimonios
# ---------------------------------------------------------------------------

Write-Host "10) Aprobando testimonios como Admin ..." -ForegroundColor Cyan
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $camilaFullName
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $andresFullName
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $marcelaFullName

# ---------------------------------------------------------------------------
# 11) Espaciar fechas de avance de Marcela (SQL directo, opcional)
# ---------------------------------------------------------------------------

if (-not $SkipBackdate) {
    Write-Host "11) Espaciando fechas de avance de Marcela (SQL directo, contenedor $ContainerName) ..." -ForegroundColor Cyan

    $escapedEmail = $marcelaEmail.Replace("'", "''")
    $sqlLines = @(
        "\set ON_ERROR_STOP on"
        "WITH target_user AS ("
        "  SELECT ""Id"" FROM ""Users"" WHERE ""Email"" = '$escapedEmail'"
        "),"
        "ordered_progress AS ("
        "  SELECT lp.""Id"" AS progress_id, ROW_NUMBER() OVER (ORDER BY l.""Order"") AS rn"
        "  FROM ""LessonProgresses"" lp"
        "  JOIN ""Lessons"" l ON l.""Id"" = lp.""LessonId"""
        "  WHERE lp.""UserId"" = (SELECT ""Id"" FROM target_user) AND lp.""IsCompleted"" = true"
        ")"
        "UPDATE ""LessonProgresses"" lp"
        "SET ""CompletedAt"" = NOW() - make_interval(days => 30 - (op.rn * 5)::int)"
        "FROM ordered_progress op"
        "WHERE lp.""Id"" = op.progress_id;"
        ""
        "UPDATE ""Certificates"""
        "SET ""IssuedAt"" = NOW() - INTERVAL '2 days'"
        "WHERE ""UserId"" = (SELECT ""Id"" FROM ""Users"" WHERE ""Email"" = '$escapedEmail');"
    )
    $sql = $sqlLines -join "`n"

    # Se manda por stdin (en vez de -c) por la misma razón que scripts/create-admin.ps1:
    # pasar comillas dobles como argumento a un ejecutable nativo desde PowerShell en Windows
    # las corrompe.
    $sql | docker exec -i $ContainerName psql -U postgres -d $DatabaseName

    if ($LASTEXITCODE -ne 0) {
        Write-Warning "No se pudieron espaciar las fechas de avance de Marcela (revisa el mensaje de psql arriba). El resto del seed ya quedó aplicado correctamente vía API."
    }
    else {
        Write-Host "   Fechas de avance de Marcela espaciadas (no consecutivas)." -ForegroundColor Green
    }
}
else {
    Write-Host "11) -SkipBackdate indicado: se omite el espaciado de fechas de Marcela." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Listo. Buyer personas sembradas:" -ForegroundColor Green
Write-Host "  - $camilaFullName  <$camilaEmail>  (contraseña: $PersonaPassword)"
Write-Host "  - $andresFullName  <$andresEmail>  (contraseña: $PersonaPassword)"
Write-Host "  - $marcelaFullName <$marcelaEmail> (contraseña: $PersonaPassword)"
Write-Host "Julián no se siembra — se simula navegando de incógnito por /nosotros, /instructores, /testimonios y /faq."
