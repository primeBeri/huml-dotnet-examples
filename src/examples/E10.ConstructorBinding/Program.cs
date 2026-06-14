using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;

namespace Examples.ConstructorBinding;

// E10 — Constructor binding: record primary constructor (auto-selected), [HumlConstructor]
// disambiguating an overloaded class, and init-only properties set during deserialisation.
// Docs: https://primeberi.github.io/huml-dotnet/docs/constructor-binding.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Constructor binding");

        // Records: the primary constructor is the only public ctor, so it is selected
        // automatically — no attribute needed.
        var point = HumlSerializer.Deserialize<Point>("""
            %HUML v0.2.0
            X: 1.5
            Y: -2.0
            """);
        Check.Equal(1.5, point.X, "record X via primary ctor");
        Check.Equal(-2.0, point.Y, "record Y via primary ctor");

        // [HumlConstructor] resolves ambiguity when a type exposes multiple constructors.
        var conn = HumlSerializer.Deserialize<Connection>("""
            %HUML v0.2.0
            Host: "db.local"
            Port: 5432
            """);
        Check.Equal("db.local", conn.Host, "[HumlConstructor] ctor bound Host");
        Check.Equal(5432, conn.Port, "[HumlConstructor] ctor bound Port");

        // Constructor parameter matching is case-insensitive (HUML key "Host" -> param "host").
        var connLower = HumlSerializer.Deserialize<Connection>("""
            %HUML v0.2.0
            host: "case-insensitive.local"
            port: 3306
            """);
        Check.Equal("case-insensitive.local", connLower.Host, "key casing is ignored when matching params");

        // Init-only properties ({ get; init; }) are settable during deserialisation.
        // No [HumlConstructor] needed — the parameterless ctor is used and init setters are applied.
        var cfg = HumlSerializer.Deserialize<ImmutableConfig>("""
            %HUML v0.2.0
            Endpoint: "https://api.example.com"
            TimeoutMs: 5000
            """);
        Check.Equal("https://api.example.com", cfg.Endpoint, "init-only Endpoint set");
        Check.Equal(5000, cfg.TimeoutMs, "init-only TimeoutMs set");

        Console.WriteLine("E10 passed.");
    }
}

// Record: single primary constructor, auto-selected.
internal sealed record Point(double X, double Y);

// Class with two constructors: [HumlConstructor] marks the one Huml.Net should use.
internal sealed class Connection
{
    public string Host { get; }
    public int Port { get; }

    [HumlConstructor]
    public Connection(string host, int port) { Host = host; Port = port; }

    public Connection() { Host = "localhost"; Port = 80; }
}

// Init-only properties: set via the parameterless constructor path without any attribute.
internal sealed class ImmutableConfig
{
    public string Endpoint { get; init; } = "";
    public int TimeoutMs { get; init; }
}
