using NotesCool.Notes.Domain;
using NotesCool.Shared.Errors;
using Xunit;

namespace NotesCool.Notes.Tests.Domain;

public class NoteTests
{
    [Fact]
    public void Create_Valid_SetsProperties()
    {
        var n = new Note("o1", "Title", "Content");
        Assert.Equal("o1", n.OwnerId);
        Assert.Equal("Title", n.Title);
        Assert.Equal("Content", n.Content);
    }
    
    [Fact]
    public void Create_WithoutOwner_Throws()
    {
        Assert.Throws<ApiException>(() => new Note("", "Title", ""));
    }
}
