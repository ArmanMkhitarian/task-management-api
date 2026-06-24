# PLAN

План реализации сервиса управления задачами. Решения — в [AI_DECISIONS.md](AI_DECISIONS.md),
правила — в [CLAUDE.md](CLAUDE.md). Перед каждым этапом — короткий план; после каждого — коммит.

## Этап 1 — Скелет решения

- `git`-структура уже есть (init, `.gitignore`, docs-коммит).
- `.sln` + 4 проекта: `TaskManagement.Api`, `.Application`, `.Infrastructure`, `.Tests`.
- Ссылки: `Api → Application`, `Infrastructure → Application`, `Api → Infrastructure`, `Tests → Api`.
- NuGet: Npgsql.EntityFrameworkCore.PostgreSQL, FluentValidation.AspNetCore, Swashbuckle,
  EFCore.Design, AspNetCore.HealthChecks (+ Testcontainers, xUnit, FluentAssertions в Tests).
- Сборка пустого решения зелёная.

## Этап 2 — Domain + EF

- Entity `Task`: Id (`Guid`), Title, Description, Status, Priority, CreatedAt, UpdatedAt, AssigneeEmail.
- Id — последовательный uuid (v7-style генерация на стороне приложения, не БД).
- Enums `TaskItemStatus` (New/InProgress/Review/Done), `TaskPriority` (Low/Medium/High/Critical).
- `AppDbContext` + `IEntityTypeConfiguration` по [схеме в AI_DECISIONS](AI_DECISIONS.md):
  - enum→string, длины (`title` 200, `description` 4000, `status`/`priority` 20, `assignee_email` 254);
  - snake_case-именование колонок;
  - индексы PK(`id`), `IX(assignee_email)`, `IX(created_at DESC)`;
  - `xmin` как concurrency-token (optimistic).
- `SaveChanges` override: проставление `CreatedAt`/`UpdatedAt` (UTC).
- Первая миграция `InitialCreate`.

## Этап 3 — Application

- DTO: `CreateTaskRequest`, `UpdateTaskRequest`, `UpdateTaskStatusRequest`, `TaskResponse`,
  `PagedResult<T>`, `TaskFilter` (status, priority, assigneeEmail, limit, offset).
- Маппинг руками (extension-методы).
- Интерфейсы: `ITaskRepository`, `IEmailSender`, `ITaskService`.
- `TaskService`: CRUD, список с фильтрами/пагинацией, смена статуса.
- `TaskStatusPolicy`: матрица переходов + понятная ошибка.
- Валидаторы FluentValidation на входные DTO.
- Доменные исключения (`NotFound`, `InvalidStatusTransition`).

## Этап 4 — Infrastructure

- `TaskRepository` поверх `AppDbContext`: запросы через `IQueryable`, проекция в DTO, пагинация в SQL.
- `LoggingEmailSender` — мок `IEmailSender` (пишет в лог).
- Регистрация сервисов (DI-расширения).

## Этап 5 — Api

- Контроллер `TasksController`: POST/GET(list)/GET(id)/PUT/PATCH status/DELETE.
- `HealthController` или `MapHealthChecks` (`/health` + `AddDbContextCheck`).
- Middleware ошибок → ProblemDetails (RFC 7807): валидация → 400, not found → 404, переход → 409.
- `JsonStringEnumConverter`, Swagger (XML-доки, примеры).
- `Program.cs`: DI, авто-apply миграций при старте.

## Этап 6 — Docker

- `Dockerfile` (multi-stage) для Api.
- `docker-compose.yml`: api + postgres, healthcheck БД, переменные окружения, том для данных.
- Проверка: `docker compose up` → доступный Swagger и `/health`.

## Этап 7 — Тесты

- xUnit + Testcontainers (Postgres) + `WebApplicationFactory`.
- Покрытие: CRUD, фильтры + пагинация, переходы статусов (валидные/невалидные → 409),
  валидация входных данных (400), отправка email при назначении.

## Конвенции процесса

- Этап = логически цельный шаг → отдельный коммит после завершения.
- Архитектурные изменения — только через обновление [AI_DECISIONS.md](AI_DECISIONS.md).
