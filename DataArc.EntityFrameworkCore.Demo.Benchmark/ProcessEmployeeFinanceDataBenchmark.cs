using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using DataArc.EntityFrameworkCore.Demo.Persistence;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataArc.EntityFrameworkCore.Demo.Benchmark
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [SimpleJob]
    [WarmupCount(1)]
    [IterationCount(10)]
    [InvocationCount(1)]
    [Config(typeof(BenchmarkConfig))]
    public class RawEmployeeBulkDataBenchmark
    {
        private const int MaxRecordCount = 1_000_000;

        private IServiceProvider? _serviceProvider;

        private IReadOnlyCollection<IDatabaseCreator> _databaseCreators =
            new List<IDatabaseCreator>();

        private IDbContextFactory<GoogleDbContext>? _googleDbContextFactory;
        private IDbContextFactory<MicrosoftDbContext>? _microsoftDbContextFactory;
        private IDbContextFactory<OpenAIDbContext>? _openAiDbContextFactory;
        private IDbContextFactory<SolidArcDbContext>? _solidArcDbContextFactory;

        private List<Employer> _allEmployers = new();
        private List<Employee> _allEmployees = new();

        private List<Employer> _benchmarkEmployers = new();
        private List<Employee> _benchmarkEmployees = new();

        [Params(62_500, 125_000, 250_000)]
        public int RecordCount { get; set; }

        public int BulkBatchSize => MaxRecordCount;

        [GlobalSetup]
        public void GlobalSetup()
        {
            var configurationManager = new ConfigurationManager();

            configurationManager
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            _serviceProvider = new ServiceCollection()
                .AddDataArcCore()
                .AddPersistence(configurationManager)
                .BuildServiceProvider();

            _databaseCreators = new IDatabaseCreator[]
            {
                _serviceProvider.GetRequiredService<IGoogleDbCreator>(),
                _serviceProvider.GetRequiredService<IMicrosoftDbCreator>(),
                _serviceProvider.GetRequiredService<IOpenAiDbCreator>(),
                _serviceProvider.GetRequiredService<ISASDbCreator>()
            };

            _googleDbContextFactory =
                _serviceProvider.GetRequiredService<
                    IDbContextFactory<GoogleDbContext>>();

            _microsoftDbContextFactory =
                _serviceProvider.GetRequiredService<
                    IDbContextFactory<MicrosoftDbContext>>();

            _openAiDbContextFactory =
                _serviceProvider.GetRequiredService<
                    IDbContextFactory<OpenAIDbContext>>();

            _solidArcDbContextFactory =
                _serviceProvider.GetRequiredService<
                    IDbContextFactory<SolidArcDbContext>>();

            var seedData =
                SeedDataGenerator.GenerateHrSeedData(MaxRecordCount);

            _allEmployers = seedData.Employers;
            _allEmployees = seedData.Employees;
        }

        [IterationSetup]
        public void IterationSetup()
        {
            if (_databaseCreators.Count == 0)
                throw new InvalidOperationException(
                    "Benchmark database creators were not resolved.");

            foreach (var databaseCreator in _databaseCreators)
            {
                databaseCreator.EnsureDeleted();
            }

            foreach (var databaseCreator in _databaseCreators)
            {
                databaseCreator.EnsureCreated();
            }

            _benchmarkEmployers = _allEmployers
                .Take(RecordCount)
                .ToList();

            _benchmarkEmployees = _allEmployees
                .Take(RecordCount)
                .ToList();
        }

        [Benchmark]
        public async Task<int> ExecuteParallelBulkInsertAsync()
        {
            if (_googleDbContextFactory == null)
                throw new InvalidOperationException(
                    $"{nameof(_googleDbContextFactory)} was not resolved.");

            if (_microsoftDbContextFactory == null)
                throw new InvalidOperationException(
                    $"{nameof(_microsoftDbContextFactory)} was not resolved.");

            if (_openAiDbContextFactory == null)
                throw new InvalidOperationException(
                    $"{nameof(_openAiDbContextFactory)} was not resolved.");

            if (_solidArcDbContextFactory == null)
                throw new InvalidOperationException(
                    $"{nameof(_solidArcDbContextFactory)} was not resolved.");

            if (_benchmarkEmployers.Count == 0 ||
                _benchmarkEmployees.Count == 0)
            {
                throw new InvalidOperationException(
                    "Benchmark data was not generated.");
            }

            await using var googleDbContext =
                await _googleDbContextFactory.CreateDbContextAsync();

            await using var microsoftDbContext =
                await _microsoftDbContextFactory.CreateDbContextAsync();

            await using var openAiDbContext =
                await _openAiDbContextFactory.CreateDbContextAsync();

            await using var solidArcDbContext =
                await _solidArcDbContextFactory.CreateDbContextAsync();

            var results = await Task.WhenAll(
                googleDbContext
                    .AsParallel()
                    .AddBulk(_benchmarkEmployers, BulkBatchSize)
                    .AddBulk(_benchmarkEmployees, BulkBatchSize)
                    .SaveChangesParallelAsync(),

                microsoftDbContext
                    .AsParallel()
                    .AddBulk(_benchmarkEmployers, BulkBatchSize)
                    .AddBulk(_benchmarkEmployees, BulkBatchSize)
                    .SaveChangesParallelAsync(),

                openAiDbContext
                    .AsParallel()
                    .AddBulk(_benchmarkEmployers, BulkBatchSize)
                    .AddBulk(_benchmarkEmployees, BulkBatchSize)
                    .SaveChangesParallelAsync(),

                solidArcDbContext
                    .AsParallel()
                    .AddBulk(_benchmarkEmployers, BulkBatchSize)
                    .AddBulk(_benchmarkEmployees, BulkBatchSize)
                    .SaveChangesParallelAsync());

            return results.Sum();
        }

        [GlobalCleanup]
        public void GlobalCleanup()
        {
            // Keep benchmark databases for inspection after the run.
        }
    }

    #region BenchMarkDotnetSetup

    public sealed class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            AddColumn(
                new TotalInsertedRecordsColumn(
                    destinationContextCount: 4,
                    bulkOperationCountPerContext: 2));
        }
    }

    public sealed class TotalInsertedRecordsColumn : IColumn
    {
        private readonly int _destinationContextCount;
        private readonly int _bulkOperationCountPerContext;

        public TotalInsertedRecordsColumn(
            int destinationContextCount,
            int bulkOperationCountPerContext)
        {
            _destinationContextCount = destinationContextCount;
            _bulkOperationCountPerContext = bulkOperationCountPerContext;
        }

        public string Id => nameof(TotalInsertedRecordsColumn);

        public string ColumnName => "Total Inserted Records";

        public bool AlwaysShow => true;

        public ColumnCategory Category => ColumnCategory.Params;

        public int PriorityInCategory => 0;

        public bool IsNumeric => true;

        public UnitType UnitType => UnitType.Dimensionless;

        public string Legend =>
            "Total records inserted across all bulk operations and participating DbContexts.";

        public string GetValue(
            Summary summary,
            BenchmarkCase benchmarkCase)
        {
            var recordCountParameter = benchmarkCase.Parameters.Items
                .Single(parameter => parameter.Name == "RecordCount");

            var recordCount =
                Convert.ToInt32(recordCountParameter.Value);

            var totalInsertedRecords =
                recordCount *
                _destinationContextCount *
                _bulkOperationCountPerContext;

            return totalInsertedRecords.ToString("N0");
        }

        public string GetValue(
            Summary summary,
            BenchmarkCase benchmarkCase,
            SummaryStyle style)
        {
            return GetValue(summary, benchmarkCase);
        }

        public bool IsAvailable(Summary summary)
        {
            return true;
        }

        public bool IsDefault(
            Summary summary,
            BenchmarkCase benchmarkCase)
        {
            return false;
        }
    }

    #endregion
}