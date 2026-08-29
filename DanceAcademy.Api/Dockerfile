FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DanceAcademy.Domain/DanceAcademy.Domain.csproj DanceAcademy.Domain/
COPY DanceAcademy.Application/DanceAcademy.Application.csproj DanceAcademy.Application/
COPY DanceAcademy.Infrastructure/DanceAcademy.Infrastructure.csproj DanceAcademy.Infrastructure/
COPY DanceAcademy.Api/DanceAcademy.Api.csproj DanceAcademy.Api/
RUN dotnet restore DanceAcademy.Api/DanceAcademy.Api.csproj

COPY DanceAcademy.Domain/ DanceAcademy.Domain/
COPY DanceAcademy.Application/ DanceAcademy.Application/
COPY DanceAcademy.Infrastructure/ DanceAcademy.Infrastructure/
COPY DanceAcademy.Api/ DanceAcademy.Api/

RUN dotnet publish DanceAcademy.Api/DanceAcademy.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render (y la mayoría de plataformas Docker) inyectan PORT en tiempo de ejecución —
# no se puede fijar en el Dockerfile porque no existe todavía en build time.
ENTRYPOINT ["sh", "-c", "ASPNETCORE_URLS=http://+:${PORT:-8080} dotnet DanceAcademy.Api.dll"]
