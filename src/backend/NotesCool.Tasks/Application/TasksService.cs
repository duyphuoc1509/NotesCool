using Microsoft.EntityFrameworkCore;
using NotesCool.Shared.Common;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Contracts;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using NotesCool.Tasks.Infrastructure;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Application;

public sealed class TasksService(TasksDbContext db)
{
    public async Task<PagedResult<TaskDto>> GetProjectTasksAsync(
        Guid projectId,
        string userId,
        TaskStatus? status,
        TaskPriority? priority,
        string? assigneeId,
        int page,
        int pageSize,
        CancellationToken ct)
    {
        await EnsureProjectMemberAsync(projectId, userId, ct);

        var query = db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .Where(t => t.ProjectId == projectId && t.ParentTaskId == null);

        if (status.HasValue)
        {
            if (status == TaskStatus.Archived)
            {
                query = query.IgnoreQueryFilters().Where(t => t.Status == TaskStatus.Archived);
            }
            else
            {
                query = query.Where(t => t.Status == status.Value);
            }
        }

        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }

        if (!string.IsNullOrEmpty(assigneeId))
        {
            query = query.Where(t => t.Assignees.Any(a => a.UserId == assigneeId && a.IsActive));
        }

        var totalItems = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var dtos = new List<TaskDto>();
        foreach (var item in items)
        {
            dtos.Add(await MapToDtoAsync(item, ct));
        }

        return new PagedResult<TaskDto>(dtos, page, pageSize, totalItems);
    }

    public async Task<TaskDto> GetTaskAsync(Guid id, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        return await MapToDtoAsync(task, ct);
    }

    public async Task<TaskDto> CreateTaskAsync(Guid projectId, CreateTaskRequest request, string userId, CancellationToken ct)
    {
        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == projectId, ct)
            ?? throw new ApiException("project_not_found", "Project not found.");

        if (project.IsArchived)
        {
            throw new ApiException("project_archived", "Cannot create task in archived project.");
        }

        await EnsureProjectMemberAsync(projectId, userId, ct);

        var task = new TaskItem(
            project.WorkspaceId,
            projectId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            userId);

        db.Tasks.Add(task);

        if (request.AssigneeUserIds != null)
        {
            foreach (var assigneeId in request.AssigneeUserIds)
            {
                await ValidateAndAddAssigneeAsync(task, assigneeId, userId, ct);
            }
        }

        db.TaskActivityLogs.Add(new TaskActivityLog(task.Id, "Created", null, null, userId));

        await db.SaveChangesAsync(ct);

        var createdTask = await db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .FirstAsync(t => t.Id == task.Id, ct);

        return await MapToDtoAsync(createdTask, ct);
    }

    public async Task<TaskDto> UpdateTaskAsync(Guid id, UpdateTaskRequest request, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        var oldTitle = task.Title;
        task.Update(request.Title, request.Description, request.Priority, request.DueDate, userId);

        if (oldTitle != task.Title)
        {
            db.TaskActivityLogs.Add(new TaskActivityLog(task.Id, "UpdatedTitle", oldTitle, task.Title, userId));
        }

        await db.SaveChangesAsync(ct);

        return await MapToDtoAsync(task, ct);
    }

    public async Task<TaskDto> ChangeTaskStatusAsync(Guid id, ChangeTaskStatusRequest request, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        var oldStatus = task.Status.ToString();
        task.ChangeStatus(request.Status, userId);

        db.TaskActivityLogs.Add(new TaskActivityLog(task.Id, "UpdatedStatus", oldStatus, task.Status.ToString(), userId));

        await db.SaveChangesAsync(ct);

        return await MapToDtoAsync(task, ct);
    }

    public async Task<TaskDto> ChangeTaskPriorityAsync(Guid id, ChangeTaskPriorityRequest request, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        var oldPriority = task.Priority.ToString();
        task.ChangePriority(request.Priority, userId);

        db.TaskActivityLogs.Add(new TaskActivityLog(task.Id, "UpdatedPriority", oldPriority, task.Priority.ToString(), userId));

        await db.SaveChangesAsync(ct);

        return await MapToDtoAsync(task, ct);
    }

    public async Task<TaskDto> SetTaskFavoriteAsync(Guid id, bool isFavorite, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        return await MapToDtoAsync(task, ct);
    }

    public async Task DeleteTaskAsync(Guid id, string userId, CancellationToken ct)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(t => t.Id == id, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        task.SoftDelete(userId);
        db.TaskActivityLogs.Add(new TaskActivityLog(task.Id, "Deleted", null, null, userId));

        await db.SaveChangesAsync(ct);
    }

    // SubTask APIs
    public async Task<List<TaskDto>> GetSubTasksAsync(Guid parentTaskId, string userId, CancellationToken ct)
    {
        var parentTask = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == parentTaskId, ct)
            ?? throw new ApiException("task_not_found", "Parent task not found.");

        await EnsureProjectMemberAsync(parentTask.ProjectId, userId, ct);

        var subtasks = await db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .Where(t => t.ParentTaskId == parentTaskId)
            .OrderBy(t => t.SortOrder)
            .ThenBy(t => t.CreatedAt)
            .ToListAsync(ct);

        var dtos = new List<TaskDto>();
        foreach (var st in subtasks)
        {
            dtos.Add(await MapToDtoAsync(st, ct));
        }
        return dtos;
    }

    public async Task<TaskDto> CreateSubTaskAsync(Guid parentTaskId, CreateTaskRequest request, string userId, CancellationToken ct)
    {
        var parentTask = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == parentTaskId, ct)
            ?? throw new ApiException("task_not_found", "Parent task not found.");

        // Rule: 1-level subtask only.
        if (parentTask.ParentTaskId.HasValue)
        {
            throw new ApiException("invalid_operation", "Cannot create subtask for a subtask. Only 1-level nesting supported.");
        }

        var project = await db.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == parentTask.ProjectId, ct);
        if (project == null || project.IsArchived)
        {
            throw new ApiException("project_archived", "Cannot create subtask in archived project.");
        }

        await EnsureProjectMemberAsync(parentTask.ProjectId, userId, ct);

        var subtask = new TaskItem(
            parentTask.WorkspaceId,
            parentTask.ProjectId,
            request.Title,
            request.Description,
            request.Priority,
            request.DueDate,
            userId,
            parentTaskId);

        db.Tasks.Add(subtask);

        if (request.AssigneeUserIds != null)
        {
            foreach (var assigneeId in request.AssigneeUserIds)
            {
                await ValidateAndAddAssigneeAsync(subtask, assigneeId, userId, ct);
            }
        }

        db.TaskActivityLogs.Add(new TaskActivityLog(subtask.Id, "CreatedSubTask", null, null, userId));
        db.TaskActivityLogs.Add(new TaskActivityLog(parentTaskId, "AddedSubTask", null, subtask.Title, userId));

        await db.SaveChangesAsync(ct);

        var createdSubtask = await db.Tasks
            .Include(t => t.Assignees)
            .AsNoTracking()
            .FirstAsync(t => t.Id == subtask.Id, ct);

        return await MapToDtoAsync(createdSubtask, ct);
    }

    // Assignee APIs
    public async Task AssignUserAsync(Guid taskId, string assigneeUserId, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        await ValidateAndAddAssigneeAsync(task, assigneeUserId, userId, ct);

        db.TaskActivityLogs.Add(new TaskActivityLog(taskId, "AssignedUser", null, assigneeUserId, userId));

        await db.SaveChangesAsync(ct);
    }

    public async Task RemoveAssigneeAsync(Guid taskId, string assigneeUserId, string userId, CancellationToken ct)
    {
        var task = await db.Tasks
            .Include(t => t.Assignees)
            .FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        var assignee = task.Assignees.FirstOrDefault(a => a.UserId == assigneeUserId && a.IsActive);
        if (assignee != null)
        {
            assignee.Deactivate();
            db.TaskActivityLogs.Add(new TaskActivityLog(taskId, "RemovedAssignee", assigneeUserId, null, userId));
            await db.SaveChangesAsync(ct);
        }
    }

    // Activity Logs
    public async Task<List<TaskActivityLogDto>> GetActivityLogsAsync(Guid taskId, string userId, CancellationToken ct)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId, ct)
            ?? throw new ApiException("task_not_found", "Task not found.");

        await EnsureProjectMemberAsync(task.ProjectId, userId, ct);

        return await db.TaskActivityLogs
            .AsNoTracking()
            .Where(l => l.TaskId == taskId)
            .OrderByDescending(l => l.CreatedAt)
            .Select(l => new TaskActivityLogDto(l.Id, l.TaskId, l.Action, l.OldValue, l.NewValue, l.OwnerId, l.CreatedAt))
            .ToListAsync(ct);
    }

    private async Task EnsureProjectMemberAsync(Guid projectId, string userId, CancellationToken ct)
    {
        var isMember = await db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == projectId && m.UserId == userId && m.IsActive, ct);

        if (!isMember)
        {
            throw new ApiException("access_denied", "You are not a member of this project.");
        }
    }

    private async Task ValidateAndAddAssigneeAsync(TaskItem task, string assigneeUserId, string currentUserId, CancellationToken ct)
    {
        // Rule: Only project members assignable.
        var isMember = await db.ProjectMembers
            .AsNoTracking()
            .AnyAsync(m => m.ProjectId == task.ProjectId && m.UserId == assigneeUserId && m.IsActive, ct);

        if (!isMember)
        {
            throw new ApiException("invalid_assignee", $"User {assigneeUserId} is not a member of this project.");
        }

        var existing = task.Assignees.FirstOrDefault(a => a.UserId == assigneeUserId);
        if (existing == null)
        {
            db.TaskAssignees.Add(new TaskAssignee(task.Id, assigneeUserId, currentUserId));
        }
        else if (!existing.IsActive)
        {
            existing.Reactivate();
        }
    }

    private async Task<TaskDto> MapToDtoAsync(TaskItem t, CancellationToken ct)
    {
        var progress = await CalculateProgressAsync(t.Id, ct);
        return new TaskDto(
            t.Id,
            t.WorkspaceId,
            t.ProjectId,
            t.ParentTaskId,
            t.Title,
            t.Description,
            false,
            t.Status,
            t.Priority,
            t.DueDate,
            t.SortOrder,
            t.OwnerId,
            t.CreatedAt,
            t.UpdatedAt,
            t.Assignees.Where(a => a.IsActive).Select(a => new TaskAssigneeDto(a.UserId, a.AssignedBy, a.AssignedAt)).ToList(),
            progress);
    }

    private async Task<SubTaskProgressDto> CalculateProgressAsync(Guid taskId, CancellationToken ct)
    {
        var subtasks = await db.Tasks
            .AsNoTracking()
            .Where(t => t.ParentTaskId == taskId)
            .Select(t => new { t.Status })
            .ToListAsync(ct);

        if (subtasks.Count == 0) return new SubTaskProgressDto(0, 0);

        return new SubTaskProgressDto(
            subtasks.Count,
            subtasks.Count(s => s.Status == TaskStatus.Done));
    }
}
