using BenchmarkDotNet.Running;
using HumlNet.Benchmarks;

// Run with: dotnet run -c Release --project benchmarks/HumlNet.Benchmarks
BenchmarkRunner.Run<SerializerBenchmarks>();
