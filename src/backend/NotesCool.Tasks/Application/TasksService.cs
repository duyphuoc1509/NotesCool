using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain;
using NotesCool.Tasks.Infrastructure;

namespace NotesCool.Tasks.Application;

public sealed class TasksService(TasksDbContext db)
{
    public async Task<PagedResult<TaskResponse>> ListAsync(string ownerId, TaskItemStatus? status, string? sort, PageRequest page, CancellationToken ct)
    {
        var q = db.Tasks.AsNoTracking().Where(t => t.OwnerId == ownerId && t.ArchivedAt == null);
        if (status.HasValue) q = q.Where(t => t.Status == status.Value);
        q = sort == "dueAt" ? q.OrderBy(t => t.DueAt) : q.OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt);
        var total = await q.CountAsync(ct);
        var items = await q.Skip(page.Skip).Take(page.Take).Select(t => ToResponse(t)).ToListAsync(ct);
        return new(items, page.Page, page.Take, total);
    }
    public async Task<TaskResponse> GetAsync(string ownerId, Guid id, CancellationToken ct) => ToResponse(await Owned(ownerId, id, ct));
    public async Task<TaskResponse> CreateAsync(string ownerId, CreateTaskRequest request, CancellationToken ct) { var t = new TaskItem(ownerId, request.Title, request.Description, request.Priority); t.Update(request.Title, request.Description, request.DueAt, request.Priority); db.Tasks.Add(t); await db.SaveChangesAsync(ct); return ToResponse(t); }
    public async Task<TaskResponse> UpdateAsync(string ownerId, Guid id, UpdateTaskRequest request, CancellationToken ct) { var t = await Owned(ownerId, id, ct); t.Update(request.Title, request.Description, request.DueAt, request.Priority); await db.SaveChangesAsync(ct); return ToResponse(t); }
    public async Task<TaskResponse> ChangeStatusAsync(string ownerId, Guid id, ChangeTaskStatusRequest request, CancellationToken ct) { var t = await Owned(ownerId, id, ct); t.ChangeStatus(request.Status); await db.SaveChangesAsync(ct); return ToResponse(t); }
    public async Task ArchiveAsync(string ownerId, Guid id, CancellationToken ct) { var t = await Owned(ownerId, id, ct); t.Archive(); await db.SaveChangesAsync(ct); }
    private async Task<TaskItem> Owned(string ownerId, Guid id, CancellationToken ct) => await db.Tasks.FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId && t.ArchivedAt == null, ct) ?? throw new ApiException("task_not_found", "Task was not found.", 404);
    private static TaskResponse ToResponse(TaskItem t) => new(t.Id, t.Title, t.Description, t.Status, t.Priority, t.DueAt, t.CreatedAt, t.UpdatedAt);
}
