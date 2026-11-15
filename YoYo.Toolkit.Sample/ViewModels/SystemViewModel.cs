using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using YoYo.Toolkit.Images;
using YoYo.Toolkit.Time;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 系统与工具页。
/// <para>
/// 顺便演示生命周期：进入页面开一个 1s 的时钟，离开时停掉并退订——
/// 这就是 <c>NavigateIn</c> / <c>NavigateOut</c> 存在的意义，而不是装饰。
/// </para>
/// <para>
/// 注意 <see cref="DateTimeHelper"/> 的单位不一致：<c>GetUnixTime()</c> 返回**秒**，
/// 而 <c>UnixTimestampToDateTimeNoTimeZone</c> 按**毫秒**解释。页面把两者都摆出来，避免踩坑。
/// </para>
/// </summary>
public sealed class SystemViewModel : PageViewModelBase
{
    private readonly DispatcherTimer _clock;
    private readonly RelayCommand _refreshCommand;

    private string _computerName = "—";
    private string _hardDiskId = "—";
    private string _serverId = "—";
    private string _fingerprint = "—";
    private string _unixSeconds = "—";
    private string _unixMilliseconds = "—";
    private string _nowLocal = "—";
    private string _tsInput;
    private string _tsOutput = "—";
    private string _dtInput = "2026-11-13 22:14:00";
    private string _dtOutput = "—";
    private string _initState = "未初始化";
    private string _imagePath = "—";
    private string _imageInfo = "未加载";
    private string _imageStatus = "选一张本地图片，再覆盖同一路径验证无文件锁";
    private BitmapImage? _loadedImage;
    private bool _canOverwrite;

    public SystemViewModel(INavigationHost? host) : base(host)
    {
        _tsInput = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();

        Modules = new[]
        {
            new ToolModule("YoYo.Toolkit.System", "SystemHelper", "硬盘序列号、主板 ID、机器名，静态读取"),
            new ToolModule("YoYo.Toolkit.Time", "DateTimeHelper", "Unix 时间戳与本地时间互转（不带时区）"),
            new ToolModule("YoYo.Toolkit.Images", "ImageHelper", "位图加载，不锁文件句柄"),
            new ToolModule("YoYo.Toolkit.Converters", "Bool / Null / Percent", "绑定转换，含百分比 → 弧线几何"),
            new ToolModule("YoYo.Toolkit.WinApis", "Kernel32", "CopyMemory 等 kernel32 互操作"),
            new ToolModule("YoYo.Toolkit.Animations", "AnimationHelper", "附加属性式动画起点")
        };

        _refreshCommand = new RelayCommand(async () => await RefreshAsync());
        RefreshCommand = _refreshCommand;
        ConvertTimestampCommand = new RelayCommand(ConvertTimestamp);
        ConvertDateTimeCommand = new RelayCommand(ConvertDateTime);
        PickImageCommand = new RelayCommand(PickImage);
        OverwriteImageCommand = new RelayCommand(OverwriteImage);

        _clock = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromSeconds(1) };
        _clock.Tick += (_, _) => Tick();
    }

    public override string ViewType => "SystemView";

    public override string Title => "系统与工具";

    public override string Lede => "硬盘序列号、主板 ID、Unix 时间戳与跨命名空间的工具清单。";

    public IReadOnlyList<ToolModule> Modules { get; }

    public ICommand RefreshCommand { get; }
    public ICommand ConvertTimestampCommand { get; }
    public ICommand ConvertDateTimeCommand { get; }
    public ICommand PickImageCommand { get; }
    public ICommand OverwriteImageCommand { get; }

    // ---------- ImageHelper ----------

    public string ImagePath
    {
        get => _imagePath;
        private set => SetField(ref _imagePath, value);
    }

    public string ImageInfo
    {
        get => _imageInfo;
        private set => SetField(ref _imageInfo, value);
    }

    public string ImageStatus
    {
        get => _imageStatus;
        private set => SetField(ref _imageStatus, value);
    }

    public BitmapImage? LoadedImage
    {
        get => _loadedImage;
        private set => SetField(ref _loadedImage, value);
    }

    public bool CanOverwrite
    {
        get => _canOverwrite;
        private set => SetField(ref _canOverwrite, value);
    }

    public string ComputerName
    {
        get => _computerName;
        private set => SetField(ref _computerName, value);
    }

    public string HardDiskId
    {
        get => _hardDiskId;
        private set => SetField(ref _hardDiskId, value);
    }

    public string ServerId
    {
        get => _serverId;
        private set => SetField(ref _serverId, value);
    }

    public string Fingerprint
    {
        get => _fingerprint;
        private set => SetField(ref _fingerprint, value);
    }

    public string UnixSeconds
    {
        get => _unixSeconds;
        private set => SetField(ref _unixSeconds, value);
    }

    public string UnixMilliseconds
    {
        get => _unixMilliseconds;
        private set => SetField(ref _unixMilliseconds, value);
    }

    public string NowLocal
    {
        get => _nowLocal;
        private set => SetField(ref _nowLocal, value);
    }

    public string InitState
    {
        get => _initState;
        private set => SetField(ref _initState, value);
    }

    public string TimestampInput
    {
        get => _tsInput;
        set => SetField(ref _tsInput, value);
    }

    public string TimestampOutput
    {
        get => _tsOutput;
        private set => SetField(ref _tsOutput, value);
    }

    public string DateTimeInput
    {
        get => _dtInput;
        set => SetField(ref _dtInput, value);
    }

    public string DateTimeOutput
    {
        get => _dtOutput;
        private set => SetField(ref _dtOutput, value);
    }

    protected override void OnNavigatedIn()
    {
        _clock.Start();
        Tick();

        if (InitState == "未初始化") _ = RefreshAsync();
    }

    protected override void OnNavigatedOut() => _clock.Stop();

    private void Tick()
    {
        UnixSeconds = DateTimeHelper.GetUnixTime();
        UnixMilliseconds = (long.Parse(UnixSeconds) * 1000L).ToString();
        NowLocal = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        OnPropertyChanged(nameof(UnitHint));
    }

    public string UnitHint => "GetUnixTime() 返回秒；UnixTimestampToDateTimeNoTimeZone(long) 按毫秒解释";

    private async Task RefreshAsync()
    {
        InitState = "读取中…";

        var (ok, message) = await Task.Run(() =>
        {
            try
            {
                YoYo.Toolkit.System.SystemHelper.Init();
                return (true, "ok");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
        });

        if (!ok)
        {
            InitState = "初始化失败";
            ComputerName = Environment.MachineName;
            HardDiskId = "—";
            ServerId = "—";
            Fingerprint = "—";
            Log($"SystemHelper.Init() 失败 · {message}", true);
            return;
        }

        ComputerName = YoYo.Toolkit.System.SystemHelper.ComputerName ?? "—";
        HardDiskId = string.IsNullOrWhiteSpace(YoYo.Toolkit.System.SystemHelper.HardDiskID)
            ? "—"
            : YoYo.Toolkit.System.SystemHelper.HardDiskID;
        ServerId = string.IsNullOrWhiteSpace(YoYo.Toolkit.System.SystemHelper.ServerId)
            ? "—"
            : YoYo.Toolkit.System.SystemHelper.ServerId;

        Fingerprint = HardDiskId == "—" && ServerId == "—"
            ? "—"
            : YoYo.Toolkit.CryptoServices.MD5Helper.Md5(HardDiskId + ServerId);

        InitState = "已初始化";
        Log($"SystemHelper.Init() 完成 · {ComputerName}");
    }

    private void ConvertTimestamp()
    {
        if (!long.TryParse(TimestampInput?.Trim(), out var value))
        {
            TimestampOutput = "请输入整数";
            return;
        }

        try
        {
            TimestampOutput = DateTimeHelper.UnixTimestampToDateTimeNoTimeZone(value)
                .ToString("yyyy-MM-dd HH:mm:ss.fff");
        }
        catch (Exception ex)
        {
            TimestampOutput = ex.Message;
        }
    }

    private void ConvertDateTime()
    {
        if (!DateTime.TryParse(DateTimeInput, out var value))
        {
            DateTimeOutput = "请输入 yyyy-MM-dd HH:mm:ss";
            return;
        }

        DateTimeOutput = DateTimeHelper.DateTimeToUnixTimestampNoTimeZone(value).ToString();
    }

    /// <summary>真实调用 string.GetBitmapImage()：先读进 MemoryStream，OnLoad 后即释放句柄。</summary>
    private void PickImage()
    {
        var dialog = new OpenFileDialog
        {
            Title = "选择一张图片（ImageHelper 演示）",
            Filter = "图片文件|*.png;*.jpg;*.jpeg;*.bmp;*.gif|所有文件|*.*"
        };
        if (dialog.ShowDialog() != true) return;

        try
        {
            var bitmap = dialog.FileName.GetBitmapImage();
            if (bitmap is null)
            {
                ImageStatus = "文件不存在或无法解码";
                Log("GetBitmapImage() 返回 null", true);
                return;
            }

            ImagePath = dialog.FileName;
            LoadedImage = bitmap;
            var size = new FileInfo(dialog.FileName).Length;
            ImageInfo = $"{bitmap.PixelWidth} × {bitmap.PixelHeight} · {size / 1024d:F0} KB";
            ImageStatus = "已加载 · MemoryStream 已释放，文件句柄未占用";
            CanOverwrite = true;
            Log($"\"{Path.GetFileName(dialog.FileName)}\".GetBitmapImage() · {ImageInfo}");
        }
        catch (Exception ex)
        {
            ImageStatus = "加载失败：" + ex.Message;
            Log("GetBitmapImage() 失败 · " + ex.Message, true);
        }
    }

    /// <summary>往同一路径写回相同字节：能写成功就说明加载后没有占着文件句柄。</summary>
    private void OverwriteImage()
    {
        if (!CanOverwrite || string.IsNullOrEmpty(ImagePath) || !File.Exists(ImagePath)) return;

        try
        {
            var bytes = File.ReadAllBytes(ImagePath);
            File.WriteAllBytes(ImagePath, bytes);
            LoadedImage = ImagePath.GetBitmapImage();
            ImageStatus = "覆盖写入成功 · 加载后文件仍可写（无锁）";
            Log($"File.WriteAllBytes(\"{Path.GetFileName(ImagePath)}\") 成功 · ImageHelper 未持有句柄");
        }
        catch (Exception ex)
        {
            ImageStatus = "覆盖失败：" + ex.Message;
            Log("覆盖写入失败 · " + ex.Message, true);
        }
    }
}

/// <summary>工具模块清单的一行。</summary>
public sealed class ToolModule
{
    public ToolModule(string ns, string type, string usage)
    {
        Namespace = ns;
        Type = type;
        Usage = usage;
    }

    public string Namespace { get; }
    public string Type { get; }
    public string Usage { get; }
}
