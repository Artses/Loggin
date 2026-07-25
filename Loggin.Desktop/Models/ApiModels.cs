namespace Loggin.Desktop.Models;

public sealed record AuthResponse(string Token, string Name, string Email, string Role);
public sealed record CollectorDto(Guid Id, string Name, string Url);
public sealed record LogDto(Guid Id, string Path, Guid CollectorId);
public sealed record LogLineDto(int Order, DateTime TimeStamp, string Line);
public sealed record ApiError(string? Message);

public enum ConnectionStatus
{
    Offline,
    Connecting,
    Online,
    Reconnecting
}

public enum ThemePreference
{
    System,
    Light,
    Dark
}
