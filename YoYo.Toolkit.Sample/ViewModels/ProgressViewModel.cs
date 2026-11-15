using System;
using System.Collections.Generic;
using System.Windows.Input;
using YoYo.Toolkit.Controls.Progress;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>进度页：把原来单窗口的演示整页搬进来，改成绑定驱动，暂停不再遍历视觉树。</summary>
public sealed class ProgressViewModel : PageViewModelBase
{
    private double _progressValue = 62;
    private int _currentStep = 2;
    private bool _isPaused;

    public ProgressViewModel(INavigationHost? host) : base(host)
    {
        RingKinds = new[]
        {
            new KindSample(ProgressKind.Arc, "08 旋转弧环", 96, 96),
            new KindSample(ProgressKind.MarchingRing, "12 行进虚线环", 96, 96),
            new KindSample(ProgressKind.DualRing, "19 对流双环", 96, 96),
            new KindSample(ProgressKind.GlowRing, "24 流光环", 96, 96),
            new KindSample(ProgressKind.Sonar, "10 声纳波纹", 96, 96),
            new KindSample(ProgressKind.Radar, "18 雷达扫描", 96, 96),
            new KindSample(ProgressKind.Breathing, "21 呼吸光球", 96, 96),
            new KindSample(ProgressKind.Petal, "23 花瓣绽放", 96, 96),
            new KindSample(ProgressKind.Orbit, "06 轨道点", 96, 96),
            new KindSample(ProgressKind.TickRing, "15 刻度游标", 96, 96),
            new KindSample(ProgressKind.Comet, "20 流星尾迹", 96, 96),
            new KindSample(ProgressKind.DotMatrix, "17 点阵涟漪", 96, 96)
        };

        BarKinds = new[]
        {
            new KindSample(ProgressKind.FlowBar, "07 流动扫光条", 190, 44),
            new KindSample(ProgressKind.CounterFlow, "13 对向游走", 190, 44),
            new KindSample(ProgressKind.Scan, "22 屏幕扫描", 150, 60),
            new KindSample(ProgressKind.Skeleton, "11 骨架扫光", 190, 50),
            new KindSample(ProgressKind.Wave, "16 行波线", 190, 44),
            new KindSample(ProgressKind.Signal, "05 信号带", 190, 50)
        };

        Steps = new[]
        {
            new ProgressStep { Label = "初始化" },
            new ProgressStep { Label = "预热" },
            new ProgressStep { Label = "采集" },
            new ProgressStep { Label = "分析" },
            new ProgressStep { Label = "报告" }
        };

        StateSteps = new[]
        {
            new ProgressStep { Label = "完成" },
            new ProgressStep { Label = "失败", State = ProgressStepState.Error },
            new ProgressStep { Label = "跳过", State = ProgressStepState.Skipped },
            new ProgressStep { Label = "进行中" },
            new ProgressStep { Label = "等待" }
        };

        TogglePauseCommand = new RelayCommand(() => IsPaused = !IsPaused);
        PrevStepCommand = new RelayCommand(() => CurrentStep = Math.Max(-1, CurrentStep - 1));
        NextStepCommand = new RelayCommand(() => CurrentStep = Math.Min(Steps.Count, CurrentStep + 1));
    }

    public override string ViewType => "ProgressView";

    public override string Title => "进度";

    public override string Lede => "20 种不确定动效 + 三套确定态 + 流程步骤条；换肤只换令牌，模板一行不改。";

    public IReadOnlyList<KindSample> RingKinds { get; }

    public IReadOnlyList<KindSample> BarKinds { get; }

    public IReadOnlyList<ProgressStep> Steps { get; }

    /// <summary>五种状态各占一格，验证显式 Error / Skipped 不被 CurrentIndex 覆盖。</summary>
    public IReadOnlyList<ProgressStep> StateSteps { get; }

    public ICommand TogglePauseCommand { get; }

    public ICommand PrevStepCommand { get; }

    public ICommand NextStepCommand { get; }

    public double ProgressValue
    {
        get => _progressValue;
        set => SetField(ref _progressValue, value);
    }

    public int CurrentStep
    {
        get => _currentStep;
        set => SetField(ref _currentStep, value);
    }

    public bool IsPaused
    {
        get => _isPaused;
        set
        {
            if (!SetField(ref _isPaused, value)) return;
            OnPropertyChanged(nameof(IsRunning));
            OnPropertyChanged(nameof(StatusText));
            OnPropertyChanged(nameof(PauseLabel));
        }
    }

    /// <summary>所有指示器的 IsActive 都绑它——暂停即状态机切到 Idle，故事板自然停。</summary>
    public bool IsRunning => !_isPaused;

    public string StatusText => _isPaused ? "已暂停：故事板已停止" : "运行中";

    public string PauseLabel => _isPaused ? "继续动画" : "暂停动画";

    /// <summary>命令行验收入口：等价于点一次「暂停动画」。</summary>
    public void SetAllPaused(bool paused) => IsPaused = paused;
}
