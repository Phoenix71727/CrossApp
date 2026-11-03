using System.Text.Json;
using Core;

namespace Cli.Demos;

public static class Lab1Demo
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static void Run(string[] args)
    {
        var report = EnvironmentInfo.Collect();

        if (args.Contains("--json"))
        {
            Console.WriteLine(JsonSerializer.Serialize(report, JsonOptions));
            return;
        }

        Console.WriteLine(new string('=', 72));
        Console.WriteLine("    ЛАБОРАТОРНА РОБОТА №1: НАЛАШТУВАННЯ СЕРЕДОВИЩА ТА СТАРТ ПРОЄКТУ");
        Console.WriteLine(new string('=', 72));
        Console.WriteLine("CrossApp – практикум з крос-платформного програмування");
        Console.WriteLine("Студент: Мотлюк Павло, група ФЕІ-36с");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine($"ОС (OSDescription)    : {report.OsDescription}");
        Console.WriteLine($"Архітектура процесу   : {report.ProcessArchitecture}");
        Console.WriteLine($"RID (визначено)       : {report.DetectedRid}");
        Console.WriteLine($"RID (від .NET)        : {report.ReportedRid}");
        Console.WriteLine($"Runtime               : {report.FrameworkDescription}");
        Console.WriteLine($"Цільовий TFM (Core)   : {report.CoreBuildTarget}");
        Console.WriteLine($"Каталог застосунку    : {report.BaseDirectory}");
        Console.WriteLine($"Поточний каталог      : {Environment.CurrentDirectory}");
        Console.WriteLine(new string('-', 72));
        Console.WriteLine("Предметна область     : Замовлення (Customer, Product, Order, OrderLine)");
        Console.WriteLine(new string('=', 72));
    }
}
