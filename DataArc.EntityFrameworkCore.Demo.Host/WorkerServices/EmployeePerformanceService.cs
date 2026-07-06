using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Features.EmployeePerformance.Services;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Google.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.Microsoft.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.OpenAi.Repository;
using DataArc.EntityFrameworkCore.Demo.Contracts.Application.Modules.SolidArc.Repository;

namespace DataArc.EntityFrameworkCore.Demo.Host.BackgroundServices
{
    internal class EmployeePerformanceService : IEmployeePerformanceService
    {
        private readonly IGoogleRepository _googleRepository;
        private readonly IOpenAiRepository _openAiRepository;
        private readonly IMicrosoftRepository _microsoftRepository;
        private readonly ISolidArcRepository _solidArcRepository;

        public EmployeePerformanceService(
            IGoogleRepository googleRepository,
            IOpenAiRepository openAiRepository,
            IMicrosoftRepository microsoftRepository,
            ISolidArcRepository solidArcRepository
            )
        {
            _googleRepository = googleRepository;
            _microsoftRepository = microsoftRepository;
            _solidArcRepository = solidArcRepository;
            _openAiRepository = openAiRepository;
        }

        public async Task<List<EmployeeDto>> GetConsolidatedTopRatedEmployeesAsync(double rating)
        {
            var topratedGoogleEmployees = await _googleRepository
                .GetTopRatedEmployeesAsync(rating);

            var toprateMicrosoftEmployees = await _microsoftRepository
                .GetTopRatedEmployeesAsync(rating);

            var topratedOpenAiEmployees = await _openAiRepository
                .GetTopRatedEmployeesAsync(rating);

            var topratedSolidArcEmployees = await _solidArcRepository
                .GetTopRatedEmployeesAsync(rating);

            var consolidatedTopRatedEmployees = new List<EmployeeDto>();

            consolidatedTopRatedEmployees
                .AddRange(topratedGoogleEmployees);

            consolidatedTopRatedEmployees
                .AddRange(toprateMicrosoftEmployees);

            consolidatedTopRatedEmployees
                .AddRange(topratedOpenAiEmployees);

            consolidatedTopRatedEmployees
                .AddRange(topratedSolidArcEmployees);

            return consolidatedTopRatedEmployees;
        }
    }
}