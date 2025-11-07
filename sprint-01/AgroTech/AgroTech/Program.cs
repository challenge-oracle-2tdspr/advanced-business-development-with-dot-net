using AgroTech.Application.Interfaces;
using AgroTech.Application.Services;
using AgroTech.Domain.Entities;
using AgroTech.Domain.Interfaces;
using AgroTech.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<IRepository<Farm>, FarmRepository>();
builder.Services.AddScoped<IRepository<Crop>, CropRepository>();
builder.Services.AddScoped<IRepository<User>, UserRepository>();

builder.Services.AddScoped<ISensorRepository, InMemorySensorRepository>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<ISensorRepository, InMemorySensorRepository>();
builder.Services.AddScoped<ISensorRepository, SensorRepository>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ISensorService, SensorService>();
builder.Services.AddScoped<IFarmService, FarmService>();
builder.Services.AddScoped<ICropService, CropService>();
builder.Services.AddScoped<IUserService, UserService>();



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