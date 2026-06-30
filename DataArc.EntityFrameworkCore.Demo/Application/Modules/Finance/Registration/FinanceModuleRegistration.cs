using DataArc.EntityFrameworkCore.Demo.Application.Modules.Finance.Features.SalaryAdjustments.Services;
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
            // Register Services
            services.AddScoped<ISalaryAdjustmentService, SalaryAdjustmentService>();
            services.AddScoped<IEmployeePerformanceService, EmployeePerformanceService>();
            return services;
        }
    }
}