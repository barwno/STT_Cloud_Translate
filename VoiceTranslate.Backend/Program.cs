using Google.Apis.Auth.OAuth2;
using Google.Cloud.Speech.V1;
using Google.Cloud.Translation.V2;
using Google.Cloud.Firestore;
using VoiceTranslate.Backend.Interfaces;
using VoiceTranslate.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// 1. Dodaj usługi kontrolerów i Swaggera
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer(); // Potrzebne dla Swaggera
builder.Services.AddSwaggerGen();           // Generator Swaggera

// Logika sprawdzania Google Cloud (bez zmian)
bool isCloudReady = false;
try {
    GoogleCredential.GetApplicationDefault();
    isCloudReady = true;
} catch { isCloudReady = false; }

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

var app = builder.Build();

// 2. Włącz interfejs Swaggera 
// Możesz to zostawić włączone zawsze lub tylko w Development: if (app.Environment.IsDevelopment())
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