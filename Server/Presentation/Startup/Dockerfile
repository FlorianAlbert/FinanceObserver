FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY .\..\..\.. .
RUN dotnet restore "FlorianAlbert.FinanceObserver.sln" && \
    dotnet build "FlorianAlbert.FinanceObserver.sln" -c Release --no-restore

FROM build AS publish
RUN dotnet publish "Server/Presentation/Startup/Startup.csproj" -c Release --no-restore --no-build -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Startup.dll"]
