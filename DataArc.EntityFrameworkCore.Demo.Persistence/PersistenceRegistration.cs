using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace DataArc.EntityFrameworkCore.Demo.Persistence
{
    public static class PersistenceRegistration
    {
        private static readonly ILoggerFactory Factory =
            LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });

        public static IServiceCollection AddPersistence(
            this IServiceCollection services,
            ConfigurationManager configurationManager)
        {
            services.AddDbContextFactory<GoogleDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("GoogleDb"))
                    .UseLoggerFactory(Factory));

            services.AddDbContextFactory<MicrosoftDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("MicrosoftDb"))
                    .UseLoggerFactory(Factory));

            services.AddDbContextFactory<OpenAIDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("OpenAiDb"))
                    .UseLoggerFactory(Factory));

            services.AddDbContextFactory<SolidArcDbContext>(options =>
                options
                    .UseSqlServer(
                        configurationManager.GetConnectionString("SASDb"))
                    .UseLoggerFactory(Factory));

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