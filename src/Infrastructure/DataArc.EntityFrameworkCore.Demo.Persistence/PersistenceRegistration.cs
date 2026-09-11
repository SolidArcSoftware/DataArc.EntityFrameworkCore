using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
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
            services.AddDbContextFactory<GoogleDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("GoogleDb"))
                    .UseLoggerFactory(factory));

            services.AddDbContextFactory<MicrosoftDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("MicrosoftDb"))
                    .UseLoggerFactory(factory));

            services.AddDbContextFactory<OpenAIDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("OpenAiDb"))
                    .UseLoggerFactory(factory));

            services.AddDbContextFactory<SolidArcDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("SASDb"))
                    .UseLoggerFactory(factory));

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