# Levanta Docker Desktop (si no esta corriendo) y la base de datos de DanceAcademy.
# Uso: doble clic, o desde una terminal: powershell -File scripts\start-db.ps1
#
# Nota: el contenedor "danceacademy-postgres" se busca por nombre fijo, no por
# "docker-compose up" directo — el proyecto vive en varios git worktrees y cada uno
# tiene un nombre de carpeta distinto, lo que hace que "docker-compose" (que deriva el
# nombre del proyecto de la carpeta actual) intente crear una red/volumen nuevos y
# choque con el contenedor ya existente (mismo nombre fijo, project distinto).
# Por eso solo se usa "docker-compose up -d" la primera vez, cuando el contenedor
# todavia no existe en ningun lado.

$ErrorActionPreference = "Stop"
$containerName = "danceacademy-postgres"

function Test-DockerReady {
    try {
        docker ps *> $null
        return $LASTEXITCODE -eq 0
    }
    catch {
        return $false
    }
}

if (-not (Test-DockerReady)) {
    Write-Host "Docker Desktop no esta corriendo. Iniciandolo..." -ForegroundColor Yellow

    $dockerExe = "$env:LOCALAPPDATA\Programs\DockerDesktop\Docker Desktop.exe"
    if (Test-Path $dockerExe) {
        Start-Process $dockerExe
    }
    else {
        Start-Process "shell:AppsFolder\Docker.DockerForWindows.Settings"
    }

    $maxWaitSeconds = 120
    $elapsed = 0
    while (-not (Test-DockerReady) -and $elapsed -lt $maxWaitSeconds) {
        Start-Sleep -Seconds 5
        $elapsed += 5
        Write-Host "Esperando a Docker Desktop... ($elapsed s)"
    }

    if (-not (Test-DockerReady)) {
        Write-Host "Docker Desktop no respondio a tiempo. Abrelo manualmente y vuelve a correr este script." -ForegroundColor Red
        exit 1
    }
}

$existing = docker ps -a --filter "name=^/$containerName$" --format "{{.Names}}\t{{.State}}"

if ($existing) {
    $state = ($existing -split "`t")[1]
    if ($state -eq "running") {
        Write-Host "La base de datos ya esta corriendo." -ForegroundColor Green
    }
    else {
        Write-Host "Contenedor '$containerName' existe pero esta detenido. Iniciandolo..." -ForegroundColor Yellow
        docker start $containerName | Out-Null
        Write-Host "Base de datos de DanceAcademy lista en el puerto 55432." -ForegroundColor Green
    }
}
else {
    Write-Host "Contenedor '$containerName' no existe todavia. Creandolo con docker-compose..." -ForegroundColor Yellow
    $repoRoot = Split-Path -Parent $PSScriptRoot
    Push-Location $repoRoot
    try {
        docker-compose -p danceacademy up -d
    }
    finally {
        Pop-Location
    }
    Write-Host "Base de datos de DanceAcademy lista en el puerto 55432." -ForegroundColor Green
}
