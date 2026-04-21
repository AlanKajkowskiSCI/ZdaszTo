using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net.Mail;
using System.Threading.Tasks;
using ZdaszToApp.Services;

namespace ZdaszToApp.ViewModels;

public partial class AddAccountViewModel : ViewModelBase
{
    private readonly ApiService _apiService = ApiService.Instance;
    private readonly AuthService _authService = AuthService.Instance;

    [ObservableProperty] private string? email;

    partial void OnEmailChanged(string? value)
    {
        if (!string.IsNullOrEmpty(value) && !IsValidEmail(value))
        {
            Error_email = "Nieprawidłowy adres e-mail";
        }
        else if (string.IsNullOrEmpty(value))
        {
            Error_email = null;
        }
        else
        {
            Error_email = null;
        }
    }

    [ObservableProperty] private string? username;

    [ObservableProperty] private string? password;

    [ObservableProperty] private string? message;

    [ObservableProperty] private string? error_email;

    [ObservableProperty] private string? error_username;

    [ObservableProperty] private string? error_password;

    [ObservableProperty] private bool accountCreated;

    [ObservableProperty] private bool isLoggedIn;

    [ObservableProperty] private bool isLoading;

    [RelayCommand]
    private async Task Signup()
    {
        Message = null;
        Error_email = null;
        Error_username = null;
        Error_password = null;
        IsLoading = true;

if (string.IsNullOrWhiteSpace(Email))
        {
            Error_email = "Wpisz e-mail";
            IsLoading = false;
            return;
        }

        if (!IsValidEmail(Email))
        {
            Console.WriteLine($"[DEBUG] IsValidEmail zwrocilo false dla: {Email}");
            Error_email = "Nieprawidłowy adres e-mail";
            IsLoading = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(Username))
        {
            Error_username = "Wpisz login";
            IsLoading = false;
            return;
        }

        if (string.IsNullOrWhiteSpace(Password))
        {
            Error_password = "Wpisz hasło";
            IsLoading = false;
            return;
        }

        var result = await _apiService.SignupAsync(Email, Username, Password);

        if (result == null || result.StartsWith("Error:"))
        {
            Message = "Brak połączenia z internetem";
            IsLoading = false;
            return;
        }

        if (result == "User created")
            {
                Message = "Konto utworzone!";
                AccountCreated = true;
                
                Console.WriteLine($"[AddAccountViewModel] Before login - Username: {Username}, Password: {(Password != null ? "***" : "null")}");
                
                var loginResult = await _apiService.LoginAsync(Username, Password);
                Console.WriteLine($"[AddAccountViewModel] LoginResult: {loginResult}");
                if (loginResult != null && !loginResult.StartsWith("Error:") && loginResult != "Invalid password" && loginResult != "No user found")
                {
                    Console.WriteLine($"[AddAccountViewModel] Calling SaveCredentials - Username: {Username}, Password: {(Password != null ? "***" : "null")}");
                    _authService.SaveCredentials(Username!, Password!);
                    IsLoggedIn = true;
                    OnLoginSuccess?.Invoke();
                }
                else if (loginResult?.StartsWith("Error:") == true)
                {
                    Message = "Konto utworzone, ale nie można się zalogować. Sprawdź połączenie.";
                }
                IsLoading = false;
                return;
            }

        if (result == "User exists")
        {
            Error_username = "Użytkownik już istnieje";
            Message = "Użytkownik już istnieje";
            IsLoading = false;
            return;
        }

        Message = result;
        IsLoading = false;
    }

    [RelayCommand]
    private void GoBack()
    {
        OnGoBackToLogin?.Invoke();
    }

public event Action? OnGoBackToLogin;
    public event Action? OnLoginSuccess;

    private static bool IsValidEmail(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;
        
        try
        {
            var addr = new MailAddress(email);
            return addr.Address == email;
        }
        catch
        {
            return false;
        }
    }
}