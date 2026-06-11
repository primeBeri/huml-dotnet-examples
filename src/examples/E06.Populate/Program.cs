using Examples.Shared;
using Huml.Net;

namespace Examples.Populate;

// E06 — Overlay a partial HUML document onto an existing object (defaults + overrides pattern).

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Populate");

        // Start from a fully-populated defaults object.
        var settings = new AppSettings { Theme = "light", FontSize = 12, Telemetry = true };

        // A user config that only overrides two of the three properties.
        HumlSerializer.Populate("""
            %HUML v0.2.0
            Theme: "dark"
            FontSize: 16
            """, settings);

        Check.Equal("dark", settings.Theme, "Theme overridden");
        Check.Equal(16, settings.FontSize, "FontSize overridden");
        Check.True(settings.Telemetry, "Telemetry left at its default (absent from the document)");

        Console.WriteLine("E06 passed.");
    }
}

internal sealed class AppSettings
{
    public string Theme { get; set; } = "";
    public int FontSize { get; set; }
    public bool Telemetry { get; set; }
}
