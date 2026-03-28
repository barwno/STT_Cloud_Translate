using Microsoft.AspNetCore.Builder;
using System.Text.RegularExpressions;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// --- KONFIGURACJA CORS ---
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalFrontEnd", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

// £adownie plików frontu do rozwi¹zania
app.UseDefaultFiles();
app.UseStaticFiles();

app.UseCors("AllowLocalFrontEnd");

// --- ENDPOINT: T£UMACZENIE I GRAMATURA ---
/*app.MapPost("/api/translate", (TranslateRequest request) =>
{
    string translatedText = $"[T³umaczenie]: {request.Text}";
    return Results.Ok(new
    {
        original = request.Text,
        translated = translatedText
    });
});*/
app.MapPost("/api/translate", async (TranslateRequest request) =>
{
    try
    {
        // Dekodowanie Base64 do tablicy bajtów
        byte[] audioBytes = Convert.FromBase64String(request.AudioBase64);

        // Folder wewn¹trz katalogu g³ównego projektu
        string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "Recordings");

        if (!Directory.Exists(folderPath))
        {
            Directory.CreateDirectory(folderPath);
        }

        string fileName = $"rec_{DateTime.Now:yyyyMMdd_HHmmss}_{request.Person}.wav";
        string filePath = Path.Combine(folderPath, fileName);

        // Fizyczny zapis pliku na dysku lokalnym
        await File.WriteAllBytesAsync(filePath, audioBytes);

        Console.WriteLine($"[SERVER]: Zapisano nagranie: {fileName} ({audioBytes.Length / 1024} KB)");

        // Symulacja odpowiedzi z t³umaczeniem
        return Results.Ok(new
        {
            original = "Odebrano dŸwiêk na serwerze",
            translated = $"Plik zapisany jako: {fileName}"
        });
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[ERROR]: {ex.Message}");
        return Results.BadRequest(new { error = "B³¹d przetwarzania audio" });
    }
});

// --- ENDPOINT: LISTA JÊZYKÓW ---
app.MapGet("/api/languages", () =>
{
    return Results.Ok(new[] {
        new { code = "pl", name = "Polski", flag = "pl" },
        new { code = "en", name = "Angielski", flag = "gb" },
        new { code = "de", name = "Niemiecki", flag = "de" },
        new { code = "fr", name = "Francuski", flag = "fr" }
    });
});



app.Run();

// Modele danych (uproszczone rekordy C#)
//public record TranslateRequest(string Text, string TargetLang);
public record TranslateRequest(string AudioBase64, string TargetLang, string Person);