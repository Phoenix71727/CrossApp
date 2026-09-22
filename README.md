# CrossApp

Наскрізний проєкт з крос-платформного програмування.  
Студент: **Мотлюк Павло**, група **ФЕІ-36с**

---

## Предметна область: Замовлення

**Сутності:**
- `Customer` — клієнт / замовник
- `Product` — товар каталогу
- `Order` — замовлення
- `OrderLine` — позиція / рядок замовлення

**Призначення застосунку:**  
Оформлення замовлень клієнтів, ведення товарних позицій та підрахунок їхньої підсумкової вартості.

---

## Запуск

```bash
# 1. Клонування репозиторію
git clone https://github.com/Phoenix71727/Cross_App
cd Cross_App

# 2. Збирання проєкту
dotnet build

# 3. Запуск консольного застосунку
dotnet run --project src/Cli
dotnet run --project src/Cli -- --json
```

## Середовище

- .NET SDK 10.0.400
- RID: win-x64
- ОС: Windows 11 Pro x64 (Build 26200)
- Редактор: Visual Studio Code + C# Dev Kit

## Структура solution

```text
CrossApp.sln
└── src/
    ├── Core/        # class library (net8.0;net10.0), без точки входу
    │   ├── EnvironmentInfo.cs
    │   ├── Dto/       # record-типи (тиждень 3)
    │   ├── Domain/    # сутності з поведінкою (тиждень 4)
    │   └── Storage/   # сховища (тиждень 5)
    └── Cli/         # консольний застосунок (net10.0), ProjectReference → Core
```

## Публікація

```bash
# Framework-dependent (потрібен встановлений .NET Runtime)
dotnet publish src/Cli -c Release -r win-x64 --self-contained false -o dist/fdd-win

# Self-contained (автономний запуск без встановленого .NET)
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -o dist/scd-win
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true -o dist/scd-linux
```

### Порівняння результатів публікації

| RID | Режим | К-ть файлів | Розмір | Потрібен runtime |
|---|---|---|---|---|
| `win-x64` | framework-dependent | 7 | 0.2 МБ | так (.NET 10) |
| `win-x64` | self-contained | 194 | 76.7 МБ | ні |
| `linux-x64` | self-contained | 194 | 78.8 МБ | ні |

**Self-contained** — у каталог публікації копіюється повний .NET runtime і системні бібліотеки. Застосунок працює автономно на машині без .NET, але каталог має більший розмір і прив'язаний до конкретного RID.

**Framework-dependent** — містить лише скомпільований код застосунку та залежності. Каталог маленький (~200 КБ), але на цільовій машині має бути встановлений .NET 10.

## Додаткові завдання

### 1. Публікація в один файл (Single-File Publish)

Усі керовані збірки пакуються в єдиний виконуваний бінарний файл `Cli.exe`:

```bash
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o dist/single-win
```

- Результат: **1 файл** (`Cli.exe`), розмір **70.1 МБ**.

### 2. Оптимізація розміру (Trimmed Publish)

Інструмент IL Trimmer аналізує граф викликів і видаляє невикористовуваний код із BCL:

```bash
# Звичайний тримінг
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -o dist/trimmed-win

# Максимальна оптимізація: Single-File + Trimmed
dotnet publish src/Cli -c Release -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -o dist/single-trimmed-win
```

| Режим оптимізації | К-ть файлів | Розмір | Економія |
|---|---|---|---|
| Self-contained (базовий) | 194 | 76.7 МБ | 0% |
| Self-contained + Trimmed | 31 | 19.2 МБ | ~75% |
| Self-contained + Single-File + Trimmed | 1 | 12.3 МБ | **~84%** |

### 3. Умовна компіляція (Multi-targeting у Core)

Бібліотека `Core` підтримує одночасну збірку під `net8.0` та `net10.0`. За допомогою директив препроцесора `#if NET10_0_OR_GREATER` код адаптується під цільовий рантайм:

```csharp
public static string BuildTarget =>
#if NET10_0_OR_GREATER
    ".NET 10.0 (Core TFM)";
#elif NET8_0
    ".NET 8.0 (Core TFM)";
#else
    "Unknown Target";
#endif
```
