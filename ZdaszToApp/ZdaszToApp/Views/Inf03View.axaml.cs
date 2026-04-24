using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.VisualTree;
using ZdaszToApp;
using ZdaszToApp.ViewModels;
using ZdaszToApp.Services;

namespace ZdaszToApp.Views;

public partial class Inf03View : UserControl, ILoadable
{
    public Inf03View()
    {
        InitializeComponent();
        SetupTheme();
        DataContext = new Inf03();
        Unloaded += OnUnloaded;
    }

    public Inf03View(int collectionId)
    {
        InitializeComponent();
        SetupTheme();
        DataContext = new Inf03(collectionId);
        Unloaded += OnUnloaded;
    }

    private void OnUnloaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is Inf03 vm)
        {
            vm.StopTimer();
        }
    }

    private void SetupTheme()
    {
        ThemeService.Instance.PropertyChanged += OnThemeChanged;
        ApplyTheme();
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
        var outerBorder = this.FindControl<Border>("OuterBorder");
        var innerBorder = this.FindControl<Border>("InnerBorder");
        
        if (mainGrid != null)
        {
            mainGrid.Classes.Clear();
            mainGrid.Classes.Add(isDark ? "dark" : "light");
            
            var bg = Application.Current?.FindResource(isDark ? "DarkPageBackground" : "PageBackground") as IBrush;
            if (bg != null)
                mainGrid.Background = bg;
        }

        if (outerBorder != null)
        {
            var overlay = Application.Current?.FindResource(isDark ? "OverlayDark" : "GlassOverlay") as IBrush;
            if (overlay != null)
                outerBorder.Background = overlay;
        }

        if (innerBorder != null)
        {
            var card = Application.Current?.FindResource(isDark ? "DarkGlossyCard" : "GlossyWhite") as IBrush;
            if (card != null)
                innerBorder.Background = card;
            else
                innerBorder.Background = isDark 
                    ? new SolidColorBrush(Avalonia.Media.Color.Parse("#1E1E3F"))
                    : new SolidColorBrush(Colors.White);
        }
    }

    public void ApplyThemeOnLoad()
    {
        ApplyTheme();
    }

    private Control? FindInRoot(Control start, string name)
    {
        var root = this.GetVisualRoot() as Control;
        if (root == null) return null;
        
        if (root is Window window)
            return window.FindControl<Control>(name);
        
        return FindControlRecursive(root, name);
    }
    
    private Control? FindControlRecursive(Control parent, string targetName)
    {
        if (parent.Name == targetName)
            return parent;
            
        if (parent is Panel panel)
        {
            foreach (var child in panel.Children)
            {
                if (child is Control c)
                {
                    var found = FindControlRecursive(c, targetName);
                    if (found != null) return found;
                }
            }
        }
        else if (parent is ContentControl cc && cc.Content is Control content)
        {
            var found = FindControlRecursive(content, targetName);
            if (found != null) return found;
        }
        
        return null;
    }

    private void OnHomeClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        var testView = FindInRoot(this, "Inf03");
        var endScreen = FindInRoot(this, "EndScreen");
        
        if (testView != null && endScreen != null)
        {
            if (testView.DataContext is Inf03 vm)
            {
                vm.Lives = 0;
                vm.StopTimer();
            }
            if (endScreen.DataContext is EndScreenViewModel endVm)
            {
                endVm.RefreshData();
                endVm.SetLastTestType("Inf03");
            }
            testView.IsVisible = false;
            endScreen.IsVisible = true;
        }
    }
}
