using Microsoft.Win32;

namespace Loggin.Desktop.Services;

public static class StartupService
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "LogginDesktop";

    public static void SetEnabled(bool enabled, bool startMinimized)
    {
        using var key = Registry.CurrentUser.OpenSubKey(RunKey, writable: true)
                        ?? Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        if (!enabled)
        {
            key.DeleteValue(ValueName, false);
            return;
        }

        var executable = Environment.ProcessPath
                         ?? throw new InvalidOperationException("Caminho do executável indisponível.");
        var arguments = startMinimized ? " --minimized" : string.Empty;
        key.SetValue(ValueName, $"\"{executable}\"{arguments}");
    }
}
