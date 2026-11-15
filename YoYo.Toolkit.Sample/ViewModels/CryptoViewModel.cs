using System;
using System.Windows.Input;
using YoYo.Toolkit.CryptoServices;
using YoYo.ToolkitSample.Models;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// 加密页：MD5 三种重载 + AES 往返。
/// 所有结果都是本机真算的，不是写死的字符串——可以拿 <c>node -e</c> 或任何 MD5 工具对照。
/// </summary>
public sealed class CryptoViewModel : PageViewModelBase
{
    private string _md5Input = "YoYo.Toolkit";
    private bool _isUpper;
    private bool _isShort;

    private string _aesInput = "SN-0001|2026-11-13";
    private string _aesKey = "yoyo-toolkit-key";
    private string _aesOutput = "—";
    private string _aesState = "—";

    private string _license;

    public CryptoViewModel(INavigationHost? host) : base(host)
    {
        EncryptCommand = new RelayCommand(Encrypt);
        DecryptCommand = new RelayCommand(Decrypt);
        _license = BuildLicense();

        RecalcMd5();
    }

    public override string ViewType => "CryptoView";

    public override string Title => "加密";

    public override string Lede => "MD5 摘要（32 / 16 位、大小写）与 AES 加解密，全部本机实时计算。";

    public ICommand EncryptCommand { get; }
    public ICommand DecryptCommand { get; }

    // ---------- MD5 ----------

    public string Md5Input
    {
        get => _md5Input;
        set
        {
            if (!SetField(ref _md5Input, value)) return;
            RecalcMd5();
        }
    }

    public bool IsUpper
    {
        get => _isUpper;
        set
        {
            if (!SetField(ref _isUpper, value)) return;
            RecalcMd5();
        }
    }

    public bool IsShort
    {
        get => _isShort;
        set
        {
            if (!SetField(ref _isShort, value)) return;
            RecalcMd5();
            OnPropertyChanged(nameof(LengthText));
        }
    }

    public string LengthText => _isShort ? "16" : "32";

    public string Md5Output { get; private set; } = "—";

    /// <summary>同时给出 3 个重载的结果，便于对照签名。</summary>
    public string Md5Plain { get; private set; } = "—";
    public string Md5NoReplace { get; private set; } = "—";
    public string Md5Encrypt { get; private set; } = "—";

    private void RecalcMd5()
    {
        try
        {
            Md5Output = MD5Helper.Md5(_md5Input ?? string.Empty, _isShort ? 16 : 32, _isUpper);
            Md5Plain = MD5Helper.Md5(_md5Input ?? string.Empty);
            Md5NoReplace = MD5Helper.Md5NoReplace(_md5Input ?? string.Empty, _isUpper);
            Md5Encrypt = MD5Helper.Md5Encrypt(_md5Input ?? string.Empty, _isShort ? 16 : 32);
        }
        catch (Exception ex)
        {
            Md5Output = ex.Message;
        }

        OnPropertyChanged(nameof(Md5Output));
        OnPropertyChanged(nameof(Md5Plain));
        OnPropertyChanged(nameof(Md5NoReplace));
        OnPropertyChanged(nameof(Md5Encrypt));
    }

    // ---------- AES ----------

    public string AesInput
    {
        get => _aesInput;
        set => SetField(ref _aesInput, value);
    }

    public string AesKey
    {
        get => _aesKey;
        set
        {
            if (!SetField(ref _aesKey, value)) return;
            OnPropertyChanged(nameof(KeyHint));
        }
    }

    public string KeyHint
    {
        get
        {
            var length = _aesKey?.Length ?? 0;
            return length is 16 or 24 or 32
                ? $"key 长度 {length} 字节 · 合法"
                : $"key 长度 {length} 字节 · 需 16 / 24 / 32";
        }
    }

    public string AesOutput
    {
        get => _aesOutput;
        private set => SetField(ref _aesOutput, value);
    }

    public string AesState
    {
        get => _aesState;
        private set => SetField(ref _aesState, value);
    }

    public string License
    {
        get => _license;
        private set => SetField(ref _license, value);
    }

    private void Encrypt()
    {
        try
        {
            AesOutput = RSAHelper.AESEncrypt(_aesInput ?? string.Empty, _aesKey);
            AesState = "AESEncrypt · Base64";
            Log("AESEncrypt 完成");
        }
        catch (Exception ex)
        {
            AesOutput = ex.Message;
            AesState = "加密失败";
            Log($"AESEncrypt 失败 · {ex.Message}", true);
        }
    }

    private void Decrypt()
    {
        try
        {
            // 先解上一格的结果；不是合法 Base64 时直接尝试把明文当密文解，失败即报错
            AesOutput = RSAHelper.AESDecrypt(AesOutput, _aesKey);
            AesState = "AESDecrypt · 明文";
            Log("AESDecrypt 完成");
        }
        catch (Exception ex)
        {
            AesState = "解密失败";
            Log($"AESDecrypt 失败 · {ex.Message}", true);
        }
    }

    /// <summary>授权串：机器指纹走 MD5，再包一层 AES——本页顶部展示的就是这条链路。</summary>
    private static string BuildLicense()
    {
        var fingerprint = MD5Helper.Md5("WD-WCC4N2PK8412" + "8F3A-2C71-9D04-B6E5");
        try
        {
            return RSAHelper.AESEncrypt($"{fingerprint}|2027-12-31", "yoyo-toolkit-key");
        }
        catch (Exception)
        {
            return fingerprint;
        }
    }
}
