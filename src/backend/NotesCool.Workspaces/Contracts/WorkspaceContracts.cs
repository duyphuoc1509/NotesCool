using NotesCool.Workspaces.Domain;
using System;
using System.Collections.Generic;

namespace NotesCool.Workspaces.Contracts;

public sealed record WorkspaceDto(Guid Id, string Name, string Description, string OwnerId, DateTimeOffset CreatedAt, DateTimeOffset? UpdatedAt);

public sealed record CreateWorkspaceRequest(string Name, string Description);

public sealed record UpdateWorkspaceRequest(string Name, string Description);

public sealed record WorkspaceMemberDto(string UserId, WorkspaceRole Role, DateTime JoinedAtUtc);

public sealed record AddMemberRequest(string UserId, WorkspaceRole Role);

public sealed record UpdateMemberRoleRequest(WorkspaceRole Role);
