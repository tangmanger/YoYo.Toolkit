using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using YoYo.Toolkit.Interfaces;
using YoYo.Toolkit.Services;

namespace YoYo.ToolkitSample.ViewModels;

/// <summary>
/// ViewModel 定位器，与 PackageEasy 的 ViewModelLocator 同构：
/// App.xaml 里注册成静态资源，各 View 用 {Binding Main, Source={StaticResource locator}} 取 VM，
/// View 的 code-behind 不再 new ViewModel。
/// </summary>
public sealed class ViewModelLocator
{
    public ViewModelLocator()
    {
        // IWindowService 必须是单例：导航页注册的窗口，关闭时得从同一本字典里取。
        var windows = new WindowService();

        Ioc.Default.ConfigureServices(
            new ServiceCollection()
                .AddSingleton<IWindowService>(windows)
                .AddSingleton<ShellViewModel>(_ => new ShellViewModel(windows))
                .BuildServiceProvider());
    }

    public ShellViewModel Main => Ioc.Default.GetService<ShellViewModel>() ?? new ShellViewModel();
}
