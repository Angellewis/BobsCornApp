FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["BobsCornApp-Back/BobsCornApp.Api/BobsCornApp.Api.csproj", "BobsCornApp-Back/BobsCornApp.Api/"]
COPY ["BobsCornApp-Back/BobsCornApp.Application/BobsCornApp.Application.csproj", "BobsCornApp-Back/BobsCornApp.Application/"]
COPY ["BobsCornApp-Back/BobsCornApp.Infrastructure/BobsCornApp.Infrastructure.csproj", "BobsCornApp-Back/BobsCornApp.Infrastructure/"]
COPY ["BobsCornApp-Back/BobsCornApp.Domain/BobsCornApp.Domain.csproj", "BobsCornApp-Back/BobsCornApp.Domain/"]

RUN dotnet restore "BobsCornApp-Back/BobsCornApp.Api/BobsCornApp.Api.csproj"

COPY BobsCornApp-Back/ ./BobsCornApp-Back/

RUN dotnet publish "BobsCornApp-Back/BobsCornApp.Api/BobsCornApp.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "BobsCornApp.Api.dll"]
