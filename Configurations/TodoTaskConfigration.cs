using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Models;

namespace TaskManager.Data.Configurations
{
    public class TodoTaskConfiguration : IEntityTypeConfiguration<TodoTask>
    {
        public void Configure(EntityTypeBuilder<TodoTask> builder)
        {
            builder.ToTable("Tasks");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Title)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500); 

            builder.Property(t => t.Deadline)
                .IsRequired();

            builder.Property(t => t.Importance)
                .IsRequired()
                .HasDefaultValue(1);

            builder.Property(t => t.CreatedAt)
                .HasDefaultValueSql("GETDATE()");
        }
    }
}