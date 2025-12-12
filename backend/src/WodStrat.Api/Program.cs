using System.Reflection;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using WodStrat.Api.Validators;
using WodStrat.Dal;
using WodStrat.Dal.Contexts;
using WodStrat.Dal.Interfaces;
using WodStrat.Services.Interfaces;
using WodStrat.Services.Services;

var builder = WebApplication.CreateBuilder(args);

// Database (PostgreSQL)
builder.Services.AddDbContext<WodStratDbContext>(options =>
    options.UseNpgsql(EnvironmentVariables.DbConnectionString));
builder.Services.AddScoped<IWodStratDatabase, WodStratDbContext>();

// Services
builder.Services.AddScoped<IAthleteService, AthleteService>();

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

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
