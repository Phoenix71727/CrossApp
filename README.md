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

# 6. Лабораторна 5: Запуск зі сховищем у пам'яті (InMemoryOrderStore)
dotnet run --project src/Cli -f net10.0

# 7. Лабораторна 5: Запуск із файловим сховищем JSON (FileOrderStore -> data/orders.json)
dotnet run --project src/Cli -f net10.0 -- --file

# 8. Лабораторна 5: Запуск із декоратором кешування (CachingOrderStore)
dotnet run --project src/Cli -f net10.0 -- --file --cached
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
│   ├── mixed.csv           # змішані товари (P) і клієнти (C) (додаткове завдання 2)
│   └── orders.json         # збережені замовлення у файловому режимі (Лаба 5)
├── src/
│   ├── Core/               # class library (net8.0; net10.0), без точки входу
│   │   ├── EnvironmentInfo.cs
│   │   ├── SampleData.cs   # зразки товарів і замовлень для наповнення сховища
│   │   ├── Abstractions/
│   │   │   └── IOrderStore.cs           # контракт сховища замовлень
│   │   ├── Domain/
│   │   │   ├── Order.cs
│   │   │   ├── OrderLine.cs
│   │   │   ├── Product.cs
│   │   │   ├── Customer.cs
│   │   │   ├── OrderStatus.cs
│   │   │   ├── DomainImportResult.cs
│   │   │   └── OrderLimitService.cs
│   │   ├── Dto/
│   │   │   ├── IEntityDto.cs
│   │   │   ├── ProductDto.cs
│   │   │   ├── CustomerDto.cs
│   │   │   ├── OrderDto.cs              # OrderDto та OrderLineDto для збереження
│   │   │   └── ImportResult.cs
│   │   └── Import/
│   │       ├── ProductCsvImporter.cs
│   │       ├── ProductJsonImporter.cs
│   │       └── OrderDataImporter.cs
│   │   ├── Services/
│   │   │   └── OrderService.cs          # доменний сервіс замовлень
│   │   └── Storage/
│   │       ├── InMemoryOrderStore.cs    # реалізація сховища в пам'яті
│   │       ├── FileOrderStore.cs        # реалізація файлового JSON-сховища
│   │       ├── CachingOrderStore.cs     # декоратор кешування (патерн Decorator)
│   │       └── OrderStoreFactory.cs     # фабрика сховищ (патерн Simple Factory)
│   └── Cli/                # консольний застосунок, ProjectReference → Core
│       └── Program.cs
└── CrossApp.sln
```

Залежність строго одностороння: `Cli` → `Core`.

---

## Лабораторна робота №5: Сховище даних, Dependency Injection (ручне), інверсія залежностей (DIP)

### 1. Домен, агрегат та інтерфейс сховища
- **Предметна область**: Замовлення (`Order`, `OrderLine`, `Customer`, `Product`).
- **Корінь агрегату**: `Order`. Замовлення зберігається та завантажується разом з усіма своїми рядками `OrderLine` як єдине неподільне ціле.
- **Контракт сховища (`IOrderStore`)**: розташований у `src/Core/Abstractions/IOrderStore.cs`:
  ```csharp
  namespace Core.Abstractions;

  public interface IOrderStore
  {
      IReadOnlyList<Order> List();
      Order? GetById(string id);
      IReadOnlyList<Order> Find(Func<Order, bool> predicate);
      void Add(Order item);
      void Update(Order item);
      bool Remove(string id);
  }
  ```
  * `IReadOnlyList<Order> List()` — повертає доступну лише для читання колекцію, щоб зовнішній клієнтський код не міг мутувати внутрішній стан сховища в обхід методів `Add`/`Update`/`Remove`.
  * `Order? GetById(string id)` — повертає nullable тип `Order?`, оскільки запису з таким ідентифікатором може не існувати (нормальний результат пошуку, а не збій).
  * `IReadOnlyList<Order> Find(Func<Order, bool> predicate)` — гнучка фільтрація за довільною предикатною умовою (підготовка до LINQ).
  * `void Add(Order item)` — валідує аргумент (`ArgumentNullException.ThrowIfNull`) та контролює унікальність `Id` у колекції (`ContainsKey` $\rightarrow$ `InvalidOperationException`).
  * `void Update(Order item)` — оновлює стан збереженого агрегату.
  * `bool Remove(string id)` — видаляє замовлення за `id`.

---

### 2. Реалізації сховища
1. **`InMemoryOrderStore` (у пам'яті):**
   - Працює на базі `Dictionary<string, Order>` з нечутливим до регістру порівнянням ключів `StringComparer.OrdinalIgnoreCase`.
   - Приймає початкові дані через первинний конструктор: `public sealed class InMemoryOrderStore(IEnumerable<Order>? seed = null) : IOrderStore`.
   - Заповнюється валідними зразками через `SampleData.Orders()`. Дані існують лише під час виконання процесу.
2. **`FileOrderStore` (файлове сховище JSON):**
   - Шлях збереження: `data/orders.json` (відносний через `AppContext.BaseDirectory`).
   - Кеш у пам'яті: приватний словник `_cache` забезпечує миттєвий доступ до замовлень.
   - Ліниве завантаження (`EnsureLoaded()`): зчитує `orders.json` лише при першому зверненні, відновлюючи доменні агрегати через DTO-мапінг `Order.FromDto(dto)`.
   - Синхронізація на диск (`Flush()`): автоматично викликається після кожної зміни (`Add`, `Update`, `Remove`), створює директорію за потреби (`Directory.CreateDirectory`) та записує відформатований JSON (`WriteIndented = true`, `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` для чистого UTF-8 тексту кирилиці).
3. **`CachingOrderStore` (декоратор кешування — Додаткове завдання 1):**
   - Реалізує патерн **Decorator** (GoF): приймає `IOrderStore inner` у конструкторі, надає той самий контракт `IOrderStore`, але прозоро кешує запити `GetById` / `Find` та веде підрахунок метрик `CacheHits` і `CacheMisses`. Демонструє принцип Open/Closed (OCP).
4. **`OrderStoreFactory` (фабрика сховищ — Додаткове завдання 3):**
   - Патерн **Simple Factory**: інкапсулює правила створення сховища залежно від прапорців CLI (`--file`, `--cached`).

---

### 3. Схема залежностей та архітектура
Застосовано принцип інверсії залежностей (**Dependency Inversion Principle, DIP**): модулі вищого рівня (`OrderService`) та деталі нижчого рівня (`InMemoryOrderStore`, `FileOrderStore`) залежать від абстракції (`IOrderStore`).

```text
       ┌──────────────┐
       │   src/Cli    │  (Composition Root: розбір аргументів, OrderStoreFactory)
       └──────┬───────┘
              │ створює та передає через new
              ▼
    ┌──────────────────┐       викликає        ┌──────────────────┐
    │   OrderService   ├──────────────────────►│   IOrderStore    │ (Abstractions)
    └──────────────────┘                       └────────▲─────────┘
                                                        │
                      ┌─────────────────────────────────┼─────────────────────────────────┐
                      │ реалізує                        │ реалізує                        │ реалізує (Decorator)
             ┌────────┴───────────┐           ┌─────────┴─────────┐           ┌───────────┴────────────┐
             │ InMemoryOrderStore │           │  FileOrderStore   │           │    CachingOrderStore   │
             └────────────────────┘           └───────────────────┘           └────────────────────────┘
```

`OrderService` не має жодної залежності від `Core.Storage` чи `FileOrderStore`, його єдина залежність — інтерфейс `IOrderStore`.

---

### 4. Доменний сервіс `OrderService`
Сервіс виконує прикладні операції над замовленнями:
* `CreateOrder(customerId)` — створення замовлення з автоматичною генерацією унікального `Id` (`Guid.NewGuid().ToString("N")[..8]`);
* `AddLine(orderId, productId, name, price, quantity)` — отримання замовлення через `_store.GetById(id) ?? throw`, додавання рядка з перевіркою інваріантів сутності, фіксація через `_store.Update(order)`;
* `Confirm(orderId)` — валідація наявності та переведення статусу у `Confirmed` через `order.Confirm()`, збереження в сховищі;
* `Cancel(orderId)` — скасування замовлення за правилами переходів станів;
* `All()` та `Find(id)` / `Find(predicate)` — отримання та гнучкий пошук замовлень.

---

### 5. Composition Root у `Program.cs`
Ручне впровадження залежностей (Manual / Pure DI) без використання сторонніх DI-контейнерів:
```csharp
// Cli/Program.cs
bool useFile = args.Contains("--file");
bool useCache = args.Contains("--cached");
string dataPath = Path.Combine(AppContext.BaseDirectory, "data", "orders.json");

IOrderStore store = OrderStoreFactory.Create(args, dataPath);
var service = new OrderService(store);
```

---

### 6. Сценарії відмови (Error Handling)
Усі помилки перехоплюються на рівні CLI за допомогою блоків `try/catch` без стек-трейсу та аварійного завершення програми:
* **Спроба додати замовлення з дублікатом `Id`** $\rightarrow$ `[InvalidOperationException]: Замовлення з id='...' уже існує.`
* **Звернення до неіснуючого замовлення (`AddLine` / `Confirm`)** $\rightarrow$ `[InvalidOperationException]: Немає замовлення з id='ORD-NON-EXISTENT'.`
* **Порушення бізнес-інваріантів (додавання у підтверджене замовлення)** $\rightarrow$ `[InvalidOperationException]: Не можна додавати рядки до замовлення зі статусом 'Confirmed'.`

---

### 7. Додаткові завдання Лабораторної №5
1. **Декоратор `CachingOrderStore`**: реалізує `IOrderStore`, огортає внутрішнє сховище `inner`, додає in-memory кеш та збір статистики звернень (перший виклик `Misses = 1, Hits = 0`, повторний `Misses = 1, Hits = 1`).
2. **Пошук за предикатом `Func<Order, bool>`**: підтримка фільтрації замовлень у `IOrderStore` та `OrderService` (наприклад, пошук усіх підтверджених замовлень на суму $> 30\,000$ грн).
3. **Фабрика `OrderStoreFactory` та аналіз підходів**:
   * *Варіант «усе в Program.cs»*: лаконічний для найпростіших утиліт, але змішує консольний інтерфейс зі створенням об'єктів.
   * *Варіант `OrderStoreFactory`*: дотримується Single Responsibility Principle (SRP), інкапсулює розбір прапорців (`--file`, `--cached`) і логіку конфігурації сховищ, дозволяє легко писати тести на створення компонентів.

---

## Лабораторна робота №4: Доменна модель, інваріанти та інкапсуляція

### Сутності та Агрегат
- `Order` — корінь агрегату (Aggregate Root). Керує власними рядками та життєвим циклом (`OrderStatus`: `Draft`, `Confirmed`, `Cancelled`), обчислює загальну суму `Total`. Внутрішній список рядків `_lines` повністю інкапсульовано і видається назовні лише як `IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly()`.
- `OrderLine` — рядок замовлення (ідентифікатор товару, назва, ціна, кількість $> 0$, підсумок рядка `LineTotal = Price * Quantity`).
- `Product` — сутність товару з методами `UpdatePrice`, `Rename`, а також мапінгом `ToDto()` та `FromDto(ProductDto dto)`.
- `Customer` — сутність клієнта з перевіркою коректності email.

Усі сутності спроєктовано за принципами **Rich Domain Model**: вони мають **приватні конструктори** та створюються виключно через статичні фабричні методи `Create(...)`. Стан змінюється лише через бізнес-методи (відсутні публічні сетери).

### Таблиця інваріантів та винятків

| Сутність / Метод | Інваріант (Бізнес-правило) | Тип винятку | Обґрунтування вибору винятку |
|---|---|---|---|
| `Order.Create` | Ідентифікатори `Id` та `CustomerId` не можуть бути порожніми | `ArgumentException` | Некоректні вхідні аргументи фабричного методу |
| `OrderLine.Create` | Ідентифікатор товару та назва обов'язкові | `ArgumentException` | Некоректні вхідні рядкові аргументи |
| `OrderLine.Create` | Ціна не може бути від'ємною ($Price \ge 0$) | `ArgumentOutOfRangeException` | Числове значення виходить за межі допустимого діапазону |
| `OrderLine.Create` | Кількість має бути більшою за нуль ($Quantity > 0$) | `ArgumentOutOfRangeException` | Неприпустиме значення кількості товару |
| `Order.AddLine` | Додавання рядків дозволено лише у статусі `Draft` | `InvalidOperationException` | Операція заборонена для поточного стану замовлення |
| `Order.Confirm` | Замовлення не може бути підтверджено без позицій (порожнім) | `InvalidOperationException` | Стан об'єкта не відповідає вимогам підтвердження |
| `Order.Confirm` | Підтвердження можливе тільки зі статусу `Draft` | `InvalidOperationException` | Заборона повторного або некоректного переходу стану |
| `Order.Cancel` | Заборонено скасовувати вже підтверджене замовлення | `InvalidOperationException` | Захист незворотності підтвердженого замовлення |
| `Product.UpdatePrice` | Нова ціна товару не може бути від'ємною ($Price \ge 0$) | `ArgumentOutOfRangeException` | Некоректне значення для оновлення ціни |
| `Customer.Create` | Email клієнта має містити символ `@` та бути непорожнім | `ArgumentException` | Порушення синтаксичного формату електронної пошти |

### Додаткові завдання Лабораторної №4

1. **Зв'язок із 3 тижнем (`Product.FromDtos`):** метод приймає `ImportResult<ProductDto>` і повертає `DomainImportResult<Product>`, розділяючи валідні сутності та виявлені порушення інваріантів («дані + помилки»).
2. **Інваріант двох сутностей (`OrderLimitService`):** правило «не більше 3 активних замовлень на одного клієнта». Винесено в Доменний Сервіс, оскільки окремі сутності не повинні мати доступу до всієї колекції замовлень системи (дотримання принципу Single Responsibility та підготовка до патерну Repository на 5 тижні).
3. **Перевірка переходів станів через `switch expression`:** метод `Order.CanTransition(current, next)` перевіряє допустимість зміни стану замовлення за допомогою pattern matching на кортежах.

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
