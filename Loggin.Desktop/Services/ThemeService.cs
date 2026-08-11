using System.Windows;
using System.Windows.Media;
using Loggin.Desktop.Models;
using Microsoft.Win32;

namespace Loggin.Desktop.Services;

public static class ThemeService
{
    public static void Apply(AppSettings settings)
    {
        var dark = settings.Theme == ThemePreference.Dark ||
                   settings.Theme == ThemePreference.System && IsWindowsDark();
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
        Set("DangerHoverBrush", Darken(settings.PrimaryColor == "#E5484D" ? settings.PrimaryColor : "#E5484D", 0.10));

        // Sidebar stays dark in both themes for a consistent app identity
        Set("SidebarBrush", dark ? "#11121B" : "#15131B");
        Set("SidebarTextBrush", dark ? "#C9C5D6" : "#C9C5D6");
        Set("SidebarActiveBrush", settings.PrimaryColor);
        Set("SidebarHoverBrush", dark ? "#1F2230" : "#221F2E");
        Set("SidebarHeaderBrush", dark ? "#F5F7FC" : "#FFFFFF");
    }

    private static string Darken(string hex, double amount)
    {
        try
        {
            var c = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(hex);
            var r = (byte)Math.Max(0, c.R * (1 - amount));
            var g = (byte)Math.Max(0, c.G * (1 - amount));
            var b = (byte)Math.Max(0, c.B * (1 - amount));
            return $"#{r:X2}{g:X2}{b:X2}";
        }
        catch { return hex; }
    }

    public static bool IsValidColor(string value)
    {
        try { _ = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value); return true; }
        catch { return false; }
    }

    private static void Set(string key, string value) =>
        System.Windows.Application.Current.Resources[key] = new SolidColorBrush(
            (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(value));

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
