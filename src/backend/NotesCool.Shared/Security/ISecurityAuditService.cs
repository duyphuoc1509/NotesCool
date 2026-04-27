using Microsoft.Extensions.Logging;

namespace NotesCool.Shared.Security;

public interface ISecurityAuditService
{
    void LogAuthEvent(string eventType, string userId, string? email, string? ipAddress, string? userAgent, object? metadata = null);
}

public sealed class SecurityAuditService(ILogger<SecurityAuditService> logger) : ISecurityAuditService
{
    private readonly ILogger<SecurityAuditService> _logger = logger;

    public void LogAuthEvent(string eventType, string userId, string? email, string? ipAddress, string? userAgent, object? metadata = null)
    {
        // For MVP, we log to ILogger which goes to console/container logs.
        // In production, this would also write to a structured audit table or external SIEM.
        _logger.LogInformation(
            "Security Audit Event: {EventType} | User: {UserId} | Email: {Email} | IP: {IpAddress} | Agent: {UserAgent} | Metadata: {@Metadata}",
            eventType, userId, email, ipAddress, userAgent, metadata);
    }
}

public static class SecurityAuditEvents
{
    public const string LoginSuccess = "LOGIN_SUCCESS";
    public const string LoginFailed = "LOGIN_FAILED";
    public const string Logout = "LOGOUT";
    public const string RefreshToken = "REFRESH_TOKEN";
    public const string SsoCallback = "SSO_CALLBACK";
    public const string SsoLink = "SSO_LINK";
    public const string SsoUnlink = "SSO_UNLINK";
}
