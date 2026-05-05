using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Infrastructure;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Application;

public class TasksService
{
    private readonly TasksDbContext _dbContext;

    public TasksService(TasksDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<TaskDto>> GetTasksAsync(
        string ownerId,
        TaskStatus? status,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<TaskItem> query = _dbContext.Tasks;

        if (status == TaskStatus.Archived)
        {
            query = query.IgnoreQueryFilters();
        }

        query = query
            .AsNoTracking()
            .Where(t => t.OwnerId == ownerId);

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }

        var totalItems = await query.CountAsync(cancellationToken);

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TaskDto(
                t.Id,
                t.Title,
                t.Description,
                false, // IsFavorite removed
                t.Status,
                t.DueDate,
                t.CreatedAt,
                t.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new PagedResult<TaskDto>(tasks, page, pageSize, totalItems);
    }

    public async Task<TaskDto> GetTaskAsync(Guid id, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        return MapToDto(task);
    }

    public async Task<TaskDto> CreateTaskAsync(CreateTaskRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = new TaskItem(Guid.Empty, Guid.Empty, request.Title, request.Description, NotesCool.Tasks.Domain.Enums.TaskPriority.Medium, request.DueDate, ownerId);
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.Update(request.Title, request.Description, task.Priority, request.DueDate, ownerId);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task<TaskDto> ChangeTaskStatusAsync(Guid id, ChangeTaskStatusRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(TaskStatus), request.Status))
            throw new ApiException("invalid_task_status", "Invalid task status.", 400);

        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.ChangeStatus(request.Status, ownerId);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task<TaskDto> SetTaskFavoriteAsync(Guid id, bool isFavorite, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);

        // task.SetFavorite(isFavorite); // Removed from Domain model for now

        await _dbContext.SaveChangesAsync(cancellationToken);

        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(Guid id, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.SoftDelete(ownerId);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskItem> GetTaskEntityAsync(Guid id, string ownerId, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id && t.OwnerId == ownerId, cancellationToken);
            
        if (task == null)
            throw new ApiException("task_not_found", "Task not found", 404);
            
        return task;
    }

    private static TaskDto MapToDto(TaskItem task)
    {
        return new TaskDto(
            task.Id,
            task.Title,
            task.Description,
            false, // IsFavorite removed
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
