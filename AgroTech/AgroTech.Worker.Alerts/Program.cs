using AgroTech.Worker.Alerts;
using AgroTech.Worker.Alerts.Configuration;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqConsumerOptions>(
    builder.Configuration.GetSection(RabbitMqConsumerOptions.SectionName));

builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();