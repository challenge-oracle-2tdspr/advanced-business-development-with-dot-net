using AgroTech.Worker.Recommendations;
using AgroTech.Worker.Recommendations.Configuration;
using AgroTech.Worker.Recommendations.Repositories;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.Configure<RabbitMqConsumerOptions>(
    builder.Configuration.GetSection(RabbitMqConsumerOptions.SectionName));

builder.Services.Configure<OracleDatabaseOptions>(
    builder.Configuration.GetSection(OracleDatabaseOptions.SectionName));

builder.Services.AddSingleton<IRecommendationEventRepository, OracleRecommendationEventRepository>();
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();