<#
.SYNOPSIS
    Crea (o promueve a) un usuario Admin en DanceAcademy.

.DESCRIPTION
    El hash de contrasena usa PBKDF2-HMACSHA256 (100k iteraciones) generado por
    DanceAcademy.Infrastructure.Security.PasswordHasher - no se puede replicar de forma
    segura con SQL plano. Por eso este script:
      1) Registra el usuario via POST /auth/register (Role=Student, hash correcto generado por la app).
      2) Promueve el Role a 'Admin' con un UPDATE directo dentro del contenedor de Postgres.

    Requiere: API corriendo (dotnet run) y el contenedor danceacademy-postgres levantado (docker-compose up -d).

.PARAMETER Email
    Email del usuario a crear/promover a Admin.

.PARAMETER Password
    Contrasena (minimo 8 caracteres). Solo se usa si el usuario todavia no existe.

.PARAMETER ApiBaseUrl
    URL base de la API. Por defecto http://localhost:5178 (puerto del perfil "http" en launchSettings.json).

.PARAMETER ContainerName
    Nombre del contenedor de Postgres. Por defecto danceacademy-postgres (docker-compose.yml).

.EXAMPLE
    ./scripts/create-admin.ps1 -Email "nuevo.admin@danceacademy.local" -Password "Admin12345!"
#>
param(
    [Parameter(Mandatory = $true)]
    [string]$Email,

    [Parameter(Mandatory = $true)]
    [string]$Password,

    [string]$ApiBaseUrl = "http://localhost:5178",

    [string]$ContainerName = "danceacademy-postgres",

    [string]$DatabaseName = "danceacademy"
)

$ErrorActionPreference = "Stop"

Write-Host "1) Registrando usuario en $ApiBaseUrl/auth/register ..." -ForegroundColor Cyan
try {
    $body = @{ email = $Email; password = $Password } | ConvertTo-Json
    Invoke-RestMethod -Method Post -Uri "$ApiBaseUrl/auth/register" -ContentType "application/json" -Body $body | Out-Null
    Write-Host "   Usuario creado como Student." -ForegroundColor Green
}
catch {
    $statusCode = $_.Exception.Response.StatusCode.value__
    if ($statusCode -eq 409) {
        Write-Host "   El usuario ya existe - se continua para promoverlo a Admin." -ForegroundColor Yellow
    }
    else {
        Write-Error "Fallo al registrar el usuario: $($_.Exception.Message)"
        exit 1
    }
}

Write-Host "2) Promoviendo el usuario a Admin en Postgres (contenedor $ContainerName) ..." -ForegroundColor Cyan

$escapedEmail = $Email.Replace("'", "''")
$sqlLines = @(
    "\set ON_ERROR_STOP on"
    "UPDATE ""Users"" SET ""Role"" = 'Admin' WHERE ""Email"" = '$escapedEmail';"
    "SELECT ""Id"", ""Email"", ""Role"" FROM ""Users"" WHERE ""Email"" = '$escapedEmail';"
)
$sql = $sqlLines -join "`n"

# Se manda por stdin (en vez de -c) porque pasar comillas dobles como argumento
# a un ejecutable nativo desde PowerShell en Windows las corrompe.
$sql | docker exec -i $ContainerName psql -U postgres -d $DatabaseName

if ($LASTEXITCODE -ne 0) {
    Write-Error "Fallo al promover el usuario a Admin (revisa el mensaje de psql arriba)."
    exit 1
}

Write-Host "Listo. El usuario ya es Admin." -ForegroundColor Green
