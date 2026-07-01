using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration;
using DataArc.EntityFrameworkCore.Demo.Application.Workers;
using DataArc.EntityFrameworkCore.Demo.Persistence;

var host = Host
    .CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        var configurationManager = new ConfigurationManager();

        configurationManager
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        services
            .AddFinanceModule(configurationManager)
            .AddHostedService<DemoWorkflowWorker>();
    })
    .Build();

using var scope = host.Services.CreateScope();
await DemoDatabaseInitializer.InitializeAsync(scope.ServiceProvider);
await host.RunAsync();

Console.WriteLine();
Console.WriteLine("Press any key to exit.");
Console.ReadKey();