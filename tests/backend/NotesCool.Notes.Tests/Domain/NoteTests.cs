using FluentAssertions;
using NotesCool.Notes.Domain;
using NotesCool.Shared.Errors;
using System;
using Xunit;

namespace NotesCool.Notes.Tests.Domain;

public class NoteTests
{
    [Fact]
    public void Constructor_ShouldCreateNote_WhenValidArguments()
    {
        var note = new Note("owner1", "Test Title", "Test Content");

        note.OwnerId.Should().Be("owner1");
        note.Title.Should().Be("Test Title");
        note.Content.Should().Be("Test Content");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Constructor_ShouldThrowException_WhenOwnerIsInvalid(string ownerId)
    {
        var action = () => new Note(ownerId, "Title", "Content");

        action.Should().Throw<ApiException>().WithMessage("Owner is required.");
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Update_ShouldThrowException_WhenTitleIsInvalid(string title)
    {
        var note = new Note("owner1", "Valid Title", "Content");

        var action = () => note.Update(title, "New Content");

        action.Should().Throw<ApiException>().WithMessage("Note title is required.");
    }

    [Fact]
    public void Update_ShouldThrowException_WhenTitleIsTooLong()
    {
        var note = new Note("owner1", "Valid Title", "Content");
        var longTitle = new string('A', 201);

        var action = () => note.Update(longTitle, "New Content");

        action.Should().Throw<ApiException>().WithMessage("Note title must be 200 characters or fewer.");
    }

    [Fact]
    public void Update_ShouldUpdateTitleAndContent_WhenValid()
    {
        var note = new Note("owner1", "Old Title", "Old Content");

        note.Update("New Title", "New Content");

        note.Title.Should().Be("New Title");
        note.Content.Should().Be("New Content");
    }
}
