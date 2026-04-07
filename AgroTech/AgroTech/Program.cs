using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;
using AgroTech.Infrastructure.Repositories;
using AgroTech.Web.Middleware;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AgroTechDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("AgroTechOracle")));

builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<ISensorService, SensorService>();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/src/Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/src/Web/Views/Shared/{0}.cshtml");
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();