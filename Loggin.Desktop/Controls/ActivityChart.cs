using System.Collections;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Media;

namespace Loggin.Desktop.Controls;

public sealed class ActivityChart : FrameworkElement
{
    public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
        nameof(Values), typeof(IEnumerable), typeof(ActivityChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnValuesChanged));

    public IEnumerable? Values { get => (IEnumerable?)GetValue(ValuesProperty); set => SetValue(ValuesProperty, value); }
    private INotifyCollectionChanged? _observable;

    private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chart = (ActivityChart)d;
        if (chart._observable is not null) chart._observable.CollectionChanged -= chart.OnCollectionChanged;
        chart._observable = e.NewValue as INotifyCollectionChanged;
        if (chart._observable is not null) chart._observable.CollectionChanged += chart.OnCollectionChanged;
        chart.InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateVisual();

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var values = Values?.Cast<object>().Select(Convert.ToDouble).ToArray() ?? [];
        var primary = (System.Windows.Media.Brush)(TryFindResource("PrimaryBrush") ?? System.Windows.Media.Brushes.MediumSlateBlue);
        var grid = (System.Windows.Media.Brush)(TryFindResource("BorderBrush") ?? System.Windows.Media.Brushes.LightGray);
        var width = ActualWidth;
        var height = ActualHeight;
        for (var i = 1; i < 4; i++)
            dc.DrawLine(new System.Windows.Media.Pen(grid, 1), new System.Windows.Point(0, height * i / 4),
                new System.Windows.Point(width, height * i / 4));
        if (values.Length == 0)
        {
            var text = new FormattedText("A atividade aparecerá após a coleta dos logs.",
                System.Globalization.CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 13,
                (System.Windows.Media.Brush)(TryFindResource("MutedTextBrush") ?? System.Windows.Media.Brushes.Gray),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(text, new System.Windows.Point(Math.Max(0, (width - text.Width) / 2),
                Math.Max(0, (height - text.Height) / 2)));
            return;
        }
        var max = Math.Max(1, values.Max());
        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            for (var i = 0; i < values.Length; i++)
            {
                var x = values.Length == 1 ? width / 2 : i * width / (values.Length - 1);
                var y = height - values[i] / max * (height - 12) - 6;
                if (i == 0) context.BeginFigure(new System.Windows.Point(x, y), false, false);
                else context.LineTo(new System.Windows.Point(x, y), true, false);
            }
        }
        dc.DrawGeometry(null, new System.Windows.Media.Pen(primary, 3) { LineJoin = PenLineJoin.Round }, geometry);
    }
}
