using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.Translation.V2;
using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Services;

// Przygotowanie głównego silnika aplikacji i rejestracja potrzebnych narzędzi systemowych.
var builder = WebApplication.CreateBuilder(args);

// Konfiguracja obsługi zapytań (kontrolerów) oraz automatycznego generatora dokumentacji API (Swagger).
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); 
builder.Services.AddSwaggerGen();           

// Weryfikacja dostępu do usług Google Cloud w celu automatycznego wyboru trybu pracy aplikacji.
bool isCloudReady = false;
try {
    GoogleCredential.GetApplicationDefault();
    isCloudReady = true;
} catch { isCloudReady = false; }

// W zależności od dostępności chmury, uruchamiamy albo silnik z prawdziwymi usługami Google, albo tryb symulacji.
if (isCloudReady)
{
    string projectId = "ivory-period-491009-b1";
    builder.Services.AddSingleton(SpeechClient.Create());
    builder.Services.AddSingleton(TranslationClient.Create());
    var firestoreDb = new FirestoreDbBuilder
    {
        ProjectId = projectId,
        DatabaseId = "voicebridge"
    }.Build();

    builder.Services.AddSingleton(firestoreDb);
    builder.Services.AddScoped<ITranscriptionService, GoogleTranscriptionService>();
    Console.WriteLine(">>> SYSTEM: Tryb Google Cloud");
}
else
{
    builder.Services.AddScoped<ITranscriptionService, MockTranscriptionService>();
    Console.WriteLine(">>> SYSTEM: Tryb Mockup");
}

// Finalizacja budowy aplikacji i uruchomienie serwera obsługującego żądania.
var app = builder.Build();

// Udostępnienie interfejsu technicznego do testowania API oraz serwowanie plików strony internetowej użytkownikowi.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "VoiceTranslate API v1");
});

app.UseDefaultFiles();
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();
app.MapFallbackToFile("frontend.html");

app.Run();