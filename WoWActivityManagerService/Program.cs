using System.Net;
using WoWActivityManagerService;

var host = Host.CreateDefaultBuilder(args)
    .UseWindowsService(options => {
        options.ServiceName = "WoW Activity Manager";
    })
    .ConfigureServices((hostContext, services) =>
    {
#if DEBUG
#else
        services.AddHostedService<Worker>();
#endif
    })
    .Build();

#if DEBUG
var worker1 = new Worker(IPAddress.Parse("1.1.1.1"), 8089,
    IPAddress.Parse("1.1.1.1"), 8089);
var worker2 = new Worker(IPAddress.Parse("1.1.1.1"), 8089,
    IPAddress.Parse("1.1.1.1"), 8090);

CancellationTokenSource cts = new();
await Task.WhenAll(worker1.Execute(cts.Token), worker2.Execute(cts.Token));

#else
    host.Run();
#endif

