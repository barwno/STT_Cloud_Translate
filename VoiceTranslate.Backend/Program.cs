using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodaj Kontrolery
builder.Services.AddControllers();

// 2. Skonfiguruj CORS (zgodnie z naszą rozmową)
var frontendUrl = builder.Configuration["FRONTEND_URL"] ?? "http://127.0.0.1:5500";
builder.Services.AddCors(options => {
    options.AddPolicy("FrontendPolicy", p => 
        p.WithOrigins(frontendUrl, "http://localhost:5500")
         .AllowAnyHeader()
         .AllowAnyMethod()
         .AllowCredentials());
});

// 3. Zarejestruj Serwis (DI)
builder.Services.AddSingleton<IFirestoreService>(sp => 
    new FirestoreService("id-twojego-projektu", "firebase-adminsdk.json"));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();  

app.UseCors("FrontendPolicy");
app.MapControllers(); // To sprawia, że TranslationController zaczyna działać

app.Run();