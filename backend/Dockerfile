FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY HostingQr.Backend.sln ./
COPY src/HostingQr.Api/HostingQr.Api.csproj src/HostingQr.Api/
COPY src/HostingQr.Application/HostingQr.Application.csproj src/HostingQr.Application/
COPY src/HostingQr.Domain/HostingQr.Domain.csproj src/HostingQr.Domain/
COPY src/HostingQr.Infrastructure/HostingQr.Infrastructure.csproj src/HostingQr.Infrastructure/
COPY tests/HostingQr.Api.Tests/HostingQr.Api.Tests.csproj tests/HostingQr.Api.Tests/

RUN dotnet restore HostingQr.Backend.sln

COPY . .
RUN dotnet publish src/HostingQr.Api -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "HostingQr.Api.dll"]
