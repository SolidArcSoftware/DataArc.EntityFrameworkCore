using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Services;
using DataArc.EntityFrameworkCore.Demo.Persistence;
using Microsoft.Extensions.DependencyInjection;

namespace DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Registration
{
    public static class FinanceRegistrationModule
    {
        public static IServiceCollection AddFinanceModule(this IServiceCollection services)
        {
            // Register persistence layer for Finance module
            services.AddPersistence();
            // Register FinanceService and its dependencies
            services.AddScoped<IFinanceService, FinanceService>();
            return services;
        }
    }
}