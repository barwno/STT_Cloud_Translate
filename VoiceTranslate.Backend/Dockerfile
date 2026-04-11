# ETAP 1: Budowanie aplikacji (Build)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiujemy plik projektu i przywracamy zależności (Restore)
# Ścieżka VoiceTranslate.Backend/VoiceTranslate.Backend.csproj
COPY ["VoiceTranslate.Backend/VoiceTranslate.Backend.csproj", "VoiceTranslate.Backend/"]
RUN dotnet restore "VoiceTranslate.Backend/VoiceTranslate.Backend.csproj"

# Kopiujemy resztę plików i budujemy aplikację
COPY . .
WORKDIR "/src/VoiceTranslate.Backend"
RUN dotnet build "VoiceTranslate.Backend.csproj" -c Release -o /app/build

# ETAP 2: Publikacja (Publish)
FROM build AS publish
RUN dotnet publish "VoiceTranslate.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ETAP 3: Obraz końcowy (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Ekspozycja portu (domyślny dla ASP.NET Core to 8080 w .NET 8)
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "VoiceTranslate.Backend.dll"]