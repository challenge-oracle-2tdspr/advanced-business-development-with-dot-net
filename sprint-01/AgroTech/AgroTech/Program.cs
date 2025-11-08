using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository<Farm>, FarmRepository>();
builder.Services.AddSingleton<IRepository<Crop>, CropRepository>();
builder.Services.AddSingleton<IRepository<User>, UserRepository>();
builder.Services.AddSingleton<ISensorRepository, InMemorySensorRepository>();

builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        options.ViewLocationFormats.Clear();
        options.ViewLocationFormats.Add("/src/Web/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/src/Web/Views/Shared/{0}.cshtml");
    });


var app = builder.Build();

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