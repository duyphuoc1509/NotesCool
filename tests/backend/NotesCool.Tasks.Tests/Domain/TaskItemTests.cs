using FluentAssertions;
using NotesCool.Tasks.Domain;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;

namespace NotesCool.Tasks.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidArguments()
    {
        var dueDate = DateTimeOffset.UtcNow.AddDays(2);
        var task = new TaskItem("Test Task", "Description", dueDate, "owner1");

        task.OwnerId.Should().Be("owner1");
        task.Title.Should().Be("Test Task");
        task.Description.Should().Be("Description");
        task.DueDate.Should().Be(dueDate);
        task.Status.Should().Be(TaskStatus.Todo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenTitleIsInvalid(string title)
    {
        var action = () => new TaskItem(title!, "Description", null, "owner1");

        action.Should().Throw<ArgumentException>().WithMessage("Title is required*");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var task = new TaskItem("Title", "Description", null, "owner1");

        task.ChangeStatus(TaskStatus.InProgress);

        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_ShouldAllowReopeningDoneTask()
    {
        var task = new TaskItem("Title", "Description", null, "owner1");
        task.ChangeStatus(TaskStatus.Done);
        task.ChangeStatus(TaskStatus.Todo);

        task.Status.Should().Be(TaskStatus.Todo);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_ShouldThrowException_WhenTitleIsInvalid(string title)
    {
        var task = new TaskItem("Title", "Description", null, "owner1");

        var action = () => task.Update(title!, "Desc", null);

        action.Should().Throw<ArgumentException>().WithMessage("Title is required*");
    }
}
