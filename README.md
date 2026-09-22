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
git clone https://github.com/Phoenix71727/CrossApp.git
cd CrossApp

# 2. Збирання проєкту
dotnet build

# 3. Запуск за замовчуванням (імпорт data/sample.csv)
dotnet run --project src/Cli

# 4. Імпорт конкретного файлу (CSV або JSON)
dotnet run --project src/Cli -- data/sample.csv
dotnet run --project src/Cli -- data/sample.json

# 5. Імпорт різнорідних даних (товари + клієнти)
dotnet run --project src/Cli -- data/mixed.csv
```

---

## Середовище розробки

- **.NET SDK**: 10.0.400 (TFM: `net10.0`, `net8.0`, RID: `win-x64`)
- **ОС**: Windows 11 Pro x64 (Build 26200)
- **Редактор**: Visual Studio Code + C# Dev Kit

---

## Структура solution

```text
CrossApp/
├── data/
│   ├── sample.csv          # 10+ валідних товарів + 3 дефектні рядки
│   ├── sample.json         # товари у форматі JSON (додаткове завдання 1)
│   └── mixed.csv           # змішані товари (P) і клієнти (C) (додаткове завдання 2)
├── src/
│   ├── Core/               # class library (net8.0; net10.0), без точки входу
│   │   ├── EnvironmentInfo.cs
│   │   ├── Dto/
│   │   │   ├── IEntityDto.cs
│   │   │   ├── ProductDto.cs
│   │   │   ├── CustomerDto.cs
│   │   │   └── ImportResult.cs
│   │   └── Import/
│   │       ├── ProductCsvImporter.cs
│   │       ├── ProductJsonImporter.cs
│   │       └── OrderDataImporter.cs
│   └── Cli/                # консольний застосунок, ProjectReference → Core
│       └── Program.cs
└── CrossApp.sln
```

Залежність строго одностороння: `Cli` → `Core`.

---

## Лабораторна робота №3: Імпорт даних та DTO

### Формат файлів даних
- **Роздільник**: `;` (крапка з комою, щоб уникнути конфліктів із комами в назвах).
- **Кодування**: `UTF-8` (забезпечує крос-платформність кирилиці).
- **Парсинг чисел**: `CultureInfo.InvariantCulture` (запобігає помилкам парсингу десяткової крапки `32500.00` на локалях із комою).

### Pattern Matching у розборі CSV (`switch expression`)
Розбір рядка реалізовано в `src/Core/Import/ProductCsvImporter.cs` через `switch expression` з використанням наступних видів патернів:
1. **Патерн властивостей та реляційний патерн**: `{ Length: < 3 } => ...`
2. **Патерни списків та константний патерн**: `["", _, ..] or [_, "", ..] => ...` (відсіювання порожніх обов'язкових полів).
3. **Патерн списку з охоронною умовою `when`**: `[_, _, var priceStr, ..] when !decimal.TryParse(...) => ...`
4. **Патерни списків для успішного розбору**: `[var id, var name, var priceStr]` або `[var id, var name, var priceStr, var category]`.
5. **Discard (універсальна гілка)**: `_ => ...`

---

## Додаткові завдання Лабораторної №3

### 1. JSON-імпортер (`ProductJsonImporter`)
- Реалізовано імпорт товарів із файлів `.json` через `System.Text.Json` на базі тих самих типів `ProductDto`.
- Застосунок у `Program.cs` автоматично обирає відповідний імпортер (`.csv` чи `.json`) за розширенням файлу через `switch expression`.

### 2. Різнорідний імпорт (`OrderDataImporter`)
- Підтримка обробки файлів змішаного типу (`data/mixed.csv`) за префіксами рядка в єдиному `switch`:
  - `"P;..."` — розпізнається як товар (`ProductDto`).
  - `"C;..."` — розпізнається як клієнт (`CustomerDto`).
- `Program.cs` використовує pattern matching для диференційованого виводу:
  `item switch { ProductDto p => ..., CustomerDto c => ... }`.

### 3. Підсумкова статистика імпорту
- Наприкінці роботи програма виводить статистику одним рядком:
  ```text
  Статистика імпорту: усього 13 | прийнято 10 | пропущено 3 | помилок 23,1%
  ```

---

## Публікація (з Лабораторної №2)

```bash
# Framework-dependent (потрібен встановлений .NET 10 Runtime)
dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained false -o dist/fdd-win

# Self-contained (автономний, завантажує runtime у пакет)
dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained true -o dist/scd-win
dotnet publish src/Cli -c Release -f net10.0 -r linux-x64 --self-contained true -o dist/scd-linux

# Single-File + Trimmed (максимальна оптимізація в один файл)
dotnet publish src/Cli -c Release -f net10.0 -r win-x64 --self-contained true -p:PublishTrimmed=true -p:PublishSingleFile=true -o dist/single-trimmed-win
```

### Порівняння результатів публікації

| RID | Режим | К-ть файлів | Розмір | Потрібен runtime |
|---|---|---|---|---|
| `win-x64` | framework-dependent | 7 | 0.2 МБ | так (.NET 10) |
| `win-x64` | self-contained | 194 | 76.7 МБ | ні |
| `linux-x64` | self-contained | 194 | 78.8 МБ | ні |
| `win-x64` | self-contained + single-file | 1 | 70.1 МБ | ні |
| `win-x64` | self-contained + single-file + trimmed | 1 | 12.3 МБ | ні (економія 84%) |
