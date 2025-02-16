// Configuration constants
var builder = DistributedApplication.CreateBuilder(args);

// Add MySQL container for WoW database
var database = builder.AddContainer("wow-vanilla-database", WowServerConfig.DbContainerImage)
    .WithEnvironment("MYSQL_APP_USER", WowServerConfig.DbUser)
    .WithEnvironment("MYSQL_APP_PASSWORD", WowServerConfig.DbPassword)
    .WithVolume(WowServerConfig.Volumes.MySqlData, WowServerConfig.Volumes.MySqlPath)
    .WithEndpoint("mysql", x =>
    {
        x.TargetPort = WowServerConfig.Ports.MySql;
    });

// Add WoW Vanilla server container
var wowServer = builder.AddContainer("wow-vanilla-server", WowServerConfig.ServerContainerImage)
    .WithEnvironment("MYSQL_APP_USER", WowServerConfig.DbUser)
    .WithEnvironment("MYSQL_APP_PASSWORD", WowServerConfig.DbPassword)
    .WithEnvironment("DATABASE_HOSTNAME", database.GetEndpoint("mysql"))
    .WithVolume(WowServerConfig.Volumes.LogData, WowServerConfig.Volumes.LogPath)
    .WithBindMount($"{WowServerConfig.Paths.ConfigDir}/mangosd.conf.tpl", $"{WowServerConfig.Paths.ServerConfigPath}/mangosd.conf.tpl")
    .WithBindMount($"{WowServerConfig.Paths.ConfigDir}/realmd.conf.tpl", $"{WowServerConfig.Paths.ServerConfigPath}/realmd.conf.tpl")
    .WithBindMount($"{WowServerConfig.Paths.DataDir}/dbc", $"{WowServerConfig.Paths.ServerDataPath}/dbc")
    .WithBindMount($"{WowServerConfig.Paths.DataDir}/maps", $"{WowServerConfig.Paths.ServerDataPath}/maps")
    .WithBindMount($"{WowServerConfig.Paths.DataDir}/mmaps", $"{WowServerConfig.Paths.ServerDataPath}/mmaps")
    .WithBindMount($"{WowServerConfig.Paths.DataDir}/vmaps", $"{WowServerConfig.Paths.ServerDataPath}/vmaps")
    .WithEndpoint("mangos-world", x =>
    {
        x.TargetPort = WowServerConfig.Ports.MangosWorld;
    })
    .WithEndpoint("mangos-realm", x =>
    {
        x.TargetPort = WowServerConfig.Ports.MangosRealm;
    })
    .WithReference(database.GetEndpoint("mysql"));


wowServer.WaitFor(database);

var app = builder.Build();
app.Run();