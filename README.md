# TripMind ✈️

**TripMind** е уеб приложение за планиране, търсене и резервиране на пътувания и полети, разработено като университетски проект по дисциплина **„Софтуерни технологии 2“** към Факултета по математика и информатика, ПУ "Паисий Хилендарски"

**Екип:** Team 10  
**Хранилище:** [ST2-2026-Team-9-TripMind](https://github.com/vanko0/ST2-2026-Team-9-TripMind)  
**Технологии:** C#, ASP.NET Core MVC, Razor Views, PostgreSQL (Supabase), Entity Framework Core (Npgsql), JavaScript, Bootstrap 5, Google Gemini REST API.

---

## Основни функционалности

- **Каталог и търсене на пътувания:** разглеждане на налични дестинации и филтриране на оферти през началния екран и списъка с пътувания (`Home/Index.cshtml`, `Trips/Index.cshtml`).
- **Пълен набор от CRUD операции за пътувания:**
  - **Create (Добавяне):** регистриране на ново пътуване чрез формата в `Views/Trips/Log.cshtml`.
  - **Read (Преглед и филтриране):** извличане на всички пътувания и филтриране чрез `TripsController` и `ITripRepository`.
  - **Update (Редактиране):** обновяване на детайлите за съществуващо пътуване през `Views/Trips/Update.cshtml`.
  - **Delete (Изтриване):** премахване на запис от базата данни чрез `TripsController` и `TripRepository`.
- **Резервационен поток и „Моите полети“ (My Flights):** резервиране на избрано пътуване чрез `Views/Trips/Book.cshtml` и `BookingFormViewModel.cs`, като статусът се записва в базата данни чрез булевото поле `IsBooked`.
- **AI туристически асистент (Google Gemini):** интерактивен плаващ чат уиджет (`Views/Shared/_ChatWidget.cshtml`), чрез който потребителите могат да задават въпроси за дестинации и полети в реално време.
- **Валидация на данните:** клиентска и сървърна проверка чрез Data Annotations, `BookingFormViewModel` и jQuery Unobtrusive Validation.

---

## MVC архитектура

Приложението следва архитектурния шаблон **Model–View–Controller (MVC)** с изнесен слой за достъп до данните (Repository) и външните услуги (Services).

| Слой | Основни компоненти в проекта | Отговорност |
| :--- | :--- | :--- |
| **Models & ViewModels** | `Trip.cs`, `BookingFormViewModel.cs`, `HomeIndexViewModelcs.cs`, `ChatModel.cs`, `ErrorViewModel.cs` | Домейн модел за пътуванията и модели за формите, филтрите и чата |
| **Views** | `Home/Index.cshtml`, `Trips/Index.cshtml`, `Trips/Book.cshtml`, `Trips/Log.cshtml`, `Trips/Update.cshtml` | Потребителски интерфейс, таблици и форми за CRUD и резервации |
| **Shared Views** | `_Layout.cshtml`, `_ChatWidget.cshtml`, `Error.cshtml`, `_ValidationScriptsPartial.cshtml` | Общ шаблон за навигация и вграден AI чат компонент |
| **Controllers** | `HomeController.cs`, `TripsController.cs`, `ChatController.cs` | Обработка на HTTP заявки, маршрутизация и координация на услугите |
| **Data & Repositories** | `TripMindDbContext.cs`, `ITripRepository.cs`, `TripRepository.cs` | Достъп до PostgreSQL базата в Supabase чрез Entity Framework Core |
| **Services** | `GeminiService.cs` (`IGeminiService`), `AppLogger.cs` | Комуникация с Google Gemini API и централизирано логване на събития |

### Поток на комуникация с AI асистента

    Потребител въвежда въпрос в чат уиджета (_ChatWidget.cshtml + chat.js)
        |
        v
    ChatController (обработва заявката чрез ChatModel)
        |
        v
    IGeminiService / GeminiService (изпраща HTTP заявка чрез HttpClient)
        |
        v
    Google Gemini API (генерира отговор от езиковия модел)
        |
        v
    Визуализиране на отговора в реално време в интерфейса (chat.js / chat.css)

---

## Design Patterns

В проекта са имплементирани следните шаблони за дизайн:

| Шаблон | Реализация в кода | Приложение |
| :--- | :--- | :--- |
| **Repository Pattern** | `ITripRepository.cs`, `TripRepository.cs` | Абстрахира достъпа до `TripMindDbContext` и изолира операциите с базата данни от `TripsController`. |
| **Singleton Pattern** | `Services/AppLogger.cs` | Осигурява единствена глобална инстанция за записване на системни събития и действия в приложението. |
| **Dependency Injection (DI)** | `Program.cs` (`AddScoped`, `AddHttpClient`, `AddDbContext`) | Инжектиране на зависимостите (`ITripRepository`, `IGeminiService`, `TripMindDbContext`) чрез вградения IoC контейнер. |
| **Adapter / Service Facade** | `IGeminiService`, `GeminiService.cs` | Скрива детайлите на HTTP комуникацията и JSON обработката към Google Gemini API зад чист C# интерфейс. |

---

## Покритие на изискванията по задание

| № | Изискване | Реализация в TripMind | Статус |
| :--- | :--- | :--- | :---: |
| **1** | **MVC архитектура** | Пълно разделение на `Models`, `Views` и `Controllers` в ASP.NET Core MVC. | ✅ |
| **2** | **Поне 5 Views / ViewControllers** | 3 контролера (`Home`, `Trips`, `Chat`) и 5 основни изгледа (`Home/Index`, `Trips/Index`, `Trips/Book`, `Trips/Log`, `Trips/Update`) + частичен изглед `_ChatWidget`. | ✅ |
| **3** | **Поне 3 Design Patterns** | **Repository**, **Singleton** и **Dependency Injection / Facade**. | ✅ |
| **4** | **CRUD операции в БД** | Добавяне (`Log`), четене (`Index`), редактиране (`Update`), резервиране (`Book`) и изтриване в PostgreSQL (Supabase). | ✅ |
| **5** | **Комуникация с ИИ модел** | Интеграция с отдалечен модел Google Gemini чрез `GeminiService.cs` и `ChatController.cs`. | ✅ |
| **6** | **Публично GitHub хранилище по шаблон** | `ST2-2026-Team-9-TripMind` | ✅ |

---

## Структура на проекта

    ST2-2026-Team-9-TripMind/
    ├── TripMind/
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
    │   │   ├── AppLogger.cs
    │   │   └── GeminiService.cs
    │   ├── Views/
    │   │   ├── Home/Index.cshtml
    │   │   ├── Shared/ (_Layout.cshtml, _ChatWidget.cshtml, Error.cshtml)
    │   │   └── Trips/ (Index.cshtml, Book.cshtml, Log.cshtml, Update.cshtml)
    │   ├── wwwroot/ (css/chat.css, css/site.css, js/chat.js, js/site.js, lib/)
    │   ├── appsettings.json
    │   ├── Program.cs
    │   ├── TripMind.csproj
    │   └── TripMind.sln
    ├── .gitignore
    ├── LICENSE
    └── README.md

---

## Инструкции за стартиране

### 1. Клониране на хранилището

    git clone https://github.com/vanko0/ST2-2026-Team-9-TripMind.git
    cd ST2-2026-Team-9-TripMind/TripMind

### 2. Конфигуриране на връзката с базата данни (Supabase) и Gemini API

Чувствителните настройки са изключени от публичното хранилище чрез `.gitignore`. В директорията `TripMind/` създайте файл `appsettings.Development.json` (или `appsettings.Local.json`) със следната структура:

    {
      "Logging": {
        "LogLevel": {
          "Default": "Information",
          "Microsoft.AspNetCore": "Warning"
        }
      },
      "ConnectionStrings": {
        "DefaultConnection": "Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=ВАШИЯТ_ПОТРЕБИТЕЛ;Password=ВАШАТА_ПАРОЛА;SSL Mode=Require;Trust Server Certificate=true"
      },
      "Gemini": {
        "ApiKey": "ВАШИЯТ_GEMINI_API_КЛЮЧ"
      }
    }

### 3. Прилагане на миграциите

Схемата на базата данни се управлява чрез Entity Framework Core миграции (`InitialCreate` и `AddIsBooked`). При необходимост от обновяване на базата изпълнете:

    dotnet ef database update

### 4. Стартиране на приложението

Отворете `TripMind.sln` във **Visual Studio** и стартирайте чрез бутона **https (F5)** или през терминала:

    dotnet run

---

## Екип (Team 10)

- **Ваня Гюзелева** (`vanko0`)
- **Инка Лешкова** (`inesleshkova7`)
- **Ивайла Хаджиева** (`ivailita`)
