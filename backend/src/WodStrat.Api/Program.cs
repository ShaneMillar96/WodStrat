using System.Reflection;
using System.Text;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using WodStrat.Api.Validators;
using WodStrat.Dal;
using WodStrat.Dal.Contexts;
using WodStrat.Dal.Enums;
using WodStrat.Dal.Interfaces;
using WodStrat.Services.Configuration;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Database (PostgreSQL) with enum type mappings
var dataSourceBuilder = new NpgsqlDataSourceBuilder(EnvironmentVariables.DbConnectionString);
dataSourceBuilder.MapEnum<ExperienceLevel>("experience_level");
dataSourceBuilder.MapEnum<AthleteGoal>("athlete_goal");
dataSourceBuilder.MapEnum<BenchmarkCategory>("benchmark_category");
dataSourceBuilder.MapEnum<BenchmarkMetricType>("benchmark_metric_type");
dataSourceBuilder.MapEnum<WorkoutType>("workout_type");
dataSourceBuilder.MapEnum<MovementCategory>("movement_category");
dataSourceBuilder.MapEnum<LoadUnit>("load_unit");
dataSourceBuilder.MapEnum<DistanceUnit>("distance_unit");
dataSourceBuilder.MapEnum<RepSchemeType>("rep_scheme_type");
dataSourceBuilder.MapEnum<PacingLevel>("pacing_level");
var dataSource = dataSourceBuilder.Build();

builder.Services.AddDbContext<WodStratDbContext>(options =>
    options.UseNpgsql(dataSource));
builder.Services.AddScoped<IWodStratDatabase, WodStratDbContext>();

// JWT Configuration
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection(JwtSettings.SectionName));

var jwtSettings = builder.Configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
    ?? throw new InvalidOperationException("JWT settings are not configured.");

var secretKey = jwtSettings.SecretKey;
if (string.IsNullOrWhiteSpace(secretKey) || secretKey.Length < 32)
{
    throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long.");
}

// JWT Authentication
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
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// HTTP Context Accessor (for CurrentUserService)
builder.Services.AddHttpContextAccessor();

// Services
builder.Services.AddScoped<IAthleteService, AthleteService>();
builder.Services.AddScoped<IBenchmarkService, BenchmarkService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IMovementDefinitionService, MovementDefinitionService>();
builder.Services.AddScoped<IPatternMatchingService, PatternMatchingService>();
builder.Services.AddScoped<IWorkoutParsingService, WorkoutParsingService>();
builder.Services.AddScoped<IWorkoutService, WorkoutService>();
builder.Services.AddScoped<IPacingService, PacingService>();
builder.Services.AddScoped<IVolumeLoadService, VolumeLoadService>();
builder.Services.AddScoped<ITimeEstimateService, TimeEstimateService>();
builder.Services.AddScoped<IStrategyInsightsService, StrategyInsightsService>();

// FluentValidation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateAthleteRequestValidator>();

// Controllers
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "WodStrat API",
        Version = "v1",
        Description = "API for WodStrat functional fitness training application"
    });

    // JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
        Name = "Authorization",
        In = ParameterLocation.Header,
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

    // Include XML comments for documentation
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
});

var app = builder.Build();

// Configure pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
