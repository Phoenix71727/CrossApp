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
Застосунок призначений для автоматизації обліку клієнтських замовлень, ведення товарних позицій та розрахунку підсумкової вартості замовлень.

---

## Інструкція із запуску

```powershell
# 1. Клонування репозиторію
git clone https://github.com/Phoenix71727/CrossApp
cd CrossApp

# 2. Збирання проєкту
dotnet build

# 3. Запуск консольного клієнта
dotnet run --project src/Cli
```

---

## Середовище розробки

- **.NET SDK**: 10.0.400 (TFM: `net10.0`, RID: `win-x64`)
- **ОС**: Windows 11 Pro x64 (Build 26200)
- **IDE**: Visual Studio Code (C# Dev Kit)

---

## Додаткові завдання

### 1. Порівняння розмірів Self-Contained під різні RID

```powershell
# Публікація під Windows (win-x64)
dotnet publish src/Cli -c Release -r win-x64 --self-contained true

# Публікація під Linux (linux-x64)
dotnet publish src/Cli -c Release -r linux-x64 --self-contained true
```

- **`win-x64`** (`src/Cli/bin/Release/net10.0/win-x64/publish`): **76.67 MB** *(файл `Cli.exe` — 158 KB)*
- **`linux-x64`** (`src/Cli/bin/Release/net10.0/linux-x64/publish`): **78.80 MB**

*Висновок:* Автономна збірка містить у собі весь .NET Runtime (CLR) та бібліотеки BCL, тому не потребує встановленого .NET на цільовій машині. Збірка для Linux дещо більша через нативні системні бібліотеки UNIX.

### 2. Прапорець командного рядка `--json`
- **Звичайний запуск** (`dotnet run --project src/Cli`): виводить форматовану консольну таблицю.
- **Запуск із прапорцем** (`dotnet run --project src/Cli -- --json`): виводить системні дані одним структурованим JSON-рядком (`System.Text.Json`).
