using System.Text;
using AgriVision.Application.Interfaces;
using AgriVision.Domain.Entities;
using AgriVision.Domain.Enums;
using AgriVision.Infrastructure.Data;
using AgriVision.Infrastructure.Identity;
using AgriVision.Infrastructure.Realtime;
using AgriVision.Infrastructure.Services;
using AgriVision.Infrastructure.Integrations.Gradio;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);

// =======================
// CORS
// =======================
builder.Services.AddCors(options =>
{
    static string? NormalizeOrigin(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (!Uri.TryCreate(value.Trim(), UriKind.Absolute, out var uri)) return null;
        return uri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
    }

    var configuredOrigins = builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>() ?? Array.Empty<string>();

    var allowedOrigins = configuredOrigins
        .Select(NormalizeOrigin)
        .Where(x => !string.IsNullOrWhiteSpace(x))
        .Cast<string>()
        .Distinct(StringComparer.OrdinalIgnoreCase)
        .ToArray();

    if (allowedOrigins.Length == 0)
    {
        allowedOrigins = new[]
        {
            "http://127.0.0.1:5500",
            "http://localhost:5500"
        };
    }

    options.AddPolicy("CorsPolicy", policy =>
    {
        policy
            .WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// =======================
// Controllers
// =======================
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters
            .Add(new JsonStringEnumConverter());
    });

// =======================
// Swagger (UI on /swagger)
// =======================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgriVision API",
        Version = "v1"
    });

    // JWT in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter JWT token like: Bearer {your_token}"
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

// =======================
// Email Sender
// =======================
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();

// =======================
// DbContext
// =======================
builder.Services.AddDbContext<AgriVisionDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =======================
// Identity
// =======================
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AgriVisionDbContext>()
    .AddDefaultTokenProviders();

// =======================
// Authentication (JWT + Google)
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Bearer";
    options.DefaultChallengeScheme = "Bearer";
})
.AddJwtBearer("Bearer", options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
    };
})
.AddGoogle("Google", options =>
{
    options.ClientId = builder.Configuration["Authentication:Google:ClientId"]!;
    options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"]!;
    options.SignInScheme = IdentityConstants.ExternalScheme;
});

// =======================
// DI Services
// =======================
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IFarmService, FarmService>();

// HF Crop Recommendation Service
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAiCropRecommendationService, HuggingFaceCropRecommendationService>();
builder.Services.AddScoped<ICropRecommendationHistoryService, CropRecommendationHistoryService>();
builder.Services.AddHttpClient<IFertilizerRecommendationService, HuggingFaceFertilizerRecommendationService>();
builder.Services.AddHttpClient<IIrrigationRecommendationService, IrrigationRecommendationService>();
builder.Services.AddScoped<IFertilizerRecommendationHistoryService, FertilizerRecommendationHistoryService>();
builder.Services.AddScoped<IWeatherService, WeatherService>();
builder.Services.AddScoped<IGeocodingService, GeocodingService>();
builder.Services.AddSignalR();
builder.Services.AddSingleton<IUserIdProvider, SignalRUserIdProvider>();
builder.Services.AddScoped<GradioProvider>();
builder.Services.AddScoped<IPlantDetectionService, PlantDetectionService>();
builder.Services.AddScoped<IInsectDetectionService, InsectDetectionService>();
builder.Services.AddScoped<IRealtimeNotifier, SignalRNotifier>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddHostedService<MqttBackgroundService>();
builder.Services.AddSingleton<IMqttMessageHandler, MqttMessageHandler>();
//var port = Environment.GetEnvironmentVariable("PORT") ?? "5000";
//builder.WebHost.UseUrls($"http://*:{port}");



var app = builder.Build();

// =======================
// Apply migrations
// =======================
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AgriVisionDbContext>();
    db.Database.Migrate();

    var planDefinitions = new[]
    {
        new { Plan = SubscriptionPlan.Free, Name = "Free", Price = 0m },
        new { Plan = SubscriptionPlan.Basic, Name = "Basic", Price = 3.99m },
        new { Plan = SubscriptionPlan.Advanced, Name = "Advanced", Price = 9.99m }
    };

    foreach (var def in planDefinitions)
    {
        var existing = db.SubscriptionPlanLookups.FirstOrDefault(x => x.Plan == def.Plan);
        if (existing == null)
        {
            db.SubscriptionPlanLookups.Add(new SubscriptionPlanLookup
            {
                Plan = def.Plan,
                Name = def.Name,
                Price = def.Price
            });
            continue;
        }

        existing.Name = def.Name;
        existing.Price = def.Price;
    }

    db.SaveChanges();
}

// =======================
// Middleware pipeline
// =======================

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgriVision API v1");
    c.RoutePrefix = "swagger"; // /swagger
});

//if (!app.Environment.IsProduction())
//{
//    app.UseHttpsRedirection();
//}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHub<NotificationHub>("/hubs/notifications");


app.Run();
