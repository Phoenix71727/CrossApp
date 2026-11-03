using Core;

namespace Cli.Demos;

public static class Lab2Demo
{
    public static void Run()
    {
        var report = EnvironmentInfo.Collect();

        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №2: БІБЛІОТЕКА CORE + CLI, MULTI-TARGETING, ПУБЛІКАЦІЯ");
        Console.WriteLine(new string('=', 72));

        Console.WriteLine("\n[1] АРХІТЕКТУРА ТА MULTI-TARGETING (CORE)");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"• Бібліотека класів : src/Core/Core.csproj");
        Console.WriteLine($"• Консольний клієнт : src/Cli/Cli.csproj (ProjectReference -> Core)");
        Console.WriteLine($"• Target Frameworks : net8.0;net10.0 (Core)");
        Console.WriteLine($"• Поточна збірка    : {report.CoreBuildTarget}");
        Console.WriteLine($"• Цільова RID       : {report.DetectedRid}");

        Console.WriteLine("\n[2] ПОРІВНЯННЯ РЕЖИМІВ ПУБЛІКАЦІЇ (DOTNET PUBLISH)");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("| Режим публікації            | К-ть файлів | Розмір   | Потрібен .NET Runtime |");
        Console.WriteLine("|-----------------------------|-------------|----------|-----------------------|");
        Console.WriteLine("| framework-dependent (FDD)   | 7           | 0.2 МБ   | Так (.NET 10)         |");
        Console.WriteLine("| self-contained (SCD) win-x64| 194         | 76.7 МБ  | Ні (вбудований)       |");
        Console.WriteLine("| SCD + single-file           | 1           | 70.1 МБ  | Ні (вбудований)       |");
        Console.WriteLine("| SCD + single-file + trimmed | 1           | 12.3 МБ  | Ні (економія 84%)     |");

        Console.WriteLine("\n[3] КОМАНДИ ПУБЛІКАЦІЇ З МЕТОДИЧКИ");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("• Framework-dependent:");
        Console.WriteLine("  dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained false");
        Console.WriteLine("• Self-contained:");
        Console.WriteLine("  dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained true");
        Console.WriteLine("• Single-file + Trimmed:");
        Console.WriteLine("  dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true");
        Console.WriteLine(new string('=', 72));
    }
}
