using System.Globalization;
using Examples.Shared;
using Huml.Net;
using Huml.Net.Parser;
using Huml.Net.Serialization;
using Huml.Net.Versioning;

namespace Examples.CustomConverters;

// E05 — A custom HumlConverter<T> that serialises a value type to/from a compact string form.
// Docs: https://primeberi.github.io/huml-dotnet/docs/custom-converters.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Custom converters");

        var options = new HumlOptions { Converters = [new TemperatureConverter()] };

        var reading = HumlSerializer.Deserialize<Reading>("""
            %HUML v0.2.0
            Sensor: "kitchen"
            Value: "21.5C"
            """, options);

        Check.Equal("kitchen", reading.Sensor, "Sensor binds normally");
        Check.Equal(21.5, reading.Value.Celsius, "converter parsed \"21.5C\" -> 21.5°C");

        string huml = HumlSerializer.Serialize(reading, options);
        Check.True(huml.Contains("\"21.5C\"", StringComparison.Ordinal), "converter wrote \"21.5C\"");

        Console.WriteLine("E05 passed.");
    }
}

internal readonly record struct Temperature(double Celsius);

internal sealed class Reading
{
    public string Sensor { get; set; } = "";
    public Temperature Value { get; set; }
}

internal sealed class TemperatureConverter : HumlConverter<Temperature>
{
    public override Temperature Read(HumlNode node)
    {
        var text = (node as HumlScalar)?.Value as string ?? "0C";
        var number = text.TrimEnd('C', 'c');
        return new Temperature(double.Parse(number, CultureInfo.InvariantCulture));
    }

    public override void Write(HumlWriterContext context, Temperature value)
        => context.AppendRaw($"\"{value.Celsius.ToString(CultureInfo.InvariantCulture)}C\"");
}
