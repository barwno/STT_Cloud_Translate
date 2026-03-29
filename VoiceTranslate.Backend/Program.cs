using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Dodanie kontrolerów
builder.Services.AddControllers();

// Konfiguracja CORS
var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://127.0.0.1:5500";
builder.Services.AddCors(options => {
    options.AddPolicy("FrontendPolicy", p => 
        p.WithOrigins(frontendUrl, "http://localhost:5500")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// Rejestracja serwisów
builder.Services.AddSingleton<IFirestoreService>(sp => 
    new FirestoreService("id-projektu", "firebase-adminsdk.json"));

builder.Services.AddSingleton<ILanguageService, LanguageService>();
builder.Services.AddSingleton<ISpeechToTextService, MockSpeechService>(); 
builder.Services.AddSingleton<ITranslationService, MockTranslationService>(); 

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();  

app.UseCors("FrontendPolicy");
app.MapControllers(); // To sprawia, że TranslationController zaczyna działać

app.Run();