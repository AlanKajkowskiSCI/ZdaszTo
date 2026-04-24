ausing Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.VisualTree;
using ZdaszToApp.ViewModels;
using ZdaszToApp.Services;
using System.Diagnostics;
using System;

namespace ZdaszToApp.Views;

public partial class MenuView : UserControl, ILoadable
{
    public Taskbar? TaskbarView { get; private set; }

    public MenuView()
    {
        InitializeComponent();
        
        Loaded += (s, e) =>
        {
            var root = this.GetVisualRoot() as Window;
            if (root != null)
            {
                var mainDock = root.FindControl<DockPanel>("Main");
                if (mainDock != null)
                {
                    TaskbarView = mainDock.FindControl<Taskbar>("Taskbar");
                }
            }
        };
        
        ThemeService.Instance.PropertyChanged += OnThemeChanged;
        ApplyTheme();
        
        QuizCounter.Reset();
        Debug.WriteLine("[MenuView] Zaladowano MenuView");
    }

    private void OnThemeChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ThemeService.IsDarkMode))
        {
            ApplyTheme();
        }
    }

    private void ApplyTheme()
    {
        var isDark = ThemeService.Instance.IsDarkMode;
        
        var mainGrid = this.FindControl<Grid>("MainGrid");
        var cardBorder = this.FindControl<Border>("CardBorder");
        
        if (mainGrid != null)
        {
            mainGrid.Classes.Clear();
            mainGrid.Classes.Add(isDark ? "dark" : "light");
            
            var bg = Application.Current?.FindResource(isDark ? "DarkPageBackground" : "PageBackground") as IBrush;
            if (bg != null)
                mainGrid.Background = bg;
        }

        if (cardBorder != null)
        {
            var overlay = Application.Current?.FindResource(isDark ? "OverlayDark" : "GlassOverlay") as IBrush;
            if (overlay != null)
                cardBorder.Background = overlay;
        }
    }

    public void ApplyThemeOnLoad()
    {
        ApplyTheme();
    }

    private Control? FindParentWithName(Control start, string name)
    {
        var current = start.Parent;
        while (current != null)
        {
            if (current is Control c && c.Name == name)
                return c;
            current = current.Parent;
        }
        return null;
    }
    
    private Control? FindInRoot(Control start, string name)
    {
        var root = this.GetVisualRoot() as Control;
        if (root == null) return null;
        
        if (root is Window window)
        {
            return window.FindControl<Control>(name);
        }
        
        return FindControlRecursive(root, name);
    }
    
    private Control? FindControlRecursive(Control parent, string name)
    {
        if (parent.Name == name)
            return parent;

        if (parent is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control c)
                {
                    var found = FindControlRecursive(c, name);
                    if (found != null) return found;
                }
            }
        }
        else if (parent is ContentControl cc && cc.Content is Control content)
        {
            var found = FindControlRecursive(content, name);
            if (found != null) return found;
        }

        return null;
    }

    private void StopAllTestTimers()
    {
        var mainDock = FindParentWithName(this, "Main") as DockPanel;
        if (mainDock == null) return;

        var window = mainDock.GetVisualRoot() as Window;
        if (window == null) return;

        var testView = window.FindControl<Inf02View>("Test");
        if (testView?.DataContext is Inf02 inf02Vm)
        {
            inf02Vm.StopTimer();
        }

        var inf03View = window.FindControl<Inf03View>("Inf03");
        if (inf03View?.DataContext is Inf03 inf03Vm)
        {
            inf03Vm.StopTimer();
        }

        var inf04View = window.FindControl<Inf04View>("Inf04");
        if (inf04View?.DataContext is Inf04 inf04Vm)
        {
            inf04Vm.StopTimer();
        }
    }

    private void OnInf02Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnInf02Click - START");
        
        var mainDock = FindParentWithName(this, "Main") as DockPanel;
        var testView = FindInRoot(this, "Test") as Inf02View;
        var inf03View = FindInRoot(this, "Inf03") as Inf03View;
        var inf04View = FindInRoot(this, "Inf04") as Inf04View;
        
        Debug.WriteLine($"[MenuView] mainDock: {mainDock}, testView: {testView}");
        
        if (testView != null && mainDock != null)
        {
            StopAllTestTimers();
            testView.DataContext = new Inf02(1);
            testView.IsVisible = true;
            mainDock.IsVisible = false;
            Debug.WriteLine("[MenuView] Przelaczono na Inf02View");
        }
    }

    private void OnInf03Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnInf03Click - START");
        
        var mainDock = FindParentWithName(this, "Main") as DockPanel;
        var inf03View = FindInRoot(this, "Inf03") as Inf03View;
        var testView = FindInRoot(this, "Test") as Inf02View;
        var inf04View = FindInRoot(this, "Inf04") as Inf04View;
        
        Debug.WriteLine($"[MenuView] mainDock: {mainDock}, inf03View: {inf03View}");
        
        if (inf03View != null && mainDock != null)
        {
            StopAllTestTimers();
            inf03View.DataContext = new Inf03(2);
            inf03View.IsVisible = true;
            mainDock.IsVisible = false;
            Debug.WriteLine("[MenuView] Przelaczono na Inf03View");
        }
    }

    private void OnInf04Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnInf04Click - START");
        
        var mainDock = FindParentWithName(this, "Main") as DockPanel;
        var inf04View = FindInRoot(this, "Inf04") as Inf04View;
        var testView = FindInRoot(this, "Test") as Inf02View;
        var inf03View = FindInRoot(this, "Inf03") as Inf03View;
        
        Debug.WriteLine($"[MenuView] mainDock: {mainDock}, inf04View: {inf04View}");
        
        if (inf04View != null && mainDock != null)
        {
            StopAllTestTimers();
            inf04View.DataContext = new Inf04(3);
            inf04View.IsVisible = true;
            mainDock.IsVisible = false;
            Debug.WriteLine("[MenuView] Przelaczono na Inf04View");
        }
    }

    private void OnSettingsClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnSettingsClick - START");
        
        var mainDock = FindParentWithName(this, "Main") as DockPanel;
        var settingsView = FindInRoot(this, "Settings") as SettingsView;
        
        Debug.WriteLine($"[MenuView] mainDock: {mainDock}, settingsView: {settingsView}");
        
        if (settingsView != null && mainDock != null)
        {
            settingsView.IsVisible = true;
            mainDock.IsVisible = false;
            Debug.WriteLine("[MenuView] Przelaczono na SettingsView");
        }
    }

    private void OnRankingClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnRankingClick");
        
        var mainDock = FindParentWithName(this, "Main");
        var rankingView = FindInRoot(this, "Ranking") as RankingView;
        
        if (rankingView != null && mainDock != null)
        {
            if (rankingView is ILoadable loadable)
            {
                loadable.ApplyThemeOnLoad();
            }
            
            if (rankingView.DataContext is RankingViewModel vm)
            {
                _ = vm.LoadRankingAsync();
            }
            
            mainDock.IsVisible = false;
            rankingView.IsVisible = true;
            Debug.WriteLine("[MenuView] Przelaczono na RankingView");
        }
    }

    private void OnLogoutClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Debug.WriteLine("[MenuView] OnLogoutClick");
        
        var root = this.GetVisualRoot() as Window;
        if (root != null)
        {
            var mainWindow = root as MainWindow;
            if (mainWindow != null)
            {
                mainWindow.DoLogout();
            }
        }
    }
}
