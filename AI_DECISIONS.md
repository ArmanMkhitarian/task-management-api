# AI_DECISIONS

Решения по точкам выбора. Дата: 2026-06-24.

- **Платформа:** .NET 8 (LTS) — не 6 (EOL).
- **СУБД:** PostgreSQL — не SQLite/MySQL.
- **ORM:** EF Core — Dapper отклонён.
- **Архитектура:** layered, 4 проекта (`Api`, `Application`, `Infrastructure`, `Tests`); Application без зависимости от EF/ASP.NET. Clean Architecture / Vertical Slice / MediatR — отклонены.
- **Маппинг:** DTO отдельно от entity, руками — не AutoMapper.
- **Валидация:** FluentValidation — не DataAnnotations.
- **Enum:** строками в БД и JSON — не int.
- **Удаление:** hard delete — soft delete вне scope.
- **Смена статуса:** отдельный `PATCH /api/tasks/{id}/status` в дополнение к `PUT`.
- **Переходы статусов:** валидируются через `TaskStatusPolicy` (в `PUT` и `PATCH`). Матрица:
  ```
  New → InProgress | InProgress → Review|New | Review → Done|InProgress | Done → терминал
  ```
  Запрещённый переход → 409 Conflict с явным сообщением. Тот же статус — no-op без email.
- **Версионирование API:** не закладываем.
- **Email при назначении:** синхронный мок (`IEmailSender`, лог) — Outbox/фон вне scope.
- **Миграции:** авто-apply при старте API.
- **Тесты эндпоинтов:** Testcontainers (Postgres) — не EF InMemory.
