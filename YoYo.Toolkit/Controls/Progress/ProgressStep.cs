using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// <see cref="StepTracker"/> 的一个步骤。
/// <see cref="State"/> 通常由控件按 <see cref="StepTracker.CurrentIndex"/> 推导；
/// 调用方把它设为 <see cref="ProgressStepState.Error"/> 或 <see cref="ProgressStepState.Skipped"/>
/// 时，控件会保留该状态不再覆盖。
/// </summary>
public sealed class ProgressStep : INotifyPropertyChanged
{
    private string? _label;
    private string? _description;
    private ProgressStepState _state = ProgressStepState.Waiting;
    private string _ordinal = string.Empty;
    private bool _isFirst;
    private bool _isLast;

    /// <summary>步骤名，显示在节点下方。</summary>
    public string? Label
    {
        get => _label;
        set => SetField(ref _label, value);
    }

    /// <summary>补充说明，可选。</summary>
    public string? Description
    {
        get => _description;
        set => SetField(ref _description, value);
    }

    /// <summary>当前状态。</summary>
    public ProgressStepState State
    {
        get => _state;
        set => SetField(ref _state, value);
    }

    /// <summary>序号文本（从 1 开始），由控件维护。</summary>
    public string Ordinal
    {
        get => _ordinal;
        internal set => SetField(ref _ordinal, value);
    }

    /// <summary>是否为第一步，由控件维护。</summary>
    public bool IsFirst
    {
        get => _isFirst;
        internal set => SetField(ref _isFirst, value);
    }

    /// <summary>是否为最后一步，用于隐藏末段连接线。</summary>
    public bool IsLast
    {
        get => _isLast;
        internal set => SetField(ref _isLast, value);
    }

    /// <summary>该状态是否由调用方显式锁定。</summary>
    internal bool IsStatePinned => _state is ProgressStepState.Error or ProgressStepState.Skipped;

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    public override string ToString() => Label ?? base.ToString() ?? string.Empty;

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
