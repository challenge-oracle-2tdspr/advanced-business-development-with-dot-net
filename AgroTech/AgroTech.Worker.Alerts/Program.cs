using AgroTech.Worker.Alerts;
using AgroTech.Worker.Alerts.Configuration;
using AgroTech.Worker.Alerts.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqConsumerOptions>(
    builder.Configuration.GetSection(RabbitMqConsumerOptions.SectionName));

builder.Services.Configure<OracleDatabaseOptions>(
    builder.Configuration.GetSection(OracleDatabaseOptions.SectionName));

builder.Services.AddSingleton<IAlertEventRepository, OracleAlertEventRepository>();

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();