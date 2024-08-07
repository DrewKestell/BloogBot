using PathfindingService;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddHostedService<PathfindingServiceWorker>();

var host = builder.Build();
host.Run();
