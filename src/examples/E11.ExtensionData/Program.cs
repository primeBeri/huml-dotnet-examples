using Examples.Shared;
using Huml.Net;
using Huml.Net.Exceptions;
using Huml.Net.Parser;
using Huml.Net.Serialization;
using Huml.Net.Versioning;

namespace Examples.ExtensionData;

// E11 — [HumlExtensionData] captures unrecognised HUML keys as raw AST nodes, enabling
// forward-compatible consumption. Without it, UnmappedMemberHandling.Disallow rejects
// unknown keys with HumlDeserializeException.
// Docs: https://primeberi.github.io/huml-dotnet/docs/extension-data.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Extension data");

        // 1. Known keys bind normally; unknown keys land in the [HumlExtensionData] property.
        var plugin = HumlSerializer.Deserialize<PluginConfig>("""
            %HUML v0.2.0
            Name: "my-plugin"
            Version: 3
            ExperimentalFlag: true
            DebugLog: "verbose"
            """);
        Check.Equal("my-plugin", plugin.Name, "known key Name");
        Check.Equal(3, plugin.Version, "known key Version");
        Check.True(plugin.Extras?.ContainsKey("ExperimentalFlag") == true, "ExperimentalFlag in Extras");
        Check.True(plugin.Extras?.ContainsKey("DebugLog") == true, "DebugLog in Extras");

        // 2. Extension data participates in serialisation — captured keys are re-emitted after
        //    declared properties, preserving round-trip fidelity.
        string roundTripped = HumlSerializer.Serialize(plugin);
        Check.True(roundTripped.Contains("ExperimentalFlag:", StringComparison.Ordinal), "ExperimentalFlag re-emitted");
        Check.True(roundTripped.Contains("DebugLog:", StringComparison.Ordinal), "DebugLog re-emitted");

        // 3. Without [HumlExtensionData], UnmappedMemberHandling.Disallow rejects unknown keys.
        //    The two behaviours are mutually exclusive: when an extension-data property is present,
        //    Disallow is suppressed and unknown keys flow into the bucket instead.
        var strict = new HumlOptions { UnmappedMemberHandling = UnmappedMemberHandling.Disallow };
        Check.Throws<HumlDeserializeException>(
            () => HumlSerializer.Deserialize<StrictConfig>("""
                %HUML v0.2.0
                Name: "strict"
                UnknownKey: 99
                """, strict),
            "Disallow throws on unknown key UnknownKey");

        Console.WriteLine("E11 passed.");
    }
}

internal sealed class PluginConfig
{
    public string Name { get; set; } = "";
    public int Version { get; set; }

    [HumlExtensionData]
    public Dictionary<string, HumlNode>? Extras { get; set; }
}

internal sealed class StrictConfig
{
    public string Name { get; set; } = "";
}
