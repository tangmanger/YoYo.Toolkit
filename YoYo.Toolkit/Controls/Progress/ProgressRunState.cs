namespace YoYo.Toolkit.Controls.Progress;

/// <summary>
/// 进度语义状态，映射到模板内 <c>ProgressStates</c> 视觉状态组。
/// 注意与 <see cref="ProgressIndicator.IsActive"/> 的分工：
/// <c>State</c> 描述“处于哪个阶段”，<c>IsActive</c> 描述“动画是否在跑”。
/// 只有 <see cref="Running"/> 且 <c>IsActive=true</c> 时才会启动循环动画。
/// </summary>
public enum ProgressRunState
{
    /// <summary>空闲，静止帧。</summary>
    Idle = 0,

    /// <summary>运行中，循环动画。</summary>
    Running,

    /// <summary>成功终态，静止帧 + 语义色。</summary>
    Success,

    /// <summary>失败终态，静止帧 + 语义色。</summary>
    Error,

    /// <summary>暂停，保持当前视觉但不推进。</summary>
    Paused
}
