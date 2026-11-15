using System;
using System.Collections.Generic;
using System.Windows;
using YoYo.Toolkit.Interfaces.Navigations;

namespace YoYo.ToolkitSample.Services;

/// <summary>
/// 宿主侧的服务容器替身：把「类型 → 工厂」登记进来，供 <see cref="YoYo.Toolkit.Navigations.BaseNavigation"/> 解析
/// View 与 ViewModel。示例不引入 DI 框架，几十行足够；真实项目里换成一个 IServiceProvider 适配器即可。
/// <para>
/// 关键点：解析结果是**单例**。BaseNavigation 会用 VMType 取一次 VM 调 NavigateIn，
/// 再用 Type 取一次 View —— 两次必须拿到同一组对象，否则生命周期钩子会打到一个没人用的 VM 上。
/// </para>
/// </summary>
public sealed class ShellService : IGetService
{
    private readonly Dictionary<Type, Func<object>> _factories = new();
    private readonly Dictionary<Type, object> _instances = new();
    private FrameworkElement? _view;

    /// <summary>视图被替换时触发，外壳据此把新视图挂到 ContentControl。</summary>
    public event Action<FrameworkElement?>? ViewChanged;

    /// <summary>登记一个类型及其创建方式。重复登记以最后一次为准。</summary>
    public void Register(Type type, Func<object> factory) => _factories[type] = factory;

    /// <summary>
    /// 登记一对「View + ViewModel」：VM 单例、View 单例，且 View 的 DataContext 就是那个 VM。
    /// 这样页面在来回切换时保留自身状态（进度值、输入内容），符合桌面软件的预期。
    /// </summary>
    public void RegisterPage<TView, TViewModel>(Func<TViewModel> viewModelFactory)
        where TView : FrameworkElement, new()
    {
        Register(typeof(TViewModel), () => Resolve(typeof(TViewModel), () => viewModelFactory()));
        Register(typeof(TView), () => new TView
        {
            DataContext = Resolve(typeof(TViewModel), () => viewModelFactory())
        });
    }

    private object Resolve(Type type, Func<object> factory)
    {
        if (_instances.TryGetValue(type, out var existing)) return existing;
        var created = factory() ?? throw new InvalidOperationException($"{type.Name} 工厂返回了 null");
        _instances[type] = created;
        return created;
    }

    public object GetService(Type? serviceType)
    {
        if (serviceType is null) return null!;
        if (serviceType == typeof(IGetService)) return this;

        if (_factories.TryGetValue(serviceType, out var factory))
            return Resolve(serviceType, factory);

        return null!;
    }

    public FrameworkElement? GetView() => _view;

    public void SetView(FrameworkElement? frameworkElement)
    {
        _view = frameworkElement;
        ViewChanged?.Invoke(frameworkElement);
    }
}
