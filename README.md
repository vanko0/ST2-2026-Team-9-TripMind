# TripMind ✈️

**TripMind** е уеб приложение за търсене, резервиране и управление на пътувания и полети с вграден ИИ туристически асистент. Разработено е като университетски проект по дисциплина **„Софтуерни технологии 2“** към Факултета по математика и информатика на ПУ „Паисий Хилендарски“.

**Екип:** Team 10  
**Хранилище:** [ST2-2026-Team-9-TripMind](https://github.com/vanko0/ST2-2026-Team-9-TripMind)  
**Технологии:** C# / .NET 8, ASP.NET Core MVC, Razor Views, PostgreSQL (Supabase), Entity Framework Core (Npgsql), JavaScript, Bootstrap 5, Google Gemini REST API, локален LLM (LM Studio / OpenAI-съвместим сървър).

---

## Съдържание

1. [Основни функционалности](#основни-функционалности)
2. [MVC архитектура](#mvc-архитектура)
3. [ИИ асистент и устойчивост на грешки](#ии-асистент-и-устойчивост-на-грешки)
4. [Design Patterns](#design-patterns)
5. [Покритие на изискванията по задание](#покритие-на-изискванията-по-задание)
6. [Структура на проекта](#структура-на-проекта)
7. [Инструкции за стартиране](#инструкции-за-стартиране)
8. [Конфигурация](#конфигурация)
9. [Екип](#екип)

---

## Основни функционалности

- **Каталог и търсене на оферти:** началният екран (`Home/Index.cshtml`) показва наличните оферти (`IsBooked = false`) и позволява филтриране по произход, дестинация и максимален бюджет. Падащите списъци се пълнят динамично от данните в базата.
- **Резервиране:** избор на оферта и попълване на формата в `Views/Trips/Book.cshtml` (брой пътници, дати, бюджет). Резервацията се записва като **ново пътуване** с `IsBooked = true`, а оригиналната оферта остава непокътната.
- **„My Flights“ / „My Trips“:** списък само с резервираните пътувания (`Trips/Index.cshtml`) с филтри по дестинация, произход, бюджет и стил на пътуване.
- **Редактиране:** промяна на данните за пътуване през `Views/Trips/Update.cshtml`.
- **Отказ на резервация:** изтриване на резервация от „My Flights“ (`Cancel`). Офертите никога не се изтриват.
- **Дневник на активността (Logs):** страница `Trips/Log`, която показва всичко, което приложението е направило от стартирането си (филтриране, резервации, откази, чат заявки).
- **ИИ туристически асистент:** плаващ чат уиджет (`_ChatWidget.cshtml` + `chat.js`), който познава запазените пътувания на потребителя и дава съвети за дестинации, бюджет и багаж. Историята на чата се пази в `sessionStorage` и оцелява при смяна на страниците.
- **Валидация:** клиентска и сървърна (Data Annotations, `BookingFormViewModel`, jQuery Unobtrusive Validation) плюс допълнителни проверки в контролера (дата в миналото, връщане преди заминаване).
- **Защита на формите:** `[ValidateAntiForgeryToken]` на всички POST действия.

---

## MVC архитектура

Приложението следва шаблона **Model–View–Controller** с изнесен слой за достъп до данните (Repository) и слой за външни услуги (Services).

| Слой | Основни компоненти | Отговорност |
| :--- | :--- | :--- |
| **Models** | `Trip.cs`, `BookingFormViewModel.cs`, `HomeIndexViewModelcs.cs`, `ChatModel.cs`, `ErrorViewModel.cs` | Домейн модел, модели за форми и филтри, DTO записи за чата (`ChatRequest`, `ChatResponse`, `ChatTurn`) |
| **Views** | `Home/Index`, `Trips/Index`, `Trips/Book`, `Trips/Update`, `Trips/Log` | Потребителски интерфейс |
| **Shared Views** | `_Layout.cshtml`, `_ChatWidget.cshtml`, `Error.cshtml`, `_ValidationScriptsPartial.cshtml` | Общ шаблон, навигация, чат уиджет |
| **Controllers** | `HomeController`, `TripsController`, `ChatController` | HTTP заявки, валидация, координация на услугите |
| **Data & Repositories** | `TripMindDbContext`, `ITripRepository`, `TripRepository` | Достъп до PostgreSQL (Supabase) чрез EF Core |
| **Services** | `FallbackAiService`, `GeminiService`, `LocalLlmService`, `CircuitBreaker`, `TripPromptBuilder`, `ChatHistory`, `AiMessages`, `AiServiceResult`, `AppLogger` | Комуникация с ИИ моделите, устойчивост на грешки, логване |
| **Configuration** | `ExternalApisOptions` | Типизирани настройки за външните ИИ услуги |

---

## ИИ асистент и устойчивост на грешки

Облачният Gemini API понякога връща грешки като `429` (изчерпана квота) или `503` (претоварване). TripMind е проектиран така, че **потребителят винаги получава отговор**, дори когато няма достъп до облака.

### Поток на заявката

```text
Потребител пише в чат уиджета (_ChatWidget.cshtml + chat.js)
        │  POST /api/chat
        ▼
ChatController              — валидира входа (непразен, до 500 символа)
        │
        ▼
FallbackAiService           — оркестратор (IGeminiAiService), подрежда доставчиците
        │
        ├──► [1] GeminiService  ── верига от Gemini модели (основен + резервни)
        │         │  всеки опит: собствен таймаут; цялата верига: общ бюджет
        │         ▼
        │     Google Gemini API
        │
        ├──► [2] LocalLlmService ── локален LLM (LM Studio, OpenAI-съвместим API)
        │
        └──► [3] Офлайн резерва  ── статично, безопасно съобщение (BG/EN)
        │
        ▼
ChatResponse { Reply, Provider, Notice, Degraded } → визуализиране в чата
```

Всеки доставчик е защитен от собствен **Circuit Breaker**. Редът може да се обърне с `UseLocalLlm = true` (първо локалният модел, облакът е резерва).

### Механизми за устойчивост

| Механизъм | Къде | Как работи |
| :--- | :--- | :--- |
| **Result Object** | `AiServiceResult` | Услугите не хвърлят изключения при мрежови/HTTP грешки, а връщат `Ok(...)`, `Fail(...)` или `Offline(...)` с метаданни (`Provider`, `Notice`, `IsOffline`). |
| **Circuit Breaker** | `CircuitBreaker.cs` | Три състояния – `Closed`, `Open`, `HalfOpen`. Потокобезопасен, с инжектируем часовник (удобно за тестове). Gemini: отваря се след 3 поредни неуспеха (охлаждане 30 сек.). Локален LLM: отваря се след 1 неуспех (охлаждане 120 сек.). В `HalfOpen` се пропуска само една пробна заявка. |
| **Таймаути** | `GeminiService`, `LocalLlmService` | 8 сек. на опит към модел и 14 сек. общ бюджет за Gemini; 30 сек. за локалния модел. Реализирани чрез свързани `CancellationTokenSource` (`HttpClient.Timeout` е безкраен). |
| **Верига от Gemini модели** | `GeminiService` | При `429`, `5xx`, `400`, таймаут или `404` се опитва следващият модел (всеки има собствена квота). Модел, върнал `404`, се прескача 15 минути. При невалиден ключ (`401/403/API_KEY_INVALID`) веригата спира веднага. |
| **Автоматичен fallback** | `FallbackAiService` | При неуспех на един доставчик се преминава към следващия. Потребителят вижда информативно съобщение (`Notice`), че отговорът е от резервния модел. |
| **Офлайн резерва** | `AiMessages` | Ако всички са недостъпни, се връща учтиво съобщение на езика на въпроса (български или английски) – никога суров текст на грешка. |
| **Отказ от потребителя** | всички услуги | `OperationCanceledException` от клиента не се счита за грешка на доставчика и не води до fallback към платения облак. |
| **Нормализиране на историята** | `ChatHistory` | История от браузъра (недоверен вход) се привежда във валиден вид: започва с `user`, ролите се редуват, текстовете се ограничават. Предотвратява `400` от Gemini. |
| **Сигурност на ключа** | `GeminiService` | API ключът се подава в хедър `x-goog-api-key`, а не в URL, за да не попада в логовете на `HttpClient`. |

### Контекст на асистента

`TripPromptBuilder` изгражда системните инструкции за двата модела и добавя до 20 пътувания от базата като контекст. Ако базата е недостъпна, чатът продължава да работи с общи съвети.

---

## Design Patterns

| Шаблон | Реализация | Приложение |
| :--- | :--- | :--- |
| **Repository** | `ITripRepository`, `TripRepository` | Скрива EF Core зад интерфейс; контролерите не работят директно с `DbContext`. |
| **Singleton** | `AppLogger` (`Lazy<T>`, частен конструктор); `GeminiCircuitBreaker`, `LocalLlmCircuitBreaker` (регистрирани като Singleton в DI) | Една глобална инстанция за логване; по един споделен прекъсвач на доставчик. |
| **Dependency Injection** | `Program.cs` (`AddScoped`, `AddSingleton`, `AddHttpClient`, `AddDbContext`, `Configure<T>`) | Всички зависимости се инжектират през вградения IoC контейнер. |
| **Facade / Adapter** | `FallbackAiService`, `GeminiService`, `LocalLlmService` | Контролерът вижда само `IGeminiAiService`; детайлите за HTTP, JSON и fallback са скрити. |
| **Result Object** | `AiServiceResult` | Явен резултат вместо изключения при очаквани грешки. |
| **Circuit Breaker** | `CircuitBreaker` и наследниците му | Спира заявките към счупен доставчик и ги възобновява след охлаждане. |
| **Options Pattern** | `ExternalApisOptions` | Типизирана конфигурация чрез `IOptions<T>`. |

---

## Покритие на изискванията по задание

| № | Изискване | Реализация в TripMind | Статус |
| :--- | :--- | :--- | :---: |
| **1** | MVC архитектура | Разделение на `Models`, `Views`, `Controllers` в ASP.NET Core MVC. | ✅ |
| **2** | Поне 5 Views | 3 контролера (`Home`, `Trips`, `Chat`) и изгледи `Home/Index`, `Trips/Index`, `Trips/Book`, `Trips/Update`, `Trips/Log` + частичен изглед `_ChatWidget`. | ✅ |
| **3** | Поне 3 Design Patterns | Repository, Singleton, Dependency Injection, Facade, Result Object, Circuit Breaker. | ✅ |
| **4** | CRUD операции в БД | Четене с филтри (`GetFilteredAsync`, `GetByIdAsync`), създаване на резервация (`BookAsync`), редакция (`UpdateAsync`), изтриване на резервация (`CancelBookingAsync`). | ✅ |
| **5** | Комуникация с ИИ модел | Google Gemini (облак) + локален LLM като резерва, през `FallbackAiService`. | ✅ |
| **6** | Публично GitHub хранилище по шаблон | `ST2-2026-Team-9-TripMind` | ✅ |

---

## Структура на проекта

```text
ST2-2026-Team-9-TripMind/
├── TripMind/
│   ├── Configuration/
│   │   └── ExternalApisOptions.cs
│   ├── Controllers/
│   │   ├── ChatController.cs
│   │   ├── HomeController.cs
│   │   └── TripsController.cs
│   ├── Data/
│   │   └── TripMindDbContext.cs
│   ├── Migrations/
│   │   ├── 20261009151536_InitialCreate.cs
│   │   ├── 20261028184317_AddIsBooked.cs
│   │   └── TripMindDbContextModelSnapshot.cs
│   ├── Models/
│   │   ├── BookingFormViewModel.cs
│   │   ├── ChatModel.cs
│   │   ├── ErrorViewModel.cs
│   │   ├── HomeIndexViewModelcs.cs
│   │   └── Trip.cs
│   ├── Repositories/
│   │   ├── ITripRepository.cs
│   │   └── TripRepository.cs
│   ├── Services/
│   │   ├── AiMessages.cs
│   │   ├── AiServiceResult.cs
│   │   ├── AppLogger.cs
│   │   ├── ChatHistory.cs
│   │   ├── CircuitBreaker.cs
│   │   ├── FallbackAiService.cs
│   │   ├── GeminiService.cs
│   │   ├── IGeminiAiService.cs      (IGeminiAiService + ILocalLlmService)
│   │   ├── LocalLlmService.cs
│   │   └── TripPromptBuilder.cs
│   ├── Views/
│   │   ├── Home/Index.cshtml
│   │   ├── Shared/ (_Layout, _ChatWidget, Error, _ValidationScriptsPartial)
│   │   └── Trips/ (Index, Book, Update, Log)
│   ├── wwwroot/
│   │   ├── css/ (site.css, chat.css)
│   │   ├── js/ (site.js, chat.js)
│   │   ├── images/destinations/ (Milan, Sofia, Paris, London)
│   │   └── lib/
│   ├── appsettings.json
│   ├── Program.cs
│   ├── TripMind.csproj
│   └── TripMind.sln
├── .gitignore
├── LICENSE
└── README.md
```

---

## Инструкции за стартиране

### Изисквания

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL база данни (проектът ползва Supabase)
- API ключ за Google Gemini
- *(по избор)* [LM Studio](https://lmstudio.ai/) или друг OpenAI-съвместим сървър за локалния резервен модел

### 1. Клониране на хранилището

```bash
git clone https://github.com/vanko0/ST2-2026-Team-9-TripMind.git
cd ST2-2026-Team-9-TripMind/TripMind
```

### 2. Конфигуриране на връзката с базата и Gemini

Чувствителните настройки не се съхраняват в хранилището (`appsettings.Development.json` и `appsettings.Local.json` са в `.gitignore`). Създайте `TripMind/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=<HOST>;Port=5432;Database=postgres;Username=<ПОТРЕБИТЕЛ>;Password=<ПАРОЛА>;SSL Mode=Require;Trust Server Certificate=true"
  },
  "ExternalApis": {
    "GeminiApiKey": "<ВАШИЯТ_GEMINI_API_КЛЮЧ>"
  }
}
```

Алтернативно ползвайте **user-secrets**:

```bash
dotnet user-secrets set "ExternalApis:GeminiApiKey" "<КЛЮЧ>"
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<CONNECTION_STRING>"
```

### 3. Прилагане на миграциите

```bash
dotnet ef database update
```

Схемата се управлява чрез миграциите `InitialCreate` и `AddIsBooked`.

### 4. (По избор) Локален резервен модел

1. Стартирайте LM Studio и заредете модел.
2. Включете локалния сървър (по подразбиране `http://localhost:1234/`).
3. Задайте името на модела в `ExternalApis:LocalLlmModel`.

Ако локалният сървър е изключен, приложението работи нормално – просто пропуска резервата.

### 5. Стартиране

Отворете `TripMind.sln` във **Visual Studio** и натиснете **https (F5)**, или от терминала:

```bash
dotnet run
```

---

## Конфигурация

Всички настройки са в секцията `ExternalApis` (`appsettings.json`) и се описват от `ExternalApisOptions`.

| Настройка | По подразбиране | Описание |
| :--- | :--- | :--- |
| `GeminiApiKey` | *(празен)* | API ключ за Gemini. **Не го записвайте в хранилището.** |
| `GeminiModel` | `gemini-3.6-flash` | Основен Gemini модел. |
| `GeminiFallbackModels` | `["gemini-3.5-flash-lite"]` | Резервни Gemini модели, опитвани по ред. |
| `GeminiTimeoutSeconds` | `8` | Таймаут на един опит към един модел. |
| `GeminiTotalBudgetSeconds` | `14` | Общ бюджет за цялата верига от Gemini модели. |
| `GeminiMaxOutputTokens` | `1024` | Максимум изходни токени (не под ~1000 заради „thinking“ моделите). |
| `GeminiCircuitBreakerFailureThreshold` | `3` | Брой поредни неуспеха, които отварят прекъсвача. |
| `GeminiCircuitBreakerCooldownSeconds` | `30` | Охлаждане на прекъсвача за Gemini. |
| `UseLocalLlm` | `false` | `true` – първо локалният модел, облакът е резерва. |
| `LocalLlmBaseUrl` | `http://localhost:1234/` | Адрес на локалния сървър. |
| `LocalLlmEndpoint` | `v1/chat/completions` | Път към chat completions API. |
| `LocalLlmModel` | `phi-2` | Име на локалния модел. |
| `LocalLlmApiKey` | *(празен)* | Незадължителен ключ за локалния сървър. |
| `LocalLlmTimeoutSeconds` | `30` | Таймаут за отговор от локалния модел. |
| `LocalLlmCircuitBreakerCooldownSeconds` | `120` | Охлаждане на прекъсвача за локалния модел. |

> ⚠️ Имената на Gemini моделите се променят с времето. Актуалният списък може да се види с `GET https://generativelanguage.googleapis.com/v1beta/models`.

---

## Екип

**Team 10**

- **Ваня Гюзелева** (`vanko0`)
- **Инка Лешкова** (`inesleshkova7`)
- **Ивайла Хаджиева** (`ivailita`)

Проектът е лицензиран съгласно файла [LICENSE](LICENSE).
