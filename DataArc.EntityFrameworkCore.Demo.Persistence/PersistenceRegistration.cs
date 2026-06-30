using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

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
                 .ConfigureDataArc(provider =>
                 {
                     provider.UseEntityFrameworkCore(context =>
                     {
                         context.AddDbExecutionContext<IFinanceDbContext, FinanceDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("FinanceDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<IHrDbContext, HrDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("HrDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<IItDbContext, ItDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("ItDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<IOperationsDbContext, OperationsDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("OperationsDb"))
                                .UseLoggerFactory(factory));
                     });
                 });

            services.TryAddScoped<IFinanceDbCreator, FinanceDbCreator>();
            services.TryAddScoped<IHrDbCreator, HrDbCreator>();
            services.TryAddScoped<IOperationsDbCreator,OperationsDbCreator>();
            services.TryAddScoped<IItDbCreator, ItDbCreator>();

            services.TryAddScoped<IHrDbSeeder, HrDbSeeder>();

            return services;
        }
    }
}