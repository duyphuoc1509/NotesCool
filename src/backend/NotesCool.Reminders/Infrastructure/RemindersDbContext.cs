using Microsoft.EntityFrameworkCore;
using NotesCool.Reminders.Domain;

namespace NotesCool.Reminders.Infrastructure;

public class RemindersDbContext : DbContext
{
    public RemindersDbContext(DbContextOptions<RemindersDbContext> options) : base(options)
    {
    }

    public DbSet<ReminderItem> Reminders => Set<ReminderItem>();
    public DbSet<ReminderDeviceSync> DeviceSyncs => Set<ReminderDeviceSync>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<ReminderItem>(entity =>
        {
            entity.ToTable("ReminderItems");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.AccountId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.OwnerId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.TaskTitle)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.DueDateSourceTimezone)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Status)
                .HasConversion(
                    status => (int)status,
                    dbValue => Enum.IsDefined(typeof(ReminderStatus), dbValue)
                        ? (ReminderStatus)dbValue
                        : ReminderStatus.Pending);

            entity.HasQueryFilter(e => e.ArchivedAt == null);

            entity.HasIndex(e => e.TaskId);
            entity.HasIndex(e => e.AccountId);
            entity.HasIndex(e => e.OwnerId);
            entity.HasIndex(e => e.ReminderTimeUtc);
        });

        modelBuilder.Entity<ReminderDeviceSync>(entity =>
        {
            entity.ToTable("ReminderDeviceSyncs");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Platform)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.NativeCalendarEventId)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.Status)
                .HasConversion(
                    status => (int)status,
                    dbValue => Enum.IsDefined(typeof(ReminderDeviceSyncStatus), dbValue)
                        ? (ReminderDeviceSyncStatus)dbValue
                        : ReminderDeviceSyncStatus.Pending);

            entity.Property(e => e.FailureReason)
                .HasMaxLength(100);

            entity.HasQueryFilter(e => e.ArchivedAt == null);

            entity.HasIndex(e => e.ReminderItemId);
            entity.HasIndex(e => e.DeviceId);
            entity.HasIndex(e => new { e.ReminderItemId, e.DeviceId }).IsUnique();
        });
    }
}
