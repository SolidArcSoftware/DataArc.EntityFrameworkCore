using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

using DataArc.Core;
using DataArc.EntityFrameworkCore.Demo.Persistence;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.Creator;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

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
        private IReadOnlyCollection<IDatabaseCreator> _databaseCreators = [];
        private ICommandFactory? _commandFactory;

        private List<Employee> _allEmployees = [];
        private List<Employee> _benchmarkEmployees = [];

        [Params(62_500, 125_000, 250_000)]
        public int RecordCount { get; set; }

        [Params(62_500)]
        public int BulkBatchSize { get; set; }

        [GlobalSetup]
        public void GlobalSetup()
        {
            _serviceProvider = new ServiceCollection()
                .AddPersistence()
                .BuildServiceProvider();

            _databaseCreators =
            [
                _serviceProvider.GetRequiredService<IFinanceDbCreator>(),
                _serviceProvider.GetRequiredService<IHrDbCreator>(),
                _serviceProvider.GetRequiredService<IItDbCreator>(),
                _serviceProvider.GetRequiredService<IOperationsDbCreator>()
            ];

            _commandFactory = _serviceProvider.GetRequiredService<ICommandFactory>();

            _allEmployees = SeedDataGenerator
                .GenerateHrSeedData(MaxRecordCount)
                .ToList();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            if (_databaseCreators.Count == 0)
                throw new InvalidOperationException("Benchmark database creators were not resolved.");

            foreach (var databaseCreator in _databaseCreators)
            {
                if (!databaseCreator.EnsureDeleted())
                    throw new InvalidOperationException(
                        $"Failed to delete benchmark database using {databaseCreator.GetType().Name}.");
            }

            foreach (var databaseCreator in _databaseCreators)
            {
                if (!databaseCreator.EnsureCreated())
                    throw new InvalidOperationException(
                        $"Failed to create benchmark database using {databaseCreator.GetType().Name}.");
            }

            _benchmarkEmployees = _allEmployees
                .Take(RecordCount)
                .ToList();
        }

        [Benchmark]
        public async Task<int> ExecuteParallelBulkInsertAsync()
        {
            if (_commandFactory == null)
                throw new InvalidOperationException($"{nameof(ICommandFactory)} was not resolved.");

            if (_benchmarkEmployees.Count == 0)
                throw new InvalidOperationException("Benchmark employee data was not generated.");

            var commandBuilder = await _commandFactory.CreateCommandBuilderAsync();

            if (commandBuilder == null)
                throw new InvalidOperationException($"{nameof(commandBuilder)} was not resolved.");

            commandBuilder
                .UseDbExecutionContext<IHrDbContext>()
                    .AddBulk(_benchmarkEmployees, BulkBatchSize);

            commandBuilder
                .UseDbExecutionContext<IFinanceDbContext>()
                    .AddBulk(_benchmarkEmployees, BulkBatchSize);

            commandBuilder
                .UseDbExecutionContext<IItDbContext>()
                    .AddBulk(_benchmarkEmployees, BulkBatchSize);

            commandBuilder
                .UseDbExecutionContext<IOperationsDbContext>()
                    .AddBulk(_benchmarkEmployees, BulkBatchSize);

            var command = await commandBuilder.BuildAsync();
            var commandResult = await command.ExecuteParallelAsync();

            if (!commandResult.Success)
            {
                throw new InvalidOperationException(
                    $"Failed to execute raw employee bulk insert benchmark. {commandResult.Exception?.Message}");
            }

            return commandResult.TotalAffected;
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
            AddColumn(new TotalInsertedRecordsColumn(destinationContextCount: 4));
        }
    }

    public sealed class TotalInsertedRecordsColumn : IColumn
    {
        private readonly int _destinationContextCount;
        public TotalInsertedRecordsColumn(int destinationContextCount)
        {
            _destinationContextCount = destinationContextCount;
        }
        public string Id => nameof(TotalInsertedRecordsColumn);
        public string ColumnName => "Total Inserted Records";
        public bool AlwaysShow => true;
        public ColumnCategory Category => ColumnCategory.Params;
        public int PriorityInCategory => 0;
        public bool IsNumeric => true;
        public UnitType UnitType => UnitType.Dimensionless;
        public string Legend => "Total records inserted across all participating DbContexts.";
        public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
        {
            var recordCountParameter = benchmarkCase.Parameters.Items
                .Single(parameter => parameter.Name == "RecordCount");

            var recordCount = Convert.ToInt32(recordCountParameter.Value);
            var totalInsertedRecords = recordCount * _destinationContextCount;

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
        public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase)
        {
            return false;
        }
    }

    #endregion
}