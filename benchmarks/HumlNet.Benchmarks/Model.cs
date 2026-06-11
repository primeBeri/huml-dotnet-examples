using System.Text.Json.Serialization;

namespace HumlNet.Benchmarks;

// A medium, nested, realistic config object used across all benchmarks. Plain properties so it
// works identically under Huml.Net and System.Text.Json (no naming policy — keys match names).
public sealed class ServiceConfig
{
    public string Name { get; set; } = "";
    public string Version { get; set; } = "";
    public bool Enabled { get; set; }
    public ServerBlock Server { get; set; } = new();
    public DatabaseBlock Database { get; set; } = new();
    public List<string> Features { get; set; } = [];
}

public sealed class ServerBlock
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool Tls { get; set; }
}

public sealed class DatabaseBlock
{
    public string Driver { get; set; } = "";
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public int PoolSize { get; set; }
    public List<string> ReadReplicas { get; set; } = [];
}

// System.Text.Json source-generation context (the reflection-free STJ path).
[JsonSerializable(typeof(ServiceConfig))]
public sealed partial class StjContext : JsonSerializerContext;
