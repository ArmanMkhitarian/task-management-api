# AI_DECISIONS

Решения по точкам выбора. Дата: 2026-06-24.

- **Платформа:** .NET 8 (LTS), не 6 — 6 уже EOL, 8 поддерживается до ноя 2026.
- **СУБД:** PostgreSQL, не SQLite/MySQL — зрелый провайдер Npgsql, нативные uuid/timestamptz, честный health-check, де-факто стандарт в .NET.
- **ORM:** EF Core, Dapper отклонён — для одной сущности с проекцией в DTO EF даёт один SQL-запрос; ручной SQL не нужен.
- **Архитектура:** layered, 4 проекта (`Api`, `Application`, `Infrastructure`, `Tests`); Application без зависимости от EF/ASP.NET. Clean Architecture / Vertical Slice / MediatR отклонены — оверкилл для одной CRUD-сущности.
- **Маппинг:** DTO отдельно от entity, руками, не AutoMapper — для одной сущности явный маппинг читаемее и без «магии».
- **Валидация:** FluentValidation, не DataAnnotations — лучше для кросс-полей и понятных сообщений (требование ТЗ).
- **Enum:** строками в БД и JSON, не int — читаемость в БД и устойчивость к переупорядочиванию значений.
- **Удаление:** hard delete — soft delete не в требованиях, не усложняем без причины.
- **Имена типов:** `TaskItem` / `TaskItemStatus` (не `Task`/`TaskStatus`) — чтобы не конфликтовать с `System.Threading.Tasks`.
- **Генерация Id:** последовательный uuid (UUIDv7) в фабрике `TaskItem.Create` через статический helper `SequentialGuid`; абстракция `IGuidProvider` отклонена — лишний слой для детерминированной генерации.
- **Смена статуса:** только через `PATCH /api/tasks/{id}/status`; `PUT` обновляет редактируемые поля (Title, Description, Priority, AssigneeEmail) **без** статуса — жизненный цикл отделён от редактирования, нельзя случайно перескочить статус полным обновлением.
- **Доступ к данным:** `ITaskRepository` со специализированными методами (`GetByIdAsync`, `GetListAsync(TaskListFilter)`, `Add`/`Remove`/`SaveChangesAsync`); `IQueryable` не протекает в Application. Минусы (жёсткость репозитория при росте числа запросов, фильтры/сортировка живут в Infrastructure) пренебрежимы для одной сущности с 3 фильтрами; взамен — Application не зависит от EF и нет риска преждевременной материализации в памяти.
- **Переходы статусов:** валидируются через `TaskStatusPolicy` (в пути `PATCH /status`) — единая точка правила. Матрица:
  ```
  New → InProgress | InProgress → Review|New | Review → Done|InProgress | Done → терминал
  ```
  Запрещённый переход → 409 Conflict с явным сообщением (конфликт состояния ресурса). Тот же статус — no-op без email.
- **Версионирование API:** не закладываем — решение заказчика, нет потребности.
- **Email при назначении:** синхронный мок (`IEmailSender`, лог), Outbox/фон вне scope — мок мгновенный, очередь усложнила бы без пользы.
- **Миграции:** авто-apply при старте API — `docker compose up` работает из коробки для демо/проверки.
- **Тесты эндпоинтов:** Testcontainers (Postgres), не EF InMemory — честный прогон против реальной СУБД, InMemory не покрывает поведение БД.

## Схема таблицы `tasks`

| Колонка | Тип | NULL | Решение и обоснование |
|---|---|---|---|
| `id` | `uuid` | NOT NULL | PK; последовательный (v7-style), не bigint — нет enumeration ID, дружит с микросервисами; v7 (а не v4) ради локальности индекса |
| `title` | `varchar(200)` | NOT NULL | заголовок задачи, 200 с запасом |
| `description` | `varchar(4000)` | NULL | не `text` — лимит длины на уровне БД, а не только валидации |
| `status` | `varchar(20)` | NOT NULL | enum→string; макс. значение 10 симв., 20 с запасом |
| `priority` | `varchar(20)` | NOT NULL | enum→string |
| `assignee_email` | `varchar(254)` | NULL | 254 = лимит длины email по RFC 5321; NULL = не назначена |
| `created_at` | `timestamptz` | NOT NULL | UTC |
| `updated_at` | `timestamptz` | NOT NULL | UTC; при вставке = `created_at` |
| `xmin` | (system) | — | concurrency-token (optimistic) → 409; защита от lost update без доп. колонки |

- **Именование колонок** — snake_case (конвенция PostgreSQL).
- **Индексы:** PK(`id`), `IX(assignee_email)` (высокая селективность, частый фильтр), `IX(created_at DESC)` (сортировка/пагинация). На `status`/`priority` индексов нет — низкая кардинальность (4 значения), планировщик их проигнорирует; при необходимости композит `(status, created_at)`.
- **Сортировка по умолчанию:** `created_at DESC, id DESC` — `id` как тай-брейк для стабильной пагинации.
