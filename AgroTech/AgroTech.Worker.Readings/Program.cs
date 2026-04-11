using AgroTech.Worker.Readings;
using AgroTech.Worker.Readings.Configuration;
using AgroTech.Worker.Readings.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqConsumerOptions>(
    builder.Configuration.GetSection(RabbitMqConsumerOptions.SectionName));

builder.Services.Configure<OracleDatabaseOptions>(
    builder.Configuration.GetSection(OracleDatabaseOptions.SectionName));

builder.Services.AddSingleton<ISensorReadingEventRepository, OracleSensorReadingEventRepository>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
