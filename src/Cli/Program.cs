using System;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Core;

Console.OutputEncoding = Encoding.UTF8;

EnvironmentReport report = EnvironmentInfo.Collect();

if (args.Length > 0 && args[0].Equals("--json", StringComparison.OrdinalIgnoreCase))
{
    Console.WriteLine(JsonSerializer.Serialize(report, AppJsonContext.Default.EnvironmentReport));
    return;
}

Console.WriteLine("CrossApp – інформація про середовище");
Console.WriteLine(new string('-', 52));
Console.WriteLine($"ОС            : {report.OsDescription}");
Console.WriteLine($"Runtime       : {report.FrameworkDescription}");
Console.WriteLine($"Архітектура   : {report.ProcessArchitecture}");
Console.WriteLine($"RID (визначено): {report.DetectedRid}");
Console.WriteLine($"RID (від .NET) : {report.ReportedRid}");
Console.WriteLine($"Core TFM       : {report.CoreBuildTarget}");
Console.WriteLine($"Каталог        : {report.BaseDirectory}");

if (!Console.IsInputRedirected)
{
    Console.WriteLine();
    Console.WriteLine("Натисніть Enter, щоб вийти...");
    Console.ReadLine();
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(EnvironmentReport))]
internal partial class AppJsonContext : JsonSerializerContext
{
}
