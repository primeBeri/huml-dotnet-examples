using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;
using Huml.Net.Versioning;

namespace Examples.Options;

// E09 — HumlOptions customisation: naming policy, collection format, ignore condition,
// and number handling. Shows how each option shapes serialised output and parsed results.
// Docs: https://primeberi.github.io/huml-dotnet/docs/options-reference.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Options");

        // 1. SnakeCase naming policy — PascalCase .NET properties bind to snake_case HUML keys.
        var snakeOpts = new HumlOptions { PropertyNamingPolicy = HumlNamingPolicy.SnakeCase };
        var server = HumlSerializer.Deserialize<ServerInfo>("""
            %HUML v0.2.0
            host_name: "api.example.com"
            max_connections: 50
            """, snakeOpts);
        Check.Equal("api.example.com", server.HostName, "snake_case key -> HostName");
        Check.Equal(50, server.MaxConnections, "snake_case key -> MaxConnections");

        // Serialising with the same policy reproduces snake_case keys.
        string snakeOut = HumlSerializer.Serialize(server, snakeOpts);
        Check.True(snakeOut.Contains("host_name:", StringComparison.Ordinal), "serialised keys are snake_case");

        // 2. CollectionFormat.Inline — HUML inline sequences emit comma-separated values on a
        //    single line after the :: vector indicator (e.g. Tags:: "rock", "classic", "live"),
        //    rather than the block format with one `- item` per line.
        var inlineOpts = new HumlOptions { CollectionFormat = CollectionFormat.Inline };
        var playlist = new Playlist { Tags = ["rock", "classic", "live"] };
        string inlineOut = HumlSerializer.Serialize(playlist, inlineOpts);
        Check.True(inlineOut.Contains("\"rock\",", StringComparison.Ordinal), "inline list items are comma-separated");
        Check.True(!inlineOut.Contains("- \"rock\"", StringComparison.Ordinal), "block-format item markers absent");

        // 3. DefaultIgnoreCondition.WhenWritingNull — null properties are absent from output.
        var ignoreNullOpts = new HumlOptions { DefaultIgnoreCondition = HumlIgnoreCondition.WhenWritingNull };
        var sparse = new Annotation { Label = "important", Note = null };
        string sparseOut = HumlSerializer.Serialize(sparse, ignoreNullOpts);
        Check.True(sparseOut.Contains("Label:", StringComparison.Ordinal), "non-null Label present");
        Check.True(!sparseOut.Contains("Note:", StringComparison.Ordinal), "null Note absent");

        // 4. NumberHandling.WriteAsString — numeric values emitted as quoted strings.
        var numOpts = new HumlOptions { NumberHandling = HumlNumberHandling.WriteAsString };
        string numOut = HumlSerializer.Serialize(new Counter { Count = 42 }, numOpts);
        Check.True(numOut.Contains("\"42\"", StringComparison.Ordinal), "integer emitted as quoted \"42\"");

        // AllowReadingFromString parses the quoted number back to int on the read side.
        var readOpts = new HumlOptions { NumberHandling = HumlNumberHandling.AllowReadingFromString };
        var counter = HumlSerializer.Deserialize<Counter>(numOut, readOpts);
        Check.Equal(42, counter.Count, "quoted number round-trips via AllowReadingFromString");

        Console.WriteLine("E09 passed.");
    }
}

internal sealed class ServerInfo
{
    public string HostName { get; set; } = "";
    public int MaxConnections { get; set; }
}

internal sealed class Playlist
{
    public List<string> Tags { get; set; } = [];
}

internal sealed class Annotation
{
    public string Label { get; set; } = "";
    public string? Note { get; set; }
}

internal sealed class Counter
{
    public int Count { get; set; }
}
