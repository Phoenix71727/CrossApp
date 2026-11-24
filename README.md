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
Оформлення замовлень клієнтів, ведення товарних позицій, підрахунок підсумкової вартості та аналітична звітність.

---

## Запуск (CLI Диспетчер команд)

Застосунок підтримує модульний запуск будь-якої лабораторної роботи або конкретних підкоманд:

```bash
# 1. Збирання проєкту
dotnet build

# 2. Запуск за замовчуванням (актуальна Лабораторна 9: Шляхи, Конфігурація, Secrets):
dotnet run --project src/Cli -f net10.0

# 3. Лабораторна 9 (Шляхи, JSON, конфігурація, Secrets, експорт/імпорт):
dotnet run --project src/Cli -f net10.0 -- export           # експорт замовлень у налаштований шлях (data/orders.json)
dotnet run --project src/Cli -f net10.0 -- import           # імпорт і відображення замовлень з розрахованого шляху
dotnet run --project src/Cli -f net10.0 -- lab9             # повна демонстрація конфігурації, секретів та сховища

# 4. Модульний запуск попередніх лабораторних робіт:
dotnet run --project src/Cli -f net10.0 -- lab1            # середовище ОС, архітектура, RID
dotnet run --project src/Cli -f net10.0 -- lab1 --json     # звіт у форматі JSON
dotnet run --project src/Cli -f net10.0 -- lab2            # архітектура Core+Cli, multi-targeting, публікація
dotnet run --project src/Cli -f net10.0 -- lab3            # імпорт data/sample.csv
dotnet run --project src/Cli -f net10.0 -- lab3 data/mixed.csv # імпорт різнорідних даних
dotnet run --project src/Cli -f net10.0 -- lab4            # доменні сутності, інваріанти в try/catch
dotnet run --project src/Cli -f net10.0 -- lab5            # сховище в пам'яті (InMemoryOrderStore)
dotnet run --project src/Cli -f net10.0 -- lab5 --file     # файлове сховище JSON (data/orders.json)
dotnet run --project src/Cli -f net10.0 -- lab5 --file --cached # сховище з декоратором кешування
dotnet run --project src/Cli -f net10.0 -- lab6            # доменні події, підписники, відписка (-=)
dotnet run --project src/Cli -f net10.0 -- lab7            # LINQ-звіти, відкладене виконання, бонуси
dotnet run --project src/Cli -f net10.0 -- lab8            # async/await, CancellationToken, DomainException
dotnet run --project src/Cli -f net10.0 -- lab8 --cancel   # демонстрація скасування токена: "Імпорт скасовано."
dotnet run --project src/Cli -f net10.0 -- lab8 --bad      # демонстрація доменного винятку: "Дані відхилено..."

# 5. Тестування (усі unit-тести: xUnit, 100% зелені):
dotnet test
dotnet test --collect:"XPlat Code Coverage"
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
│   ├── sample.json         # товари у форматі JSON
│   ├── mixed.csv           # змішані товари (P) і клієнти (C)
│   ├── orders.json         # збережені замовлення у файловому режимі (Лаба 5, 8)
│   └── orders_corrupt.json # дефектний файл для демонстрації DomainException (Лаба 8)
├── logs/
│   └── app.log             # журнал доменних подій (Лаба 6)
├── src/
│   ├── Core/               # бібліотека класів (net8.0; net10.0), без точки входу
│   │   ├── EnvironmentInfo.cs   # збір параметрів ОС і середовища (Лаба 1-2)
│   │   ├── SampleData.cs        # розширений генератор товарів, клієнтів і 17 замовлень
│   │   ├── DomainException.cs       # доменний виняток для бізнес-правил (Лаба 8)
│   │   ├── Configuration/       # Модель налаштувань застосунку (Лаба 9)
│   │   │   └── AppOptions.cs    # AppOptions, StorageOptions, OrdersOptions, LoggingOptions
│   │   ├── Abstractions/
│   │   │   ├── IOrderStore.cs       # контракт сховища замовлень (Лаба 5)
│   │   │   ├── INotificationSink.cs # контракт сповіщень для ручного DI (Лаба 6)
│   │   │   └── IOrderImporter.cs    # асинхронний контракт імпорту (Лаба 8)
│   │   ├── Domain/
│   │   │   ├── Order.cs             # корінь агрегату замовлення (Лаба 4, 8)
│   │   │   ├── OrderLine.cs         # рядок замовлення
│   │   │   ├── Product.cs           # сутність товару
│   │   │   ├── Customer.cs          # сутність клієнта
│   │   │   ├── OrderStatus.cs       # enum статусів (Draft, Confirmed, Cancelled)
│   │   │   ├── DomainImportResult.cs
│   │   │   └── OrderLimitService.cs # інваріант двох сутностей
│   │   ├── Dto/
│   │   │   ├── IEntityDto.cs
│   │   │   ├── ProductDto.cs
│   │   │   ├── CustomerDto.cs
│   │   │   ├── OrderDto.cs
│   │   │   └── ImportResult.cs
│   │   ├── Events/                  # Доменні події (Лаба 6)
│   │   │   ├── OrderChangeKind.cs
│   │   │   └── OrderChangedEventArgs.cs
│   │   ├── Import/
│   │   │   ├── ProductCsvImporter.cs # підтримка Action<string> для онлайн-звіту
│   │   │   ├── ProductJsonImporter.cs
│   │   │   └── OrderDataImporter.cs
│   │   ├── Reports/                 # Аналітичні звіти LINQ (Лаба 7)
│   │   │   ├── OrderReportLine.cs        # DTO рядка аналітичного звіту
│   │   │   ├── GroupSummary.cs           # DTO підсумку групи (GroupBy)
│   │   │   ├── CustomerOrdersSummary.cs  # DTO зведеного звіту (GroupJoin)
│   │   │   ├── LinqExtensions.cs         # власний метод розширення TopBy
│   │   │   └── ReportService.cs          # аналітичні запити (чисті функції)
│   │   ├── Services/
│   │   │   └── OrderService.cs      # доменний сервіс замовлень (асинхронний імпорт)
│   │   └── Storage/
│   │       ├── InMemoryOrderStore.cs
│   │       ├── FileOrderStore.cs    # атомарний запис (.tmp + Move), незмінна кирилиця (Лаба 9)
│   │       ├── CachingOrderStore.cs # декоратор кешування (патерн Decorator)
│   │       ├── OrderStoreFactory.cs # фабрика сховищ (патерн Simple Factory)
│   │       └── JsonOrderImporter.cs # асинхронний імпортер (useAsync: true)
│   ├── Cli/                # консольний клієнт, конфігураційний стек, диспетчер команд
│   │   ├── appsettings.json             # базові налаштування (без секретів!)
│   │   ├── appsettings.Development.json # налаштування профілю розробки
│   │   ├── Demos/          # модульні сценарії демонстрації для кожної лаби
│   │   │   ├── Lab1Demo.cs
│   │   │   ├── Lab2Demo.cs
│   │   │   ├── Lab3Demo.cs
│   │   │   ├── Lab4Demo.cs
│   │   │   ├── Lab5Demo.cs
│   │   │   ├── Lab6Demo.cs
│   │   │   ├── Lab7Demo.cs
│   │   │   ├── Lab8Demo.cs
│   │   │   └── Lab9Demo.cs # конфігураційний конвеєр, читання secrets, експорт/імпорт
│   │   ├── Notifications/
│   │   │   └── NotificationSinks.cs
│   │   └── Program.cs      # точка входу (компактний CLI-диспетчер команд)
│   ├── Api/                # HTTP Minimal API (net10.0, Лаба 10)
│   │   ├── Contracts/
│   │   │   └── OrderContracts.cs        # вихідні (OrderResponse) і вхідні (SaveOrderRequest) DTO
│   │   ├── Endpoints/
│   │   │   └── OrderEndpoints.cs        # маршрути CRUD /api/orders, /total, /confirm
│   │   ├── Infrastructure/
│   │   │   ├── ApiExceptionHandler.cs   # мапінг BadHttpRequestException -> 400, DomainException -> 409
│   │   │   └── ValidationFilter.cs      # EndpointFilter для FluentValidation
│   │   ├── Validators/
│   │   │   └── SaveOrderRequestValidator.cs
│   │   ├── Properties/
│   │   │   └── launchSettings.json      # фіксований порт http://localhost:5080
│   │   ├── Api.http                     # запити для VS Code REST Client
│   │   ├── appsettings.json
│   │   └── Program.cs
│   └── tests/
│       ├── Core.Tests/     # проєкт unit-тестів xUnit (net10.0, посилання лише на Core)
│       │   ├── TestData.cs
│       │   ├── OrderServiceTests.cs
│       │   ├── ReportServiceTests.cs
│       │   ├── ImportTests.cs
│       │   └── ConfigurationTests.cs # тести ResolveFullPath, атомарного запису та кирилиці
│       └── Api.Tests/      # інтеграційні тести на WebApplicationFactory<Program> (Лаба 10)
│           └── OrderEndpointsTests.cs
└── CrossApp.sln
```

---

## Лабораторна робота №10: Minimal API: CRUD над Core, валідація, OpenAPI (Scalar)

### 1. Запуск API, фіксований порт і документація
* **Фіксований порт:** `http://localhost:5080` (зафіксовано у `src/Api/Properties/launchSettings.json`).
* **OpenAPI / Scalar (лише у `Development`):**
  * Документ OpenAPI: `http://localhost:5080/openapi/v1.json`
  * Інтерактивний UI Scalar: `http://localhost:5080/scalar/v1`

```bash
# Запуск HTTP API
dotnet run --project src/Api --urls http://localhost:5080

# Запуск усіх тестів (Core.Tests + Api.Tests)
dotnet test
```

### 2. Таблиця маршрутів (`/api/orders`)

| Метод | Шлях | Тіло запиту | Коди відповідей |
|---|---|---|---|
| `GET` | `/health` | — | `200 OK` (`{ "status": "ok" }`) |
| `GET` | `/api/orders` | — *(опційно `?page=1&pageSize=20&search=...`)* | `200 OK` (список або `{ items, total, page, pageSize }`) |
| `GET` | `/api/orders/{id}` | — | `200 OK`, `404 Not Found` |
| `GET` | `/api/orders/{id}/total` | — | `200 OK` (`{ id, total, currency }`), `404 Not Found` |
| `POST` | `/api/orders` | `SaveOrderRequest` (`customerId`, `lines[]`) | `201 Created` (`Location`), `400 ValidationProblem` |
| `PUT` | `/api/orders/{id}` | `SaveOrderRequest` (`customerId`, `lines[]`) | `200 OK`, `400 ValidationProblem`, `404 Not Found` |
| `POST` | `/api/orders/{id}/confirm` | — | `200 OK`, `404 Not Found`, `409 Conflict` (`DomainException`) |
| `DELETE` | `/api/orders/{id}` | — | `204 No Content`, `404 Not Found` |

### 3. Приклади запитів `curl`

```bash
# 1. Створення замовлення (201 Created + Location)
curl -i -X POST http://localhost:5080/api/orders -H "Content-Type: application/json" \
  -d '{"customerId":"CUST-KIEV-1","lines":[{"productId":"P-100","productName":"Цемент М400","price":245.5,"quantity":10}]}'

# 2. Невалідний запит (400 ValidationProblemDetails, не 500)
curl -i -X POST http://localhost:5080/api/orders -H "Content-Type: application/json" \
  -d '{"customerId":"","lines":[]}'

# 3. Отримання списку з пагінацією та пошуком
curl -i "http://localhost:5080/api/orders?page=1&pageSize=10&search=Цемент"

# 4. Отримання підсумкової суми конкретного замовлення
curl -i http://localhost:5080/api/orders/ORD-1001/total

# 5. Видалення замовлення (204 No Content, після чого GET повертає 404)
curl -i -X DELETE http://localhost:5080/api/orders/ORD-1001
```

### 4. Архітектурні рішення
* **DTO окремо від доменної сутності:** `Order` містить приватні колекції, інваріанти та доменну поведінку (`Confirm`, `Cancel`), які не повинні потрапляти у JSON напряму. Контракти `OrderResponse` та `SaveOrderRequest` фіксують стабільний HTTP-контракт, незмінний при рефакторингу ядра або заміні сховища на EF Core.
* **Час життя сервісів у DI:** `IOrderStore` (`InMemoryOrderStore` на потокобезпечному `ConcurrentDictionary`) та `IValidator<SaveOrderRequest>` зареєстровані як **`Singleton`**, щоб зберігати спільний стан замовлень між HTTP-запитами без створення нових екземплярів на кожен запит (на відміну від `Scoped`, який використовуватиметься для `DbContext` на 11-му тижні).
* **Додаткові завдання (Бонуси 1–4):** реалізовано пагінацію/пошук, валідацію через `FluentValidation` + `ValidationFilter`, мапінг `DomainException -> 409 Conflict` у `ApiExceptionHandler` та 6 інтеграційних тестів на `WebApplicationFactory<Program>` у `tests/Api.Tests`.

---

## Лабораторна робота №9: Шляхи, JSON, конфігурація, Secrets

### 1. Мета та правила реалізації
- **Кросплатформність шляхів:** Повне усунення захардкодzoneних роздільників (`"\"`, `"/"`) та абсолютних Windows-дисків (`"C:\"`). Шляхи формуються виключно через `Path.Combine` та приводяться до канонічного абсолютного вигляду через `Path.GetFullPath(Path.Combine(root, FileName))`.
- **Базовий каталог:** Файли даних за замовчуванням обчислюються відносно `AppContext.BaseDirectory` (каталог розміщення бінарних файлів застосунку), що гарантує однаковий доступ до файлів незалежно від того, з якої робочої директорії (`Environment.CurrentDirectory`) запущено процес (`solution root` чи `src/Cli`).
- **Стек конфігурації (`Microsoft.Extensions.Configuration`):** Пакет підключено виключно у проєкті `src/Cli`. Бібліотека `src/Core` залишається чистим POCO і не має жодних зовнішніх залежностей.
- **Ієрархія джерел конфігурації (порядок перекриття):**
  1. `appsettings.json` (базовий конфіг)
  2. `appsettings.{EnvironmentName}.json` (профіль `Development` або `Production`)
  3. `UserSecrets` (`secrets.json` поза Git-репозиторієм)
  4. Змінні середовища з префіксом `CROSSAPP_` (вкладені секції розділені подвійним підкресленням `__`)
  5. Спрощена legacy-змінна середовища `CROSSAPP_DATA_PATH` (перекриває секцію `Storage:DataPath`)
  6. Аргументи командного рядка (`CommandLine`)
- **Безпека секретів (Secrets):** Ключ `Integration:ApiKey` ніколи не додається у файли конфігурації репозиторію та не комітиться у Git. У консоль виводиться виключно факт наявності та довжина секрету: `ApiKey: присутній, довжина {apiKey.Length}` (або `ApiKey: не заданий`).
- **Атомарний запис JSON (.tmp + File.Move):** Запис даних у `FileOrderStore` здійснюється спочатку в тимчасовий файл із розширенням `.tmp`, після чого атомарно замінює цільовий файл через `File.Move(tmp, path, overwrite: true)`. Це унеможливлює пошкодження файлу при раптовому вимкненні живлення чи збої процесу під час запису.
- **Збереження кирилиці:** Для серіалізації налаштовано `JavaScriptEncoder.UnsafeRelaxedJsonEscaping`, що записує українські літери («Цемент», «Ціна») прямими UTF-8 символами без шістнадцяткових послідовностей екранування (`\u0426`).

---

### 2. Таблиця ключів конфігурації

| Ключ конфігурації | Опис | Замовчування | Змінна середовища (PowerShell) | Змінна середовища (Bash) |
|---|---|---|---|---|
| `EnvironmentName` | Профіль середовища | `"Production"` / `"Development"` | `$env:CROSSAPP_EnvironmentName="Development"` | `export CROSSAPP_EnvironmentName="Development"` |
| `Storage:DataPath` | Відносний/абсолютний каталог | `"data"` | `$env:CROSSAPP_Storage__DataPath="custom_data"` | `export CROSSAPP_Storage__DataPath="custom_data"` |
| `Storage:FileName` | Ім'я файлу збереження | `"orders.json"` | `$env:CROSSAPP_Storage__FileName="orders.json"` | `export CROSSAPP_Storage__FileName="orders.json"` |
| `Orders:Currency` | Валюта замовлень | `"UAH"` | `$env:CROSSAPP_Orders__Currency="USD"` | `export CROSSAPP_Orders__Currency="USD"` |
| `Logging:MinimumLevel` | Рівень логування | `"Information"` / `"Debug"` | `$env:CROSSAPP_Logging__MinimumLevel="Debug"` | `export CROSSAPP_Logging__MinimumLevel="Debug"` |
| `Integration:ApiKey` | Навчальний секретний ключ | *(немає в git)* | `$env:CROSSAPP_Integration__ApiKey="prod-key"` | `export CROSSAPP_Integration__ApiKey="prod-key"` |
| `CROSSAPP_DATA_PATH` | Спрощене пряме перевизначення | *(не задана)* | `$env:CROSSAPP_DATA_PATH="C:\custom_orders"` | `export CROSSAPP_DATA_PATH="/tmp/orders"` |

---

### 3. Управління User Secrets

Ідентифікатор User Secrets зберігається у `src/Cli/Cli.csproj`:
```xml
<PropertyGroup>
  <UserSecretsId>c7f53a81-e2e1-482a-9f0a-8d1976bc4028</UserSecretsId>
</PropertyGroup>
```

Фізичне розташування файлу `secrets.json` у системі:
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\c7f53a81-e2e1-482a-9f0a-8d1976bc4028\secrets.json`
  (`C:\Users\<користувач>\AppData\Roaming\Microsoft\UserSecrets\c7f53a81-e2e1-482a-9f0a-8d1976bc4028\secrets.json`)
- **Linux / macOS:** `~/.microsoft/usersecrets/c7f53a81-e2e1-482a-9f0a-8d1976bc4028/secrets.json`

**Команди ініціалізації та перевірки:**
```bash
# 1. Запис секретного ключа для локальної розробки
dotnet user-secrets set "Integration:ApiKey" "local-dev-key" --project src/Cli

# 2. Перегляд списку зареєстрованих секретів
dotnet user-secrets list --project src/Cli

# 3. Перевірка статусу git (має залишатися абсолютно чистим!)
git status
```

---

### 4. Приклади запуску та демонстрації

```bash
# 1. Експорт замовлень за замовчуванням
dotnet run --project src/Cli -f net10.0 -- export
# Очікуваний вивід:
# Файл даних : <шлях>/src/Cli/bin/Debug/net10.0/data/orders.json
# ApiKey: присутній, довжина 13
# Експортовано 3 записи.

# 2. Імпорт збережених замовлень:
dotnet run --project src/Cli -f net10.0 -- import

# 3. Запуск з кореня репозиторію та з каталогу src/Cli (шлях до даних однаковий):
cd c:\Users\LENOVO\lnu\3_course\1_semester\cross-platform\lab\CrossApp
dotnet run --project src/Cli -f net10.0 -- export

cd src\Cli
dotnet run -f net10.0 -- export
cd ..\..

# 4. Перевизначення каталогу через спрощену змінну CROSSAPP_DATA_PATH:
# PowerShell:
$env:CROSSAPP_DATA_PATH = "C:\Users\LENOVO\crossapp_custom_data"
dotnet run --project src/Cli -f net10.0 -- export
$env:CROSSAPP_DATA_PATH = $null

# Bash:
# export CROSSAPP_DATA_PATH="/tmp/crossapp_data"
# dotnet run --project src/Cli -f net10.0 -- export
# unset CROSSAPP_DATA_PATH

# 5. Перевизначення вкладеного ключа конфігурації через префікс CROSSAPP_:
# PowerShell:
$env:CROSSAPP_Orders__Currency = "EUR"
$env:CROSSAPP_Storage__DataPath = "data_eur"
dotnet run --project src/Cli -f net10.0 -- export
$env:CROSSAPP_Orders__Currency = $null
$env:CROSSAPP_Storage__DataPath = $null
```

---

### 5. Таблиця нових Unit-тестів (`ConfigurationTests.cs`)

| Назва тесту | Що перевіряє | Очікуваний результат |
|---|---|---|
| `StorageOptions_ResolveFullPath_WithRelativePath_CombinesAndCreatesDirectory` | Розрахунок відносного шляху та автоматичне створення каталогу | Каталог створюється, повертається абсолютний нормалізований шлях |
| `StorageOptions_ResolveFullPath_WithRootedPath_IgnoresBaseDirectory` | Розрахунок при передачі абсолютного (rooted) шляху | Базовий каталог ігнорується, повертається шлях з кореневого шляху |
| `AppOptions_HasExpectedDefaultValues` | Значення POCO-налаштувань за замовчуванням | `DataPath="data"`, `FileName="orders.json"`, `Currency="UAH"`, `Level="Information"` |
| `FileOrderStore_WritesAtomically_AndCleansTmpFile` | Атомарність запису через `.tmp` + `File.Move` | Цільовий файл створено, `.tmp` файл повністю відсутній, дані коректні |
| `FileOrderStore_SavesCyrillicWithoutUnicodeEscapes` | Незмінність української кирилиці при JSON-серіалізації | Символи «Цегла рядова М100» збережено без `\u0426` |
| `FileOrderStore_WithCorruptedJson_ThrowsFormatExceptionWithHelpfulMessage` | Обробка дефектного JSON-файлу | Викидає зрозумілий `FormatException` із зазначенням шляху до файлу |

---

## Лабораторна робота №8: async/await, скасування, доменні винятки, unit-тести Core

### 1. Мета та правила реалізації
- **Справжній неблокуючий I/O:** читання файлу через `FileStream(..., useAsync: true)` та `JsonSerializer.DeserializeAsync`, без викликів `Task.Run` навколо синхронного читання.
- **Кооперативне скасування:** передача `CancellationToken` у всі асинхронні методи, перевірка `ct.ThrowIfCancellationRequested()` на кожній ітерації циклу обробки.
- **Класифікація винятків:**
  - `DomainException` — порушення бізнес-правил (наприклад, спроба додати позицію у вже підтверджене замовлення або підтвердити порожнє).
  - `ArgumentException` / `ArgumentOutOfRangeException` — помилки вхідних параметрів (порожні рядки, від'ємні ціни).
  - `OperationCanceledException` — очікуване скасування операції користувачем або за таймаутом (обробляється першим у ланцюжку `catch`).
- **Модульне тестування (xUnit):** проєкт `tests/Core.Tests` посилається лише на `Core` (повна ізоляція від `Cli`). Тести використовують `InMemoryOrderStore` та `TestData.cs` (без звернення до диска, крім тесту зі штучно пошкодженим файлом у `Path.GetTempPath()`).

### 2. Запуск та перевірка

```bash
# 1. Запуск усіх тестів (зелений результат: 10 тестів пройдено)
dotnet test

# 2. Очікувана поведінка імпорту (17 замовлень з data/orders.json)
dotnet run --project src/Cli -f net10.0 -- --file
# Вивід:
# Імпортовано записів: 17

# 3. Перевірка реакції на скасування (Ctrl+C або таймаут)
dotnet run --project src/Cli -f net10.0 -- lab8 --cancel
# Вивід:
# Імпорт скасовано.

# 4. Перевірка доменного винятку зі збереженням номера запису
dotnet run --project src/Cli -f net10.0 -- lab8 --bad
# Вивід:
# Дані відхилено: Запис 1: Ідентифікатор клієнта не може бути порожнім

# 5. Бонус 1: Потоковий імпорт (IAsyncEnumerable)
dotnet run --project src/Cli -f net10.0 -- lab8 --stream

# 6. Бонус 2: Зв'язані токени скасування (CreateLinkedTokenSource)
dotnet run --project src/Cli -f net10.0 -- lab8 --linked
```

### 3. Таблиця Unit-тестів (`tests/Core.Tests`)

| Файл тесту | Назва тесту | Що перевіряє | Очікуваний результат |
|---|---|---|---|
| `OrderServiceTests` | `Confirm_WhenEmpty_ThrowsDomainException_AndKeepsDraftState` | Спроба підтвердити порожнє замовлення | Викидає `DomainException`, стан лишається `Draft` |
| `OrderServiceTests` | `AddLine_ToConfirmedOrder_ThrowsDomainException_AndKeepsState` | Додавання рядка до підтвердженого замовлення | Викидає `DomainException`, сума `Total` не змінюється |
| `OrderServiceTests` | `AddLine_IncreasesOrderTotal` | Розрахунок підсумкової вартості замовлення | Сума збільшується на $Price \times Qty$, 2 рядки |
| `OrderServiceTests` | `CreateOrder_RaisesChangedExactlyOnce` | Підняття доменної події `Changed` | `Assert.Single(events)`, тип події `OrderCreated` |
| `ReportServiceTests` | `HighValueOrders_CountsOnlyAboveThreshold` | `[Theory]` звіт на порогах 20 000 і 30 000 грн | Повертає відповідно 2 та 0 замовлень |
| `ReportServiceTests` | `TopOrdersByTotal_IsDeterministic` | Топ-2 замовлення з однаковою вартістю | Детермінований порядок за `Id`: `ORD-T02`, `ORD-T04` |
| `ReportServiceTests` | `Totals_WhenEmpty_ReturnsZerosWithoutException` | Агрегація над порожньою колекцією | Повертає нулі без `InvalidOperationException` |
| `ImportTests` | `ImportAsync_WhenAlreadyCancelled_ThrowsOperationCanceledException` | Заздалегідь скасований `CancellationToken` | Викидає `OperationCanceledException` |
| `ImportTests` | `ImportAsync_WithCorruptedData_ThrowsDomainException_AndCleansTempFile` | Файл із дефектним записом у `GetTempPath()` | `DomainException` («Запис 1: ...»), файл видалено |
| `ImportTests` | `ImportAsync_Idempotency_SkipsExistingOrders` | Повторний імпорт однакового списку | Додано 1, при повторі 0 (дублікат пропущено) |
| `ImportTests` | `ImportAsync_WithLinkedToken_TimeoutCancelsOperation` | Бонус 2: зв'язані токени користувача і таймауту | Скасування за таймаутом без участі користувача |
| `ImportTests` | `LoadStreamAsync_YieldsOrdersAsynchronously` | Бонус 1: асинхронний потоковий перебір через `await foreach` | Отримано всі замовлення потоково без `List<T>` |

### 4. Додаткові завдання (Бонуси на максимальний бал)

1. **Потоковий імпорт (`IAsyncEnumerable<Order>`):**  
   Реалізовано метод `LoadStreamAsync` у `JsonOrderImporter.cs` із передачею токена скасування через атрибут `[EnumeratorCancellation]`. Споживання даних у `OrderService.ImportStreamAsync` відбувається через конструкцію `await foreach`. Це дозволяє обробляти великі масиви даних «на льоту» без виділення пам'яті під монолітний `List<Order>`.
2. **Зв'язані токени (`CancellationTokenSource.CreateLinkedTokenSource`):**  
   У CLI та тесті `ImportAsync_WithLinkedToken_TimeoutCancelsOperation` продемонстровано об'єднання токена скасування користувача (`userToken`) та токена таймауту (`timeoutToken`). Спеціальний тест доводить, що скасування відбувається саме внаслідок закінчення часового ліміту (`timeoutCts.IsCancellationRequested == true`), тоді як `userCts.IsCancellationRequested == false`.
3. **Звіт про хід імпорту (`IProgress<int>`):**  
   Контракт `ImportAsync` підтримує передачу об'єкта `IProgress<int>`. Підписник у CLI (`ConsoleProgressReporter`) візуалізує реальний анімований поступ імпорту у консолі: `[████████████████████] 100%`.
4. **Збір звіту про покриття коду (`XPlat Code Coverage`):**  
   У проєкті `tests/Core.Tests` підключено пакет `coverlet.collector`. Команда:
   ```bash
   dotnet test --collect:"XPlat Code Coverage"
   ```
   формує файл покриття `coverage.cobertura.xml`. Ключова доменна логіка, інваріанти замовлень, аналітичні звіти та асинхронні операції покриті на >85%. Непокритими залишено лише тестовий генератор `SampleData.cs` та допоміжний вивід CLI, що є стандартом індустрії для модульних тестів ядра.

---

## Лабораторна робота №7: Звіти через LINQ

### 1. Мета та правила реалізації
Реалізовано аналітичну обробку колекції доменних об’єктів за допомогою **LINQ to Objects**.
* **Ізоляція логіки:** Уся логіка формування звітів розміщена виключно в `Core` (`ReportService.cs`).
* **Чистота клієнта:** У `Program.cs` та класах CLI немає жодного оператора `Where`, `GroupBy` чи `Select` — лише виклик сервісу та форматування виводу.
* **Чисті функції:** Методи `ReportService` приймають `IEnumerable<Order>` та повертають `IReadOnlyList<DTO>`, що робить їх повністю незалежними від сховищ і легкими для тестування.
* **Відсутність витоку сутностей:** Звіти повертають незмінні `record`-DTO (`OrderReportLine`, `GroupSummary`, `CustomerOrdersSummary`), ізолюючи внутрішню структуру агрегату `Order`.

### 2. Аналітичні запити (`ReportService.cs`)

| Назва методу | Питання бізнес-аналітики | Використані оператори LINQ | Повертає |
|---|---|---|---|
| `HighValueOrders` | Які замовлення перевищують встановлений поріг суми ($threshold$)? | `Where` $\rightarrow$ `OrderBy` $\rightarrow$ `ThenBy` $\rightarrow$ `Select` $\rightarrow$ `ToList` | `IReadOnlyList<OrderReportLine>` |
| `TopOrdersByTotal` | Які топ-N замовлень мають найбільшу вартість? | `TopBy` / `OrderByDescending` $\rightarrow$ `ThenBy` $\rightarrow$ `Take` $\rightarrow$ `Select` $\rightarrow$ `ToList` | `IReadOnlyList<OrderReportLine>` |
| `ByStatus` | Як розподіляються замовлення за статусами (кількість та сума)? | `GroupBy` $\rightarrow$ `Select` $\rightarrow$ `OrderByDescending` $\rightarrow$ `ToList` | `IReadOnlyList<GroupSummary>` |
| `Totals` | Які загальні підсумки: кількість замовлень, загальна сума, середній та максимальний чек? | `ToList` $\rightarrow$ `Count`, `Sum`, `Average`, `Max` | Іменований кортеж `(int, decimal, decimal, decimal)` |
| `CustomerOrdersReport` | Зведений звіт по клієнтах: скільки замовлень оформив кожен клієнт і на яку суму? | `GroupJoin` $\rightarrow$ `Select` $\rightarrow$ `OrderByDescending` $\rightarrow$ `ThenBy` $\rightarrow$ `ToList` | `IReadOnlyList<CustomerOrdersSummary>` |

### 3. Дослідження відкладеного виконання (Deferred Execution)

У `Lab7Demo.cs` реалізовано практичний дослід відкладеного виконання запитів:
```csharp
var localSource = items.Take(5).ToList();
var deferredQuery = localSource.Where(o => o.Total > 10000m); // Запит лише оголошено

Console.WriteLine(deferredQuery.Count()); // 1) Перший виклик: результат = 3

var newOrder = Order.Create("ORD-TMP-999", "CUST-999");
newOrder.AddLine("P-001", "Ноутбук", 32500m, 1);
localSource.Add(newOrder); // Зміна вихідного джерела даних

Console.WriteLine(deferredQuery.Count()); // 2) Другий виклик: результат = 4

var snapshot = deferredQuery.ToList(); // Матеріалізація (фіксація результату в пам'яті)
localSource.Clear();                   // Очищення джерела
Console.WriteLine(snapshot.Count);     // 3) У знімку залишилося = 4
```

**Пояснення результатів:**
1. Рядок `localSource.Where(...)` **не виконує фільтрацію одразу** в момент створення, а лише будує дерево викликів ітератора (`IEnumerable<Order>`).
2. Перший виклик `deferredQuery.Count()` змушує ітератор пройтися по колекції й повертає `3`.
3. Після додавання нового замовлення в `localSource` повторний виклик `deferredQuery.Count()` повторно запускає обхід джерела, «бачить» щойно доданий елемент і повертає вже `4`. Це доводить, що запит обчислюється щоразу заново над актуальним станом колекції.
4. Виклик `.ToList()` матеріалізує послідовність у новий список `snapshot`. Після цього очищення вихідного списку `localSource.Clear()` не впливає на зафіксований знімок — `snapshot.Count` стабільно дорівнює `4`.

### 4. Обробка порожнього джерела (Zero-Elements Safety)
Стандартні агрегатні функції LINQ `Average()` та `Max()` генерують фатальний виняток `InvalidOperationException` («Sequence contains no elements»), якщо колекція порожня.  
У методі `ReportService.Totals` реалізовано перевірку:
```csharp
var list = source.ToList();
return list.Count == 0
    ? (0, 0m, 0m, 0m)
    : (list.Count, list.Sum(o => o.Total), list.Average(o => o.Total), list.Max(o => o.Total));
```
При передачі `Enumerable.Empty<Order>()` метод повертає нульові значення без аварійного завершення програми.

### 5. Додаткові завдання (Бонуси на максимальний бал)

1. **`GroupJoin` двох колекцій (`CustomerOrdersReport`):**  
   З'єднання колекцій `Customer` та `Order` за спільним полем `CustomerId`. Працює як LEFT OUTER JOIN у SQL: клієнти, які ще не зробили жодного замовлення, не губляться, а виводяться з показниками `0 замовлень | 0.00 грн`.
2. **Власний метод-розширення `TopBy<T, TKey>` (`LinqExtensions.cs`):**  
   Створено метод розширення для `IEnumerable<T>`, який інкапсулює шаблон `OrderByDescending(key).Take(count)`. Використано у двох незалежних аналітичних звітах: `TopOrdersByTotal` (топ замовлень) та `TopCustomersReport` (топ покупців).
3. **Query Syntax (`from ... group ... into ...`):**  
   Метод `ByStatusQuerySyntax` реалізує групування декларативним SQL-подібним синтаксисом C#:
   ```csharp
   var query = from o in source
               group o by o.Status.ToString() into g
               orderby g.Sum(x => x.Total) descending
               select new GroupSummary(g.Key, g.Count(), g.Sum(x => x.Total));
   ```
   *Порівняння:* Query Syntax є більш наочним та читабельним для операцій із багатьма з'єднаннями та групуваннями (`into g`), тоді як Method Syntax краще підходить для послідовних лінійних конвеєрів (`Where -> Select -> Take`).
4. **Бенчмарк `Stopwatch` на 100 000 замовлень (Streaming vs Intermediate `ToList`):**  
   Порівняно час фільтрації та вибірки перших 10 замовлень на 100 000 згенерованих об'єктах:
   * **Потоковий LINQ (без проміжного `ToList`):** `0.488 мс` (завдяки лінивому виконанню `Take(10)` зупиняє сканування одразу після знаходження 10 записів, не перебираючи решту 99 980 елементів);
   * **З проміжним `ToList()` посередині конвеєра:** `33.289 мс` (змушений проітерувати всі 100 000 елементів, виділити пам'ять під новий список на ~95 000 об'єктів і навантажити Garbage Collector);
   * **Висновок:** Потоковий конвеєр LINQ без передчасної матеріалізації працює у **~68.2x разів швидше**.

---

## Лабораторна робота №6: Доменні події, делегати Action/Func, лямбди

### 1. Мета та архітектурне призначення
Реалізовано механізм сповіщення зовнішнього коду про зміну стану доменної моделі без створення зворотної залежності `Core` від деталей виведення (консолі, логерів, файлів). `Core` виступає видавцем (Publisher), а `Cli` — підписником (Subscriber) у патерні **Observer**.

### 2. Опис доменних подій (`src/Core/Events`)
* **`OrderChangeKind` (enum):** `OrderCreated`, `LineAdded`, `OrderConfirmed`, `OrderCancelled`.
* **`OrderChangedEventArgs` (`EventArgs`):** містить незмінний зліпок даних зміни: `OrderId`, `Kind`, `LinesCount`, `Total`, `At` (`DateTimeOffset.Now`).

### 3. Оголошення та підняття події в `OrderService`
* Оголошення події на базі стандартного делегата .NET `EventHandler<T>`:
  ```csharp
  public event EventHandler<OrderChangedEventArgs>? Changed;
  ```
* Безпечне підняття через приватний метод та умовний оператор `?.Invoke`:
  ```csharp
  private void OnChanged(Order order, OrderChangeKind kind)
  {
      Changed?.Invoke(this, new OrderChangedEventArgs(order.Id, kind, order.Lines.Count, order.Total));
      _sink?.Notify($"Подія {kind} для замовлення #{order.Id} (сума: {order.Total:F2} грн)");
  }
  ```
* **Захист інваріантів:** виклик `OnChanged` розташовано **суворо після** збереження в `_store.Add()` / `_store.Update()`. Якщо операція викидає виняток, подія **не надсилається**.

### 4. Підписники, замикання та відписка в `Cli`
1. **Підписник №1 (іменований метод `OnOrderChanged`)**: виводить форматований рядок у консоль і дописує його у `logs/app.log`.
2. **Підписник №2 (лямбда із замиканням)**: `service.Changed += (_, _) => changes++;` (рахує кількість подій через замикання над змінною `changes`).
3. **Керована відписка (`-=`)**: `service.Changed -= OnOrderChanged;` успішно відписує логер. Подальші операції не пишуться в консоль і файл, але лічильник подій від другого підписника продовжує зростати.
4. **Сценарій відмови**: помилка при спробі підтвердити неіснуюче замовлення перехоплюється `try/catch`. Подія не піднімається, лічильник не змінюється.

---

## Лабораторна робота №5: Сховище даних, Dependency Injection (ручне), інверсія залежностей (DIP)

### 1. Домен, агрегат та інтерфейс сховища
- **Предметна область**: Замовлення (`Order`, `OrderLine`, `Customer`, `Product`).
- **Корінь агрегату**: `Order`.
- **Контракт сховища (`IOrderStore`)**: `List()`, `GetById(id)`, `Find(predicate)`, `Add(item)`, `Update(item)`, `Remove(id)`.

### 2. Реалізації сховища
1. **`InMemoryOrderStore` (у пам'яті):** на базі `Dictionary<string, Order>` з нечутливим до регістру порівнянням `OrdinalIgnoreCase`. Початкові дані — `SampleData.Orders()`.
2. **`FileOrderStore` (файлове сховище JSON):** збереження у `data/orders.json`, in-memory кеш `_cache`, ліниве завантаження `EnsureLoaded()` та скидання на диск через `Flush()`.
3. **`CachingOrderStore` (декоратор кешування):** патерн Decorator, кешує запити, підраховує `CacheHits` та `CacheMisses`.
4. **`OrderStoreFactory` (фабрика сховищ):** патерн Simple Factory, інкапсулює правила створення залежно від аргументів CLI (`--file`, `--cached`).

---

## Лабораторна робота №4: Доменна модель, інваріанти та інкапсуляція

### Сутності та Агрегат
- `Order` — корінь агрегату. Керує списком `_lines` (`IReadOnlyList<OrderLine>`), статусом `OrderStatus` (`Draft`, `Confirmed`, `Cancelled`) та загальною сумою `Total`.
- `OrderLine` — рядок замовлення (ціна $\ge 0$, кількість $> 0$).
- `Product` — товар каталогу з методами оновлення цін і мапінгом DTO.
- `Customer` — сутність замовника з перевіркою валідності email.

Усі сутності спроєктовано за принципами **Rich Domain Model** із приватними конструкторами та фабричними методами `Create(...)`.

### Таблиця інваріантів та винятків

| Сутність / Метод | Інваріант (Бізнес-правило) | Тип винятку |
|---|---|---|
| `Order.Create` | Ідентифікатори `Id` та `CustomerId` обов'язкові | `ArgumentException` |
| `OrderLine.Create` | Ціна не може бути від'ємною ($Price \ge 0$) | `ArgumentOutOfRangeException` |
| `OrderLine.Create` | Кількість має бути більшою за нуль ($Quantity > 0$) | `ArgumentOutOfRangeException` |
| `Order.AddLine` | Додавання рядків дозволено лише у статусі `Draft` | `InvalidOperationException` |
| `Order.Confirm` | Заборонено підтверджувати порожнє замовлення | `InvalidOperationException` |
| `Order.Cancel` | Заборонено скасовувати вже підтверджене замовлення | `InvalidOperationException` |

---

## Лабораторна робота №3: Імпорт даних та DTO

- **Формат файлів**: CSV (роздільник `;`, UTF-8, `CultureInfo.InvariantCulture`) та JSON.
- **Pattern Matching у розборі рядка**: використання `switch expression` із патернами списків `["", _, ..]`, реляційними перевірками `{ Length: < 3 }` та охоронними умовами `when`.
- **Різнорідний імпорт**: автоматичне розпізнавання товарів (`P;...`) та клієнтів (`C;...`) у файлі `data/mixed.csv`.

---

## Лабораторні роботи №1–2: Налаштування середовища, Core + Cli, публікація

- **Архітектура**: розділення на незалежну бібліотеку `src/Core` та консольний клієнт `src/Cli`.
- **Multi-targeting**: одночасна підтримка `net8.0` та `net10.0` у бібліотеці `Core.csproj`.
- **Публікація**:
  - `Framework-dependent (FDD)`: малий розмір (~0.2 МБ), вимагає встановленого runtime.
  - `Self-contained (SCD)`: автономний виконуваний пакет під RID `win-x64` (~76.7 МБ).
  - `SCD + Single-file + Trimmed`: монолітний оптимізований файл (~12.3 МБ, економія 84%).
