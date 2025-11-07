using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Registrando repositórios como Singleton para manter estado em lista
builder.Services.AddSingleton<IRepository<Farm>, FarmRepository>();
builder.Services.AddSingleton<IRepository<Crop>, CropRepository>();
builder.Services.AddSingleton<IRepository<User>, UserRepository>();
builder.Services.AddSingleton<ISensorRepository, InMemorySensorRepository>();

// Serviços com ciclo Scoped
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddControllersWithViews();

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