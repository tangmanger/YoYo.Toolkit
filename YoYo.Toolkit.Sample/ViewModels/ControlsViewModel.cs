using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using System.Windows.Media;
using YoYo.Toolkit.Controls.Interfaces;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 控件页。
/// 本页同时是「宿主必须自己写模板」的示例：<c>TProgressRing</c> / <c>IconRadioButton</c> /
/// <c>TreeComboBox</c> / <c>PlaceholderTextBox</c> 都没有内置主题字典，样式定义在 <c>Themes/Controls.xaml</c>。
/// </summary>
public sealed class ControlsViewModel : PageViewModelBase
{
    private string _placeholderText = string.Empty;
    private string _errorText = "SAMPLE-0000";
    private string _selectedDevice = "请选择设备";
    private bool _isDropDown;
    private bool _isOnlyLastNode = true;
    private string _selectedModeLabel = "运行";
    private bool _isRingActive = true;
    private bool _isPasswordVisible;
    private string _password = "admin123";
    private StrokeKind _stroke = StrokeKind.Outside;
    private SolidColorBrush _iconBrush = new(Color.FromRgb(0x0E, 0x7C, 0x7B));

    // 值转换器演示
    private bool _converterFlag = true;
    private string _nullDemo = "SN-0001";
    private double _percentDemo = 0.62;

    public ControlsViewModel(INavigationHost? host) : base(host)
    {
        DeviceRoots = new[]
        {
            new DeviceNode("产线 A · 装配", new[]
            {
                new DeviceNode("设备 A-01"), new DeviceNode("设备 A-02"), new DeviceNode("设备 A-03")
            }),
            new DeviceNode("产线 B · 老化", new[]
            {
                new DeviceNode("设备 B-01"), new DeviceNode("设备 B-02")
            })
        };

        Modes = new[]
        {
            new ModeOption("运行", "M8,5.4 L18,12 L8,18.6 Z") { IsSelected = true },
            new ModeOption("暂停", "M9,5.4 L9,18.6 M15,5.4 L15,18.6"),
            new ModeOption("停止", "M7,7 L17,17 M17,7 L7,17")
        };

        foreach (var mode in Modes) mode.PropertyChanged += OnModeChanged;

        SelectDeviceCommand = new RelayCommand<object?>(OnDeviceSelected);

        TintAccentCommand = new RelayCommand(() => IconBrush = Pick("Sample.Accent"));
        TintWarnCommand = new RelayCommand(() => IconBrush = Pick("Sample.Warn"));
        TintOkCommand = new RelayCommand(() => IconBrush = Pick("Sample.Ok"));
        TintErrCommand = new RelayCommand(() => IconBrush = Pick("Sample.Err"));

        StrokeOutsideCommand = new RelayCommand(() => Stroke = StrokeKind.Outside);
        StrokeCenterCommand = new RelayCommand(() => Stroke = StrokeKind.Center);
        StrokeInsideCommand = new RelayCommand(() => Stroke = StrokeKind.Inside);

        TogglePasswordCommand = new RelayCommand(() => IsPasswordVisible = !IsPasswordVisible);

        // 示例不真的拉起 osk.exe，避免在演示机上弹出系统键盘；换成 OSKHelper.StartOsk() 即可。
        OskCommand = new RelayCommand(() => Log("OSKHelper.StartOsk() —— 示例不实际启动系统键盘"));
        CopyMd5NoteCommand = new RelayCommand(() => Log("PasswordHelper.Attach=\"True\" 已在 PasswordBox 上启用"));
    }

    public override string ViewType => "ControlsView";

    public override string Title => "控件";

    public override string Lede => "水印输入框、树形下拉、图标选项、描边文字、圆环；四个控件都需要宿主提供模板。";

    // ---------- PlaceholderTextBox ----------

    public string PlaceholderText
    {
        get => _placeholderText;
        set => SetField(ref _placeholderText, value);
    }

    public string ErrorText
    {
        get => _errorText;
        set => SetField(ref _errorText, value);
    }

    // ---------- TreeComboBox ----------

    public IReadOnlyList<DeviceNode> DeviceRoots { get; }

    public string SelectedDevice
    {
        get => _selectedDevice;
        private set => SetField(ref _selectedDevice, value);
    }

    public bool IsDropDown
    {
        get => _isDropDown;
        set
        {
            if (!SetField(ref _isDropDown, value)) return;
            OnPropertyChanged(nameof(DropStateText));
        }
    }

    public bool IsOnlyLastNode
    {
        get => _isOnlyLastNode;
        set
        {
            if (!SetField(ref _isOnlyLastNode, value)) return;
            OnPropertyChanged(nameof(LeafStateText));
        }
    }

    public string DropStateText => _isDropDown ? "下拉展开" : "已收起";
    public string LeafStateText => _isOnlyLastNode ? "仅末级可选" : "任意级可选";

    public ICommand SelectDeviceCommand { get; }

    /// <summary>
    /// TreeView 的 SelectedItem 在数据绑定下是数据项；用 ElementName 静态节点时会是 TreeViewItem。
    /// 两种都兜住，免得换个写法就崩。
    /// </summary>
    private void OnDeviceSelected(object? parameter)
    {
        var data = parameter switch
        {
            DeviceNode node => node,
            System.Windows.Controls.TreeViewItem item => item.DataContext as DeviceNode,
            _ => null
        };

        var text = data?.DisplayName ?? "—";
        SelectedDevice = text;
        Log($"SelectCommand.Execute({text})");
    }

    // ---------- IconRadioButton ----------

    public IReadOnlyList<ModeOption> Modes { get; }

    public string SelectedModeLabel
    {
        get => _selectedModeLabel;
        private set => SetField(ref _selectedModeLabel, value);
    }

    public SolidColorBrush IconBrush
    {
        get => _iconBrush;
        private set => SetField(ref _iconBrush, value);
    }

    public ICommand TintAccentCommand { get; }
    public ICommand TintWarnCommand { get; }
    public ICommand TintOkCommand { get; }
    public ICommand TintErrCommand { get; }

    private static SolidColorBrush Pick(string key)
        => System.Windows.Application.Current?.TryFindResource(key) as SolidColorBrush ?? new SolidColorBrush(Colors.Black);

    private void OnModeChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName != nameof(ModeOption.IsSelected)) return;
        if (sender is ModeOption { IsSelected: true } mode) SelectedModeLabel = mode.Label;
    }

    // ---------- OutlineText ----------

    public StrokeKind Stroke
    {
        get => _stroke;
        private set
        {
            if (!SetField(ref _stroke, value)) return;
            OnPropertyChanged(nameof(IsStrokeOutside));
            OnPropertyChanged(nameof(IsStrokeCenter));
            OnPropertyChanged(nameof(IsStrokeInside));
        }
    }

    public bool IsStrokeOutside => _stroke == StrokeKind.Outside;
    public bool IsStrokeCenter => _stroke == StrokeKind.Center;
    public bool IsStrokeInside => _stroke == StrokeKind.Inside;

    /// <summary>命令行验收注入：--stroke Outside/Center/Inside。</summary>
    public void SetStroke(StrokeKind kind) => Stroke = kind;

    public ICommand StrokeOutsideCommand { get; }
    public ICommand StrokeCenterCommand { get; }
    public ICommand StrokeInsideCommand { get; }

    // ---------- TProgressRing ----------

    public bool IsRingActive
    {
        get => _isRingActive;
        set => SetField(ref _isRingActive, value);
    }

    // ---------- PasswordHelper ----------

    public string Password
    {
        get => _password;
        set => SetField(ref _password, value);
    }

    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set
        {
            if (!SetField(ref _isPasswordVisible, value)) return;
            OnPropertyChanged(nameof(RevealLabel));
            OnPropertyChanged(nameof(HiddenPassword));
        }
    }

    public string RevealLabel => _isPasswordVisible ? "隐藏" : "显示";

    /// <summary>PasswordBox 不能直接 TwoWay 绑定，显示明文时用等长星号代替。</summary>
    public string HiddenPassword => new('•', _password.Length);

    public ICommand TogglePasswordCommand { get; }

    public ICommand OskCommand { get; }

    public ICommand CopyMd5NoteCommand { get; }

    // ---------- 值转换器 ----------

    public bool ConverterFlag
    {
        get => _converterFlag;
        set
        {
            if (!SetField(ref _converterFlag, value)) return;
            OnPropertyChanged(nameof(ConverterFlagText));
        }
    }

    public string ConverterFlagText => _converterFlag ? "可见" : "已折叠";

    public string NullDemo
    {
        get => _nullDemo;
        set
        {
            if (!SetField(ref _nullDemo, value)) return;
            OnPropertyChanged(nameof(NullDemoText));
        }
    }

    public string NullDemoText => string.IsNullOrEmpty(_nullDemo) ? "null → 折叠" : "非空";

    public double PercentDemo
    {
        get => _percentDemo;
        set => SetField(ref _percentDemo, value);
    }
}

/// <summary>描边位置的三档，仅用于 UI 选择。</summary>
public enum StrokeKind
{
    Outside,
    Center,
    Inside
}

/// <summary>树节点：实现 <see cref="ITreeModel"/> 就能被 TreeComboBox 取用。</summary>
public sealed class DeviceNode : ITreeModel
{
    private readonly IReadOnlyList<DeviceNode> _children;

    public DeviceNode(string displayName, IReadOnlyList<DeviceNode>? children = null)
    {
        DisplayName = displayName;
        _children = children ?? Array.Empty<DeviceNode>();
    }

    public string DisplayName { get; }

    public IReadOnlyList<DeviceNode> Children => _children;

    public bool HasChildren() => _children.Count > 0;
}

/// <summary>图标选项：<c>IconRadioButton.Icon</c> 收的是路径数据字符串。</summary>
public sealed class ModeOption : INotifyPropertyChanged
{
    private bool _isSelected;

    public ModeOption(string label, string iconData) 
    {
        Label = label;
        IconData = iconData;
    }

    public string Label { get; }
    public string IconData { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
