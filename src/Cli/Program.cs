using System.Text;
using Cli.Demos;

Console.OutputEncoding = Encoding.UTF8;

if (args.Contains("--file") && !args.Any(a => a.StartsWith("lab") || a == "5"))
{
    await Lab8Demo.RunAsync(args);
    return;
}

string firstArg = args.FirstOrDefault(a => !a.StartsWith("--"))?.ToLowerInvariant() ?? string.Empty;

if (firstArg.StartsWith("report-"))
{
    Lab7Demo.Run(args);
    return;
}

string targetLab = firstArg switch
{
    "lab1" or "lab-1" or "1" => "lab1",
    "lab2" or "lab-2" or "2" => "lab2",
    "lab3" or "lab-3" or "3" => "lab3",
    "lab4" or "lab-4" or "4" => "lab4",
    "lab5" or "lab-5" or "5" => "lab5",
    "lab6" or "lab-6" or "6" => "lab6",
    "lab7" or "lab-7" or "7" => "lab7",
    "lab8" or "lab-8" or "8" => "lab8",
    _ => "lab8"
};

string[] subArgs = string.IsNullOrEmpty(firstArg) || firstArg == targetLab || firstArg.StartsWith("lab")
    ? [.. args.Skip(string.IsNullOrEmpty(firstArg) ? 0 : 1)]
    : args;

if (string.IsNullOrEmpty(firstArg))
{
    Console.WriteLine("[CLI DISPATCHER] Запуск актуальної лабораторної (Lab 8: Async/Await & Cancellation).");
    Console.WriteLine("Доступні команди: lab1 [--json] | lab2 | lab3 [файл] | lab4 | lab5 [--file] [--cached] | lab6 | lab7 [report-*] | lab8 [--file] [--cancel] [--stream]\n");
}

switch (targetLab)
{
    case "lab1":
        Lab1Demo.Run(subArgs);
        break;
    case "lab2":
        Lab2Demo.Run();
        break;
    case "lab3":
        Lab3Demo.Run(subArgs);
        break;
    case "lab4":
        Lab4Demo.Run();
        break;
    case "lab5":
        Lab5Demo.Run(subArgs);
        break;
    case "lab6":
        Lab6Demo.Run();
        break;
    case "lab7":
        Lab7Demo.Run(subArgs);
        break;
    case "lab8":
        await Lab8Demo.RunAsync(subArgs);
        break;
}
