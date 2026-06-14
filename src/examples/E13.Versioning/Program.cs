using Examples.Shared;
using Huml.Net;
using Huml.Net.Exceptions;
using Huml.Net.Versioning;

namespace Examples.Versioning;

// E13 — Version handling: HumlDocument.DetectedVersion from the %HUML header,
// VersionSource.Options for header-independent parsing, version-preserving round-trips,
// and UnknownVersionBehaviour for forward-compatible consumption.
// Docs: https://primeberi.github.io/huml-dotnet/docs/versioning.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Versioning");

        // 1. DetectedVersion: always read from the %HUML header token, independent of options.
        var doc = HumlSerializer.Parse("""
            %HUML v0.2.0
            Host: "example.com"
            """);
        Check.Equal(HumlSpecVersion.V0_2, doc.DetectedVersion.GetValueOrDefault(), "v0.2 header -> DetectedVersion");

        // 2. VersionSource.Options — ignore the %HUML header; apply the configured version.
        //    Useful when consuming documents from systems that omit a version directive.
        var fixedOpts = new HumlOptions
        {
            VersionSource = VersionSource.Options,
            SpecVersion = HumlSpecVersion.V0_2,
        };
        var result = HumlSerializer.Deserialize<Config>("""
            %HUML v0.2.0
            Host: "options-source.example.com"
            """, fixedOpts);
        Check.Equal("options-source.example.com", result.Host, "VersionSource.Options deserialises correctly");

        // 3. Version-preserving round-trip: detect the version from the source document and
        //    re-emit with the same version so the output header matches the input.
        const string original = """
            %HUML v0.2.0
            Host: "roundtrip.example.com"
            """;
        var parsed = HumlSerializer.Parse(original);
        var roundTripOpts = new HumlOptions
        {
            SpecVersion = parsed.DetectedVersion ?? HumlSpecVersion.V0_2,
        };
        var dto = HumlSerializer.Deserialize<Config>(original, roundTripOpts);
        string reserialised = HumlSerializer.Serialize(dto, roundTripOpts);
        Check.True(reserialised.Contains("%HUML", StringComparison.Ordinal), "re-serialised output has version directive");
        Check.Equal("roundtrip.example.com", HumlSerializer.Deserialize<Config>(reserialised).Host, "round-trip Host stable");

        // 4. UnknownVersionBehaviour.UseLatest — tolerate documents declaring a future version
        //    by falling back to the latest known rules rather than throwing.
        var lenientOpts = new HumlOptions
        {
            VersionSource = VersionSource.Header,
            UnknownVersionBehaviour = UnknownVersionBehaviour.UseLatest,
        };
        var lenient = HumlSerializer.Deserialize<Config>("""
            %HUML v9.9.0
            Host: "future.example.com"
            """, lenientOpts);
        Check.Equal("future.example.com", lenient.Host, "UseLatest falls back on unknown version v9.9.0");

        // 5. UnknownVersionBehaviour.Throw — strict version enforcement rejects unknown versions.
        var strictOpts = new HumlOptions
        {
            VersionSource = VersionSource.Header,
            UnknownVersionBehaviour = UnknownVersionBehaviour.Throw,
        };
        Check.Throws<HumlUnsupportedVersionException>(
            () => HumlSerializer.Deserialize<Config>("""
                %HUML v9.9.0
                Host: "x"
                """, strictOpts),
            "Throw rejects unknown version v9.9.0");

        Console.WriteLine("E13 passed.");
    }
}

internal sealed class Config
{
    public string Host { get; set; } = "";
}
