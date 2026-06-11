namespace Examples.Shared;

/// <summary>
/// Minimal assertion helper so every example doubles as an end-to-end test: a failed check
/// throws, the example's <c>Main</c> propagates it, and the process exits non-zero — which the
/// CI runner treats as a failing example. Successful checks print a tick so the console output
/// also reads as a runnable tutorial.
/// </summary>
public static class Check
{
    public static void Equal<T>(T expected, T actual, string label)
    {
        if (!EqualityComparer<T>.Default.Equals(expected, actual))
            throw new ExampleAssertionException($"{label}: expected <{expected}>, got <{actual}>");
        Console.WriteLine($"  ✓ {label}");
    }

    public static void True(bool condition, string label)
    {
        if (!condition)
            throw new ExampleAssertionException($"{label}: expected true");
        Console.WriteLine($"  ✓ {label}");
    }

    public static void Throws<TException>(Action action, string label)
        where TException : Exception
    {
        try
        {
            action();
        }
        catch (TException)
        {
            Console.WriteLine($"  ✓ {label} (threw {typeof(TException).Name})");
            return;
        }
        throw new ExampleAssertionException($"{label}: expected {typeof(TException).Name} to be thrown");
    }

    /// <summary>Prints a heading for the example. Call once at the top of <c>Main</c>.</summary>
    public static void Heading(string title) => Console.WriteLine($"== {title} ==");
}

/// <summary>Thrown when an example assertion fails.</summary>
public sealed class ExampleAssertionException(string message) : Exception(message);
