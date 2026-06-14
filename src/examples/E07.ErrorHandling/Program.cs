using Examples.Shared;
using Huml.Net;
using Huml.Net.Exceptions;
using Huml.Net.Versioning;

namespace Examples.ErrorHandling;

// E07 — The exception contract: parse errors, required-member enforcement, strict unknown keys.
// Docs: https://primeberi.github.io/huml-dotnet/docs/error-handling.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Error handling");

        // Malformed input -> HumlParseException with line/column.
        Check.Throws<HumlParseException>(
            () => HumlSerializer.Deserialize<Account>("Id: unquoted"),
            "unquoted string is a parse error");

        // Missing [HumlRequired] member -> HumlDeserializeException.
        Check.Throws<HumlDeserializeException>(
            () => HumlSerializer.Deserialize<Account>("""
                %HUML v0.2.0
                Name: "no id here"
                """),
            "missing required Id throws");

        // HumlOptions.Strict rejects unknown keys.
        Check.Throws<HumlDeserializeException>(
            () => HumlSerializer.Deserialize<Account>("""
                %HUML v0.2.0
                Id: "a1"
                Unknown: "x"
                """, HumlOptions.Strict),
            "Strict options reject an unknown key");

        // The happy path still works.
        var ok = HumlSerializer.Deserialize<Account>("""
            %HUML v0.2.0
            Id: "a1"
            Name: "Ada"
            """);
        Check.Equal("a1", ok.Id, "valid document binds");

        Console.WriteLine("E07 passed.");
    }
}

internal sealed class Account
{
    [Huml.Net.Serialization.HumlRequired]
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
