using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using DataArc.EntityFrameworkCore.Demo.Application.Features.EmployeePerformance.Services;
using DataArc.EntityFrameworkCore.Demo.Application.Features.SalaryAdjustments.Services;

using DataArc.EntityFrameworkCore.Demo.Application.Repositories;
using DataArc.EntityFrameworkCore.Demo.Host.Repositories;
using DataArc.EntityFrameworkCore.Demo.Host.Services;

namespace DataArc.EntityFrameworkCore.Demo.Host
{
    public static class HostRegistration
    {
        public static IServiceCollection AddHostApplicationServices(this IServiceCollection services, ConfigurationManager configurationManager)
        {
            // Register repos
            services.AddScoped<IGoogleRepository, GoogleRepository>();
            services.AddScoped<IMicrosoftRepository, MicrosoftRepository>();
            services.AddScoped<IOpenAiRepository, OpenAiRepository>();
            services.AddScoped<ISolidArcRepository, SolidArcRepository>();

            // Register Background worker services
            services.AddScoped<ISalaryAdjustmentService, SalaryAdjustmentService>();
            services.AddScoped<IEmployeePerformanceService, EmployeePerformanceService>();
            return services;
        }
    }
}