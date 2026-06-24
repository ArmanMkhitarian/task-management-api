# CLAUDE.md

Правила проекта для генерации кода. Сервис управления задачами (внутренний REST API).
Полный контекст решений — в [AI_DECISIONS.md](AI_DECISIONS.md).

## Стек

- .NET 8 (LTS), ASP.NET Core Web API (контроллеры).
- EF Core 8 + Npgsql, PostgreSQL.
- FluentValidation, Swagger (Swashbuckle), xUnit + Testcontainers.

## Структура слоёв

Прагматичный layered, 4 проекта. Зависимости направлены внутрь: `Api → Application ← Infrastructure`.

```
TaskManagement.Api             контроллеры, Program/DI, middleware, Swagger, health checks
TaskManagement.Application     entities, enums, DTO, сервисы, интерфейсы, валидаторы, маппинг
TaskManagement.Infrastructure  AppDbContext, EF-конфигурации, миграции, репозитории, email-мок
TaskManagement.Tests           xUnit + Testcontainers + WebApplicationFactory
```

- **Application не зависит от EF Core и ASP.NET Core.** Доступ к данным — через `ITaskRepository`.
- Инфраструктурные интерфейсы (`ITaskRepository`, `IEmailSender`) объявлены в Application, реализованы в Infrastructure.

## Конвенции

- **DTO отдельно от entity**, наружу entity не отдаём. Маппинг — руками (extension-методы), без AutoMapper.
- **Валидация — FluentValidation**, по валидатору на входной DTO. Понятные сообщения.
- **Ошибки — ProblemDetails (RFC 7807)** через единый middleware. 409 для запрещённого перехода статуса с явным сообщением.
- **Enum — строками** и в БД (`HasConversion<string>`), и в JSON (`JsonStringEnumConverter`).
- **Аудит-поля** `CreatedAt`/`UpdatedAt` — только в `SaveChanges` override (UTC), никогда из клиента.
- **Переходы статусов** — через `TaskStatusPolicy` (Application), применяется и в `PUT`, и в `PATCH /status`.
- **Запросы списка** — через `IQueryable` с проекцией в DTO; фильтры/пагинация до материализации (никаких `ToList()` раньше времени).
- **Email при назначении** — синхронно через `IEmailSender` при заполнении/смене `AssigneeEmail`.
- Время — всегда UTC. Удаление — hard delete.

## Процесс

- Перед каждым этапом — короткий план; реализация только после согласования.
- После каждого завершённого этапа — отдельный коммит (этап = логически цельный шаг из плана).
- Менять архитектурное решение — только обновив [AI_DECISIONS.md](AI_DECISIONS.md).

## Вне scope (НЕ делаем)

- Версионирование API.
- Soft delete.
- CQRS / MediatR / Vertical Slice.
- Outbox / фоновая отправка email.
- AutoMapper, Dapper.
