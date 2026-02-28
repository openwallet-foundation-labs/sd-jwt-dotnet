namespace SdJwt.Net.Samples.Shared;

/// <summary>
/// Console output formatting helpers for consistent sample presentation.
/// </summary>
public static class ConsoleHelpers
{
    /// <summary>
    /// Prints a styled section header.
    /// </summary>
    public static void PrintHeader(string title, char borderChar = '=')
    {
        var border = new string(borderChar, 70);
        Console.WriteLine();
        Console.WriteLine(border);
        Console.WriteLine($"  {title}");
        Console.WriteLine(border);
    }

    /// <summary>
    /// Prints a subsection header.
    /// </summary>
    public static void PrintSubHeader(string title)
    {
        Console.WriteLine();
        Console.WriteLine($"--- {title} ---");
    }

    /// <summary>
    /// Prints a step in a tutorial sequence.
    /// </summary>
    public static void PrintStep(int stepNumber, string description)
    {
        Console.WriteLine();
        Console.WriteLine($"Step {stepNumber}: {description}");
        Console.WriteLine(new string('-', 50));
    }

    /// <summary>
    /// Prints a success message with checkmark.
    /// </summary>
    public static void PrintSuccess(string message)
    {
        Console.WriteLine($"  [OK] {message}");
    }

    /// <summary>
    /// Prints an info message.
    /// </summary>
    public static void PrintInfo(string label, object? value)
    {
        Console.WriteLine($"  {label}: {value}");
    }

    /// <summary>
    /// Prints a warning message.
    /// </summary>
    public static void PrintWarning(string message)
    {
        Console.WriteLine($"  [!] {message}");
    }

    /// <summary>
    /// Prints an error message.
    /// </summary>
    public static void PrintError(string message)
    {
        Console.WriteLine($"  [ERROR] {message}");
    }

    /// <summary>
    /// Prints a key-value pair with indentation.
    /// </summary>
    public static void PrintKeyValue(string key, object? value, int indent = 2)
    {
        var indentation = new string(' ', indent);
        Console.WriteLine($"{indentation}{key}: {value}");
    }

    /// <summary>
    /// Prints a list of items.
    /// </summary>
    public static void PrintList(IEnumerable<string> items, string bullet = "-", int indent = 4)
    {
        var indentation = new string(' ', indent);
        foreach (var item in items)
        {
            Console.WriteLine($"{indentation}{bullet} {item}");
        }
    }

    /// <summary>
    /// Prints a completion summary.
    /// </summary>
    public static void PrintCompletion(string tutorialName, string[] completedItems)
    {
        Console.WriteLine();
        Console.WriteLine(new string('=', 70));
        Console.WriteLine($"  {tutorialName} - Complete!");
        Console.WriteLine();
        foreach (var item in completedItems)
        {
            Console.WriteLine($"  [X] {item}");
        }
        Console.WriteLine(new string('=', 70));
    }

    /// <summary>
    /// Prints a truncated preview of a long string (like a JWT).
    /// </summary>
    public static void PrintPreview(string label, string value, int maxLength = 80)
    {
        var preview = value.Length > maxLength
            ? $"{value[..maxLength]}..."
            : value;
        Console.WriteLine($"  {label}: {preview}");
    }

    /// <summary>
    /// Waits for user to press a key before continuing.
    /// </summary>
    public static void WaitForKey(string message = "Press any key to continue...")
    {
        Console.WriteLine();
        Console.WriteLine(message);
        if (!Console.IsInputRedirected)
        {
            Console.ReadKey(true);
        }
    }
}
