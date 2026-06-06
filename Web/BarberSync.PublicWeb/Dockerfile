ARG DOTNET_VERSION=8.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY . .
RUN dotnet restore Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj
RUN dotnet publish Web/BarberSync.PublicWeb/BarberSync.PublicWeb.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "BarberSync.PublicWeb.dll"]
