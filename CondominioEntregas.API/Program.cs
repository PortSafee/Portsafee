using Microsoft.OpenApi.Models;
using PortSafe.Data;
using Microsoft.EntityFrameworkCore;
using PortSafe.Services;
using PortSafe.Services.AI;
using PortSafe.Services.AI.Agents;
using PortSafe.Services.AI.Interfaces;
using PortSafe.Services.AI.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configurar connection string (prioriza DATABASE_URL do Render)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<PortSafeContext>(options => options.UseNpgsql(connectionString)); // Configura o DbContext com PostgreSQL

builder.Services.AddControllers();

builder.Services.AddScoped<AuthService>();

// Configurar AI (Gemini)
builder.Services.Configure<AIConfiguration>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IIntelligentValidationAgent, IntelligentValidationAgent>();

// Registrar GmailService com as credenciais (prioriza variáveis de ambiente)
builder.Services.AddScoped<GmailService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var email = Environment.GetEnvironmentVariable("GMAIL_EMAIL") 
        ?? config["Gmail:Email"] 
        ?? "";
    var appPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD") 
        ?? config["Gmail:AppPassword"] 
        ?? "";
    return new GmailService(email, appPassword);
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PortSafe", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Insira o token JWT no formato: Bearer {token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Aplicar migrations automaticamente ao iniciar
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PortSafeContext>();
    db.Database.Migrate();
}

app.UseCors("AllowFrontend");

// Configure the HTTP request pipeline.
// Swagger sempre disponível (útil para produção)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PortSafe API v1");
    c.RoutePrefix = string.Empty; // Define a raiz do Swagger UI
});

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
{
    var forecast = Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.UseAuthorization();
app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
