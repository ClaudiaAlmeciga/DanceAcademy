FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DanceAcademy.Domain/DanceAcademy.Domain.csproj DanceAcademy.Domain/
COPY DanceAcademy.Application/DanceAcademy.Application.csproj DanceAcademy.Application/
COPY DanceAcademy.Public/DanceAcademy.Public.csproj DanceAcademy.Public/
RUN dotnet restore DanceAcademy.Public/DanceAcademy.Public.csproj

COPY DanceAcademy.Domain/ DanceAcademy.Domain/
COPY DanceAcademy.Application/ DanceAcademy.Application/
COPY DanceAcademy.Public/ DanceAcademy.Public/

RUN dotnet publish DanceAcademy.Public/DanceAcademy.Public.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY DanceAcademy.Public/nginx.conf.template /etc/nginx/templates/default.conf.template
