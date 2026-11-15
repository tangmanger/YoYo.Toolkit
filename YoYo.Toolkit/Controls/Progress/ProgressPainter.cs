using System;
using System.Windows;
using System.Windows.Media;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 参数化阵列的绘制宿主。把 5 种「元素数量/相位由参数决定」的动效收敛到同一个元素：
/// 模板只需要动画 <see cref="Phase"/>（0 → 1 循环），本元素按相位出帧。
/// <para>
/// 只做绘制，不持有计时器；<see cref="Phase"/> 变化通过 <c>AffectsRender</c> 自动触发重绘，
/// 因此 <c>IsActive=false</c> 时故事板一停，绘制也随之停止。
/// </para>
/// </summary>
public sealed class ProgressPainter : FrameworkElement
{
    #region Dependency properties

    /// <summary>Identifies the <see cref="Pattern"/> property.</summary>
    public static readonly DependencyProperty PatternProperty = DependencyProperty.Register(
        nameof(Pattern),
        typeof(ProgressPainterPattern),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(ProgressPainterPattern.Signal, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>绘制模式。</summary>
    public ProgressPainterPattern Pattern
    {
        get => (ProgressPainterPattern)GetValue(PatternProperty);
        set => SetValue(PatternProperty, value);
    }

    /// <summary>Identifies the <see cref="Phase"/> property.</summary>
    public static readonly DependencyProperty PhaseProperty = DependencyProperty.Register(
        nameof(Phase),
        typeof(double),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>动效相位，取值 0..1。模板用 Storyboard 循环驱动。</summary>
    public double Phase
    {
        get => (double)GetValue(PhaseProperty);
        set => SetValue(PhaseProperty, value);
    }

    /// <summary>Identifies the <see cref="Accent"/> property.</summary>
    public static readonly DependencyProperty AccentProperty = DependencyProperty.Register(
        nameof(Accent),
        typeof(Brush),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>主色。</summary>
    public Brush Accent
    {
        get => (Brush)GetValue(AccentProperty);
        set => SetValue(AccentProperty, value);
    }

    /// <summary>Identifies the <see cref="Dim"/> property.</summary>
    public static readonly DependencyProperty DimProperty = DependencyProperty.Register(
        nameof(Dim),
        typeof(Brush),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>弱化色，用于轨道、非高亮刻度、点阵底色。</summary>
    public Brush Dim
    {
        get => (Brush)GetValue(DimProperty);
        set => SetValue(DimProperty, value);
    }

    /// <summary>Identifies the <see cref="Count"/> property.</summary>
    public static readonly DependencyProperty CountProperty = DependencyProperty.Register(
        nameof(Count),
        typeof(int),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(28, FrameworkPropertyMetadataOptions.AffectsRender),
        value => (int)value is > 0 and <= 512);

    /// <summary>柱体 / 刻度 / 尾迹段的数量。</summary>
    public int Count
    {
        get => (int)GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    /// <summary>Identifies the <see cref="Rows"/> property.</summary>
    public static readonly DependencyProperty RowsProperty = DependencyProperty.Register(
        nameof(Rows),
        typeof(int),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(9, FrameworkPropertyMetadataOptions.AffectsRender),
        value => (int)value is > 1 and <= 64);

    /// <summary>点阵的行列数（方阵）。</summary>
    public int Rows
    {
        get => (int)GetValue(RowsProperty);
        set => SetValue(RowsProperty, value);
    }

    /// <summary>Identifies the <see cref="Thickness"/> property.</summary>
    public static readonly DependencyProperty ThicknessProperty = DependencyProperty.Register(
        nameof(Thickness),
        typeof(double),
        typeof(ProgressPainter),
        new FrameworkPropertyMetadata(2.4, FrameworkPropertyMetadataOptions.AffectsRender));

    /// <summary>线宽。</summary>
    public double Thickness
    {
        get => (double)GetValue(ThicknessProperty);
        set => SetValue(ThicknessProperty, value);
    }

    #endregion

    /// <summary>透明度档位数。逐点 PushOpacity 会为每一点创建一层中间合成，改成按档取冻结画笔。</summary>
    private const int OpacitySteps = 16;

    private Brush? _accentSource;
    private double _accentThickness = -1d;
    private Brush[] _accentSteps = Array.Empty<Brush>();
    private Pen[] _accentPens = Array.Empty<Pen>();

    private static readonly RadialGradientBrush HighlightBrush = CreateHighlightBrush();

    /// <summary>
    /// 主色按 Accent 变化缓存成 16 档冻结画笔 / 笔。动画期间只做一次 OnRender，
    /// 既不逐帧重建渐变，也不分配 Brush/Pen，更不用 PushOpacity 叠合成层。
    /// </summary>
    private void EnsureAccentSteps()
    {
        var source = Accent;
        var thickness = Thickness;
        if (ReferenceEquals(_accentSource, source)
            && _accentSteps.Length == OpacitySteps
            && Math.Abs(_accentThickness - thickness) < 0.001) return;

        _accentSource = source;
        _accentThickness = thickness;

        if (source is null)
        {
            _accentSteps = Array.Empty<Brush>();
            _accentPens = Array.Empty<Pen>();
            return;
        }

        var baseOpacity = source.Opacity;
        var brushes = new Brush[OpacitySteps];
        var pens = new Pen[OpacitySteps];

        for (var i = 0; i < OpacitySteps; i++)
        {
            var clone = (Brush)source.CloneCurrentValue();
            clone.Opacity = baseOpacity * (i + 1d) / OpacitySteps;
            clone.Freeze();
            brushes[i] = clone;

            var pen = new Pen(clone, thickness)
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round
            };
            pen.Freeze();
            pens[i] = pen;
        }

        _accentSteps = brushes;
        _accentPens = pens;
    }

    private static int StepFor(double opacity)
        => opacity <= 0 ? 0 : opacity >= 1 ? OpacitySteps - 1 : (int)(opacity * (OpacitySteps - 1) + 0.5);

    private Brush StepBrush(double opacity)
        => _accentSteps.Length == OpacitySteps ? _accentSteps[StepFor(opacity)] : Accent;

    private Pen StepPen(double opacity)
        => _accentPens.Length == OpacitySteps
            ? _accentPens[StepFor(opacity)]
            : new Pen(Accent, Thickness);

    private static RadialGradientBrush CreateHighlightBrush()
    {
        var stops = new GradientStopCollection
        {
            new GradientStop(Color.FromArgb(0x99, 0xFF, 0xFF, 0xFF), 0d),
            new GradientStop(Color.FromArgb(0x00, 0xFF, 0xFF, 0xFF), 1d)
        };

        var brush = new RadialGradientBrush(stops)
        {
            GradientOrigin = new Point(0.4, 0.36),
            Center = new Point(0.45, 0.42),
            RadiusX = 0.6,
            RadiusY = 0.6
        };
        brush.Freeze();
        return brush;
    }

    /// <inheritdoc />
    protected override void OnRender(DrawingContext dc)
    {
        var width = ActualWidth;
        var height = ActualHeight;
        if (width <= 0 || height <= 0) return;

        EnsureAccentSteps();

        switch (Pattern)
        {
            case ProgressPainterPattern.Signal:
                RenderSignal(dc, width, height);
                break;
            case ProgressPainterPattern.Ticks:
                RenderTicks(dc, width, height);
                break;
            case ProgressPainterPattern.Wave:
                RenderWave(dc, width, height);
                break;
            case ProgressPainterPattern.Matrix:
                RenderMatrix(dc, width, height);
                break;
            case ProgressPainterPattern.Comet:
                RenderComet(dc, width, height);
                break;
            case ProgressPainterPattern.TicksProgress:
                RenderTicksProgress(dc, width, height);
                break;
            case ProgressPainterPattern.Sonar:
                RenderSonar(dc, width, height);
                break;
            case ProgressPainterPattern.Breathing:
                RenderBreathing(dc, width, height);
                break;
        }
    }

    private void RenderSignal(DrawingContext dc, double width, double height)
    {
        var count = Math.Max(3, Count);
        const double barWidth = 3d;
        var step = count > 1 ? Math.Max(barWidth + 1, width / count) : width;
        var offPhase = 1d / count;

        for (var i = 0; i < count; i++)
        {
            // 每根柱用索引派生的固定相位差，保证帧间稳定（不能用 Random）
            var phase = Phase + i * offPhase * 3.7;
            var pulse = 0.5 - 0.5 * Math.Cos(2 * Math.PI * phase);
            var amplitudeFactor = 0.35 + 0.65 * ((i % 5) / 4d);
            var scale = 0.18 + 0.82 * amplitudeFactor * pulse;

            var barHeight = Math.Max(2, height * scale);
            var x = i * step;
            dc.DrawRoundedRectangle(Accent, null, new Rect(x, height - barHeight, barWidth, barHeight), 1, 1);
        }
    }

    private void RenderTicks(DrawingContext dc, double width, double height)
    {
        var cx = width / 2;
        var cy = height / 2;
        var outer = Math.Min(cx, cy) - 2;
        var inner = outer - Math.Max(6, outer * 0.16);
        var count = Math.Max(6, Count);
        var activeIndex = (int)Math.Floor(Phase * count) % count;

        var dimPen = new Pen(Dim, 2);
        var accentPen = new Pen(Accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        for (var i = 0; i < count; i++)
        {
            var angle = i * 2 * Math.PI / count - Math.PI / 2;
            var isActive = i == activeIndex;
            var start = new Point(cx + inner * Math.Cos(angle), cy + inner * Math.Sin(angle));
            var end = new Point(cx + outer * Math.Cos(angle), cy + outer * Math.Sin(angle));
            dc.DrawLine(isActive ? accentPen : dimPen, start, end);
        }
    }

    private void RenderWave(DrawingContext dc, double width, double height)
    {
        var mid = height / 2;
        var amplitude = height * 0.42;
        var period = Math.Max(48, width / 4);
        var step = Math.Max(2, width / 200);

        var geometry = new StreamGeometry();
        using (var context = geometry.Open())
        {
            var started = false;
            for (var x = 0d; x <= width + step; x += step)
            {
                var y = mid - amplitude * Math.Sin((x / period + Phase) * 2 * Math.PI);
                var point = new Point(x, y);
                if (!started)
                {
                    context.BeginFigure(point, false, false);
                    started = true;
                }
                else
                {
                    context.LineTo(point, true, false);
                }
            }
        }

        geometry.Freeze();
        var pen = new Pen(Accent, Thickness) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };
        dc.DrawGeometry(null, pen, geometry);
    }

    private void RenderMatrix(DrawingContext dc, double width, double height)
    {
        var rows = Math.Max(2, Rows);
        var cell = Math.Min(width / rows, height / rows);
        var radius = cell * 0.3;
        var offsetX = (width - cell * rows) / 2;
        var offsetY = (height - cell * rows) / 2;
        var center = (rows - 1) / 2d;
        var maxDistance = Math.Max(1, Math.Sqrt(2) * center);

        for (var r = 0; r < rows; r++)
        {
            for (var c = 0; c < rows; c++)
            {
                var distance = Math.Sqrt((c - center) * (c - center) + (r - center) * (r - center));
                var t = Phase - distance / maxDistance;
                t -= Math.Floor(t);

                var falloff = 1 - t;
                var scale = 0.78 + 0.22 * falloff;
                var opacity = 0.16 + 0.84 * falloff * falloff;
                var point = new Point(offsetX + (c + 0.5) * cell, offsetY + (r + 0.5) * cell);

                // 原来每点一次 PushOpacity/DrawEllipse/Pop，81 个点就是 81 层中间合成。
                // 现在按 16 档取冻结画笔，绘制指令从 243 条降到 81 条，且零合成层。
                dc.DrawEllipse(StepBrush(opacity), null, point, radius * scale, radius * scale);
            }
        }
    }

    /// <summary>声纳：两圈同心环按 0.5 相位差扩散并淡出，中心一个实心点。</summary>
    private void RenderSonar(DrawingContext dc, double width, double height)
    {
        var cx = width / 2;
        var cy = height / 2;
        var maxRadius = Math.Min(cx, cy) * 0.7;
        var center = new Point(cx, cy);

        for (var i = 0; i < 2; i++)
        {
            var t = Phase + i * 0.5;
            t -= Math.Floor(t);

            var radius = maxRadius * (0.4 + 0.6 * t);
            var opacity = 0.6 * (1d - t);
            dc.DrawEllipse(null, StepPen(opacity), center, radius, radius);
        }

        dc.DrawEllipse(Accent, null, center, maxRadius * 0.08, maxRadius * 0.08);
    }

    /// <summary>呼吸光球：半径与亮度同相位起伏，高光用一枚冻结的径向渐变刷。</summary>
    private void RenderBreathing(DrawingContext dc, double width, double height)
    {
        var cx = width / 2;
        var cy = height / 2;
        var baseRadius = Math.Min(cx, cy) * 0.66;
        // 模板的 Phase 动画是 AutoReverse，Phase 本身就是 0→1→0 的呼吸量
        var breath = Phase;
        var radius = baseRadius * (0.84 + 0.22 * breath);
        var center = new Point(cx, cy);

        dc.DrawEllipse(StepBrush(0.72 + 0.28 * breath), null, center, radius, radius);
        dc.DrawEllipse(HighlightBrush, null, center, radius, radius);
    }

    private void RenderTicksProgress(DrawingContext dc, double width, double height)
    {
        var cx = width / 2;
        var cy = height / 2;
        var outer = Math.Min(cx, cy) - 2;
        var inner = outer - Math.Max(6, outer * 0.16);
        var count = Math.Max(6, Count);
        var filled = (int)Math.Round(Phase * count);

        var dimPen = new Pen(Dim, 2);
        var accentPen = new Pen(Accent, 3) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round };

        for (var i = 0; i < count; i++)
        {
            var angle = i * 2 * Math.PI / count - Math.PI / 2;
            var start = new Point(cx + inner * Math.Cos(angle), cy + inner * Math.Sin(angle));
            var end = new Point(cx + outer * Math.Cos(angle), cy + outer * Math.Sin(angle));
            dc.DrawLine(i < filled ? accentPen : dimPen, start, end);
        }
    }

    private void RenderComet(DrawingContext dc, double width, double height)
    {
        var cx = width / 2;
        var cy = height / 2;
        var radius = Math.Min(cx, cy) * 0.76;
        var segments = Math.Max(3, Count);
        var head = Phase * 2 * Math.PI;

        for (var i = 0; i < segments; i++)
        {
            var angle = head - i * 0.085;
            var factor = 1 - i / (double)segments;
            var size = Thickness + 2.4 * factor;

            dc.PushOpacity(Math.Max(0.08, factor * factor));
            dc.DrawEllipse(Accent, null, new Point(cx + radius * Math.Cos(angle), cy + radius * Math.Sin(angle)), size, size);
            dc.Pop();
        }
    }
}
