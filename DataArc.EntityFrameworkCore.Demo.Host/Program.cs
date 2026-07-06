using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using DataArc.EntityFrameworkCore.Demo.Persistence;
using DataArc.EntityFrameworkCore.Demo.Host.Workers;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules;
using DataArc.EntityFrameworkCore.Demo.Host;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var configurationManager = new ConfigurationManager();

        configurationManager
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services
            .AddModules(configurationManager)
            .AddBackgroundServices(configurationManager)
            .AddHostedService<DemoWorkflowWorker>();
    })
    .Build();

using var scope = host.Services.CreateScope();
await DemoDatabaseInitializer.InitializeAsync(scope.ServiceProvider);
await host.RunAsync();

Console.WriteLine();
Console.WriteLine("Press any key to exit.");
Console.ReadKey();