using System;
using System.Text.Json;
using YoYo.Toolkit.Net.Enums;
using YoYo.Toolkit.Net.Http;

namespace YoYo.ToolkitSample.Net;

// ---------------------------------------------------------------------------
// BaseRequest 的四种子类。这里是真的可运行的请求定义：RootUrl / BusinessUrl /
// Method / ResponseType / SerializeObject 全部按框架约定实现，直接 DoWorkerAsync()
// 就会走 BackgroundWorker + HttpClient，回调与错误都回到 UI 线程。
// 示例默认用「本地模拟」跑完这五个阶段，避免依赖外网；取消模拟即走真实调用。
// ---------------------------------------------------------------------------

/// <summary>基类：示例里统一根路径，且不弹框架自带的进度层（等待界面由页面自己画）。</summary>
public abstract class DemoRequestBase : BaseRequest
{
    public override string RootUrl => "https://api.local";

    /// <summary>示例关闭框架进度层，改由页面上的阶段条呈现。</summary>
    public override bool IsShowProcess => false;

    public override void SerializeObject() { }
}

/// <summary>POST /api/login：最常见的「带 body、拿 token」。</summary>
public sealed class LoginRequest : DemoRequestBase
{
    public string User { get; set; } = "operator";
    public string Password { get; set; } = "******";

    public override string BusinessUrl => "/api/login";

    public override HttpMethodTypes Method => HttpMethodTypes.POST;

    public override Type ResponseType => typeof(LoginResponse);

    public override void SerializeObject()
        => Data = JsonSerializer.Serialize(new { user = User, password = Password });
}

/// <summary>登录返回。</summary>
public sealed class LoginResponse : BaseResponse
{
    public int Code { get; private set; }
    public string Token { get; private set; } = string.Empty;

    public override void DeserializeObj()
    {
        try
        {
            using var document = JsonDocument.Parse(Data);
            if (document.RootElement.TryGetProperty("code", out var code)) Code = code.GetInt32();
            if (document.RootElement.TryGetProperty("token", out var token)) Token = token.GetString() ?? string.Empty;
            IsOk = Code == 0;
        }
        catch (JsonException ex)
        {
            IsOk = false;
            Msg = ex.Message;
        }
    }
}

/// <summary>GET /api/device/status：纯查询，无 body。</summary>
public sealed class DeviceStatusRequest : DemoRequestBase
{
    public override string BusinessUrl => "/api/device/status";

    public override HttpMethodTypes Method => HttpMethodTypes.GET;

    public override Type ResponseType => typeof(DeviceStatusResponse);
}

/// <summary>设备状态返回。</summary>
public sealed class DeviceStatusResponse : BaseResponse
{
    public string State { get; private set; } = string.Empty;
    public int Temperature { get; private set; }

    public override void DeserializeObj()
    {
        try
        {
            using var document = JsonDocument.Parse(Data);
            if (document.RootElement.TryGetProperty("state", out var state)) State = state.GetString() ?? string.Empty;
            if (document.RootElement.TryGetProperty("temperature", out var t)) Temperature = t.GetInt32();
            IsOk = true;
        }
        catch (JsonException ex)
        {
            IsOk = false;
            Msg = ex.Message;
        }
    }
}

/// <summary>POST /api/result/upload：把结果推给 LIS。</summary>
public sealed class UploadResultRequest : DemoRequestBase
{
    public override string BusinessUrl => "/api/result/upload";

    public override HttpMethodTypes Method => HttpMethodTypes.POST;

    public override Type ResponseType => typeof(UploadResponse);

    public override void SerializeObject()
        => Data = JsonSerializer.Serialize(new { sampleNo = "SAMPLE-0000", well = 12, value = 21.6 });
}

/// <summary>上传返回。</summary>
public sealed class UploadResponse : BaseResponse
{
    public string Receipt { get; private set; } = string.Empty;

    public override void DeserializeObj()
    {
        try
        {
            using var document = JsonDocument.Parse(Data);
            if (document.RootElement.TryGetProperty("receipt", out var receipt)) Receipt = receipt.GetString() ?? string.Empty;
            IsOk = true;
        }
        catch (JsonException ex)
        {
            IsOk = false;
            Msg = ex.Message;
        }
    }
}

/// <summary>GET /api/legacy/poll：老接口，常用于演示超时与错误出口。</summary>
public sealed class LegacyPollRequest : DemoRequestBase
{
    public override string BusinessUrl => "/api/legacy/poll";

    public override HttpMethodTypes Method => HttpMethodTypes.GET;

    public override Type ResponseType => typeof(LegacyResponse);
}

/// <summary>老接口返回。</summary>
public sealed class LegacyResponse : BaseResponse
{
    public override void DeserializeObj()
    {
        try
        {
            using var document = JsonDocument.Parse(Data);
            IsOk = document.RootElement.ValueKind == JsonValueKind.Object;
        }
        catch (JsonException ex)
        {
            IsOk = false;
            Msg = ex.Message;
        }
    }
}
