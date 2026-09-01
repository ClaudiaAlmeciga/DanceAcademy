<#
.SYNOPSIS
    Siembra un catálogo completo y visualmente atractivo de DanceAcademy (niveles, planes de
    suscripción, instructores con foto, cursos con imagen y contenido, eventos, noticias, FAQ
    y un par de testimonios) usando únicamente los endpoints públicos/admin de la API — pensado
    para dejar la demo desplegada (portafolio) con datos reales de presentar, no de prueba.

.DESCRIPTION
    Crea, si no existen ya:
      - 3 Levels (Principiante, Intermedio, Avanzado)
      - 3 SubscriptionPlans (Mensual, Trimestral, Anual)
      - 4 Instructor, cada uno con foto (PhotoUrl)
      - 6 Course publicados, cada uno con imagen de portada, duración estimada, un Module y
        2-3 Lessons publicadas, cubriendo los 3 niveles y los 4 tipos de tarificación
        (Free, IndividualPurchase, SubscriptionIncluded, Both)
      - 4 Event publicados, con imagen
      - 3 NewsPost publicados, con imagen
      - 5 FaqItem
      - 3 usuarios Student de muestra, uno inscrito y con el curso gratuito completo
        (certificado incluido) y los tres con un testimonio publicado

    Es un script hermano de seed-personas.ps1 (que siembra datos angostos para pruebas de
    M8/roadmap) — este siembra el catálogo amplio pensado para mostrar la aplicación como
    portafolio. Ambos son idempotentes y pueden correr en el mismo entorno sin chocar entre sí
    (buscan por título/nombre antes de crear).

    Solo se puede inscribir (vía API) en cursos gratuitos — la inscripción a cursos de pago
    está pendiente de la integración de Wompi (Fase 5) — por eso el único curso usado para la
    demo de inscripción/progreso/certificado es "Bachata Sensual" (Free). Los testimonios de
    las otras dos personas se envían sin curso vinculado (CourseId = null), válido según
    MeTestimonialsEndpoints.

.PARAMETER ApiBaseUrl
    URL base de la API. Por defecto la API en producción (Render). Pasa
    "http://localhost:5178" para sembrar el entorno local en su lugar.

.PARAMETER AdminEmail / AdminPassword
    Credenciales del Admin.

.PARAMETER PersonaPassword
    Contraseña usada para registrar los 3 estudiantes de muestra (mínimo 8 caracteres).

.EXAMPLE
    ./scripts/seed-portfolio.ps1

.EXAMPLE
    ./scripts/seed-portfolio.ps1 -ApiBaseUrl "http://localhost:5178"
#>
param(
    [string]$ApiBaseUrl = "https://danceacademy-api.onrender.com",

    [string]$AdminEmail = "admin@danceacademy.local",
    [string]$AdminPassword = "Admin12345!",

    [string]$PersonaPassword = "Persona12345!"
)

$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------------------
# Helpers (mismo patrón que scripts/seed-personas.ps1)
# ---------------------------------------------------------------------------

function Invoke-Api {
    param(
        [Parameter(Mandatory = $true)][string]$Method,
        [Parameter(Mandatory = $true)][string]$Path,
        $Body,
        [string]$Token,
        [int[]]$IgnoreStatusCodes = @()
    )

    $uri = "$ApiBaseUrl$Path"
    $headers = @{}
    if ($Token) { $headers["Authorization"] = "Bearer $Token" }

    try {
        if ($null -ne $Body) {
            $json = $Body | ConvertTo-Json -Depth 8
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

function Ensure-SubscriptionPlan {
    param([string]$Name, [string]$Description, [decimal]$Price, [int]$BillingPeriodDays, [string]$AdminToken)

    $plans = Invoke-Api -Method Get -Path "/admin/subscription-plans" -Token $AdminToken
    $existing = $plans | Where-Object { $_.name -eq $Name }
    if ($existing) {
        Write-Host "   Plan '$Name' ya existe." -ForegroundColor Yellow
        return $existing.id
    }

    $created = Invoke-Api -Method Post -Path "/admin/subscription-plans" -Token $AdminToken -Body @{
        Name = $Name
        Description = $Description
        Price = $Price
        BillingPeriodDays = $BillingPeriodDays
    }
    Write-Host "   Plan '$Name' creado." -ForegroundColor Green
    return $created.id
}

function Ensure-Instructor {
    param([string]$FullName, [string]$Specialty, [string]$Bio, [string]$PhotoUrl, [string]$AdminToken)

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
        PhotoUrl = $PhotoUrl
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

function Ensure-Event {
    param(
        [string]$Title, [string]$Description, [string]$Location, [string]$EventDate,
        [decimal]$Price, [int]$Capacity, [string]$ImageUrl, [string]$AdminToken
    )

    $events = Invoke-Api -Method Get -Path "/admin/events" -Token $AdminToken
    $existing = $events | Where-Object { $_.title -eq $Title }
    if ($existing) {
        Write-Host "   Evento '$Title' ya existe." -ForegroundColor Yellow
        if (-not $existing.isPublished) {
            Invoke-Api -Method Patch -Path "/admin/events/$($existing.id)/publish" -Token $AdminToken | Out-Null
        }
        return
    }

    $created = Invoke-Api -Method Post -Path "/admin/events" -Token $AdminToken -Body @{
        Title = $Title
        Description = $Description
        Location = $Location
        EventDate = $EventDate
        Price = $Price
        Capacity = $Capacity
        ImageUrl = $ImageUrl
    }
    Invoke-Api -Method Patch -Path "/admin/events/$($created.id)/publish" -Token $AdminToken | Out-Null
    Write-Host "   Evento '$Title' creado y publicado." -ForegroundColor Green
}

function Ensure-NewsPost {
    param([string]$Title, [string]$Content, [string]$ImageUrl, [string]$PublishedAt, [string]$AdminToken)

    $posts = Invoke-Api -Method Get -Path "/admin/news" -Token $AdminToken
    $existing = $posts | Where-Object { $_.title -eq $Title }
    if ($existing) {
        Write-Host "   Noticia '$Title' ya existe." -ForegroundColor Yellow
        if (-not $existing.isPublished) {
            Invoke-Api -Method Patch -Path "/admin/news/$($existing.id)/publish" -Token $AdminToken | Out-Null
        }
        return
    }

    $created = Invoke-Api -Method Post -Path "/admin/news" -Token $AdminToken -Body @{
        Title = $Title
        Content = $Content
        ImageUrl = $ImageUrl
        PublishedAt = $PublishedAt
    }
    Invoke-Api -Method Patch -Path "/admin/news/$($created.id)/publish" -Token $AdminToken | Out-Null
    Write-Host "   Noticia '$Title' creada y publicada." -ForegroundColor Green
}

# Crea (si no existe) un curso publicado con imagen, duración, tarificación y un único módulo
# con sus lecciones, todo publicado. $Lessons es un arreglo de @{ Title=...; Content=... }.
# $SubscriptionPlanIds puede ser @() si el curso no está incluido en ningún plan.
function Ensure-CourseWithContent {
    param(
        [string]$Title,
        [string]$LevelId,
        [string]$Description,
        [string]$ImageUrl,
        [int]$DurationHours,
        [int]$PricingType,
        $Price,
        [array]$SubscriptionPlanIds,
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
            PricingType = $PricingType
            Price = $Price
            SubscriptionPlanIds = $SubscriptionPlanIds
            ImageUrl = $ImageUrl
            DurationHours = $DurationHours
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
            Invoke-Api -Method Post -Path "/admin/modules/$moduleId/lessons" -Token $AdminToken -Body @{
                Title = $lesson.Title
                Order = $order
                Content = $lesson.Content
                VideoUrl = $null
            } | Out-Null
            Write-Host "   Lección '$($lesson.Title)' creada." -ForegroundColor Green
        }
        $order++
    }

    # Re-lee y publica lo que falte (lecciones -> módulo -> curso, en ese orden).
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
    param([string]$Token, [string]$Content, [int]$Rating, $CourseId, [string]$PersonaLabel)

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
# Imágenes (Unsplash, licencia libre para uso comercial, verificadas antes de sembrar)
# ---------------------------------------------------------------------------

$imgSalsa       = "https://images.unsplash.com/photo-1508642054-5e6cade8ff13?w=1200&q=80&auto=format&fit=crop"
$imgBalletGroup = "https://images.unsplash.com/photo-1685339009948-d807094b1457?w=1200&q=80&auto=format&fit=crop"
$imgBalletPortrait = "https://plus.unsplash.com/premium_photo-1663013493901-21a2de459aec?w=1200&q=80&auto=format&fit=crop"
$imgHipHop      = "https://plus.unsplash.com/premium_photo-1682089706055-d5ef14dc14e4?w=1200&q=80&auto=format&fit=crop"
$imgTango1      = "https://images.unsplash.com/photo-1751059268031-8a9b5805f6f3?w=1200&q=80&auto=format&fit=crop"
$imgTango2      = "https://images.unsplash.com/photo-1750863773776-7f8d8f826e4d?w=1200&q=80&auto=format&fit=crop"
$imgTangoDim    = "https://images.unsplash.com/photo-1760428299850-338d418e2bb4?w=1200&q=80&auto=format&fit=crop"
$imgBachata     = "https://images.unsplash.com/photo-1736552723645-9ce633f134c6?w=1200&q=80&auto=format&fit=crop"
$imgClub1       = "https://images.unsplash.com/photo-1766650551665-45f1998bd671?w=1200&q=80&auto=format&fit=crop"
$imgClub2       = "https://plus.unsplash.com/premium_photo-1661369901339-f6ac6d76541f?w=1200&q=80&auto=format&fit=crop"
$imgTangoTeacher = "https://images.unsplash.com/photo-1663691219171-93494f63b5c9?w=1200&q=80&auto=format&fit=crop"
$imgTeacherF1   = "https://plus.unsplash.com/premium_photo-1663011483768-2cd153a2d07a?w=1200&q=80&auto=format&fit=crop"
$imgManSmile    = "https://images.unsplash.com/photo-1527585743534-7113e3211270?w=1200&q=80&auto=format&fit=crop"
$imgWomanSmile  = "https://images.unsplash.com/photo-1687360440094-949b8fe71c8c?w=1200&q=80&auto=format&fit=crop"

# ---------------------------------------------------------------------------
# 1) Login como Admin
# ---------------------------------------------------------------------------

Write-Host "1) Iniciando sesión como Admin en $ApiBaseUrl ..." -ForegroundColor Cyan
$adminToken = Get-Token -Email $AdminEmail -Password $AdminPassword
if (-not $adminToken) {
    Write-Error "No se pudo autenticar como Admin."
    exit 1
}
Write-Host "   Sesión de Admin OK." -ForegroundColor Green

# ---------------------------------------------------------------------------
# 2) Levels
# ---------------------------------------------------------------------------

Write-Host "2) Asegurando niveles ..." -ForegroundColor Cyan
$principianteId = Ensure-Level -Name "Principiante" -Order 1 -AdminToken $adminToken
$intermedioId   = Ensure-Level -Name "Intermedio"   -Order 2 -AdminToken $adminToken
$avanzadoId     = Ensure-Level -Name "Avanzado"     -Order 3 -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 3) Planes de suscripción
# ---------------------------------------------------------------------------

Write-Host "3) Asegurando planes de suscripción ..." -ForegroundColor Cyan
$planMensualId = Ensure-SubscriptionPlan -Name "Plan Mensual" -Description "Acceso ilimitado a todos los cursos incluidos en suscripción, mes a mes." -Price 79000 -BillingPeriodDays 30 -AdminToken $adminToken
$planTrimestralId = Ensure-SubscriptionPlan -Name "Plan Trimestral" -Description "El mismo acceso que el plan mensual, pagando cada tres meses y ahorrando frente al mes a mes." -Price 210000 -BillingPeriodDays 90 -AdminToken $adminToken
$planAnualId = Ensure-SubscriptionPlan -Name "Plan Anual" -Description "El mejor precio por mes para quienes quieren bailar todo el año, sin preocuparse por renovar." -Price 750000 -BillingPeriodDays 365 -AdminToken $adminToken

$allPlanIds = @($planMensualId, $planTrimestralId, $planAnualId)

# ---------------------------------------------------------------------------
# 4) Instructores
# ---------------------------------------------------------------------------

Write-Host "4) Asegurando instructores ..." -ForegroundColor Cyan
Ensure-Instructor -FullName "Camila Torres" -Specialty "Salsa y Bachata" -Bio "Instructora certificada de salsa y bachata con más de 8 años enseñando baile social en academias y eventos privados." -PhotoUrl $imgTeacherF1 -AdminToken $adminToken
Ensure-Instructor -FullName "Andrés Rodríguez" -Specialty "Hip Hop y Danza Urbana" -Bio "Bailarín y coreógrafo de danza urbana, ha representado a Colombia en competencias internacionales de hip hop." -PhotoUrl $imgManSmile -AdminToken $adminToken
Ensure-Instructor -FullName "Valentina Gómez" -Specialty "Ballet y Danza Contemporánea" -Bio "Formada en ballet clásico, combina técnica y expresión contemporánea en clases pensadas para todos los niveles." -PhotoUrl $imgWomanSmile -AdminToken $adminToken
Ensure-Instructor -FullName "Isabella Ramírez" -Specialty "Tango y Danzas de Salón" -Bio "Bailarina de tango con formación en Buenos Aires, enseña la técnica y la conexión de pareja propias del tango argentino." -PhotoUrl $imgTangoTeacher -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 5) Cursos
# ---------------------------------------------------------------------------

Write-Host "5) Asegurando cursos ..." -ForegroundColor Cyan

$salsaCourse = Ensure-CourseWithContent -Title "Salsa desde Cero" -LevelId $principianteId `
    -Description "Aprende salsa desde el primer paso: timing, vueltas básicas y todo lo necesario para animarte a bailar en cualquier fiesta." `
    -ImageUrl $imgSalsa -DurationHours 12 -PricingType 2 -Price 120000 -SubscriptionPlanIds @() `
    -ModuleTitle "Primeros pasos" -Lessons @(
        @{ Title = "El paso básico y el conteo"; Content = "El paso básico de salsa y cómo contar la música (1-2-3, 5-6-7) para no perder el tiempo, la base de todo lo que viene después." },
        @{ Title = "Vueltas sencillas"; Content = "Las primeras vueltas de salsa, practicadas solo antes de combinarlas con pareja, para ganar seguridad en el giro." },
        @{ Title = "Baile social: cómo sacar a bailar y guiar"; Content = "Cómo invitar a bailar, la postura de pareja y los fundamentos de guía (lead) y seguimiento (follow) en la pista." }
    ) -AdminToken $adminToken

$balletCourse = Ensure-CourseWithContent -Title "Ballet Clásico" -LevelId $principianteId `
    -Description "Curso introductorio de ballet clásico: postura, posiciones básicas de brazos y piernas, y ejercicios de barra." `
    -ImageUrl $imgBalletGroup -DurationHours 20 -PricingType 3 -Price $null -SubscriptionPlanIds $allPlanIds `
    -ModuleTitle "Fundamentos" -Lessons @(
        @{ Title = "Postura y alineación"; Content = "La postura correcta del ballet: alineación de columna, hombros y cadera, base de todos los ejercicios posteriores." },
        @{ Title = "Las cinco posiciones de pies"; Content = "Explicación y práctica guiada de las cinco posiciones básicas de pies del ballet clásico." },
        @{ Title = "Ejercicios en la barra"; Content = "Rutina básica de barra: pliés, tendus y elevaciones sencillas para fortalecer piernas y equilibrio." }
    ) -AdminToken $adminToken

$hipHopCourse = Ensure-CourseWithContent -Title "Hip Hop Urbano" -LevelId $intermedioId `
    -Description "Groove, aislamientos y una coreografía freestyle para quienes ya tienen una base de baile y quieren soltarse." `
    -ImageUrl $imgHipHop -DurationHours 15 -PricingType 4 -Price 90000 -SubscriptionPlanIds $allPlanIds `
    -ModuleTitle "Groove y estilo" -Lessons @(
        @{ Title = "Aislamientos y groove básico"; Content = "Aislamientos de cabeza, hombros y cadera, y cómo mantener el groove sobre el beat de principio a fin." },
        @{ Title = "Coreografía freestyle: primeros pasos"; Content = "Una combinación corta pensada para practicar estilo personal dentro de una estructura freestyle." }
    ) -AdminToken $adminToken

$tangoCourse = Ensure-CourseWithContent -Title "Tango Argentino" -LevelId $avanzadoId `
    -Description "El abrazo, la caminata y los primeros adornos del tango argentino, para bailarines con experiencia previa en pareja." `
    -ImageUrl $imgTango1 -DurationHours 18 -PricingType 2 -Price 150000 -SubscriptionPlanIds @() `
    -ModuleTitle "El abrazo y la caminata" -Lessons @(
        @{ Title = "El abrazo y la conexión con la pareja"; Content = "Cómo construir un abrazo cómodo y una conexión de peso que permita comunicarse sin hablar." },
        @{ Title = "La caminata y el cambio de peso"; Content = "La caminata del tango: cambios de peso, dirección y musicalidad, la base de toda improvisación." },
        @{ Title = "Ochos y giros básicos"; Content = "Los primeros ochos y giros, construidos sobre la caminata y el abrazo ya trabajados." }
    ) -AdminToken $adminToken

$bachataCourse = Ensure-CourseWithContent -Title "Bachata Sensual" -LevelId $intermedioId `
    -Description "Paso básico, movimiento de cadera y musicalidad de la bachata sensual — curso gratuito, ideal para empezar." `
    -ImageUrl $imgBachata -DurationHours 8 -PricingType 1 -Price $null -SubscriptionPlanIds @() `
    -ModuleTitle "Fundamentos de bachata" -Lessons @(
        @{ Title = "Paso básico y movimiento de cadera"; Content = "El paso básico de bachata y el movimiento de cadera característico del estilo, trabajados por separado antes de unirlos." },
        @{ Title = "Sensualidad y musicalidad"; Content = "Cómo interpretar la música de bachata con el cuerpo, sin perder el paso básico ya aprendido." }
    ) -AdminToken $adminToken

$contemporaryCourse = Ensure-CourseWithContent -Title "Danza Contemporánea" -LevelId $avanzadoId `
    -Description "Trabajo de suelo, contacto e improvisación guiada para bailarines que buscan ampliar su vocabulario de movimiento." `
    -ImageUrl $imgBalletPortrait -DurationHours 16 -PricingType 3 -Price $null -SubscriptionPlanIds $allPlanIds `
    -ModuleTitle "Movimiento y expresión" -Lessons @(
        @{ Title = "Suelo y contacto con el piso"; Content = "Ejercicios de suelo: cómo caer, rodar y recuperarse sin tensión, base del trabajo contemporáneo." },
        @{ Title = "Improvisación guiada"; Content = "Estructuras simples de improvisación para explorar movimiento propio dentro de una consigna clara." }
    ) -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 6) Eventos
# ---------------------------------------------------------------------------

Write-Host "6) Asegurando eventos ..." -ForegroundColor Cyan
Ensure-Event -Title "Festival de Salsa y Bachata" -Description "Una noche de baile social abierta a todo público, con presentaciones de nuestros equipos y DJ en vivo." -Location "Auditorio Principal — Sede DanceAcademy" -EventDate "2026-10-17T19:00:00-05:00" -Price 25000 -Capacity 150 -ImageUrl $imgClub1 -AdminToken $adminToken
Ensure-Event -Title "Noche de Tango" -Description "Milonga abierta con una breve clase de introducción antes de la pista libre." -Location "Salón Milonga" -EventDate "2026-09-27T20:00:00-05:00" -Price 30000 -Capacity 60 -ImageUrl $imgTangoDim -AdminToken $adminToken
Ensure-Event -Title "Masterclass de Hip Hop con Andrés Rodríguez" -Description "Clase especial de dos horas con nuestro instructor de danza urbana, abierta a nivel intermedio y avanzado." -Location "Estudio 2" -EventDate "2026-09-13T16:00:00-05:00" -Price 0 -Capacity 40 -ImageUrl $imgHipHop -AdminToken $adminToken
Ensure-Event -Title "Exhibición de Fin de Año" -Description "Muestra anual de todos los cursos: cada grupo presenta lo aprendido durante el semestre." -Location "Teatro Municipal" -EventDate "2026-12-06T18:00:00-05:00" -Price 0 -Capacity 300 -ImageUrl $imgClub2 -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 7) Noticias
# ---------------------------------------------------------------------------

Write-Host "7) Asegurando noticias ..." -ForegroundColor Cyan
Ensure-NewsPost -Title "¡Abrimos inscripciones para el nuevo semestre!" -Content "Ya están abiertas las inscripciones a todos nuestros cursos, con dos nuevos horarios de hip hop y ballet para facilitar el acceso a quienes trabajan de día." -ImageUrl $imgBalletGroup -PublishedAt "2026-08-28T09:00:00-05:00" -AdminToken $adminToken
Ensure-NewsPost -Title "Nuestros estudiantes brillaron en el Festival Nacional de Danza" -Content "El equipo de salsa y bachata de la academia representó a la ciudad en el Festival Nacional de Danza, con una presentación que emocionó al jurado y al público." -ImageUrl $imgClub1 -PublishedAt "2026-08-20T09:00:00-05:00" -AdminToken $adminToken
Ensure-NewsPost -Title "Nueva alianza con estudios de tango de Buenos Aires" -Content "Firmamos una alianza con dos estudios de Buenos Aires para intercambios y clases magistrales de tango durante el próximo año." -ImageUrl $imgTango2 -PublishedAt "2026-08-10T09:00:00-05:00" -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 8) FAQ
# ---------------------------------------------------------------------------

Write-Host "8) Asegurando preguntas frecuentes ..." -ForegroundColor Cyan
Ensure-FaqItem -Question "¿Necesito experiencia previa para inscribirme a un curso?" -Answer "No. Cada curso indica su nivel (Principiante, Intermedio o Avanzado) para que elijas el que mejor se ajuste a tu experiencia actual." -Category "Cursos" -Order 1 -AdminToken $adminToken
Ensure-FaqItem -Question "¿Cómo funcionan los planes de suscripción?" -Answer "Un plan de suscripción te da acceso a todos los cursos marcados como 'Incluido en suscripción' mientras esté activo, sin pagar cada curso por separado." -Category "Pagos" -Order 1 -AdminToken $adminToken
Ensure-FaqItem -Question "¿Puedo comprar un curso sin suscribirme?" -Answer "Sí, varios cursos están disponibles como compra individual, sin necesidad de una suscripción activa." -Category "Pagos" -Order 2 -AdminToken $adminToken
Ensure-FaqItem -Question "¿Necesito estar suscrito para inscribirme a los eventos?" -Answer "No, los eventos son independientes de los cursos y las suscripciones: se pagan e inscriben por separado." -Category "Eventos" -Order 1 -AdminToken $adminToken
Ensure-FaqItem -Question "¿Qué necesito para tomar las clases desde casa?" -Answer "Solo un navegador y conexión a internet — DanceAcademy funciona desde el computador o el celular, sin instalar nada." -Category "Técnico" -Order 1 -AdminToken $adminToken

# ---------------------------------------------------------------------------
# 9) Estudiantes de muestra y testimonios
# ---------------------------------------------------------------------------

Write-Host "9) Sembrando estudiantes de muestra ..." -ForegroundColor Cyan

$sofiaEmail = "sofia.demo@danceacademy.local"
$sofiaFullName = "Sofía Martínez"
$sofiaToken = Register-Persona -Email $sofiaEmail -FullName $sofiaFullName
Enroll-InCourse -Token $sofiaToken -CourseId $bachataCourse.CourseId
Complete-Lessons -Token $sofiaToken -LessonIds $bachataCourse.LessonIds
Submit-TestimonialIfMissing -Token $sofiaToken -Content "Tomé Bachata Sensual sin saber nada de baile y en pocas semanas ya me sentía cómoda en la pista. Las lecciones son cortas y claras, perfectas para repasar cuando quiera." -Rating 5 -CourseId $bachataCourse.CourseId -PersonaLabel "Sofía"

$julianaEmail = "juliana.demo@danceacademy.local"
$julianaFullName = "Juliana Restrepo"
$julianaToken = Register-Persona -Email $julianaEmail -FullName $julianaFullName
Submit-TestimonialIfMissing -Token $julianaToken -Content "Empecé con Salsa desde Cero sin haber bailado nunca y hoy ya salgo a bailar los fines de semana. Los instructores explican con mucha paciencia." -Rating 5 -CourseId $null -PersonaLabel "Juliana"

$mateoEmail = "mateo.demo@danceacademy.local"
$mateoFullName = "Mateo Osorio"
$mateoToken = Register-Persona -Email $mateoEmail -FullName $mateoFullName
Submit-TestimonialIfMissing -Token $mateoToken -Content "Me suscribí al plan mensual para tomar Ballet Clásico y Danza Contemporánea a la vez. Poder acceder a los dos cursos con una sola suscripción es justo lo que buscaba." -Rating 4 -CourseId $null -PersonaLabel "Mateo"

Write-Host "10) Aprobando testimonios como Admin ..." -ForegroundColor Cyan
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $sofiaFullName
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $julianaFullName
Publish-TestimonialsByStudentName -AdminToken $adminToken -StudentName $mateoFullName

Write-Host ""
Write-Host "Listo. Catálogo de portafolio sembrado:" -ForegroundColor Green
Write-Host "  - 3 niveles, 3 planes de suscripción, 4 instructores"
Write-Host "  - 6 cursos publicados (Salsa desde Cero, Ballet Clásico, Hip Hop Urbano, Tango Argentino, Bachata Sensual, Danza Contemporánea)"
Write-Host "  - 4 eventos publicados, 3 noticias publicadas, 5 preguntas frecuentes"
Write-Host "  - 3 estudiantes de muestra con testimonio publicado:"
Write-Host "      $sofiaFullName   <$sofiaEmail>   (contraseña: $PersonaPassword)"
Write-Host "      $julianaFullName <$julianaEmail> (contraseña: $PersonaPassword)"
Write-Host "      $mateoFullName   <$mateoEmail>   (contraseña: $PersonaPassword)"
