using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using WpfBrush = System.Windows.Media.Brush;
using WpfBrushes = System.Windows.Media.Brushes;
using WpfColor = System.Windows.Media.Color;
using WpfPen = System.Windows.Media.Pen;
using WpfPoint = System.Windows.Point;

namespace Loggin.Desktop.Controls;

public sealed class ActivityChart : FrameworkElement
{
    private const double TopPadding = 10.0;
    private const double BottomPadding = 14.0;
    private const double LineThickness = 2.5;
    private const double DotRadius = 5.0;

    public static readonly DependencyProperty ValuesProperty = DependencyProperty.Register(
        nameof(Values), typeof(IEnumerable), typeof(ActivityChart),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, OnValuesChanged));

    public IEnumerable? Values
    {
        get => (IEnumerable?)GetValue(ValuesProperty);
        set => SetValue(ValuesProperty, value);
    }

    private INotifyCollectionChanged? _observable;

    private static void OnValuesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var chart = (ActivityChart)d;
        if (chart._observable is not null)
            chart._observable.CollectionChanged -= chart.OnCollectionChanged;
        chart._observable = e.NewValue as INotifyCollectionChanged;
        if (chart._observable is not null)
            chart._observable.CollectionChanged += chart.OnCollectionChanged;
        chart.InvalidateVisual();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => InvalidateVisual();

    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);

        var primary = (WpfBrush)(TryFindResource("PrimaryBrush") ?? WpfBrushes.MediumSlateBlue);
        var gridBrush = (WpfBrush)(TryFindResource("BorderBrush") ?? WpfBrushes.LightGray);
        var muted = (WpfBrush)(TryFindResource("MutedTextBrush") ?? WpfBrushes.Gray);

        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        var dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;

        DrawGridLines(dc, gridBrush, width, height);

        var values = Values?.Cast<object>().Select(Convert.ToDouble).ToArray() ?? Array.Empty<double>();

        if (values.Length == 0)
        {
            DrawEmptyState(dc, muted, width, height, dpi);
            return;
        }

        var max = Math.Max(1, values.Max(v => Math.Max(0, v)));
        var chartHeight = height - TopPadding - BottomPadding;
        var points = ComputePoints(values, max, width, chartHeight);

        DrawAreaFill(dc, primary, points, height);
        DrawBaseline(dc, gridBrush, width, height);
        DrawLine(dc, primary, points);
        DrawEndPointDot(dc, primary, points[^1]);
        DrawValueLabel(dc, muted, values[^1], points[^1], dpi);
    }

    private static void DrawGridLines(DrawingContext dc, WpfBrush gridBrush, double width, double height)
    {
        var gridPen = new WpfPen(gridBrush, 1) { DashStyle = DashStyles.Dash };
        for (var i = 1; i < 4; i++)
        {
            var y = TopPadding + (height - TopPadding - BottomPadding) * i / 4;
            dc.DrawLine(gridPen, new WpfPoint(0, y), new WpfPoint(width, y));
        }
    }

    private static void DrawEmptyState(DrawingContext dc, WpfBrush muted, double width, double height, double dpi)
    {
        var text = new FormattedText(
            "A atividade aparecerá após a coleta dos logs.",
            CultureInfo.CurrentCulture, System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 13, muted, dpi);
        dc.DrawText(text, new WpfPoint(
            Math.Max(0, (width - text.Width) / 2),
            Math.Max(0, (height - text.Height) / 2)));
    }

    private static WpfPoint[] ComputePoints(double[] values, double max, double width, double chartHeight)
    {
        var points = new WpfPoint[values.Length];
        for (var i = 0; i < values.Length; i++)
        {
            var x = values.Length == 1 ? width / 2 : i * width / (values.Length - 1);
            var y = TopPadding + chartHeight - Math.Max(0, values[i]) / max * chartHeight;
            points[i] = new WpfPoint(x, y);
        }
        return points;
    }

    private static void DrawAreaFill(DrawingContext dc, WpfBrush primary, WpfPoint[] points, double height)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(new WpfPoint(points[0].X, height - BottomPadding), true, true);
            foreach (var pt in points)
                ctx.LineTo(pt, true, false);
            ctx.LineTo(new WpfPoint(points[^1].X, height - BottomPadding), true, false);
        }

        if (primary is not SolidColorBrush sb) return;
        var baseColor = sb.Color;
        var transparent = WpfColor.FromArgb(0, baseColor.R, baseColor.G, baseColor.B);
        var gradient = new LinearGradientBrush(
            new GradientStopCollection
            {
                new GradientStop(baseColor, 0.0),
                new GradientStop(transparent, 1.0)
            })
        {
            StartPoint = new WpfPoint(0, 0),
            EndPoint = new WpfPoint(0, 1)
        };
        dc.DrawGeometry(gradient, null, geometry);
    }

    private static void DrawBaseline(DrawingContext dc, WpfBrush gridBrush, double width, double height)
    {
        dc.DrawLine(
            new WpfPen(gridBrush, 1.5),
            new WpfPoint(0, height - BottomPadding),
            new WpfPoint(width, height - BottomPadding));
    }

    private static void DrawLine(DrawingContext dc, WpfBrush primary, WpfPoint[] points)
    {
        var geometry = new StreamGeometry();
        using (var ctx = geometry.Open())
        {
            ctx.BeginFigure(points[0], false, false);
            for (var i = 1; i < points.Length; i++)
                ctx.LineTo(points[i], true, false);
        }
        var pen = new WpfPen(primary, LineThickness) { LineJoin = PenLineJoin.Round };
        dc.DrawGeometry(null, pen, geometry);
    }

    private static void DrawEndPointDot(DrawingContext dc, WpfBrush primary, WpfPoint last)
    {
        dc.DrawEllipse(
            WpfBrushes.White,
            new WpfPen(primary, LineThickness),
            last,
            DotRadius,
            DotRadius);
    }

    private static void DrawValueLabel(DrawingContext dc, WpfBrush muted, double value, WpfPoint anchor, double dpi)
    {
        var label = new FormattedText(
            value.ToString("0", CultureInfo.CurrentCulture),
            CultureInfo.CurrentCulture,
            System.Windows.FlowDirection.LeftToRight,
            new Typeface("Segoe UI"), 10, muted, dpi);
        dc.DrawText(label, new WpfPoint(
            anchor.X + 8,
            Math.Max(TopPadding, anchor.Y - label.Height - 4)));
    }
}
