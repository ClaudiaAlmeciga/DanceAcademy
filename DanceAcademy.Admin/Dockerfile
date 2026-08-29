FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY DanceAcademy.Domain/DanceAcademy.Domain.csproj DanceAcademy.Domain/
COPY DanceAcademy.Application/DanceAcademy.Application.csproj DanceAcademy.Application/
COPY DanceAcademy.Admin/DanceAcademy.Admin.csproj DanceAcademy.Admin/
RUN dotnet restore DanceAcademy.Admin/DanceAcademy.Admin.csproj

COPY DanceAcademy.Domain/ DanceAcademy.Domain/
COPY DanceAcademy.Application/ DanceAcademy.Application/
COPY DanceAcademy.Admin/ DanceAcademy.Admin/

RUN dotnet publish DanceAcademy.Admin/DanceAcademy.Admin.csproj -c Release -o /app/publish --no-restore

FROM nginx:alpine AS final
COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY DanceAcademy.Admin/nginx.conf.template /etc/nginx/templates/default.conf.template
