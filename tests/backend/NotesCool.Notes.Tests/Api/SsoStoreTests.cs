using System;
using FluentAssertions;
using NotesCool.Api.Identity;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class SsoStoreTests
{
    [Fact]
    public void ValidateState_WithValidState_ReturnsTrue()
    {
        var store = new SsoStore();
        var state = store.CreateState("google", "https://localhost/callback");
        
        var result = store.ValidateAndConsumeState(state, "google", out var returnUrl);
        
        result.Should().BeTrue();
        returnUrl.Should().Be("https://localhost/callback");
    }

    [Fact]
    public void ValidateState_WithInvalidOrConsumedState_ReturnsFalse()
    {
        var store = new SsoStore();
        var state = store.CreateState("google", "https://localhost/callback");
        
        // Consume once
        store.ValidateAndConsumeState(state, "google", out _).Should().BeTrue();
        
        // Consume twice
        store.ValidateAndConsumeState(state, "google", out _).Should().BeFalse();
    }
}
