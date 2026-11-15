using System.Collections.Generic;
using System.ComponentModel;

namespace YoYo.ToolkitSample.Models;

/// <summary>侧栏的一项，来源于 <see cref="YoYo.Toolkit.Models.Navigations.NavigateModel"/>，额外带分组与图标。</summary>
public sealed class SidebarItem : INotifyPropertyChanged
{
    private bool _isActive;
    private bool _isVisible = true;

    public SidebarItem(string group, string label, string iconKey, string viewType)
    {
        Group = group;
        Label = label;
        IconKey = iconKey;
        ViewType = viewType;
    }

    public string Group { get; }
    public string Label { get; }
    public string IconKey { get; }
    public string ViewType { get; }

    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsActive)));
        }
    }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}

/// <summary>侧栏分组。</summary>
public sealed class NavGroup : INotifyPropertyChanged
{
    private bool _isVisible = true;

    public NavGroup(string title, IReadOnlyList<SidebarItem> items)
    {
        Title = title;
        Items = items;
    }

    public string Title { get; }
    public IReadOnlyList<SidebarItem> Items { get; }

    public bool IsVisible
    {
        get => _isVisible;
        set
        {
            if (_isVisible == value) return;
            _isVisible = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsVisible)));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
}
