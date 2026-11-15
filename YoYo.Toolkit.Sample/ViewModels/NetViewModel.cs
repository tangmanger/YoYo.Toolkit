using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using YoYo.Toolkit.Net.Http;
using YoYo.Toolkit.Net.Pings;
using YoYo.ToolkitSample.Models;
using YoYo.ToolkitSample.Net;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 网络页：请求定义、DoWorkerAsync 的五个阶段、统一错误出口、连通性检测。
/// <para>
/// 「本地模拟」勾着时按阶段演示并产出示例 JSON，不真正发请求（示例不该依赖外网）；
/// 取消勾选则真的调 <see cref="BaseRequest.DoWorkerAsync"/>，会走真实的 HttpClient，
/// 失败时由 <see cref="BaseRequest.Error"/> 统一出口接住——这正是本页想讲的那件事。
/// </para>
/// </summary>
public sealed class NetViewModel : PageViewModelBase
{
    private RequestRow? _selected;
    private bool _isSimulated = true;
    private bool _isRunning;
    private string _elapsedText = "— ms";
    private string _payload = "{\n  // 点击「发送请求」查看返回\n}";
    private string _errorText = string.Empty;
    private bool _hasError;

    public NetViewModel(INavigationHost? host) : base(host)
    {
        Requests = new ObservableCollection<RequestRow>
        {
            new("POST", "/api/login", "LoginResponse", "LoginRequest",
                "{\n  \"code\": 0,\n  \"token\": \"eyJhbGciOiJIUzI1NiJ9\",\n  \"expiresIn\": 7200\n}", false),
            new("GET", "/api/device/status", "DeviceStatusResponse", "DeviceStatusRequest",
                "{\n  \"state\": \"Running\",\n  \"temperature\": 37,\n  \"cycles\": 18\n}", false),
            new("POST", "/api/result/upload", "UploadResponse", "UploadResultRequest",
                "{\n  \"receipt\": \"LIS-20261113-0017\",\n  \"accepted\": true\n}", false),
            new("GET", "/api/legacy/poll", "LegacyResponse", "LegacyPollRequest",
                "{\n  \"error\": \"upstream timeout\"\n}", true)
        };
        _selected = Requests[0];
        _selected.IsSelected = true;

        Stages = new ObservableCollection<RequestStage>
        {
            new("ShowProcess"), new("建连"), new("收发"), new("DeserializeObj"), new("CloseProcess")
        };

        SelectCommand = new RelayCommand<RequestRow>(row =>
        {
            if (row is null) return;
            foreach (var item in Requests) item.IsSelected = ReferenceEquals(item, row);
            _selected = row;
            OnPropertyChanged(nameof(SelectedRequest));
            ResetStages();
        });

        SendCommand = new RelayCommand(async () => await SendAsync());

        Pings = new ObservableCollection<PingRow>
        {
            new("127.0.0.1"), new("localhost"), new("192.0.2.1")
        };

        PingCommand = new RelayCommand(async () => await PingAllAsync());
    }

    public override string ViewType => "NetView";

    public override string Title => "网络";

    public override string Lede => "请求 / 响应基类、同步与异步、统一错误出口；连通性检测返回 PingReply?。";

    public ObservableCollection<RequestRow> Requests { get; }

    public RequestRow? SelectedRequest => _selected;

    public ObservableCollection<RequestStage> Stages { get; }

    public ObservableCollection<PingRow> Pings { get; }

    public ICommand SelectCommand { get; }

    public ICommand SendCommand { get; }

    public ICommand PingCommand { get; }

    /// <summary>默认本地模拟；取消后真的发请求。</summary>
    public bool IsSimulated
    {
        get => _isSimulated;
        set
        {
            if (!SetField(ref _isSimulated, value)) return;
            OnPropertyChanged(nameof(ModeText));
        }
    }

    public string ModeText => _isSimulated ? "本地模拟" : "真实调用";

    public bool IsRunning
    {
        get => _isRunning;
        private set => SetField(ref _isRunning, value);
    }

    public string ElapsedText
    {
        get => _elapsedText;
        private set => SetField(ref _elapsedText, value);
    }

    public string Payload
    {
        get => _payload;
        private set => SetField(ref _payload, value);
    }

    public string ErrorText
    {
        get => _errorText;
        private set => SetField(ref _errorText, value);
    }

    public bool HasError
    {
        get => _hasError;
        private set => SetField(ref _hasError, value);
    }

    private void ResetStages()
    {
        foreach (var stage in Stages)
        {
            stage.IsDone = false;
            stage.Milliseconds = "—";
        }
    }

    private async Task SendAsync()
    {
        if (IsRunning || _selected is null) return;

        IsRunning = true;
        HasError = false;
        ErrorText = string.Empty;
        ResetStages();

        var stopwatch = Stopwatch.StartNew();
        var request = _selected.CreateRequest();

        try
        {
            if (IsSimulated)
            {
                await RunStagesAsync(_selected.Fails);

                Payload = _selected.Fails
                    ? "{\n  \"isOk\": false,\n  \"msg\": \"服务器未返回信息!\"\n}"
                    : _selected.SampleJson;

                if (_selected.Fails)
                {
                    HasError = true;
                    ErrorText = "当前请求:https://api.local/api/legacy/poll发生错误! → 超时 30s";
                    Log($"Error · {_selected.BusinessUrl} 超时", true);
                }
                else
                {
                    Log($"DoWorkerAsync 完成 · {_selected.BusinessUrl}", false);
                }
            }
            else
            {
                var completion = new TaskCompletionSource<BaseResponse?>();

                request.DoServiceAsyncCompleted += response => completion.TrySetResult(response);
                request.Error += (message, exception) =>
                {
                    completion.TrySetResult(null);
                    HasError = true;
                    ErrorText = message;
                };

                request.DoWorkerAsync();
                var response = await completion.Task;

                Payload = response is null
                    ? "{\n  // 没有响应对象\n}"
                    : $"// IsOk = {response.IsOk}, Msg = \"{response.Msg}\"\n{response.Data}";

                if (response is null && !HasError)
                {
                    HasError = true;
                    ErrorText = "请求未返回响应对象";
                }

                Log($"DoWorkerAsync(真实) · {_selected.BusinessUrl}", HasError);
            }
        }
        finally
        {
            stopwatch.Stop();
            ElapsedText = $"{stopwatch.ElapsedMilliseconds} ms";
            IsRunning = false;
        }
    }

    /// <summary>按顺序点亮五个阶段，每段带上自己耗时——真实的阶段划分见 BaseRequest 源码。</summary>
    private async Task RunStagesAsync(bool fails)
    {
        var budget = new[] { 6, 38, 96, 12, 4 };

        for (var i = 0; i < Stages.Count; i++)
        {
            var stage = Stages[i];
            stage.IsDone = true;

            // 失败请求在「收发」阶段停住，后面的阶段不再执行
            if (fails && i == 2)
            {
                stage.Milliseconds = "30000";
                return;
            }

            await Task.Delay(budget[i]);
            stage.Milliseconds = budget[i].ToString();
        }
    }

    private async Task PingAllAsync()
    {
        var tasks = new List<Task>();
        foreach (var row in Pings)
        {
            row.Result = "…";
            tasks.Add(ProbeAsync(row));
        }

        await Task.WhenAll(tasks);

        var reachable = 0;
        foreach (var row in Pings) if (row.IsOk) reachable++;
        Log($"NetworkPing 检测完成 · {reachable}/{Pings.Count} 可达", reachable == 0);
    }

    private static async Task ProbeAsync(PingRow row)
    {
        var reply = await NetworkPing.PingHost(row.Host);
        if (reply is null)
        {
            row.IsOk = false;
            row.Result = "不可达";
            return;
        }

        row.IsOk = true;
        row.Result = $"{reply.RoundtripTime} ms";
    }
}

/// <summary>请求表的一行。</summary>
public sealed class RequestRow : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isSelected;

    public RequestRow(string method, string businessUrl, string responseType, string requestType, string sampleJson, bool fails)
    {
        Method = method;
        BusinessUrl = businessUrl;
        ResponseType = responseType;
        RequestType = requestType;
        SampleJson = sampleJson;
        Fails = fails;
    }

    public string Method { get; }
    public string BusinessUrl { get; }
    public string ResponseType { get; }
    public string RequestType { get; }
    public string SampleJson { get; }

    /// <summary>该请求是否用来演示失败路径。</summary>
    public bool Fails { get; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value) return;
            _isSelected = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }

    /// <summary>真实调用时创建对应的 BaseRequest 子类实例。</summary>
    public BaseRequest CreateRequest() => RequestType switch
    {
        nameof(LoginRequest) => new LoginRequest(),
        nameof(DeviceStatusRequest) => new DeviceStatusRequest(),
        nameof(UploadResultRequest) => new UploadResultRequest(),
        _ => new LegacyPollRequest()
    };

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>DoWorkerAsync 里的一个阶段。</summary>
public sealed class RequestStage : System.ComponentModel.INotifyPropertyChanged
{
    private bool _isDone;
    private string _milliseconds = "—";

    public RequestStage(string label) => Label = label;

    public string Label { get; }

    public bool IsDone
    {
        get => _isDone;
        set
        {
            if (_isDone == value) return;
            _isDone = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsDone)));
        }
    }

    public string Milliseconds
    {
        get => _milliseconds;
        set
        {
            if (_milliseconds == value) return;
            _milliseconds = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Milliseconds)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>一个待检测的主机。</summary>
public sealed class PingRow : System.ComponentModel.INotifyPropertyChanged
{
    private string _result = "—";
    private bool _isOk;

    public PingRow(string host) => Host = host;

    public string Host { get; }

    public string Result
    {
        get => _result;
        set
        {
            if (_result == value) return;
            _result = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(Result)));
        }
    }

    public bool IsOk
    {
        get => _isOk;
        set
        {
            if (_isOk == value) return;
            _isOk = value;
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(nameof(IsOk)));
        }
    }

    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
}
