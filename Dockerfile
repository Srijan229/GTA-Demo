FROM --platform=$BUILDPLATFORM node:24-alpine AS web-build
WORKDIR /src
COPY package.json package-lock.json ./
COPY apps/web/package.json apps/web/package.json
RUN npm ci
COPY apps/web apps/web
RUN npm run build:web

FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS api-build
WORKDIR /src
COPY global.json ./
COPY apps/api apps/api
RUN dotnet restore apps/api/Gta.Application.Api/Gta.Application.Api.csproj
RUN dotnet publish apps/api/Gta.Application.Api/Gta.Application.Api.csproj -c Release -o /app/publish --no-restore
COPY --from=web-build /src/apps/web/dist /app/publish/wwwroot
RUN mkdir -p /app/documents

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=api-build --chown=$APP_UID:$APP_UID /app/publish ./
COPY --from=api-build --chown=$APP_UID:$APP_UID /app/documents /var/lib/gta-application/documents
USER $APP_UID
ENV ASPNETCORE_URLS=http://0.0.0.0:5080
EXPOSE 5080
ENTRYPOINT ["dotnet", "Gta.Application.Api.dll"]
