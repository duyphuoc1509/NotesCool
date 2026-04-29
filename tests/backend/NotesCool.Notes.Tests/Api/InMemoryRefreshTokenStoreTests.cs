using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using NotesCool.Api.Contracts;
using Xunit;

namespace NotesCool.Notes.Tests.Api;

public class InMemoryRefreshTokenStoreTests
{
    [Fact]
    public void Issue_ReturnsToken_AndSavesSessionWithExpiry()
    {
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var ttl = TimeSpan.FromDays(7);
        var store = new InMemoryRefreshTokenStore(timeProvider, ttl);

        var token = store.Issue("user-1", "user@example.com");

        token.Should().NotBeNullOrWhiteSpace();
        store.TryRevoke(token, out var session).Should().BeTrue();
        session.UserId.Should().Be("user-1");
        session.Email.Should().Be("user@example.com");
        session.CreatedAtUtc.Should().Be(timeProvider.GetUtcNow());
        session.ExpiresAtUtc.Should().Be(timeProvider.GetUtcNow().Add(ttl));
        session.IsRevoked.Should().BeFalse();
    }

    [Fact]
    public void TryRevoke_AfterExpiry_ReturnsFalse()
    {
        var timeProvider = new FakeTimeProvider();
        timeProvider.SetUtcNow(new DateTimeOffset(2024, 1, 1, 12, 0, 0, TimeSpan.Zero));
        var ttl = TimeSpan.FromHours(1);
        var store = new InMemoryRefreshTokenStore(timeProvider, ttl);
        var token = store.Issue("user-1", "user@example.com");

        // Move time past expiry
        timeProvider.Advance(TimeSpan.FromMinutes(61));

        store.TryRevoke(token, out _).Should().BeFalse();
    }

    [Fact]
    public void TryRevoke_Twice_ReturnsFalseSecondTime()
    {
        var store = new InMemoryRefreshTokenStore();
        var token = store.Issue("user-1", "user@example.com");

        store.TryRevoke(token, out _).Should().BeTrue();
        store.TryRevoke(token, out _).Should().BeFalse();
    }
}
