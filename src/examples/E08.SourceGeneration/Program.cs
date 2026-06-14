using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;
using Huml.Net.Serialization.Attributes;
using Huml.Net.Versioning;

namespace Examples.SourceGeneration;

// E08 — Reflection-free metadata via the source generator. The generated context resolver
// is wired into HumlOptions; serialise/deserialise then use generated delegates.
// Docs: https://primeberi.github.io/huml-dotnet/docs/source-generator.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Source generation");

        var options = new HumlOptions { TypeInfoResolver = AppContext_.Default };

        var product = HumlSerializer.Deserialize<Product>("""
            %HUML v0.2.0
            Sku: "ABC-123"
            Price: 9
            """, options);

        Check.Equal("ABC-123", product.Sku, "Sku via generated resolver");
        Check.Equal(9, product.Price, "Price via generated resolver");

        string huml = HumlSerializer.Serialize(product, options);
        Check.True(huml.Contains("Sku:", StringComparison.Ordinal), "serialised via generated resolver");

        Console.WriteLine("E08 passed.");
    }
}

internal sealed class Product
{
    public string Sku { get; set; } = "";
    public int Price { get; set; }
}

[HumlSerializable(typeof(Product))]
internal partial class AppContext_ : HumlGeneratedContext
{
}
