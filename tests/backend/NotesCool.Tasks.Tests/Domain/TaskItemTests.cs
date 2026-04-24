using NotesCool.Tasks.Domain;
using NotesCool.Shared.Errors;
using Xunit;

namespace NotesCool.Tasks.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void StatusTransition_ToDone_Succeeds()
    {
        var t = new TaskItem("o1", "T", null);
        t.ChangeStatus(TaskItemStatus.Done);
        Assert.Equal(TaskItemStatus.Done, t.Status);
    }
    
    [Fact]
    public void StatusTransition_DoneToTodo_Throws()
    {
        var t = new TaskItem("o1", "T", null);
        t.ChangeStatus(TaskItemStatus.Done);
        Assert.Throws<ApiException>(() => t.ChangeStatus(TaskItemStatus.Todo));
    }
}
