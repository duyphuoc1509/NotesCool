using NotesCool.Tasks.Domain;
using NotesCool.Shared.Errors;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;
using System;

namespace NotesCool.Tasks.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void StatusTransition_ToDone_Succeeds()
    {
        var t = new TaskItem("T", null, null, "o1");
        t.ChangeStatus(TaskStatus.Done);
        Assert.Equal(TaskStatus.Done, t.Status);
    }
}
