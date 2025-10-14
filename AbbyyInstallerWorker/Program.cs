using AbbyyInstallerWorker;

var builder = Host.CreateDefaultBuilder(args);


builder.ConfigureServices((hostBuilderContext, services) => { services.AddHostedService<Worker>(); });
builder.UseWindowsService();


var host = builder.Build();
host.Run();