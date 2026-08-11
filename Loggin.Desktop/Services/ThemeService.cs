using System.Windows;
using System.Windows.Media;
using Loggin.Desktop.Models;
using Microsoft.Win32;

namespace Loggin.Desktop.Services;

public static class ThemeService
{
    private const string DangerColor = "#E5484D";

    public static void Apply(AppSettings settings)
    {
        var dark = settings.Theme == ThemePreference.Dark ||
                   settings.Theme == ThemePreference.System && IsWindowsDark();

        // Main palette
        Set("PrimaryBrush", settings.PrimaryColor);
        Set("PrimaryHoverBrush", Darken(settings.PrimaryColor, 0.08));
        Set("SecondaryBrush", settings.SecondaryColor);
        Set("AccentBrush", settings.AccentColor);
        Set("WindowBrush", dark ? "#0E1019" : "#F4F6FB");
        Set("SurfaceBrush", dark ? "#191C2A" : "#FFFFFF");
        Set("SurfaceAltBrush", dark ? "#232738" : "#EEF1F8");
        Set("SurfaceHoverBrush", dark ? "#2A3042" : "#E8EDF8");
        Set("TextBrush", dark ? "#F5F7FC" : "#172033");
        Set("MutedTextBrush", dark ? "#9BA3BA" : "#687189");
        Set("BorderBrush", dark ? "#2D3344" : "#DDE2EE");
        Set("DangerBrush", DangerColor);
        Set("DangerHoverBrush", Darken(DangerColor, 0.10));
        Set("SuccessBrush", dark ? "#34D399" : "#2DA44E");

        // Sidebar
        var sidebarBg = dark ? "#11121B" : "#15131B";
        Set("SidebarBrush", sidebarBg);
        Set("SidebarTextBrush", "#C9C5D6");
        Set("SidebarActiveBrush", settings.PrimaryColor);
        Set("SidebarHoverBrush", Darken(sidebarBg, 0.30));
        Set("SidebarHeaderBrush", dark ? "#F5F7FC" : "#FFFFFF");

        // Log Viewer — adapt to theme
        if (dark)
        {
            Set("LogLevelTrace", "#9CA3AF");
            Set("LogLevelDebug", "#60A5FA");
            Set("LogLevelInfo", "#34D399");
            Set("LogLevelWarning", "#FBBF24");
            Set("LogLevelError", "#F87171");
            Set("LogLevelFatal", "#EF4444");
            Set("LogLevelNone", "#6B7280");

            Set("LogRowError", "#450A0A");        // dark red bg
            Set("LogRowFatal", "#450A0A");
            Set("LogRowWarning", "#451A03");      // dark amber bg

            Set("LogSearchHighlight", "#713F12"); // dark amber highlight
            Set("LogTimeBrush", "#6B7280");
            Set("LogCollectorBadgeBrush", "#312E81");
            Set("LogCollectorBadgeText", "#C7D2FE");
        }
        else
        {
            Set("LogLevelTrace", "#6B7280");
            Set("LogLevelDebug", "#3B82F6");
            Set("LogLevelInfo", "#10B981");
            Set("LogLevelWarning", "#F59E0B");
            Set("LogLevelError", "#EF4444");
            Set("LogLevelFatal", "#DC2626");
            Set("LogLevelNone", "#9CA3AF");

            Set("LogRowError", "#FEF2F2");
            Set("LogRowFatal", "#FEF2F2");
            Set("LogRowWarning", "#FFFBEB");

            Set("LogSearchHighlight", "#FDE047");
            Set("LogTimeBrush", "#9CA3AF");
            Set("LogCollectorBadgeBrush", "#E0E7FF");
            Set("LogCollectorBadgeText", "#4F46E5");
        }
    }

    private static string Darken(string hex, double amount)
    {
        if (TryParseColor(hex, out var c))
        {
            var r = (byte)Math.Max(0, c.R * (1 - amount));
            var g = (byte)Math.Max(0, c.G * (1 - amount));
            var b = (byte)Math.Max(0, c.B * (1 - amount));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        return hex;
    }

    public static bool IsValidColor(string value) => TryParseColor(value, out _);

    private static bool TryParseColor(string value, out System.Windows.Media.Color color)
    {
        try
        {
            color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value);
            return true;
        }
        catch
        {
            color = default;
            return false;
        }
    }

    private static void Set(string key, string value)
    {
        var app = System.Windows.Application.Current;
        if (app is null) return;
        if (!TryParseColor(value, out var color)) return;
        app.Resources[key] = new SolidColorBrush(color);
    }

    private static bool IsWindowsDark()
    {
        try
        {
            var value = Registry.GetValue(
                @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                "AppsUseLightTheme", 1);
            return value is int number && number == 0;
        }
        catch { return false; }
    }
}