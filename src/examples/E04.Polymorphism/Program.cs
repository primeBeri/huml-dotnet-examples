using Examples.Shared;
using Huml.Net;
using Huml.Net.Serialization;
using Huml.Net.Versioning;

namespace Examples.Polymorphism;

// E04 — Discriminator-based polymorphism, including derived types nested in a collection
// (the case the G3 review found was previously broken — proves the fix end-to-end).
// Docs: https://primeberi.github.io/huml-dotnet/docs/polymorphism.html

internal static class Program
{
    private static void Main()
    {
        Check.Heading("Polymorphism");

        var zoo = new Zoo
        {
            Star = new Cat { Name = "Felix", Lives = 9 },
            Animals = [new Dog { Name = "Rex", Tricks = 3 }, new Cat { Name = "Tom", Lives = 7 }],
        };

        string huml = HumlSerializer.Serialize(zoo);
        Check.True(huml.Contains("kind: \"cat\"", StringComparison.Ordinal), "discriminator emitted for nested property");

        var restored = HumlSerializer.Deserialize<Zoo>(huml, HumlOptions.Default);
        Check.Equal(typeof(Cat), restored.Star.GetType(), "nested Star keeps concrete type Cat");
        Check.Equal(9, ((Cat)restored.Star).Lives, "Cat.Lives round-trips");
        Check.Equal(typeof(Dog), restored.Animals[0].GetType(), "collection element 0 is Dog");
        Check.Equal(typeof(Cat), restored.Animals[1].GetType(), "collection element 1 is Cat");

        Console.WriteLine("E04 passed.");
    }
}

[HumlPolymorphic("kind")]
[HumlDerivedType(typeof(Dog), "dog")]
[HumlDerivedType(typeof(Cat), "cat")]
internal abstract class Animal
{
    public string Name { get; set; } = "";
}

internal sealed class Dog : Animal
{
    public int Tricks { get; set; }
}

internal sealed class Cat : Animal
{
    public int Lives { get; set; }
}

internal sealed class Zoo
{
    public Animal Star { get; set; } = new Dog();
    public IList<Animal> Animals { get; set; } = [];
}
