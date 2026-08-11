using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
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

        System.Windows.Media.Brush primary = (System.Windows.Media.Brush)(TryFindResource("PrimaryBrush") ?? System.Windows.Media.Brushes.MediumSlateBlue);
        System.Windows.Media.Brush gridBrush = (System.Windows.Media.Brush)(TryFindResource("BorderBrush") ?? System.Windows.Media.Brushes.LightGray);
        System.Windows.Media.Brush muted = (System.Windows.Media.Brush)(TryFindResource("MutedTextBrush") ?? System.Windows.Media.Brushes.Gray);

        double width = ActualWidth;
        double height = ActualHeight;
        double topPadding = 10.0;
        double bottomPadding = 14.0;

        var gridPen = new System.Windows.Media.Pen(gridBrush, 1) { DashStyle = DashStyles.Dash };
        for (int i = 1; i < 4; i++)
        {
            double y = topPadding + (height - topPadding - bottomPadding) * i / 4;
            dc.DrawLine(gridPen, new System.Windows.Point(0, y), new System.Windows.Point(width, y));
        }

        double[] values = Values?.Cast<object>().Select(Convert.ToDouble).ToArray() ?? System.Array.Empty<double>();

        if (values.Length == 0)
        {
            var text = new FormattedText("A atividade aparecerá após a coleta dos logs.",
                CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
                new Typeface("Segoe UI"), 13, muted,
                VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(text, new System.Windows.Point(Math.Max(0, (width - text.Width) / 2),
                Math.Max(0, (height - text.Height) / 2)));
            return;
        }

        double max = Math.Max(1, values.Max());
        double chartHeight = height - topPadding - bottomPadding;

        var points = new System.Windows.Point[values.Length];
        for (int i = 0; i < values.Length; i++)
        {
            double x = values.Length == 1 ? width / 2 : i * width / (values.Length - 1);
            double y = topPadding + chartHeight - values[i] / max * chartHeight;
            points[i] = new System.Windows.Point(x, y);
        }

        var fillGeometry = new StreamGeometry();
        using (var ctx = fillGeometry.Open())
        {
            ctx.BeginFigure(new System.Windows.Point(points[0].X, height - bottomPadding), true, true);
            foreach (var pt in points)
                ctx.LineTo(pt, true, false);
            ctx.LineTo(new System.Windows.Point(points[^1].X, height - bottomPadding), true, false);
        }

        var primaryColor = ((System.Windows.Media.SolidColorBrush)primary).Color;
        var transparentColor = System.Windows.Media.Color.FromArgb(0, primaryColor.R, primaryColor.G, primaryColor.B);
        var gradientFill = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(primaryColor, 0.0),
                new GradientStop(transparentColor, 1.0),
            })
        {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(0, 1),
        };
        dc.DrawGeometry(gradientFill, null, fillGeometry);

        var basePen = new System.Windows.Media.Pen(gridBrush, 1.5);
        dc.DrawLine(basePen,
            new System.Windows.Point(0, height - bottomPadding),
            new System.Windows.Point(width, height - bottomPadding));

        var lineGeometry = new StreamGeometry();
        using (var ctx = lineGeometry.Open())
        {
            ctx.BeginFigure(points[0], false, false);
            for (int i = 1; i < points.Length; i++)
                ctx.LineTo(points[i], true, false);
        }
        var linePen = new System.Windows.Media.Pen(primary, 2.5) { LineJoin = PenLineJoin.Round };
        dc.DrawGeometry(null, linePen, lineGeometry);

        var last = points[^1];
        var dotPen = new System.Windows.Media.Pen(primary, 2.5);
        dc.DrawEllipse(System.Windows.Media.Brushes.White, dotPen, last, 5, 5);

        var label = new FormattedText(
            values[^1].ToString("0", CultureInfo.CurrentCulture),
            CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 10, muted,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);
        dc.DrawText(label, new System.Windows.Point(last.X + 8, Math.Max(topPadding, last.Y - label.Height - 4)));
    }
}
