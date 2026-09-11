using DataArc.EntityFrameworkCore.Demo.Application.Dtos;
using DataArc.EntityFrameworkCore.Demo.Application.Repositories;
using DataArc.EntityFrameworkCore.Demo.Application.Features.EmployeePerformance.Services;

namespace DataArc.EntityFrameworkCore.Demo.Host.Services
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
            var topratedGoogleEmployees = await _googleRepository.GetTopRatedGoogleEmployeesAsync(rating);
            var topratedMicrosoftEmployees = await _microsoftRepository.GetTopRatedMicrosoftEmployeesAsync(rating);
            var topratedOpenAiEmployees = await _openAiRepository.GetTopRatedOpenAIEmployeesAsync(rating);
            var topratedSolidArcEmployees = await _solidArcRepository.GetTopRatedSolidArcEmployeesAsync(rating);

            var googleEmployees = topratedGoogleEmployees
                .Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary
                })
                .ToList();

            var microsoftEmployees = topratedMicrosoftEmployees
                .Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary
                })
                .ToList();

            var openAiEmployees = topratedOpenAiEmployees
                .Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary
                })
                .ToList();

            var solidArcEmployees = topratedSolidArcEmployees
                .Select(employee => new EmployeeDto
                {
                    Id = employee.Id,
                    Name = employee.Name,
                    Surname = employee.Surname,
                    Salary = employee.Salary
                })
                .ToList();

            var consolidatedTopRatedEmployees = new List<EmployeeDto>();

            consolidatedTopRatedEmployees.AddRange(googleEmployees);
            consolidatedTopRatedEmployees.AddRange(microsoftEmployees);
            consolidatedTopRatedEmployees.AddRange(openAiEmployees);
            consolidatedTopRatedEmployees.AddRange(solidArcEmployees);

            return consolidatedTopRatedEmployees;
        }
    }
}