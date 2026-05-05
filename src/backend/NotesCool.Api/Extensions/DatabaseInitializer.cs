using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using NotesCool.Api.Auth;
using NotesCool.Notes.Infrastructure;
using NotesCool.Tasks.Infrastructure;
using NotesCool.Reminders.Infrastructure;
using NotesCool.Workspaces.Infrastructure;
using AppIdentityDbContext = NotesCool.Identity.Infrastructure.IdentityDbContext;

namespace NotesCool.Api.Extensions;

/// <summary>
/// Bootstraps the database schema for every module's DbContext when no EF Core
/// migrations are present. The check is idempotent and uses a marker table per
/// context, because <c>EnsureCreatedAsync</c> is database-wide and would skip
/// secondary contexts as soon as the first one creates the database.
/// </summary>
public static class DatabaseInitializer
{
    public static async Task EnsureSchemaAsync(this IServiceProvider services, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var sp = scope.ServiceProvider;
        var env = sp.GetRequiredService<IHostEnvironment>();
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseInitializer");

        // Skip DB bootstrap in test environment to avoid noise from missing postgres.
        if (env.IsEnvironment("Testing"))
        {
            return;
        }

        // Order matters: identity tables must exist before IdentityDataSeeder runs.
        await EnsureContextSchemaAsync<AppIdentityDbContext>(sp, "AspNetUsers", logger, ct, applyMigrationsIfAvailable: false);
        await EnsureContextSchemaAsync<AuthDbContext>(sp, "user_accounts", logger, ct);
        await EnsureContextSchemaAsync<NotesDbContext>(sp, "notes", logger, ct);
        await EnsureContextColumnAsync<NotesDbContext>(sp, "notes", "IsFavorite", "boolean NOT NULL DEFAULT FALSE", logger, ct);
        await EnsureContextSchemaAsync<TasksDbContext>(sp, "Tasks", logger, ct);
        await EnsureTasksLegacyTablesAsync(sp, logger, ct);
        await EnsureContextColumnAsync<TasksDbContext>(sp, "Tasks", "IsFavorite", "boolean NOT NULL DEFAULT FALSE", logger, ct);
        await EnsureContextSchemaAsync<RemindersDbContext>(sp, "ReminderItems", logger, ct);
        await EnsureContextSchemaAsync<WorkspacesDbContext>(sp, "workspaces", logger, ct);
    }

    private static async Task EnsureContextSchemaAsync<TContext>(
        IServiceProvider sp,
        string markerTable,
        ILogger logger,
        CancellationToken ct,
        bool applyMigrationsIfAvailable = true) where TContext : DbContext
    {
        var ctx = sp.GetRequiredService<TContext>();
        var providerName = ctx.Database.ProviderName ?? string.Empty;

        // Skip non-relational providers (e.g. InMemory used in tests).
        if (!providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            // Prefer migrations when present so incremental schema changes
            // (new tables/columns) are applied on existing databases.
            var hasMigrations = ctx.Database.GetMigrations().Any();

            if (applyMigrationsIfAvailable && hasMigrations)
            {
                var pendingMigrations = await ctx.Database.GetPendingMigrationsAsync(ct);
                if (pendingMigrations.Any())
                {
                    var appliedMigrations = await ctx.Database.GetAppliedMigrationsAsync(ct);
                    var markerExists = await TableExistsAsync(ctx, markerTable, ct);

                    // Legacy bootstrap case: tables already exist but EF migration history
                    // is empty. Running initial migrations would try to recreate tables.
                    if (!appliedMigrations.Any() && markerExists)
                    {
                        logger.LogWarning(
                            "Skipped migration apply for {Context}: marker table '{Marker}' exists but no applied migrations were found.",
                            typeof(TContext).Name,
                            markerTable);
                        return;
                    }

                    try
                    {
                        await ctx.Database.MigrateAsync(ct);
                        logger.LogInformation(
                            "Applied pending migrations for {Context}.",
                            typeof(TContext).Name);
                    }
                    catch (PostgresException ex) when (ex.SqlState == "42P07")
                    {
                        // Legacy databases can have pre-existing tables without
                        // migration history; don't fail startup in that case.
                        logger.LogWarning(
                            ex,
                            "Skipped migration apply for {Context} because a relation already exists ({SqlState}).",
                            typeof(TContext).Name,
                            ex.SqlState);
                    }
                }

                return;
            }

            var creator = (RelationalDatabaseCreator)ctx.Database.GetService<IDatabaseCreator>();

            if (!await creator.ExistsAsync(ct))
            {
                await creator.CreateAsync(ct);
            }

            if (!await TableExistsAsync(ctx, markerTable, ct))
            {
                await creator.CreateTablesAsync(ct);
                logger.LogInformation(
                    "Created schema for {Context} (marker table '{Marker}' was missing).",
                    typeof(TContext).Name, markerTable);
            }
        }
        catch (Exception ex)
        {
            // Don't fail startup; surface the error so it shows up in container logs.
            logger.LogError(ex, "Failed to ensure schema for {Context}.", typeof(TContext).Name);
        }
    }

    private static async Task EnsureContextColumnAsync<TContext>(
        IServiceProvider sp,
        string tableName,
        string columnName,
        string columnDefinition,
        ILogger logger,
        CancellationToken ct) where TContext : DbContext
    {
        var ctx = sp.GetRequiredService<TContext>();
        var providerName = ctx.Database.ProviderName ?? string.Empty;

        if (!providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            if (!await TableExistsAsync(ctx, tableName, ct))
            {
                return;
            }

            await ctx.Database.ExecuteSqlRawAsync($"ALTER TABLE \"{tableName}\" ADD COLUMN IF NOT EXISTS \"{columnName}\" {columnDefinition};", ct);
            logger.LogInformation("Ensured column {Column} exists on table {Table} for {Context}.", columnName, tableName, typeof(TContext).Name);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure column {Column} on table {Table} for {Context}.", columnName, tableName, typeof(TContext).Name);
        }
    }

    private static async Task EnsureTasksLegacyTablesAsync(
        IServiceProvider sp,
        ILogger logger,
        CancellationToken ct)
    {
        var ctx = sp.GetRequiredService<TasksDbContext>();
        var providerName = ctx.Database.ProviderName ?? string.Empty;
        if (!providerName.Contains("Npgsql", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            await ctx.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "Projects" (
                    "Id" uuid NOT NULL,
                    "WorkspaceId" uuid NOT NULL,
                    "Name" character varying(150) NOT NULL,
                    "Description" text NULL,
                    "OwnerId" character varying(100) NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NULL,
                    "ArchivedAt" timestamp with time zone NULL,
                    CONSTRAINT "PK_Projects" PRIMARY KEY ("Id")
                );
                CREATE INDEX IF NOT EXISTS "IX_Projects_WorkspaceId" ON "Projects" ("WorkspaceId");
                """,
                ct);

            await ctx.Database.ExecuteSqlRawAsync(
                """
                CREATE TABLE IF NOT EXISTS "ProjectMembers" (
                    "Id" uuid NOT NULL,
                    "ProjectId" uuid NOT NULL,
                    "UserId" text NOT NULL,
                    "Role" integer NOT NULL,
                    "AddedBy" text NULL,
                    "IsActive" boolean NOT NULL,
                    "OwnerId" text NOT NULL,
                    "CreatedAt" timestamp with time zone NOT NULL,
                    "UpdatedAt" timestamp with time zone NULL,
                    "ArchivedAt" timestamp with time zone NULL,
                    CONSTRAINT "PK_ProjectMembers" PRIMARY KEY ("Id")
                );
                CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProjectMembers_ProjectId_UserId" ON "ProjectMembers" ("ProjectId", "UserId");
                """,
                ct);

            logger.LogInformation("Ensured legacy Tasks project tables exist (Projects, ProjectMembers).");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to ensure legacy Tasks project tables.");
        }
    }

    private static async Task<bool> TableExistsAsync(DbContext ctx, string tableName, CancellationToken ct)
    {
        var conn = ctx.Database.GetDbConnection();
        if (conn.State != ConnectionState.Open)
        {
            await conn.OpenAsync(ct);
        }

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = """
            SELECT 1
            FROM information_schema.tables
            WHERE table_schema = 'public' AND table_name = @table
            LIMIT 1;
            """;

        var p = cmd.CreateParameter();
        p.ParameterName = "@table";
        p.Value = tableName;
        cmd.Parameters.Add(p);

        var result = await cmd.ExecuteScalarAsync(ct);
        return result is not null;
    }
}
