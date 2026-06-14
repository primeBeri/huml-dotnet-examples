using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;

namespace Examples.Enums;

// E03 — Enums round-trip as quoted strings; [HumlEnumValue] customises the wire name.
// Docs: https://primeberi.github.io/huml-dotnet/docs/enum-serialisation.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Enums");

        var parsed = HumlSerializer.Deserialize<LogSettings>("""
            %HUML v0.2.0
            Level: "warning"
            """);
        Check.Equal(LogLevel.Warning, parsed.Level, "\"warning\" -> LogLevel.Warning via [HumlEnumValue]");

        string huml = HumlSerializer.Serialize(new LogSettings { Level = LogLevel.Error });
        Check.True(huml.Contains("\"error\"", StringComparison.Ordinal), "LogLevel.Error serialises to \"error\"");

        // Round-trip preserves the value.
        var restored = HumlSerializer.Deserialize<LogSettings>(huml);
        Check.Equal(LogLevel.Error, restored.Level, "enum round-trips");

        Console.WriteLine("E03 passed.");
    }
}

internal enum LogLevel
{
    [HumlEnumValue("info")] Information,
    [HumlEnumValue("warning")] Warning,
    [HumlEnumValue("error")] Error,
}

internal sealed class LogSettings
{
    public LogLevel Level { get; set; }
}
