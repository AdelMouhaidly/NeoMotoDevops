FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["NeoMoto.Api/NeoMoto.Api.csproj", "NeoMoto.Api/"]
COPY ["NeoMoto.Domain/NeoMoto.Domain.csproj", "NeoMoto.Domain/"]
COPY ["NeoMoto.Infrastructure/NeoMoto.Infrastructure.csproj", "NeoMoto.Infrastructure/"]
RUN dotnet restore "NeoMoto.Api/NeoMoto.Api.csproj"

COPY . .
WORKDIR "/src/NeoMoto.Api"
RUN dotnet build "NeoMoto.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NeoMoto.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "NeoMoto.Api.dll"]

