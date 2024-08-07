using DatabaseDomain;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<DatabaseDomainWorker>();

var host = builder.Build();
host.Run();
