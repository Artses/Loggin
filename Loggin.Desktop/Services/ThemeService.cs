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
        Set("SecondaryBrush", settings.SecondaryColor);
        Set("AccentBrush", settings.AccentColor);
        Set("WindowBrush", dark ? "#111522" : "#F4F6FB");
        Set("SurfaceBrush", dark ? "#1A2030" : "#FFFFFF");
        Set("SurfaceAltBrush", dark ? "#232B3E" : "#EEF1F8");
        Set("TextBrush", dark ? "#F5F7FC" : "#172033");
        Set("MutedTextBrush", dark ? "#AAB3C8" : "#687189");
        Set("BorderBrush", dark ? "#323C52" : "#DDE2EE");
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
