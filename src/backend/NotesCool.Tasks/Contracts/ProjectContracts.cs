using NotesCool.Tasks.Domain.Enums;

namespace NotesCool.Tasks.Contracts;

public sealed record ProjectDto(
    Guid Id,
    Guid WorkspaceId,
    string Name,
    string? Description,
    string OwnerId,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    bool IsArchived);

public sealed record CreateProjectRequest(string Name, string? Description);

public sealed record UpdateProjectRequest(string Name, string? Description);

public sealed record ProjectMemberDto(
    Guid ProjectId,
    string UserId,
    ProjectRole Role,
    string? AddedBy,
    bool IsActive,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);

public sealed record AddProjectMemberRequest(string UserId, ProjectRole Role);

public sealed record UpdateProjectMemberRoleRequest(ProjectRole Role);
