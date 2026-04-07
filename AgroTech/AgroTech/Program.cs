using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Data;
using AgroTech.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AgroTechDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("AgroTechOracle")));

builder.Services.AddScoped<IRepository<User>, UserRepository>();
builder.Services.AddScoped<IRepository<Farm>, FarmRepository>();
builder.Services.AddScoped<IRepository<Crop>, CropRepository>();
builder.Services.AddScoped<ISensorRepository, SensorRepository>();

builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<ISensorService, SensorService>();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/src/Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/src/Web/Views/Shared/{0}.cshtml");
    });

var app = builder.Build();

app.UseMiddleware<AgroTech.Web.Middleware.ExceptionHandlingMiddleware>();

if (!app.Environment.IsDevelopment())
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