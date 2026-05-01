using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain;
using NotesCool.Tasks.Infrastructure;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

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
        var query = _dbContext.Tasks
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
        var task = new TaskItem(request.Title, request.Description, request.DueDate, ownerId);
        
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.Update(request.Title, request.Description, request.DueDate);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task<TaskDto> ChangeTaskStatusAsync(Guid id, ChangeTaskStatusRequest request, string ownerId, CancellationToken cancellationToken = default)
    {
        if (!Enum.IsDefined(typeof(TaskStatus), request.Status))
            throw new ApiException("invalid_task_status", "Invalid task status. Allowed values are Todo, InProgress, Done, Cancelled.", 400);

        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.ChangeStatus(request.Status);
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        
        return MapToDto(task);
    }

    public async Task DeleteTaskAsync(Guid id, string ownerId, CancellationToken cancellationToken = default)
    {
        var task = await GetTaskEntityAsync(id, ownerId, cancellationToken);
        
        task.Archive();
        
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<TaskItem> GetTaskEntityAsync(Guid id, string ownerId, CancellationToken cancellationToken)
    {
        var task = await _dbContext.Tasks
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
            task.Status,
            task.DueDate,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
