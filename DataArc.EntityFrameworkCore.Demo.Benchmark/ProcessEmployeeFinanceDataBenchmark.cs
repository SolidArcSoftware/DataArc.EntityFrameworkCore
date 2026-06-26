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
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBContexts;
using DataArc.EntityFrameworkCore.Demo.Persistence.Database.DBModels;
using DataArc.EntityFrameworkCore.Demo.Persistence.Utils;

namespace DataArc.EntityFrameworkCore.Demo.Benchmark
{
    [MemoryDiagnoser]
    [ThreadingDiagnoser]
    [SimpleJob]
    [WarmupCount(1)]
    [IterationCount(4)]
    [Config(typeof(BenchmarkConfig))]
    public class RawEmployeeBulkDataBenchmark
    {
        private IServiceProvider? _serviceProvider;
        private IDatabaseCreator? _databaseCreator;
        private ICommandFactory? _commandFactory;
        private IReadOnlyList<Employee>? _employees;

        [Params(10_000, 100_000, 250_000)]
        public int RecordCount { get; set; }
        public int BulkBatchSize => RecordCount;

        [GlobalSetup]
        public void GlobalSetup()
        {
            _serviceProvider = new ServiceCollection()
                .AddPersistence()
                .BuildServiceProvider();

            _databaseCreator = _serviceProvider.GetRequiredService<IDatabaseCreator>();
            _commandFactory = _serviceProvider.GetRequiredService<ICommandFactory>();
        }

        [IterationSetup]
        public void IterationSetup()
        {
            if (_databaseCreator == null)
                throw new InvalidOperationException($"{nameof(IDatabaseCreator)} was not resolved.");

            if (!_databaseCreator.EnsureDeleted())
                throw new InvalidOperationException("Failed to delete benchmark databases.");

            if (!_databaseCreator.EnsureCreated())
                throw new InvalidOperationException("Failed to create benchmark databases.");

            _employees = SeedDataGenerator.GenerateHrSeedData(RecordCount);
        }

        [Benchmark]
        public async Task<int> ExecuteParallelBulkInsertAsync()
        {
            var commandBuilder = await _commandFactory!.CreateCommandBuilderAsync();

            if (commandBuilder == null)
                throw new InvalidOperationException($"{nameof(commandBuilder)} was not resolved.");

            if (_employees == null)
                throw new InvalidOperationException("Benchmark employee data was not generated.");

            commandBuilder
                .UseDbExecutionContext<IHrDbContext>()
                    .AddBulk(_employees, BulkBatchSize);

            commandBuilder
                .UseDbExecutionContext<IFinanceDbContext>()
                    .AddBulk(_employees, BulkBatchSize);

            commandBuilder.UseDbExecutionContext<IItDbContext>()
                    .AddBulk(_employees, BulkBatchSize);

            commandBuilder
                .UseDbExecutionContext<IOperationsDbContext>()
                    .AddBulk(_employees, BulkBatchSize);

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
            //_databaseCreator?.EnsureDeleted(); 
            // Cleanup if needed after all iterations are complete
        }
    }

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
}