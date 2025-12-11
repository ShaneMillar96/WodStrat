using WodStrat.Dal;
using WodStrat.Dal.Contexts;
using WodStrat.Dal.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Database (PostgreSQL)
builder.Services.AddDbContext<WodStratDbContext>(options =>
    options.UseNpgsql(EnvironmentVariables.DbConnectionString));
builder.Services.AddScoped<IWodStratDatabase, WodStratDbContext>();

// Services (register as needed)
// builder.Services.AddScoped<IExampleService, ExampleService>();

// Controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
