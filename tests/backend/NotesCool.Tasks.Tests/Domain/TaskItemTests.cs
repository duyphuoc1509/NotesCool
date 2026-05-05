using FluentAssertions;
using NotesCool.Tasks.Domain.Entities;
using NotesCool.Tasks.Domain.Enums;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.Enums.TaskStatus;

namespace NotesCool.Tasks.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidArguments()
    {
        var dueDate = DateTimeOffset.UtcNow.AddDays(2);
        var task = new TaskItem(Guid.Empty, Guid.Empty, "Test Task",  "Description", TaskPriority.Medium,  dueDate,  "owner1");

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
        var action = () => new TaskItem(Guid.Empty, Guid.Empty, title!, "Description", TaskPriority.Medium, null, "owner1");

        action.Should().Throw<ArgumentException>().WithMessage("Title is required*");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var task = new TaskItem(Guid.Empty, Guid.Empty, "Title",  "Description", TaskPriority.Medium,  null,  "owner1");

        task.ChangeStatus(TaskStatus.InProgress, "tester");

        task.Status.Should().Be(TaskStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_ShouldAllowReopeningDoneTask()
    {
        var task = new TaskItem(Guid.Empty, Guid.Empty, "Title",  "Description", TaskPriority.Medium,  null,  "owner1");
        task.ChangeStatus(TaskStatus.Done, "tester");
        task.ChangeStatus(TaskStatus.Todo, "tester");

        task.Status.Should().Be(TaskStatus.Todo);
    }

    [Fact]
    public void Constructor_ShouldNotThrowException_WhenDescriptionIsNull()
    {
        var task = new TaskItem(Guid.Empty, Guid.Empty, "Title",  null, TaskPriority.Medium,  null,  "owner1");

        task.Description.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_ShouldThrowException_WhenTitleIsInvalid(string title)
    {
        var task = new TaskItem(Guid.Empty, Guid.Empty, "Title",  "Description", TaskPriority.Medium,  null,  "owner1");

        var action = () => task.Update(title!,  "Desc", TaskPriority.Medium,  null, "tester");

        action.Should().Throw<ArgumentException>().WithMessage("Title is required*");
    }
}
