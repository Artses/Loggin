using Loggin.Desktop.Models;

namespace Loggin.Desktop.Services;

public sealed class AppSettings
{
    public string BackendUrl { get; set; } = "https://localhost:7001/";
    public int TimeoutSeconds { get; set; } = 15;
    public int RefreshIntervalSeconds { get; set; } = 5;
    public bool AutoReconnect { get; set; } = true;
    public bool StartWithWindows { get; set; }
    public bool StartMinimized { get; set; }
    public bool MinimizeToTray { get; set; } = true;
    public bool CloseToTray { get; set; } = true;
    public ThemePreference Theme { get; set; } = ThemePreference.System;
    public string PrimaryColor { get; set; } = "#6D5EF5";
    public string SecondaryColor { get; set; } = "#20C7B7";
    public string AccentColor { get; set; } = "#F59E0B";

    public static AppSettings Default() => new();
}
