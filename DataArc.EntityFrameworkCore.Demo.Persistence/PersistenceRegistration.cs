using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence.Contracts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders;

namespace DataArc.EntityFrameworkCore.Demo.Persistence
{
    public static class PersistenceRegistration
    {
        static ILoggerFactory factory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
        });

        public static IServiceCollection AddPersistence(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            services
                 .AddDataArcCore()
                 .ConfigureDataArc(provider =>
                 {
                     provider.UseEntityFrameworkCore(context =>
                     {
                         context.AddDbExecutionContext<IGoogleDbContext, GoogleDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("GoogleDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<IMicrosoftDbContext, MicrosoftDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("MicrosoftDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<IOpenAIDbContext, OpenAIDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("OpenAiDb"))
                                .UseLoggerFactory(factory));

                         context.AddDbExecutionContext<ISolidArcDbContext, SolidArcDbContext>(options => options
                                .UseSqlServer(configurationManager.GetConnectionString("SASDb"))
                                .UseLoggerFactory(factory));
                     });
                 });

            services.TryAddScoped<IGoogleDbCreator, GoogleDbCreator>();
            services.TryAddScoped<IMicrosoftDbCreator, MicrosoftDbCreator>();
            services.TryAddScoped<IOpenAiDbCreator, OpenAiDbCreator>();
            services.TryAddScoped<ISASDbCreator, SASDbCreator>();

            services.TryAddScoped<IMicrosoftDbSeeder, MicrosoftDbSeeder>();
            services.TryAddScoped<IOpenAIDbSeeder, OpenAIDbSeeder>();
            services.TryAddScoped<ISolidArcDbSeeder, SolidArcDbSeeder>();
            services.TryAddScoped<IGoogleDBSeeder, GoogleDBSeeder>();

            return services;
        }
    }
}