using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using Loggin.Desktop.Models;

namespace Loggin.Desktop.ViewModels;

public sealed class LogLineViewModel : INotifyPropertyChanged
{
    private static readonly Regex LevelRegex = new(
        @"\b(ERROR|ERR|FATAL|CRITICAL|WARN|WARNING|INFO|INFORMATION|DEBUG|TRACE|VERBOSE)\b",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public LogLineDto Source { get; }
    public string CollectorName { get; }
    public string CollectorId { get; }

    public int Order => Source.Order;
    public DateTime TimeStamp => Source.TimeStamp;
    public string RawLine => Source.Line;

    public string DisplayLine { get; }
    public LogLevel Level { get; }
    public bool IsExpanded { get; set; }
    public bool HasMatches { get; private set; }

    public LogLineViewModel(LogLineDto source, string collectorName, string collectorId)
    {
        Source = source;
        CollectorName = collectorName;
        CollectorId = collectorId;

        var match = LevelRegex.Match(source.Line);
        Level = match.Success ? LogLevelExtensions.Parse(match.Value) : LogLevel.None;

        DisplayLine = source.Line.Length > 5000 ? source.Line[..5000] + "… [truncado]" : source.Line;
    }

    public void UpdateHighlight(string? searchText, bool useRegex)
    {
        if (string.IsNullOrWhiteSpace(searchText))
        {
            HasMatches = false;
            OnPropertyChanged(nameof(HasMatches));
            return;
        }

        try
        {
            var regex = useRegex
                ? new Regex(searchText, RegexOptions.IgnoreCase)
                : new Regex(Regex.Escape(searchText), RegexOptions.IgnoreCase);

            HasMatches = regex.IsMatch(DisplayLine);
        }
        catch
        {
            HasMatches = false;
        }

        OnPropertyChanged(nameof(HasMatches));
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

public enum LogLevel
{
    None,
    Trace,
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public static class LogLevelExtensions
{
    public static LogLevel Parse(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return LogLevel.None;
        return text.ToUpperInvariant() switch
        {
            "TRACE" or "VERBOSE" => LogLevel.Trace,
            "DEBUG" => LogLevel.Debug,
            "INFO" or "INFORMATION" => LogLevel.Info,
            "WARN" or "WARNING" => LogLevel.Warning,
            "ERROR" or "ERR" => LogLevel.Error,
            "FATAL" or "CRITICAL" => LogLevel.Fatal,
            _ => LogLevel.None
        };
    }

    public static string ToCssClass(this LogLevel level) => level switch
    {
        LogLevel.Trace => "log-trace",
        LogLevel.Debug => "log-debug",
        LogLevel.Info => "log-info",
        LogLevel.Warning => "log-warning",
        LogLevel.Error => "log-error",
        LogLevel.Fatal => "log-fatal",
        _ => "log-none"
    };
}