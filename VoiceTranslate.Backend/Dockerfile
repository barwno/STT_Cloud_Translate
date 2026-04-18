# Etap 1: Budowanie
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Kopiujemy plik csproj z bieżącego katalogu
COPY ["VoiceTranslate.Backend.csproj", "./"]
RUN dotnet restore "VoiceTranslate.Backend.csproj"

# Kopiujemy resztę plików
COPY . .
RUN dotnet build "VoiceTranslate.Backend.csproj" -c Release -o /app/build

# Etap 2: Publikacja
FROM build AS publish
RUN dotnet publish "VoiceTranslate.Backend.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etap 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "VoiceTranslate.Backend.dll"]