using System.Text.Json;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using Huml.Net;
using Huml.Net.Versioning;

namespace HumlNet.Benchmarks;

// Huml.Net vs System.Text.Json on the serialise / deserialise / parse paths, each in both
// reflection and source-generated modes. HUML and JSON payloads encode the same object, so the
// comparison is HUML-vs-JSON throughput for equivalent data — not an apples-to-oranges contest.
[MemoryDiagnoser]
[MarkdownExporterAttribute.GitHub]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
[CategoriesColumn]
public class SerializerBenchmarks
{
    private static readonly ServiceConfig Model = new()
    {
        Name = "acme-service",
        Version = "1.4.2",
        Enabled = true,
        Server = new ServerBlock { Host = "0.0.0.0", Port = 443, Tls = true },
        Database = new DatabaseBlock
        {
            Driver = "postgres", Host = "db.internal", Port = 5432, PoolSize = 20,
            ReadReplicas = ["replica-1", "replica-2", "replica-3"],
        },
        Features = ["search", "export", "audit-log"],
    };

    private static readonly HumlOptions HumlReflectionOpts = HumlOptions.Default;
    private static readonly HumlOptions HumlSourceGenOpts = new() { TypeInfoResolver = HumlContext.Default };
    private static readonly JsonSerializerOptions StjReflectionOpts = new();
    private static readonly JsonSerializerOptions StjSourceGenOpts = new() { TypeInfoResolver = StjContext.Default };

    private string _huml = "";
    private string _json = "";

    [GlobalSetup]
    public void Setup()
    {
        _huml = HumlSerializer.Serialize(Model, HumlReflectionOpts);
        _json = JsonSerializer.Serialize(Model, StjReflectionOpts);
    }

    // ── Serialize ──────────────────────────────────────────────────────────────

    [Benchmark, BenchmarkCategory("Serialize")]
    public string Huml_Serialize_Reflection() => HumlSerializer.Serialize(Model, HumlReflectionOpts);

    [Benchmark, BenchmarkCategory("Serialize")]
    public string Huml_Serialize_SourceGen() => HumlSerializer.Serialize(Model, HumlSourceGenOpts);

    [Benchmark(Baseline = true), BenchmarkCategory("Serialize")]
    public string Stj_Serialize_Reflection() => JsonSerializer.Serialize(Model, StjReflectionOpts);

    [Benchmark, BenchmarkCategory("Serialize")]
    public string Stj_Serialize_SourceGen() => JsonSerializer.Serialize(Model, typeof(ServiceConfig), StjSourceGenOpts) ?? "";

    // ── Deserialize ────────────────────────────────────────────────────────────

    [Benchmark, BenchmarkCategory("Deserialize")]
    public ServiceConfig Huml_Deserialize_Reflection() => HumlSerializer.Deserialize<ServiceConfig>(_huml, HumlReflectionOpts);

    [Benchmark, BenchmarkCategory("Deserialize")]
    public ServiceConfig Huml_Deserialize_SourceGen() => HumlSerializer.Deserialize<ServiceConfig>(_huml, HumlSourceGenOpts);

    [Benchmark(Baseline = true), BenchmarkCategory("Deserialize")]
    public ServiceConfig? Stj_Deserialize_Reflection() => JsonSerializer.Deserialize<ServiceConfig>(_json, StjReflectionOpts);

    [Benchmark, BenchmarkCategory("Deserialize")]
    public ServiceConfig? Stj_Deserialize_SourceGen() => (ServiceConfig?)JsonSerializer.Deserialize(_json, typeof(ServiceConfig), StjSourceGenOpts);

    // ── Parse-only (no object materialisation) ──────────────────────────────────

    [Benchmark, BenchmarkCategory("Parse")]
    public object Huml_Parse() => HumlSerializer.Parse(_huml);

    [Benchmark(Baseline = true), BenchmarkCategory("Parse")]
    public object Stj_Parse()
    {
        using var doc = JsonDocument.Parse(_json);
        return doc.RootElement.ValueKind;
    }
}
