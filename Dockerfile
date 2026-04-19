FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Digestron.sln .
COPY Directory.Build.props .
COPY Directory.Packages.props .
COPY src/Digestron.Domain/Digestron.Domain.csproj src/Digestron.Domain/
COPY src/Digestron.Infra/Digestron.Infra.csproj src/Digestron.Infra/
COPY src/Digestron.Service/Digestron.Service.csproj src/Digestron.Service/
COPY src/Digestron.Hosting/Digestron.Hosting.csproj src/Digestron.Hosting/

RUN dotnet restore src/Digestron.Hosting/Digestron.Hosting.csproj

COPY src/ src/

RUN dotnet publish src/Digestron.Hosting/Digestron.Hosting.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "Digestron.Hosting.dll"]
