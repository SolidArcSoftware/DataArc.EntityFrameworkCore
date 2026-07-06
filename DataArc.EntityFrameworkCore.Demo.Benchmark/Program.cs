using BenchmarkDotNet.Running;
using DataArc.EntityFrameworkCore.Demo.Benchmark;

static class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<RawEmployeeBulkDataBenchmark>();
    }
}