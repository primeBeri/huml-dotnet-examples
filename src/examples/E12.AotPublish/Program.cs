using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;
using Huml.Net.Serialization.Attributes;
using Huml.Net.Versioning;

namespace Examples.AotPublish;

// E12 — AOT / trim-safe publishing.
//
// HumlSerializer.Parse builds a HumlDocument AST with no reflection on user types — fully
// safe in a PublishAot=true or PublishTrimmed=true build with no configuration needed.
//
// HumlSerializer.Serialize<T> and Deserialize<T> use reflection and carry:
//   [RequiresUnreferencedCode] — warns ILLink that types may be trimmed away
//   [RequiresDynamicCode]      — warns the AOT compiler that runtime code gen is needed
// In a trimmed/AOT build these appear as IL2026/IL3050 at the call site. Suppress with a
// targeted #pragma, or — better — register a source-generated context that removes the
// annotations entirely.
//
// Docs: https://primeberi.github.io/huml-dotnet/docs/aot-trimming.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("AOT publish");

        const string document = """
            %HUML v0.2.0
            Title: "Huml.Net"
            Rating: 5
            """;

        // Parse is always AOT-safe — lexer/parser pipeline has no user-type reflection.
        var doc = HumlSerializer.Parse(document);
        Check.True(doc.Entries.Count > 0, "Parse produces AST with no user-type reflection");
        Check.Equal(HumlSpecVersion.V0_2, doc.DetectedVersion.GetValueOrDefault(), "version detected from header");

        // Source-gen path: [HumlSerializable] emits compile-time metadata into AotContext.
        // No runtime reflection; IL2026/IL3050 do not apply to these call sites.
        var aotOpts = new HumlOptions { TypeInfoResolver = AotContext.Default };

        var result = HumlSerializer.Deserialize<DocInfo>(document, aotOpts);
        Check.Equal("Huml.Net", result.Title, "source-gen path deserialises Title");
        Check.Equal(5, result.Rating, "source-gen path deserialises Rating");

        string serialised = HumlSerializer.Serialize(result, aotOpts);
        Check.True(serialised.Contains("Title:", StringComparison.Ordinal), "source-gen path serialises Title");
        Check.True(serialised.StartsWith("%HUML", StringComparison.Ordinal), "version directive emitted first");

        // Round-trip stability via source-gen resolver.
        var restored = HumlSerializer.Deserialize<DocInfo>(serialised, aotOpts);
        Check.Equal(result.Title, restored.Title, "round-trip Title stable");
        Check.Equal(result.Rating, restored.Rating, "round-trip Rating stable");

        Console.WriteLine("E12 passed.");
    }
}

internal sealed class DocInfo
{
    public string Title { get; set; } = "";
    public int Rating { get; set; }
}

[HumlSerializable(typeof(DocInfo))]
internal partial class AotContext : HumlGeneratedContext
{
}
