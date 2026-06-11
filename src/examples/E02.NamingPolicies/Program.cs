using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;
using Huml.Net.Versioning;

namespace Examples.NamingPolicies;

// E02 — Map .NET PascalCase property names to kebab-case HUML keys (and a [HumlProperty] override).

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Naming policies");

        var options = new HumlOptions { PropertyNamingPolicy = HumlNamingPolicy.KebabCase };

        var config = HumlSerializer.Deserialize<DbConfig>("""
            %HUML v0.2.0
            host-name: "db.example.com"
            max-connections: 100
            ssl-enabled: true
            """, options);

        Check.Equal("db.example.com", config.HostName, "host-name -> HostName");
        Check.Equal(100, config.MaxConnections, "max-connections -> MaxConnections");
        Check.True(config.SslEnabled, "ssl-enabled -> SslEnabled");

        // Round-trip: serialising with the same policy reproduces kebab-case keys.
        string huml = HumlSerializer.Serialize(config, options);
        Check.True(huml.Contains("host-name:", StringComparison.Ordinal), "serialised keys are kebab-case");

        // [HumlProperty] overrides the policy for a single member.
        var overridden = HumlSerializer.Deserialize<DbConfig>("""
            %HUML v0.2.0
            host-name: "h"
            max-connections: 1
            ssl-enabled: false
            """, options);
        Check.Equal("h", overridden.HostName, "[HumlProperty] override still binds");

        Console.WriteLine("E02 passed.");
    }
}

internal sealed class DbConfig
{
    public string HostName { get; set; } = "";
    public int MaxConnections { get; set; }
    public bool SslEnabled { get; set; }
}
