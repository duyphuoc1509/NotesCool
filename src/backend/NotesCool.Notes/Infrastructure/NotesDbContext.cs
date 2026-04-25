using Microsoft.EntityFrameworkCore;
using NotesCool.Notes.Domain;

namespace NotesCool.Notes.Infrastructure;

public sealed class NotesDbContext(DbContextOptions<NotesDbContext> options) : DbContext(options)
{
    public DbSet<Note> Notes => Set<Note>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Note>(b =>
        {
            b.ToTable("notes"); b.HasKey(x => x.Id);
            b.Property(x => x.OwnerId).HasMaxLength(128).IsRequired();
            b.Property(x => x.Title).HasMaxLength(200).IsRequired();
            b.HasIndex(x => new { x.OwnerId, x.ArchivedAt });
        });
    }
}
