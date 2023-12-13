FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["FlorianAlbert.FinanceObserver.Server/FlorianAlbert.FinanceObserver.Server.csproj", "FlorianAlbert.FinanceObserver.Server/"]
RUN dotnet restore "FlorianAlbert.FinanceObserver.Server/FlorianAlbert.FinanceObserver.Server.csproj"
COPY . .
WORKDIR "/src/FlorianAlbert.FinanceObserver.Server"
RUN dotnet build "FlorianAlbert.FinanceObserver.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FlorianAlbert.FinanceObserver.Server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FlorianAlbert.FinanceObserver.Server.dll"]
