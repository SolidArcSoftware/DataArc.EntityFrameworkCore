using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Seeding;

using DataArc.Core;

namespace DataArc.EntityFrameworkCore.Demo.Persistence
{
    public static class PersistenceRegistration
    {
        static ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        public static IServiceCollection AddPersistence(this IServiceCollection services)
        {
            var configurationManager = new ConfigurationManager();
            configurationManager.AddJsonFile("appsettings.json", optional: false)
                          .Build();

            services
                 .AddDataArcCore()
                 .UseEntityFrameworkCoreProviders(provider =>
                 {
                     provider.ConfigureExecutionContexts(context =>
                     {
                         context.AddDbContext<IFinanceDbContext, FinanceDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("FinanceDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbContext<IHrDbContext, HrDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("HrDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbContext<IItDbContext, ItDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("ItDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbContext<IOperationsDbContext, OperationsDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("OperationsDb"))
                                .UseLoggerFactory(factory));
                     });
                 });

            services.TryAddScoped<IDatabaseCreator, DatabaseCreator>();
            services.TryAddScoped<IDatabaseSeeder, DatabaseSeeder>();

            return services;
        }
    }
}