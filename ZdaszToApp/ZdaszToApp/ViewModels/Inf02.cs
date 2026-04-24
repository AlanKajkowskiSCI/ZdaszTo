using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using ZdaszToApp.Services;
using Avalonia.Media.Imaging;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using ZdaszToApp;

namespace ZdaszToApp.ViewModels;

public partial class Inf02 : ViewModelBase
{
    private readonly ApiService _apiService = ApiService.Instance;
    private System.Timers.Timer? _timer;

    [ObservableProperty]
    private int _lives = 5;

    [ObservableProperty]
    private int _timeRemaining = 30;

    [ObservableProperty]
    private string _userAvatar = "avares://ZdaszToApp/Assets/user.ico";

    [ObservableProperty]
    private string? _questionImage = null;

    [ObservableProperty]
    private Bitmap? _questionImageBitmap = null;

    [ObservableProperty]
    private bool _hasImage = false;

    [ObservableProperty]
    private string _questionText = "Ładowanie pytania...";

    [ObservableProperty]
    private string _questionTypeStr = "ABCD";

    [ObservableProperty]
    private int _questionType = 1;

    [ObservableProperty]
    private string _userAnswer = "";

    [ObservableProperty]
    private string? _selectedOption = null;

    [ObservableProperty]
    private bool? _trueFalseAnswer = null;

    [ObservableProperty]
    private string _selectedAnswerKey = "";

    [ObservableProperty]
    private bool _isCorrect = false;

    [ObservableProperty]
    private bool _answerSubmitted = false;

    [ObservableProperty]
    private bool _selectedTrueFalse = false;

    [ObservableProperty]
    private bool _hasSelectedTrueFalse = false;

    [ObservableProperty]
    private bool _selectedTrue = false;

    [ObservableProperty]
    private bool _selectedFalse = false;

    [ObservableProperty]
    private bool _correctAnswerTrue = false;

    private Question? _currentQuestion;
    private int _collectionId = 1;

    public ObservableCollection<AnswerOption> Options { get; } = new();

    public bool ShowTextInput => QuestionType == 0;
    public bool ShowABCD => QuestionType == 1;
    public bool ShowTrueFalse => QuestionType == 2;

    public Inf02()
    {
        StartTimer();
        _ = LoadQuestionAsync();
    }

    public Inf02(int collectionId)
    {
        _collectionId = collectionId;
        StartTimer();
        _ = LoadQuestionAsync();
    }

    private void StartTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
        TimeRemaining = 30;
        _timer = new System.Timers.Timer(1000);
        _timer.Elapsed += OnTimerTick;
        _timer.AutoReset = true;
        _timer.Start();
    }

    private void OnTimerTick(object? sender, ElapsedEventArgs e)
    {
        if (TimeRemaining > 0)
        {
            TimeRemaining--;
        }
        else
        {
            _timer?.Stop();
            _ = HandleTimeOut();
        }
    }

    private async Task HandleTimeOut()
    {
        if (AnswerSubmitted || _currentQuestion == null) return;
        
        AnswerSubmitted = true;
        QuizCounter.AddIncorrect();
        QuizCounter.AddAnswer(_currentQuestion.Id, false);
        _ = _apiService.SubmitQuizResultsAsync($"{_currentQuestion.Id}:0");

        if (QuestionType == 1)
        {
            foreach (var opt in Options)
            {
                opt.IsCorrectAnswer = opt.Key.ToUpper() == _currentQuestion.CorrectAnswer.ToUpper();
            }
        }
        else if (QuestionType == 2)
        {
            CorrectAnswerTrue = _currentQuestion.CorrectAnswer.ToUpper() == "TRUE";
            HasSelectedTrueFalse = true;
        }

        if (Lives > 0)
        {
            Lives--;
        }

        if (Lives <= 0)
        {
            Dispatcher.UIThread.Post(() => ShowEndScreen());
            return;
        }

        await Task.Delay(500);
        await LoadNextQuestionAsync();
    }

    private async Task LoadQuestionAsync()
    {
        _timer?.Stop();
        TimeRemaining = 30;
        _timer?.Start();
        
        var question = await _apiService.GetRandomQuestionAsync(_collectionId);
        Console.WriteLine($"Question loaded: {question?.Text}");
        Console.WriteLine($"Answers: {question?.Answers?.Count}");
        
        if (question == null)
        {
            QuestionText = "Nie udało się pobrać pytania";
            return;
        }

        _currentQuestion = question;
        QuestionText = question.Text;
        QuestionTypeStr = question.Type;
        QuestionImage = question.ImagePath;

        if (!string.IsNullOrEmpty(question.ImagePath))
        {
            await LoadQuestionImageAsync(question.Id);
        }
        else
        {
            QuestionImageBitmap = null;
            HasImage = false;
        }

        Options.Clear();
        
        if (question.Type == "ABCD")
        {
            QuestionType = 1;
            var keys = new[] { "A", "B", "C", "D" };
            foreach (var key in keys)
            {
                if (question.Answers.TryGetValue(key, out var answer))
                {
                    Console.WriteLine($"Option {key}: {answer}");
                    Options.Add(new AnswerOption { Key = key, Text = answer });
                }
            }
        }
        else if (question.Type == "TRUE_FALSE")
        {
            QuestionType = 2;
        }
        else
        {
            QuestionType = 0;
        }

        OnPropertyChanged(nameof(Options));
        OnPropertyChanged(nameof(ShowTextInput));
        OnPropertyChanged(nameof(ShowABCD));
        OnPropertyChanged(nameof(ShowTrueFalse));
        
        SelectedTrue = false;
        SelectedFalse = false;
        CorrectAnswerTrue = false;
    }

    private async Task LoadQuestionImageAsync(int questionId)
    {
        try
        {
            var imageData = await _apiService.GetQuestionImageAsync(questionId);
            if (imageData != null && imageData.Length > 0)
            {
                using var stream = new MemoryStream(imageData);
                QuestionImageBitmap = new Bitmap(stream);
                HasImage = true;
                Console.WriteLine($"Image loaded: {imageData.Length} bytes");
            }
            else
            {
                QuestionImageBitmap = null;
                HasImage = false;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading image: {ex.Message}");
            QuestionImageBitmap = null;
            HasImage = false;
        }
    }

    [RelayCommand]
    private void SelectOption(string optionKey)
    {
        if (AnswerSubmitted) return;
        SelectedAnswerKey = optionKey;
        SelectedOption = optionKey;
    }

    [RelayCommand]
    private void SelectTrueFalse(string value)
    {
        if (AnswerSubmitted) return;
        bool boolValue = value == "True";
        TrueFalseAnswer = boolValue;
        SelectedAnswerKey = boolValue ? "TRUE" : "FALSE";
        SelectedTrue = boolValue;
        SelectedFalse = !boolValue;
        HasSelectedTrueFalse = true;
    }

    [RelayCommand]
    private async Task SubmitAnswer()
    {
        if (_currentQuestion == null || AnswerSubmitted) return;

        string userAnswer;
        if (QuestionType == 0)
        {
            userAnswer = UserAnswer;
        }
        else if (QuestionType == 1)
        {
            if (string.IsNullOrEmpty(SelectedAnswerKey)) return;
            userAnswer = SelectedAnswerKey;
        }
        else
        {
            if (TrueFalseAnswer == null) return;
            userAnswer = TrueFalseAnswer.Value ? "TRUE" : "FALSE";
        }

        IsCorrect = userAnswer.ToUpper() == _currentQuestion.CorrectAnswer.ToUpper();
        AnswerSubmitted = true;
        _timer?.Stop();

        if (IsCorrect)
        {
            QuizCounter.AddCorrect();
        }
        else
        {
            QuizCounter.AddIncorrect();
        }

        QuizCounter.AddAnswer(_currentQuestion.Id, IsCorrect);
        _ = _apiService.SubmitQuizResultsAsync($"{_currentQuestion.Id}:{(IsCorrect ? 1 : 0)}");

        if (QuestionType == 1)
        {
            foreach (var opt in Options)
            {
                opt.IsCorrectAnswer = opt.Key.ToUpper() == _currentQuestion.CorrectAnswer.ToUpper();
            }
        }
        else if (QuestionType == 2)
        {
            CorrectAnswerTrue = _currentQuestion.CorrectAnswer.ToUpper() == "TRUE";
            HasSelectedTrueFalse = true;
        }

        bool wasLastLife = false;
        if (!IsCorrect && Lives > 0)
        {
            Lives--;
            wasLastLife = Lives == 0;
        }

        if (Lives <= 0)
        {
            if (wasLastLife)
            {
                await Task.Delay(2000);
            }
            ShowEndScreen();
            return;
        }

        await Task.Delay(IsCorrect ? 1000 : 2000);
        await LoadNextQuestionAsync();
    }

    private async void ShowEndScreen()
    {
        if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var window = desktop.MainWindow;
            if (window != null)
            {
                var testView = window.FindControl<Avalonia.Controls.UserControl>("Test");
                var endScreen = window.FindControl<Avalonia.Controls.UserControl>("EndScreen");
                if (testView != null && endScreen != null)
                {
                    if (endScreen.DataContext is EndScreenViewModel endVm)
                    {
                        endVm.RefreshData();
                        endVm.SetLastTestType("Inf02");
                    }
                    testView.IsVisible = false;
                    endScreen.IsVisible = true;
                }
            }
        }
        else if (App.Current?.ApplicationLifetime is ISingleViewApplicationLifetime mobile)
        {
            var root = mobile.MainView as Control;
            if (root != null)
            {
                var testView = FindControlRecursive(root, "Test");
                var endScreen = FindControlRecursive(root, "EndScreen");
                if (testView != null && endScreen != null)
                {
                    if (endScreen.DataContext is EndScreenViewModel endVm)
                    {
                        endVm.RefreshData();
                        endVm.SetLastTestType("Inf02");
                    }
                    testView.IsVisible = false;
                    endScreen.IsVisible = true;
                }
            }
        }
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

    private async Task LoadNextQuestionAsync()
    {
        AnswerSubmitted = false;
        SelectedAnswerKey = "";
        SelectedOption = null;
        TrueFalseAnswer = null;
        UserAnswer = "";
        IsCorrect = false;
        HasSelectedTrueFalse = false;
        SelectedTrue = false;
        SelectedFalse = false;
        CorrectAnswerTrue = false;
        await LoadQuestionAsync();
    }

    [RelayCommand]
    private void SelectABCD(string key)
    {
        if (AnswerSubmitted) return;
        SelectedAnswerKey = key;
        SelectedOption = key;
        foreach (var opt in Options)
        {
            opt.IsSelected = opt.Key == key;
            opt.IsCorrectAnswer = false;
        }
    }
}

public partial class AnswerOption : ObservableObject
{
    [ObservableProperty]
    private string _key = "";

    [ObservableProperty]
    private string _text = "";

    [ObservableProperty]
    private bool _isSelected = false;

    [ObservableProperty]
    private bool _isCorrectAnswer = false;
}
