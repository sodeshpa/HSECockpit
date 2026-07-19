FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["D4HSE.Api/D4HSE.Api.csproj", "D4HSE.Api/"]
COPY ["D4HSE.Core/D4HSE.Core.csproj", "D4HSE.Core/"]
COPY ["D4HSE.Infrastructure/D4HSE.Infrastructure.csproj", "D4HSE.Infrastructure/"]
COPY ["D4HSE.Services/D4HSE.Services.csproj", "D4HSE.Services/"]
RUN dotnet restore "D4HSE.Api/D4HSE.Api.csproj"
COPY . .
WORKDIR "/src/D4HSE.Api"
RUN dotnet build "D4HSE.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "D4HSE.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "D4HSE.Api.dll"]
