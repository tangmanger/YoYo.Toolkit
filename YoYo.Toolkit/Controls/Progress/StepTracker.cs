using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 流程步骤条。与 <see cref="ProgressIndicator"/> 语义正交：
/// 前者回答"处在流程的哪一步"，后者回答"还要等多久"。
/// <para>
/// 各步的 <see cref="ProgressStep.State"/> 由 <see cref="CurrentIndex"/> 自动推导；
/// 调用方显式设为 Error / Skipped 的步骤会被保留，不再被推导覆盖。
/// </para>
/// </summary>
public class StepTracker : Control
{
    private INotifyCollectionChanged? _observableSteps;

    static StepTracker()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(StepTracker),
            new FrameworkPropertyMetadata(typeof(StepTracker)));
    }

    /// <summary>Initializes a new instance of the <see cref="StepTracker"/> class.</summary>
    public StepTracker()
    {
        ProgressTheme.EnsureLoaded();
        SetCurrentValue(FocusableProperty, false);
        SetCurrentValue(IsTabStopProperty, false);
    }

    #region Steps

    /// <summary>Identifies the <see cref="Steps"/> property.</summary>
    public static readonly DependencyProperty StepsProperty = DependencyProperty.Register(
        nameof(Steps),
        typeof(IEnumerable<ProgressStep>),
        typeof(StepTracker),
        new FrameworkPropertyMetadata(null, OnStepsChanged));

    /// <summary>步骤集合。</summary>
    [Category("Progress")]
    public IEnumerable<ProgressStep>? Steps
    {
        get => (IEnumerable<ProgressStep>?)GetValue(StepsProperty);
        set => SetValue(StepsProperty, value);
    }

    private static void OnStepsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var tracker = (StepTracker)d;

        if (tracker._observableSteps is not null)
            tracker._observableSteps.CollectionChanged -= tracker.OnCollectionChanged;

        tracker._observableSteps = e.NewValue as INotifyCollectionChanged;

        if (tracker._observableSteps is not null)
            tracker._observableSteps.CollectionChanged += tracker.OnCollectionChanged;

        tracker.Refresh();
    }

    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Refresh();

    #endregion

    #region CurrentIndex

    /// <summary>Identifies the <see cref="CurrentIndex"/> property.</summary>
    public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register(
        nameof(CurrentIndex),
        typeof(int),
        typeof(StepTracker),
        new FrameworkPropertyMetadata(-1, OnCurrentIndexChanged));

    /// <summary>当前步骤索引，-1 表示尚未开始。</summary>
    [Category("Progress")]
    public int CurrentIndex
    {
        get => (int)GetValue(CurrentIndexProperty);
        set => SetValue(CurrentIndexProperty, value);
    }

    private static void OnCurrentIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((StepTracker)d).Refresh();

    #endregion

    #region Appearance

    /// <summary>Identifies the <see cref="Orientation"/> property.</summary>
    public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
        nameof(Orientation),
        typeof(Orientation),
        typeof(StepTracker),
        new FrameworkPropertyMetadata(Orientation.Horizontal));

    /// <summary>排列方向。</summary>
    [Category("Progress")]
    public Orientation Orientation
    {
        get => (Orientation)GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>Identifies the <see cref="ShowConnector"/> property.</summary>
    public static readonly DependencyProperty ShowConnectorProperty = DependencyProperty.Register(
        nameof(ShowConnector),
        typeof(bool),
        typeof(StepTracker),
        new FrameworkPropertyMetadata(true));

    /// <summary>是否显示节点之间的连接线。</summary>
    [Category("Progress")]
    public bool ShowConnector
    {
        get => (bool)GetValue(ShowConnectorProperty);
        set => SetValue(ShowConnectorProperty, value);
    }

    /// <summary>Identifies the <see cref="AccentBrush"/> property.</summary>
    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
        nameof(AccentBrush),
        typeof(Brush),
        typeof(StepTracker),
        new FrameworkPropertyMetadata(null));

    /// <summary>主色，默认由样式从 <c>Progress.Brush.Accent</c> 注入。</summary>
    [Category("Progress")]
    public Brush? AccentBrush
    {
        get => (Brush?)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    private static readonly DependencyPropertyKey ProgressRatioPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(ProgressRatio),
            typeof(double),
            typeof(StepTracker),
            new FrameworkPropertyMetadata(0d));

    /// <summary>Identifies the <see cref="ProgressRatio"/> property.</summary>
    public static readonly DependencyProperty ProgressRatioProperty = ProgressRatioPropertyKey.DependencyProperty;

    /// <summary>整体完成比例（0..1），供模板驱动连线填充。</summary>
    public double ProgressRatio => (double)GetValue(ProgressRatioProperty);

    #endregion

    private void Refresh()
    {
        var steps = Steps?.ToList();

        if (steps is null || steps.Count == 0)
        {
            SetValue(ProgressRatioPropertyKey, 0d);
            return;
        }

        var index = CurrentIndex;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            step.IsFirst = i == 0;
            step.IsLast = i == steps.Count - 1;
            step.Ordinal = (i + 1).ToString();

            if (step.IsStatePinned) continue;

            step.State = i < index
                ? ProgressStepState.Done
                : i == index
                    ? ProgressStepState.Active
                    : ProgressStepState.Waiting;
        }

        var ratio = steps.Count == 1
            ? (index >= 0 ? 1d : 0d)
            : (index + 1d) / steps.Count;

        SetValue(ProgressRatioPropertyKey, Math.Clamp(ratio, 0d, 1d));
    }
}
