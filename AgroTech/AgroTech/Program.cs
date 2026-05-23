using AgroTech.Infrastructure.Mongo.Repositories;
using MongoDB.Driver;
using Microsoft.Extensions.Options;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using AgroTech.Configuration;
using AgroTech.Messaging;
using AgroTech.Web.HealthChecks;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Events;
using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;
using AgroTech.Infrastructure.Repositories;
using AgroTech.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

static Task WriteHealthResponse(HttpContext context, HealthReport report)
{
    context.Response.ContentType = "application/json";

    var response = new
    {
        status = report.Status.ToString(),
        totalDurationMs = report.TotalDuration.TotalMilliseconds,
        checks = report.Entries.Select(entry => new
        {
            name = entry.Key,
            status = entry.Value.Status.ToString(),
            description = entry.Value.Description ?? "OK",
            durationMs = entry.Value.Duration.TotalMilliseconds
        })
    };

    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });

    return context.Response.WriteAsync(json);
}
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] [CorrelationId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/agrotech-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 7,
        shared: true,
        outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] [CorrelationId: {CorrelationId}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

builder.Services.AddHttpClient();

builder.Services.AddDbContext<AgroTechDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("AgroTechOracle")));

builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISensorReadingMongoRepository, SensorReadingMongoRepository>();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/src/Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/src/Web/Views/Shared/{0}.cshtml");
    });

builder.Services.AddEndpointsApiExplorer();
//builder.Services.AddSwaggerGen();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "AgroTech API",
        Version = "v1",
        Description = "API RESTful para monitoramento de sensores do AgroTech"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "Informe o token JWT no formato: Bearer {seu_token}",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
    options.EnableAnnotations();
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICorrelationIdAccessor, HttpCorrelationIdAccessor>();


builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));

builder.Services.Configure<MongoDbOptions>(
    builder.Configuration.GetSection(MongoDbOptions.SectionName));

builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;

    if (string.IsNullOrWhiteSpace(options.ConnectionString))
        throw new InvalidOperationException("MongoDb:ConnectionString não configurada.");

    return new MongoClient(options.ConnectionString);
});

builder.Services.AddScoped(sp =>
{
    var options = sp.GetRequiredService<IOptions<MongoDbOptions>>().Value;
    var client = sp.GetRequiredService<IMongoClient>();

    if (string.IsNullOrWhiteSpace(options.DatabaseName))
        throw new InvalidOperationException("MongoDb:DatabaseName não configurado.");

    return client.GetDatabase(options.DatabaseName);
});

var jwtSection = builder.Configuration.GetSection(JwtOptions.SectionName);
var jwtOptions = jwtSection.Get<JwtOptions>()
                 ?? throw new InvalidOperationException("Configuração JWT não encontrada.");

if (string.IsNullOrWhiteSpace(jwtOptions.Key))
    throw new InvalidOperationException("A chave JWT não foi configurada.");

var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key));

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtOptions.Issuer,

            ValidateAudience = true,
            ValidAudience = jwtOptions.Audience,

            ValidateIssuerSigningKey = true,
            IssuerSigningKey = signingKey,

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.AddSingleton<IEventPublisher, RabbitMqEventPublisher>();

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AgroTechDbContext>(
        "oracle",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "db", "oracle" })
    .AddCheck(
        "self",
        () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("API está saudável"),
        tags: new[] { "api" })
    .AddCheck<ExternalServiceHealthCheck>(
        "external_service",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "external", "http" });

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(
        serviceName: "AgroTech.Api",
        serviceVersion: "1.0.0"))
    .WithTracing(tracing => tracing
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddConsoleExporter())
    .WithMetrics(metrics => metrics
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddConsoleExporter());

var app = builder.Build();

app.UseCorrelationId();

app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RequestPath", httpContext.Request.Path.Value ?? string.Empty);
        diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
    };
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "AgroTech API v1");
        c.DocumentTitle = "AgroTech API";

        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.DefaultModelsExpandDepth(-1);

        c.ConfigObject.AdditionalItems["syntaxHighlight"] = new Dictionary<string, object>
        {
            ["activated"] = false
        };
    });

}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Name == "self",
    ResponseWriter = WriteHealthResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = WriteHealthResponse
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("\u001b[34m====================================\u001b[0m");
Console.WriteLine("\u001b[32mAPI AGROTECH INICIADA COM SUCESSO\u001b[0m");
Log.Information("   Ambiente: {Environment}", app.Environment.EnvironmentName);
Console.WriteLine("\u001b[33m   Swagger: /swagger\u001b[0m");
Console.WriteLine("\u001b[36m   Health: /health\u001b[0m");
Log.Information("   Endpoints principais:");
Log.Information("-> GET    /api/sensors");
Log.Information("-> GET    /api/sensors/search");
Log.Information("-> POST   /api/sensors");
Log.Information("-> GET    /health");
Console.WriteLine("\u001b[34m====================================\u001b[0m");
Console.WriteLine("\u001b[32mDESENVOLVIDO POR RUAN GASPAR\u001b[0m");
app.Run();
