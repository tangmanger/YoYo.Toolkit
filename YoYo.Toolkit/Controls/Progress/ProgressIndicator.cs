using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 统一的进度 / 等待指示器。控件本体不含任何视觉，全部外观由 <see cref="ProgressKind"/>
/// 对应的 <c>ControlTemplate</c> 提供。
/// <para>
/// 模板解析按四级回退：显式 <see cref="Control.Template"/> &gt; <see cref="TemplateKey"/> &gt;
/// <c>Progress.Template.{Kind}.{Mode}</c> &gt; <c>Progress.Template.Default</c>。
/// 宿主只需在自有资源中定义同名资源，即可覆盖任意一层，无需继承控件。
/// </para>
/// </summary>
[TemplateVisualState(GroupName = GroupProgressStates, Name = StateIdle)]
[TemplateVisualState(GroupName = GroupProgressStates, Name = StateRunning)]
[TemplateVisualState(GroupName = GroupProgressStates, Name = StateRunningReduced)]
[TemplateVisualState(GroupName = GroupProgressStates, Name = StateSuccess)]
[TemplateVisualState(GroupName = GroupProgressStates, Name = StateError)]
[TemplateVisualState(GroupName = GroupProgressStates, Name = StatePaused)]
public class ProgressIndicator : Control
{
    /// <summary>视觉状态组名，模板需在此组下实现各状态。</summary>
    public const string GroupProgressStates = "ProgressStates";

    /// <summary>空闲静止帧。</summary>
    public const string StateIdle = "Idle";

    /// <summary>运行中，循环动画。</summary>
    public const string StateRunning = "Running";

    /// <summary>运行中（已降级），静止帧。</summary>
    public const string StateRunningReduced = "RunningReduced";

    /// <summary>成功终态。</summary>
    public const string StateSuccess = "Success";

    /// <summary>失败终态。</summary>
    public const string StateError = "Error";

    /// <summary>暂停状态。</summary>
    public const string StatePaused = "Paused";

    private const string TemplateKeyPrefix = "Progress.Template.";
    private const string DefaultTemplateKey = TemplateKeyPrefix + "Default";

    private bool _resolvingTemplate;

    static ProgressIndicator()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(ProgressIndicator),
            new FrameworkPropertyMetadata(typeof(ProgressIndicator)));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ProgressIndicator"/> class.
    /// </summary>
    public ProgressIndicator()
    {
        ProgressTheme.EnsureLoaded();
        SetCurrentValue(FocusableProperty, false);
        SetCurrentValue(IsTabStopProperty, false);
    }

    #region Kind

    /// <summary>Identifies the <see cref="Kind"/> property.</summary>
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(
        nameof(Kind),
        typeof(ProgressKind),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(ProgressKind.Arc, OnVisualAffectingPropertyChanged));

    /// <summary>选择视觉语言。切换不会中断调用方代码，只换模板。</summary>
    [Category("Progress")]
    [Description("选择视觉语言，对应一套内置 ControlTemplate。")]
    public ProgressKind Kind
    {
        get => (ProgressKind)GetValue(KindProperty);
        set => SetValue(KindProperty, value);
    }

    #endregion

    #region State / IsActive

    /// <summary>Identifies the <see cref="State"/> property.</summary>
    public static readonly DependencyProperty StateProperty = DependencyProperty.Register(
        nameof(State),
        typeof(ProgressRunState),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(ProgressRunState.Idle, OnVisualAffectingPropertyChanged));

    /// <summary>语义状态，驱动模板内的 <c>ProgressStates</c> 视觉状态组。</summary>
    [Category("Progress")]
    [Description("语义状态：Idle / Running / Success / Error / Paused。")]
    public ProgressRunState State
    {
        get => (ProgressRunState)GetValue(StateProperty);
        set => SetValue(StateProperty, value);
    }

    /// <summary>Identifies the <see cref="IsActive"/> property.</summary>
    public static readonly DependencyProperty IsActiveProperty = DependencyProperty.Register(
        nameof(IsActive),
        typeof(bool),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(false, OnVisualAffectingPropertyChanged));

    /// <summary>
    /// 动画开关。<c>false</c> 时模板必须停止故事板，不得后台空转。
    /// </summary>
    [Category("Progress")]
    [Description("动画开关。false 时停止故事板，避免后台空转。")]
    public bool IsActive
    {
        get => (bool)GetValue(IsActiveProperty);
        set => SetValue(IsActiveProperty, value);
    }

    #endregion

    #region Value

    /// <summary>Identifies the <see cref="IsIndeterminate"/> property.</summary>
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(
        nameof(IsIndeterminate),
        typeof(bool),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(true, OnVisualAffectingPropertyChanged));

    /// <summary>不确定进度时不展示百分比，符合“不谎报进度”的原则。</summary>
    [Category("Progress")]
    [Description("true 表示未知时长的等待；false 表示可按 Value 度量。")]
    public bool IsIndeterminate
    {
        get => (bool)GetValue(IsIndeterminateProperty);
        set => SetValue(IsIndeterminateProperty, value);
    }

    /// <summary>Identifies the <see cref="Value"/> property.</summary>
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(
        nameof(Value),
        typeof(double),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(0d, OnRangePropertyChanged));

    /// <summary>当前值，仅在 <see cref="IsIndeterminate"/> 为 <c>false</c> 时生效。</summary>
    [Category("Progress")]
    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>Identifies the <see cref="Minimum"/> property.</summary>
    public static readonly DependencyProperty MinimumProperty = DependencyProperty.Register(
        nameof(Minimum),
        typeof(double),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(0d, OnRangePropertyChanged));

    /// <summary>区间下界。</summary>
    [Category("Progress")]
    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    /// <summary>Identifies the <see cref="Maximum"/> property.</summary>
    public static readonly DependencyProperty MaximumProperty = DependencyProperty.Register(
        nameof(Maximum),
        typeof(double),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(100d, OnRangePropertyChanged));

    /// <summary>区间上界。</summary>
    [Category("Progress")]
    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    private static readonly DependencyPropertyKey PercentPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(Percent),
            typeof(double),
            typeof(ProgressIndicator),
            new FrameworkPropertyMetadata(0d));

    /// <summary>Identifies the <see cref="Percent"/> property.</summary>
    public static readonly DependencyProperty PercentProperty = PercentPropertyKey.DependencyProperty;

    /// <summary>
    /// 已钳制到 0..1 的完成比例。模板可直接用于 <c>ScaleTransform.ScaleX</c>，
    /// 无需值转换器。
    /// </summary>
    public double Percent => (double)GetValue(PercentProperty);

    private static void OnRangePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressIndicator)d).UpdatePercent();

    private void UpdatePercent()
    {
        var range = Maximum - Minimum;
        double percent;
        if (range <= 0 || double.IsNaN(range))
        {
            percent = 0d;
        }
        else
        {
            percent = (Value - Minimum) / range;
            if (double.IsNaN(percent)) percent = 0d;
            else if (percent < 0d) percent = 0d;
            else if (percent > 1d) percent = 1d;
        }

        SetValue(PercentPropertyKey, percent);
    }

    #endregion

    #region Appearance

    /// <summary>Identifies the <see cref="StatusText"/> property.</summary>
    public static readonly DependencyProperty StatusTextProperty = DependencyProperty.Register(
        nameof(StatusText),
        typeof(string),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(null, OnStatusTextChanged));

    /// <summary>伴随文案，如“搜索设备中…”。为空时模板可只显示图形。</summary>
    [Category("Progress")]
    public string? StatusText
    {
        get => (string?)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    /// <summary>Identifies the <see cref="ShowStatusText"/> property.</summary>
    public static readonly DependencyProperty ShowStatusTextProperty = DependencyProperty.Register(
        nameof(ShowStatusText),
        typeof(bool),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(true));

    /// <summary>是否显示 <see cref="StatusText"/>。</summary>
    [Category("Progress")]
    public bool ShowStatusText
    {
        get => (bool)GetValue(ShowStatusTextProperty);
        set => SetValue(ShowStatusTextProperty, value);
    }

    /// <summary>Identifies the <see cref="AccentBrush"/> property.</summary>
    public static readonly DependencyProperty AccentBrushProperty = DependencyProperty.Register(
        nameof(AccentBrush),
        typeof(Brush),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(null));

    /// <summary>
    /// 主色。默认值由样式从 <c>Progress.Brush.Accent</c> 令牌注入，
    /// 需要单实例换色时直接赋值即可。
    /// </summary>
    [Category("Progress")]
    public Brush? AccentBrush
    {
        get => (Brush?)GetValue(AccentBrushProperty);
        set => SetValue(AccentBrushProperty, value);
    }

    /// <summary>Identifies the <see cref="Motion"/> property.</summary>
    public static readonly DependencyProperty MotionProperty = DependencyProperty.Register(
        nameof(Motion),
        typeof(ProgressMotion),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(ProgressMotion.Auto, OnVisualAffectingPropertyChanged));

    /// <summary>动效强度，用于无障碍降级。</summary>
    [Category("Progress")]
    public ProgressMotion Motion
    {
        get => (ProgressMotion)GetValue(MotionProperty);
        set => SetValue(MotionProperty, value);
    }

    /// <summary>Identifies the <see cref="TemplateKey"/> property.</summary>
    public static readonly DependencyProperty TemplateKeyProperty = DependencyProperty.Register(
        nameof(TemplateKey),
        typeof(string),
        typeof(ProgressIndicator),
        new FrameworkPropertyMetadata(null, OnVisualAffectingPropertyChanged));

    /// <summary>
    /// 显式指定模板资源键，跳过按 <see cref="Kind"/> 的约定查找。
    /// 用于引入设计稿之外的第三方动效，无需扩充 <see cref="ProgressKind"/> 枚举。
    /// </summary>
    [Category("Progress")]
    [Description("模板资源键，用于接入自定义动效。")]
    public string? TemplateKey
    {
        get => (string?)GetValue(TemplateKeyProperty);
        set => SetValue(TemplateKeyProperty, value);
    }

    #endregion

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        ResolveTemplate();
        UpdateVisualState(false);
        UpdateAutomationName();
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.Property == KindProperty || e.Property == IsIndeterminateProperty || e.Property == TemplateKeyProperty)
        {
            ResolveTemplate();
            return;
        }

        if (e.Property == IsActiveProperty || e.Property == StateProperty || e.Property == MotionProperty)
        {
            UpdateVisualState(true);
            return;
        }

        if (e.Property == StatusTextProperty)
        {
            UpdateAutomationName();
        }
    }

    private static void OnVisualAffectingPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressIndicator)d).OnVisualAffectingPropertyChanged();

    private void OnVisualAffectingPropertyChanged()
    {
        ResolveTemplate();
        UpdateVisualState(true);
    }

    private static void OnStatusTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        => ((ProgressIndicator)d).UpdateAutomationName();

    #region Template resolution

    private void ResolveTemplate()
    {
        if (_resolvingTemplate) return;

        // 宿主通过 Local / Style / 触发器提供了模板时一律尊重，只接管 ThemeStyle 给的默认值。
        // ThemeStyle 的 BaseValueSource 也是 Default，因此用 Default 作为“可接管”的判据。
        if (IsTemplateProvidedByHost()) return;

        var key = TemplateKey;
        if (string.IsNullOrWhiteSpace(key))
            key = BuildTemplateKey(Kind, IsIndeterminate);

        if (TryResolveTemplate(key, out var template))
        {
            ApplyResolvedTemplate(template);
            return;
        }

        if (!IsIndeterminate)
        {
            var fallback = DeterminateFallback(Kind);
            if (TryResolveTemplate(BuildTemplateKey(fallback, false), out template))
            {
                ApplyResolvedTemplate(template);
                return;
            }
        }

        if (TryResolveTemplate(DefaultTemplateKey, out template))
        {
            ApplyResolvedTemplate(template);
            return;
        }

        ProgressTheme.ReportOnce(
            $"未找到模板 {key} 与 {DefaultTemplateKey}，请确认 Themes/Progress.Templates.xaml 已随程序集部署。");
    }

    private bool TryResolveTemplate(string? key, out ControlTemplate? template)
    {
        template = null;
        if (string.IsNullOrWhiteSpace(key)) return false;

        template = TryFindResource(key) as ControlTemplate;
        return template is not null;
    }

    private void ApplyResolvedTemplate(ControlTemplate? template)
    {
        if (template is null || ReferenceEquals(Template, template)) return;

        _resolvingTemplate = true;
        try
        {
            SetCurrentValue(TemplateProperty, template);
        }
        finally
        {
            _resolvingTemplate = false;
        }
    }

    /// <summary>
    /// 判断模板是否由宿主提供。只有主题样式（Generic.xaml 的默认样式）允许被接管，
    /// 其来源是 <see cref="BaseValueSource.DefaultStyle"/> / <see cref="BaseValueSource.Default"/>，
    /// 而宿主显式设值、自定义 Style、隐式 Style 分别是 Local / Style / ImplicitStyleReference，均需尊重。
    /// </summary>
    private bool IsTemplateProvidedByHost()
    {
        var source = DependencyPropertyHelper.GetValueSource(this, TemplateProperty).BaseValueSource;

        return source != BaseValueSource.Default
            && source != BaseValueSource.DefaultStyle
            && source != BaseValueSource.DefaultStyleTrigger;
    }

    private static string BuildTemplateKey(ProgressKind kind, bool indeterminate)
        => string.Concat(TemplateKeyPrefix, kind.ToString(), indeterminate ? ".Indeterminate" : ".Determinate");

    private static ProgressKind DeterminateFallback(ProgressKind kind) => kind switch
    {
        ProgressKind.FlowBar or ProgressKind.CounterFlow or ProgressKind.Skeleton => ProgressKind.FlowBar,
        _ => ProgressKind.Arc
    };

    #endregion

    #region Visual states

    private ProgressMotion ResolvedMotion => Motion != ProgressMotion.Auto
        ? Motion
        : SystemParameters.ClientAreaAnimation ? ProgressMotion.Full : ProgressMotion.Reduced;

    private void UpdateVisualState(bool useTransitions)
    {
        // IsActive 是更直接的意图表达：只写 IsActive="True" 也应当动起来，
        // 不必强制调用方同时把 State 设为 Running。
        var state = State == ProgressRunState.Idle && IsActive ? ProgressRunState.Running : State;

        if (state != ProgressRunState.Running)
        {
            VisualStateManager.GoToState(this, MapState(state), useTransitions);
            return;
        }

        if (ResolvedMotion == ProgressMotion.Reduced)
        {
            // 模板未实现降级态时回落到静止帧，避免故事板继续空转
            if (!VisualStateManager.GoToState(this, StateRunningReduced, useTransitions))
                VisualStateManager.GoToState(this, StateIdle, useTransitions);
            return;
        }

        VisualStateManager.GoToState(this, StateRunning, useTransitions);
    }

    private static string MapState(ProgressRunState state) => state switch
    {
        ProgressRunState.Success => StateSuccess,
        ProgressRunState.Error => StateError,
        ProgressRunState.Paused => StatePaused,
        _ => StateIdle
    };

    private void UpdateAutomationName()
    {
        var source = DependencyPropertyHelper.GetValueSource(this, AutomationProperties.NameProperty).BaseValueSource;
        if (source == BaseValueSource.Local) return;

        var name = StatusText;
        if (string.IsNullOrWhiteSpace(name)) name = Kind.ToString();
        SetCurrentValue(AutomationProperties.NameProperty, name);
    }

    #endregion
}
