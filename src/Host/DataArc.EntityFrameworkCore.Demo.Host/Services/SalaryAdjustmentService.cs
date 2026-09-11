using DataArc.EntityFrameworkCore.Demo.Application.Features.SalaryAdjustments.Services;
using DataArc.EntityFrameworkCore.Demo.Application.Repositories;

namespace DataArc.EntityFrameworkCore.Demo.Host.Services
{
    public class SalaryAdjustmentService : ISalaryAdjustmentService
    {
        private readonly IGoogleRepository _googleRepository;
        private readonly IOpenAiRepository _openAiRepository;
        private readonly IMicrosoftRepository _microsoftRepository;
        private readonly ISolidArcRepository _solidArcRepository;

        public SalaryAdjustmentService(
            IGoogleRepository googleRepository,
            IOpenAiRepository openAiRepository,
            IMicrosoftRepository microsoftRepository,
            ISolidArcRepository solidArcRepository)
        {
            _googleRepository = googleRepository;
            _openAiRepository = openAiRepository;
            _microsoftRepository = microsoftRepository;
            _solidArcRepository = solidArcRepository;
        }

        public async Task<int> ProcessEmployeeSalaryAdjustmentsAsync(
            decimal salaryAdjustmentBaseRate,
            double rating,
            int batchSize)
        {

            var employees = await _solidArcRepository.GetTopRatedSolidArcEmployeesAsync(rating);

            foreach (var employee in employees)
            {
                employee.Salary += employee.Salary * salaryAdjustmentBaseRate;
            }

            var googleProcessedCount = await _googleRepository
                .ProcessBulkGoogleEmployeesAsync(employees, batchSize);

            var microsoftProcessedCount = await _microsoftRepository
                .ProcessBulkMicrosoftEmployeesAsync(employees, batchSize);

            var openAiProcessedCount = await _openAiRepository
                .ProcessBulkOpenAIEmployeesAsync(employees, batchSize);

            return new[]
                {
                    googleProcessedCount,
                    microsoftProcessedCount,
                    openAiProcessedCount
                }.Sum();
        }
    }
}