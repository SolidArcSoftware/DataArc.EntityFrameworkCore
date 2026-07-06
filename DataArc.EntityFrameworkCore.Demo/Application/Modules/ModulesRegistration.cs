using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DataArc.EntityFrameworkCore.Demo.Application.Modules.Google.Repository;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.Microsoft.Repository;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.OpenAi.Repository;
using DataArc.EntityFrameworkCore.Demo.Application.Modules.SAS.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Finance.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.IT.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Operations.Repository;

namespace DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules
{
    public static class FinanceRegistrationModule
    {
        public static IServiceCollection AddModules(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            // Register repositories
            services.AddScoped<IGoogleRepository, GoogleRepository>();
            services.AddScoped<IMicrosoftRepository, MicrosoftRepository>();
            services.AddScoped<IOpenAiRepository, OpenAiRepository>();
            services.AddScoped<ISolidArcRepository, SASRepository>();

            return services;
        }
    }
}