using Personalhandboksassistent.Configuration;
using Personalhandboksassistent.Endpoints;
using Personalhandboksassistent.Models;
using Personalhandboksassistent.Services;

// .NET läser miljövariabler automatiskt, men inte lokala .env-filer.
DotEnvLoader.LoadFromCurrentDirectory();

var builder = WebApplication.CreateBuilder(args);

// Tjänsterna gör arbetet åt API-rutterna och kan bytas ut eller testas separat.
builder.Services.AddSingleton<IDocumentStore, InMemoryDocumentStore>();
builder.Services.AddScoped<DocumentService>();
builder.Services.AddHttpClient<AzureOpenAiService>();

var app = builder.Build();

// Servera webbgränssnittet från wwwroot.
app.UseDefaultFiles();
app.UseStaticFiles();

// Varje grupp samlar API-rutter för ett eget område.
app.MapStatusEndpoints();
app.MapDocumentEndpoints();
app.MapChatEndpoints();

app.Run();

