using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;

Console.OutputEncoding = Encoding.UTF8;

// Додаткове завдання: режим --json
if (args.Any(a => string.Equals(a, "--json", StringComparison.OrdinalIgnoreCase)))
{
    var jsonInfo = new
    {
        Application = "CrossApp – практикум з крос-платформного програмування",
        Student = "Мотлюк Павло, група ФЕІ-36с",
        OSDescription = RuntimeInformation.OSDescription,
        EnvironmentOS = Environment.OSVersion.ToString(),
        ProcessArchitecture = RuntimeInformation.ProcessArchitecture.ToString(),
        ClrVersion = Environment.Version.ToString(),
        Runtime = RuntimeInformation.FrameworkDescription,
        BaseDirectory = AppContext.BaseDirectory,
        CurrentDirectory = Environment.CurrentDirectory,
        Domain = "Замовлення (клієнти, товари, замовлення, рядки замовлення)"
    };

    var options = new JsonSerializerOptions
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
    Console.WriteLine(JsonSerializer.Serialize(jsonInfo, options));
    return;
}

Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
Console.WriteLine("Студент: Мотлюк Павло, група ФЕІ-36с");
Console.WriteLine(new string('-', 52));

Console.WriteLine($"ОС (OSDescription)   : {RuntimeInformation.OSDescription}");
Console.WriteLine($"ОС (Environment)     : {Environment.OSVersion}");
Console.WriteLine($"Архітектура процесу  : {RuntimeInformation.ProcessArchitecture}");
Console.WriteLine($"Версія .NET (CLR)    : {Environment.Version}");
Console.WriteLine($"Runtime              : {RuntimeInformation.FrameworkDescription}");
Console.WriteLine($"Каталог застосунку   : {AppContext.BaseDirectory}");
Console.WriteLine($"Поточний каталог     : {Environment.CurrentDirectory}");

Console.WriteLine(new string('-', 52));
Console.WriteLine("Предметна область: Замовлення (клієнти, товари, замовлення, рядки замовлення)");

if (!Console.IsInputRedirected)
{
    Console.WriteLine();
    Console.WriteLine("Натисніть Enter, щоб вийти...");
    Console.ReadLine();
}