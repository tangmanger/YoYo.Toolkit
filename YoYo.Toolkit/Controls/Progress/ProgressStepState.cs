namespace YoYo.Toolkit.Controls.Progress;

/// <summary>步骤状态。<see cref="Error"/> 与 <see cref="Skipped"/> 由调用方显式指定，不会被自动推导覆盖。</summary>
public enum ProgressStepState
{
    /// <summary>尚未到达。</summary>
    Waiting = 0,

    /// <summary>当前进行中。</summary>
    Active,

    /// <summary>已完成。</summary>
    Done,

    /// <summary>失败。显式状态，自动推导不会覆盖。</summary>
    Error,

    /// <summary>已跳过。显式状态，自动推导不会覆盖。</summary>
    Skipped
}
