# YoYo.Toolkit

面向 WPF 桌面应用的轻量工具集，为上位机、工控、仪器等「界面 + 设备通信」类项目准备。

全部基于原生 WPF 实现，除 `System.Management` 外无第三方依赖；不引入 MVVM 框架，也不强制目录约定，可按需取用其中任意一块。

- 目标框架：`net6.0-windows`、`net8.0-windows`
- 平台：`AnyCPU` / `x64` / `x86`
- 许可证：MIT

## 特性一览

| 模块 | 命名空间 | 内容 |
| --- | --- | --- |
| 导航 | `YoYo.Toolkit.Navigations` | `BaseNavigation` 页面导航，含进入/离开生命周期钩子与强类型参数 |
| 窗口 | `YoYo.Toolkit.Services` | `WindowService` 多窗口注册、跨线程关闭 |
| 网络 | `YoYo.Toolkit.Net.Http`、`.Pings` | `BaseRequest` / `BaseResponse` 请求基类；`NetworkPing` 连通性检测 |
| 控件 | `YoYo.Toolkit.Controls.*` | 环形进度、树形下拉、水印输入框、图标选项、描边文字 |
| 附加行为 | `YoYo.Toolkit.Input`、`.Animations` | `PasswordHelper` 密码绑定；`OSKHelper` 触摸屏软键盘 |
| 通知 | `YoYo.Toolkit.Notifies` | `TMessageBox` 可换肤消息框 |
| 系统 | `YoYo.Toolkit.System`、`.WinApis` | 硬盘序列号、主板 ID、机器名；`Kernel32.CopyMemory` |
| 加密 | `YoYo.Toolkit.CryptoServices` | `MD5Helper` 摘要；`RSAHelper` AES/RSA 加解密 |
| 工具 | `YoYo.Toolkit.Time`、`.Images`、`.Converters` | Unix 时间戳、位图加载、常用值转换器 |

## 安装

```bash
dotnet add package YoYo.Toolkit
```

或在 `PackageReference` 中指定版本：

```xml
<PackageReference Include="YoYo.Toolkit" Version="1.0.0" />
```

宿主项目需启用 WPF：

```xml
<PropertyGroup>
  <UseWPF>true</UseWPF>
</PropertyGroup>
```

## 快速开始

### 1. 页面导航

导航以 `IGetService` 为底座，视图与 ViewModel 都由外部容器提供，包本身不绑定任何 DI 框架。

```csharp
public sealed class ShellService : IGetService
{
    private readonly IServiceProvider _provider;
    private FrameworkElement? _view;

    public ShellService(IServiceProvider provider) => _provider = provider;

    public object GetService(Type? t) => _provider.GetService(t!)!;
    public FrameworkElement? GetView() => _view;
    public void SetView(FrameworkElement? view) => _view = view;
}

public sealed class MainNavigation : BaseNavigation
{
    public MainNavigation(IGetService service) : base(service) { }

    public override string NavigationName => "Main";

    public override List<NavigateModel> Navigates { get; } = new()
    {
        new NavigateModel
        {
            ViewType = "HomeView",
            Type = typeof(HomeView),
            VMType = typeof(HomeViewModel),
            Name = "首页",
        },
        new NavigateModel
        {
            ViewType = "DeviceView",
            Type = typeof(DeviceView),
            VMType = typeof(DeviceViewModel),
            Name = "设备",
        },
    };

    public override void InitView() { }
    public override void LogWrite(string? msg) { }
    public override void LogWrite(string? msg, Exception? exception) { }
}
```

页面 ViewModel 实现 `INavigateIn` / `INavigateIn<T>` / `INavigateOut`，即可在切换时收到回调：

```csharp
public sealed class DeviceViewModel : INavigateIn<string>, INavigateOut
{
    public void NavigateIn(string? deviceId) { /* 初始化并加载 deviceId */ }
    public void NavigateOut() { /* 停止轮询、释放资源 */ }
}

// 跳转
navigation.GoTo("DeviceView", "SN-0001");
navigation.GoTo("HomeView");
```

### 2. HTTP 请求

继承 `BaseRequest` 描述接口，继承 `BaseResponse` 描述返回，序列化方式可自行覆盖。

```csharp
public sealed class LoginRequest : BaseRequest
{
    public override string RootUrl => "https://api.example.com";
    public override string BusinessUrl => "/api/login";
    public override HttpMethodTypes Method => HttpMethodTypes.POST;
    public override Type ResponseType => typeof(LoginResponse);

    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public override void SerializeObject()
        => Data = JsonSerializer.Serialize(new { User, Password });
}

public sealed class LoginResponse : BaseResponse
{
    public string Token { get; private set; } = string.Empty;

    public override void DeserializeObj()
    {
        using var doc = JsonDocument.Parse(Data);
        var root = doc.RootElement;
        IsOk = root.GetProperty("code").GetInt32() == 0;
        Msg = root.TryGetProperty("msg", out var msg) ? msg.GetString() ?? string.Empty : string.Empty;
        if (IsOk && root.TryGetProperty("token", out var token))
            Token = token.GetString() ?? string.Empty;
    }
}
```

异步调用（内部使用 `BackgroundWorker`，回调回到 UI 线程）：

```csharp
var request = new LoginRequest { User = "admin", Password = "123456" };
request.DoServiceAsyncCompleted += response => { /* 处理结果 */ };
request.Error += (message, ex) => { /* 统一错误处理 */ };
request.DoWorkerAsync();

// 同步调用
var syncResponse = request.DoWorkerSync();
```

支持的请求方式：`GET`、`POST`、`PUT`、`DELETE`，以及带 Body 的 `GetBody`。鉴权头可在 `SetHeader(HttpClient)` 中统一注入；请求前后的等待界面通过重写 `ShowProcess()` / `CloseProcess()` 接管，`IsShowProcess` 控制是否启用。

### 3. 控件

```xml
<Window xmlns:yy="clr-namespace:YoYo.Toolkit.Controls.Text;assembly=YoYo.Toolkit"
        xmlns:busy="clr-namespace:YoYo.Toolkit.Controls.BusyIndicators;assembly=YoYo.Toolkit"
        xmlns:btn="clr-namespace:YoYo.Toolkit.Controls.Button;assembly=YoYo.Toolkit">
  <StackPanel>
    <!-- 水印输入框 -->
    <yy:PlaceholderTextBox PlaceHolderText="请输入样本编号" />

    <!-- 环形进度，可在模板中用 VisualState 驱动 -->
    <busy:TProgressRing IsActive="True" Width="36" Height="36" />

    <!-- 图标 + 文字的选项按钮 -->
    <btn:IconRadioButton Icon="&#xE7BA;" Content="运行" GroupName="Mode" />

    <!-- 描边文字 -->
    <yy:OutlineText Text="SYSTEM READY" StrokePosition="Outside" />
  </StackPanel>
</Window>
```

树形下拉要求节点实现 `ITreeModel`（`DisplayName`、`HasChildren()`），选中后可通过 `SelectCommand` 回传节点：

```xml
<combo:TreeComboBox ItemsSource="{Binding Devices}"
                    IsDorpDown="True"
                    IsOnlyLastNode="True"
                    SelectCommand="{Binding SelectDeviceCommand}" />
```

> **注意**：本包不内置主题资源字典。`TProgressRing`、`IconRadioButton` 等通过 `DefaultStyleKey` 查找样式的控件，需要在宿主应用中提供 `Style` / `ControlTemplate`，否则控件不会呈现内容。`OutlineText`、`PlaceholderTextBox` 以代码绘制为主，可直接使用。

### 4. 消息框

`TMessageBox` 不依赖系统 `MessageBox`，外观由你提供的窗口决定：

```csharp
// 启动时注册弹窗实现，构造函数须为 (string caption, string msg, MessageLevel level)
TMessageBox.Init(typeof(MyMessageDialog));

var result = TMessageBox.ShowMsg("提示", "确认开始运行？", TMessageBox.MessageLevel.Question);
if (result == TMessageBox.TMessageBoxResult.OK)
{
    // ...
}
```

`MyMessageDialog` 继承 `BaseMessageDialog`，在关闭前给 `MessageBoxResult` 赋值即可。

### 5. 设备与授权

```csharp
SystemHelper.Init();                       // 读取硬盘序列号、主板 ID、机器名
var diskId = SystemHelper.HardDiskID;      // 可用于授权绑定
var boardId = SystemHelper.ServerId;
var machine = SystemHelper.ComputerName;

var reachable = await NetworkPing.PingHost("192.168.1.10") is not null;

var md5 = MD5Helper.Md5("hello");
var cipher = RSAHelper.AESEncrypt("plain-text", "your-key");
```

> 加解密示例仅作接口参考。用于生产环境前，请自行确认密钥管理、IV 与填充模式是否符合你的安全要求。

### 6. 密码框绑定

```xml
<PasswordBox input:PasswordHelper.Attach="True"
             input:PasswordHelper.Password="{Binding Password, Mode=TwoWay}" />
```

### 7. 值转换器

```xml
<ResourceDictionary xmlns:conv="clr-namespace:YoYo.Toolkit.Converters;assembly=YoYo.Toolkit">
  <conv:BoolToVisibilityConverter x:Key="BoolToVis" />
  <conv:BoolToReverseConverter x:Key="BoolReverse" />
  <conv:NullToVisibilityConverter x:Key="NullToVis" />
</ResourceDictionary>
```

## 项目结构

```
YoYo.Toolkit/
├─ Animations/      动画附加属性
├─ Controls/        BusyIndicators、Button、ComboBox、Text
├─ Converters/      Bool / Null 与 Visibility 等转换器
├─ CryptoServices/  MD5、RSA(AES)
├─ Images/          位图加载扩展
├─ Input/           PasswordBox 绑定、触摸屏软键盘
├─ Interfaces/      窗口与导航接口
├─ Models/          NavigateModel
├─ Navigations/     BaseNavigation
├─ Net/             HTTP 基类、Ping
├─ Notifies/        TMessageBox
├─ Services/        WindowService
├─ System/          硬件与机器信息
├─ Time/            Unix 时间戳
└─ WinApis/         kernel32 互操作
```

示例工程见 `YoYo.Toolkit.Sample`。

## 构建与打包

```bash
# 构建
dotnet build YoYo.Toolkit/YoYo.Toolkit.csproj -c Release -p:Platform=x64

# 打包
dotnet pack YoYo.Toolkit/YoYo.Toolkit.csproj -c Release -p:Platform=x64 -o artifacts
```

## 许可证

[MIT](LICENSE) © 2025 在梦想路上狂奔的蜗牛
