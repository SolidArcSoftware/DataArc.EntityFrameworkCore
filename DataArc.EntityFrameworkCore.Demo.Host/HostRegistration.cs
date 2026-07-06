using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.EmployeePerformance.Services;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Host.BackgroundServices;
using DataArc.EntityFrameworkCore.Demo.Persistence;

namespace DataArc.EntityFrameworkCore.Demo.Host
{
    public static class HostRegistration
    {
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            // Register Persistence
            services.AddPersistence(configurationManager);

            // Register Background worker services
            services.AddScoped<ISalaryAdjustmentService, SalaryAdjustmentService>();
            services.AddScoped<IEmployeePerformanceService, EmployeePerformanceService>();
            return services;
        }
    }
}
