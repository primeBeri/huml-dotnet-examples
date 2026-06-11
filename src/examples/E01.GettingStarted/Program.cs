using Examples.Shared;
using Huml.Net;

// E01 — Getting started: deserialise, serialise, round-trip.
// Mirrors the docs "Getting Started" tutorial. Doubles as an end-to-end test.
//
// Note the clean idiom: `using Huml.Net;` then `HumlSerializer.Deserialize(...)`. The facade is
// named HumlSerializer (not Huml) precisely so this resolves cleanly from any consumer namespace.

namespace Examples.GettingStarted;

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Getting started");

        const string document = """
            %HUML v0.2.0
            Host: "localhost"
            Port: 8080
            Debug: true
            """;

        // Deserialise a HUML document into a typed object.
        var config = HumlSerializer.Deserialize<ServerConfig>(document);
        Check.Equal("localhost", config.Host, "Host deserialised");
        Check.Equal(8080, config.Port, "Port deserialised");
        Check.Equal(true, config.Debug, "Debug deserialised");

        // Serialise it back out — the version directive is always written first.
        string huml = HumlSerializer.Serialize(config);
        Check.True(huml.StartsWith("%HUML", StringComparison.Ordinal), "serialised output begins with the version directive");

        // Round-trip stability: deserialising the re-serialised form yields an equivalent object.
        var roundTripped = HumlSerializer.Deserialize<ServerConfig>(huml);
        Check.Equal(config.Host, roundTripped.Host, "round-trip Host");
        Check.Equal(config.Port, roundTripped.Port, "round-trip Port");

        Console.WriteLine("E01 passed.");
    }
}

internal sealed class ServerConfig
{
    public string Host { get; set; } = "";
    public int Port { get; set; }
    public bool Debug { get; set; }
}
