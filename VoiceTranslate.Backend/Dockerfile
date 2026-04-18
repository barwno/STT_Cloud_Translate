# Etap 1: Przygotowanie "warsztatu" i pobranie wszystkich niezbędnych narzędzi do zbudowania aplikacji.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Pobranie informacji o wymaganych bibliotekach i przygotowanie projektu do pracy.
COPY ["VoiceTranslate.Backend.csproj", "./"]
RUN dotnet restore "VoiceTranslate.Backend.csproj"

# Przeniesienie wszystkich plików źródłowych Twojego projektu do wirtualnego warsztatu.
COPY . .
RUN dotnet build "VoiceTranslate.Backend.csproj" -c Release -o /app/build

# Etap 2: Stworzenie finalnej wersji aplikacji gotowej do uruchomienia na serwerze.
FROM build AS publish
RUN dotnet publish "VoiceTranslate.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etap 3: Uruchomienie aplikacji w lekkim i bezpiecznym środowisku produkcyjnym.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ustawienie portu komunikacyjnego i polecenie startowe dla systemu, aby aplikacja zaczęła działać.
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "VoiceTranslate.Backend.dll"]