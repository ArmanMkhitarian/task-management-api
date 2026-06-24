using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManagement.Application.Domain;

namespace TaskManagement.Infrastructure.Persistence.Configurations;

public class TaskItemConfiguration : IEntityTypeConfiguration<TaskItem>
{
    public void Configure(EntityTypeBuilder<TaskItem> builder)
    {
        builder.ToTable("tasks");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasColumnName("description")
            .HasMaxLength(4000);

        builder.Property(t => t.Status)
            .HasColumnName("status")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.Priority)
            .HasColumnName("priority")
            .HasMaxLength(20)
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.AssigneeEmail)
            .HasColumnName("assignee_email")
            .HasMaxLength(254);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        // Оптимистичная блокировка через системный столбец PostgreSQL xmin — без отдельной колонки в таблице.
        // Shadow-свойство (не на сущности): EF использует его как concurrency-token.
        builder.Property<uint>("Version")
            .HasColumnName("xmin")
            .HasColumnType("xid")
            .ValueGeneratedOnAddOrUpdate()
            .IsConcurrencyToken();

        builder.HasIndex(t => t.AssigneeEmail)
            .HasDatabaseName("ix_tasks_assignee_email");

        // Дефолтная сортировка списка — created_at DESC; индекс в том же порядке.
        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_tasks_created_at")
            .IsDescending();
    }
}
