using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using Loggin.Desktop.ViewModels;
using WpfApplication = System.Windows.Application;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfFontWeights = System.Windows.FontWeights;

namespace Loggin.Desktop.Controls;

public sealed class InlineCollectionBinder
{
    public static readonly DependencyProperty InlinesProperty = DependencyProperty.RegisterAttached(
        "Inlines", typeof(IEnumerable<Inline>), typeof(InlineCollectionBinder),
        new FrameworkPropertyMetadata(null, OnInlinesChanged));

    public static IEnumerable<Inline> GetInlines(DependencyObject obj) => (IEnumerable<Inline>)obj.GetValue(InlinesProperty);
    public static void SetInlines(DependencyObject obj, IEnumerable<Inline> value) => obj.SetValue(InlinesProperty, value);

    private static void OnInlinesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is TextBlock tb)
        {
            tb.Inlines.Clear();
            if (e.NewValue is IEnumerable<Inline> inlines)
                tb.Inlines.AddRange(inlines);
        }
    }
}

public sealed class LogLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is LogLevel l ? l : LogLevel.None;
        var key = level switch
        {
            LogLevel.Trace => "LogLevelTrace",
            LogLevel.Debug => "LogLevelDebug",
            LogLevel.Info => "LogLevelInfo",
            LogLevel.Warning => "LogLevelWarning",
            LogLevel.Error => "LogLevelError",
            LogLevel.Fatal => "LogLevelFatal",
            _ => "LogLevelNone"
        };
        if (WpfApplication.Current?.TryFindResource(key) is WpfBrush brush)
            return brush;
        return WpfBrushes.Gray;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class LogLevelToBackgroundConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is LogLevel l ? l : LogLevel.None;
        var key = level switch
        {
            LogLevel.Error => "LogRowError",
            LogLevel.Fatal => "LogRowFatal",
            LogLevel.Warning => "LogRowWarning",
            _ => "Transparent"
        };
        if (key == "Transparent") return WpfBrushes.Transparent;
        if (WpfApplication.Current?.TryFindResource(key) is WpfBrush brush)
            return brush;
        return WpfBrushes.Transparent;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class NotNullToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        bool isNotNull;
        if (value is string s)
            isNotNull = !string.IsNullOrEmpty(s);
        else
            isNotNull = value is not null;
        var invert = parameter is string p && p.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) return isNotNull ? Visibility.Collapsed : Visibility.Visible;
        return isNotNull ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class InvertedBoolToVisibilityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        var isTrue = value is bool b && b;
        return isTrue ? Visibility.Collapsed : Visibility.Visible;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class LogLevelNotNoneToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var level = value is LogLevel l ? l : LogLevel.None;
        return level != LogLevel.None ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class CountToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object? parameter, CultureInfo culture)
    {
        var hasItems = value switch
        {
            int count => count > 0,
            System.Collections.ICollection col => col.Count > 0,
            System.Collections.IEnumerable en => en.GetEnumerator().MoveNext(),
            _ => value is not null
        };
        var invert = parameter is string s && s.Equals("invert", StringComparison.OrdinalIgnoreCase);
        if (invert) return hasItems ? Visibility.Collapsed : Visibility.Visible;
        return hasItems ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}

public sealed class LogLineSearchHighlightConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        var line = values.Length > 0 ? values[0] as string : null;
        var search = values.Length > 1 ? values[1] as string : null;
        var useRegex = values.Length > 2 && values[2] is bool b && b;

        var inlines = new List<Inline>();

        if (string.IsNullOrEmpty(line))
            return inlines;

        if (string.IsNullOrWhiteSpace(search))
        {
            inlines.Add(new Run(line));
            return inlines;
        }

        try
        {
            var regex = useRegex
                ? new System.Text.RegularExpressions.Regex(search, System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                : new System.Text.RegularExpressions.Regex(System.Text.RegularExpressions.Regex.Escape(search), System.Text.RegularExpressions.RegexOptions.IgnoreCase);

            var lastIndex = 0;
            foreach (System.Text.RegularExpressions.Match m in regex.Matches(line))
            {
                if (m.Index > lastIndex)
                    inlines.Add(new Run(line[lastIndex..m.Index]));

                var highlightBrush = (WpfBrush)(WpfApplication.Current?.TryFindResource("LogSearchHighlight") ?? WpfBrushes.Yellow);
                inlines.Add(new Run(line[m.Index..(m.Index + m.Length)])
                {
                    Background = highlightBrush,
                    FontWeight = WpfFontWeights.Bold
                });
                lastIndex = m.Index + m.Length;
            }
            if (lastIndex < line.Length)
                inlines.Add(new Run(line[lastIndex..]));
        }
        catch
        {
            inlines.Clear();
            inlines.Add(new Run(line));
        }
        return inlines;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object? parameter, CultureInfo culture) =>
        throw new NotSupportedException();
}
