using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using YoYo.Toolkit.Interfaces.Navigations;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>页面与外壳之间的最小契约：页面内需要跳转或写日志时用它，不直接依赖外壳实现。</summary>
public interface INavigationHost
{
    void GoTo(string viewType);
    void GoToWithParam(string viewType, string param);
    void Log(string message, bool isError = false);
}

/// <summary>
/// 所有页面 ViewModel 的基类。
/// 实现 <see cref="INavigateIn"/> / <see cref="INavigateOut"/>，由 BaseNavigation.GoTo 在切页时回调——
/// 这是示例想演示的核心：页面自己决定进来要做什么、离开要清理什么。
/// </summary>
public abstract class PageViewModelBase : INotifyPropertyChanged, INavigateIn, INavigateOut
{
    protected PageViewModelBase(INavigationHost? host) => Host = host;

    protected INavigationHost? Host { get; }

    /// <summary>与 NavigateModel.ViewType 一致。</summary>
    public abstract string ViewType { get; }

    public abstract string Title { get; }

    public abstract string Lede { get; }

    public void NavigateIn() => OnNavigatedIn();

    public void NavigateOut() => OnNavigatedOut();

    /// <summary>进入页面：加载数据、启动轮询。</summary>
    protected virtual void OnNavigatedIn() { }

    /// <summary>离开页面：停轮询、退订阅。</summary>
    protected virtual void OnNavigatedOut() { }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void Log(string message, bool isError = false) => Host?.Log(message, isError);

    /// <summary>页面内跳转。</summary>
    protected void Go(string viewType) => Host?.GoTo(viewType);

    /// <summary>带强类型参数的跳转。</summary>
    protected void GoWithParam(string viewType, string param) => Host?.GoToWithParam(viewType, param);
}

/// <summary>带强类型参数的页面：实现 <c>INavigateIn&lt;T&gt;</c> 即可被 <c>GoTo(name, param)</c> 命中。</summary>
public abstract class PageViewModelBase<TParam> : PageViewModelBase, INavigateIn<TParam>
{
    protected PageViewModelBase(INavigationHost? host) : base(host) { }

    public void NavigateIn(TParam? param) => OnNavigatedIn(param);

    protected virtual void OnNavigatedIn(TParam? param) { }
}
