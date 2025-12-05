using Microsoft.OpenApi.Models;
using PortSafe.Data;
using Microsoft.EntityFrameworkCore;
using PortSafe.Services;
using PortSafe.Services.AI;
using PortSafe.Services.AI.Agents;
using PortSafe.Services.AI.Interfaces;
using PortSafe.Services.AI.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

// Configurar connection string (prioriza DATABASE_URL do Render)
var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string não configurada. Defina DATABASE_URL ou ConnectionStrings:DefaultConnection.");
}

builder.Services.AddDbContext<PortSafeContext>(options => options.UseNpgsql(connectionString)); // Configura o DbContext com PostgreSQL

builder.Services.AddControllers();

builder.Services.AddScoped<AuthService>();

// Configurar JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(jwtKey) || jwtKey.Length < 16)
{
    throw new InvalidOperationException("Jwt:Key não configurada ou muito curta (mínimo 16 caracteres).");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };
});

// Configurar AI (Gemini)
builder.Services.Configure<AIConfiguration>(builder.Configuration.GetSection("AI"));
builder.Services.AddHttpClient<IGeminiService, GeminiService>();
builder.Services.AddScoped<IIntelligentValidationAgent, IntelligentValidationAgent>();

// Registrar GmailService com as credenciais (prioriza variáveis de ambiente)
builder.Services.AddScoped<GmailService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var email = Environment.GetEnvironmentVariable("GMAIL_EMAIL") 
        ?? config["Gmail:Email"];
    var appPassword = Environment.GetEnvironmentVariable("GMAIL_APP_PASSWORD") 
        ?? config["Gmail:AppPassword"];
    
    if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(appPassword))
    {
        throw new InvalidOperationException("Credenciais Gmail não configuradas. Defina GMAIL_EMAIL e GMAIL_APP_PASSWORD.");
    }
    
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
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");



// Configure the HTTP request pipeline.
// Swagger sempre disponível (útil para produção)
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PortSafe API v1");
    c.RoutePrefix = string.Empty; // Define a raiz do Swagger UI
});

//app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

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

app.MapControllers();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
