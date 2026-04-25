using FluentAssertions;
using NotesCool.Shared.Errors;
using NotesCool.Tasks.Domain;
using System;
using Xunit;
using TaskStatus = NotesCool.Tasks.Domain.TaskStatus;
using System;

namespace NotesCool.Tasks.Tests.Domain;

public class TaskItemTests
{
    [Fact]
    public void Constructor_ShouldCreateTask_WhenValidArguments()
    {
<<<<<<< HEAD
        var t = new TaskItem("T", null, null, "o1");
        t.ChangeStatus(TaskStatus.Done);
        Assert.Equal(TaskStatus.Done, t.Status);
=======
        var task = new TaskItem("owner1", "Test Task", "Description");

        task.OwnerId.Should().Be("owner1");
        task.Title.Should().Be("Test Task");
        task.Description.Should().Be("Description");
        task.Status.Should().Be(TaskItemStatus.Todo);
        task.Priority.Should().Be(TaskItemPriority.Medium);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenOwnerIsInvalid(string ownerId)
    {
        var action = () => new TaskItem(ownerId, "Title", "Content");

        action.Should().Throw<ApiException>().WithMessage("Owner is required.");
    }

    [Fact]
    public void ChangeStatus_ShouldUpdateStatus()
    {
        var task = new TaskItem("owner1", "Title", "Description");

        task.ChangeStatus(TaskItemStatus.InProgress);

        task.Status.Should().Be(TaskItemStatus.InProgress);
    }

    [Fact]
    public void ChangeStatus_ShouldThrowException_WhenReopeningDoneTask()
    {
        var task = new TaskItem("owner1", "Title", "Description");
        task.ChangeStatus(TaskItemStatus.Done);

        var action = () => task.ChangeStatus(TaskItemStatus.Todo);

        action.Should().Throw<ApiException>().WithMessage("Done tasks cannot be reopened in MVP V1.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_ShouldThrowException_WhenTitleIsInvalid(string title)
    {
        var task = new TaskItem("owner1", "Title", "Description");

        var action = () => task.Update(title, "Desc");

        action.Should().Throw<ApiException>().WithMessage("Task title is required.");
    }

    [Fact]
    public void Update_ShouldThrowException_WhenTitleIsTooLong()
    {
        var task = new TaskItem("owner1", "Title", "Description");
        var longTitle = new string('A', 201);

        var action = () => task.Update(longTitle, "Desc");

        action.Should().Throw<ApiException>().WithMessage("Task title must be 200 characters or fewer.");
>>>>>>> dev
    }
}
