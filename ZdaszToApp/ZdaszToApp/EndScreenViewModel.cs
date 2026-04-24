using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using ZdaszToApp.Views;
using ZdaszToApp.ViewModels;
using ZdaszToApp.Services;

namespace ZdaszToApp;

public partial class EndScreenViewModel : ViewModelBase
{
    [ObservableProperty]
    private int _correctAnswers;

    [ObservableProperty]
    private int _incorrectAnswers;

    [ObservableProperty]
    private int _totalQuestions;

    [ObservableProperty]
    private double _percentage;

    [ObservableProperty]
    private string _resultMessage = "";

    private string _lastTestType = "";

    public EndScreenViewModel()
    {
        RefreshData();
    }

    public EndScreenViewModel(int correct, int incorrect)
    {
        CorrectAnswers = correct;
        IncorrectAnswers = incorrect;
        TotalQuestions = correct + incorrect;
        CalculateResult();
    }

    public void RefreshData()
    {
        CorrectAnswers = QuizCounter.CorrectAnswers;
        IncorrectAnswers = QuizCounter.IncorrectAnswers;
        TotalQuestions = CorrectAnswers + IncorrectAnswers;
        CalculateResult();
    }

    public void SetLastTestType(string testType)
    {
        _lastTestType = testType;
    }

    private void CalculateResult()
    {
        if (TotalQuestions > 0)
        {
            Percentage = (double)CorrectAnswers / TotalQuestions * 100;
        }
        else
        {
            Percentage = 0;
        }

        ResultMessage = Percentage switch
        {
            >= 90 => "Fantastycznie! Jesteś ekspertem!",
            >= 70 => "Świetnie! Dużo zaliczyłeś!",
            >= 50 => "Nieźle! Jeszcze trochę i będzie perfekcyjnie!",
            >= 30 => "Słabo... Musisz się uczyć!",
            _ => "Beznadziejnie! Koniecznie powtórz materiał!"
        };
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

    [RelayCommand]
    private void GoToMenu()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                var endScreen = window.FindControl<UserControl>("EndScreen");
                var mainDock = window.FindControl<DockPanel>("Main");
                if (endScreen != null && mainDock != null)
                {
                    QuizCounter.Reset();
                    
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
                    
                    endScreen.IsVisible = false;
                    mainDock.IsVisible = true;
                }
            }
        }
        else if (App.Current?.ApplicationLifetime is ISingleViewApplicationLifetime mobile)
        {
            var root = mobile.MainView as Control;
            if (root != null)
            {
                var endScreen = FindControlRecursive(root, "EndScreen");
                var mainDock = FindControlRecursive(root, "Main") as DockPanel;
                if (endScreen != null && mainDock != null)
                {
                    QuizCounter.Reset();
                    
                    var testView = FindControlRecursive(root, "Test") as Inf02View;
                    if (testView?.DataContext is Inf02 inf02Vm)
                    {
                        inf02Vm.StopTimer();
                    }
                    var inf03View = FindControlRecursive(root, "Inf03") as Inf03View;
                    if (inf03View?.DataContext is Inf03 inf03Vm)
                    {
                        inf03Vm.StopTimer();
                    }
                    var inf04View = FindControlRecursive(root, "Inf04") as Inf04View;
                    if (inf04View?.DataContext is Inf04 inf04Vm)
                    {
                        inf04Vm.StopTimer();
                    }
                    
                    endScreen.IsVisible = false;
                    mainDock.IsVisible = true;
                }
            }
        }
    }

    [RelayCommand]
    private void Retry()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                var endScreen = window.FindControl<UserControl>("EndScreen");
                if (endScreen != null && endScreen.DataContext is EndScreenViewModel endVm)
                {
                    QuizCounter.Reset();
                    endScreen.IsVisible = false;

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

                    switch (_lastTestType)
                    {
                        case "Inf02":
                            var testView2 = window.FindControl<Inf02View>("Test");
                            if (testView2 != null)
                            {
                                testView2.DataContext = new Inf02(1);
                                testView2.IsVisible = true;
                            }
                            break;
                        case "Inf03":
                            var inf03View2 = window.FindControl<Inf03View>("Inf03");
                            if (inf03View2 != null)
                            {
                                inf03View2.DataContext = new Inf03(2);
                                inf03View2.IsVisible = true;
                            }
                            break;
                        case "Inf04":
                            var inf04View2 = window.FindControl<Inf04View>("Inf04");
                            if (inf04View2 != null)
                            {
                                inf04View2.DataContext = new Inf04(3);
                                inf04View2.IsVisible = true;
                            }
                            break;
                    }
                }
            }
        }
        else if (App.Current?.ApplicationLifetime is ISingleViewApplicationLifetime mobile)
        {
            var root = mobile.MainView as Control;
            if (root != null)
            {
                var endScreen = FindControlRecursive(root, "EndScreen");
                if (endScreen != null)
                {
                    QuizCounter.Reset();
                    endScreen.IsVisible = false;

                    var testView = FindControlRecursive(root, "Test") as Inf02View;
                    if (testView?.DataContext is Inf02 inf02Vm)
                    {
                        inf02Vm.StopTimer();
                    }
                    var inf03View = FindControlRecursive(root, "Inf03") as Inf03View;
                    if (inf03View?.DataContext is Inf03 inf03Vm)
                    {
                        inf03Vm.StopTimer();
                    }
                    var inf04View = FindControlRecursive(root, "Inf04") as Inf04View;
                    if (inf04View?.DataContext is Inf04 inf04Vm)
                    {
                        inf04Vm.StopTimer();
                    }

                    switch (_lastTestType)
                    {
                        case "Inf02":
                            var testView2 = FindControlRecursive(root, "Test");
                            if (testView2 != null)
                            {
                                testView2.DataContext = new Inf02(1);
                                testView2.IsVisible = true;
                            }
                            break;
                        case "Inf03":
                            var inf03View2 = FindControlRecursive(root, "Inf03");
                            if (inf03View2 != null)
                            {
                                inf03View2.DataContext = new Inf03(2);
                                inf03View2.IsVisible = true;
                            }
                            break;
                        case "Inf04":
                            var inf04View2 = FindControlRecursive(root, "Inf04");
                            if (inf04View2 != null)
                            {
                                inf04View2.DataContext = new Inf04(3);
                                inf04View2.IsVisible = true;
                            }
                            break;
                    }
                }
            }
        }
    }
}
