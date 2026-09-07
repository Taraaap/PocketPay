using System.Net.Http.Json;
using PocketPay.Mobile.Services;

namespace PocketPay.Mobile;

public partial class AccountPage : ContentPage
{
    private readonly ApiService _apiService;

    public AccountPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var token =
            await SecureStorage.Default.GetAsync("accessToken");

        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        await LoadAccount();
    }

    private async Task LoadAccount()
    {
        try
        {
            var fullName =
                await SecureStorage.Default.GetAsync("fullName");

            if (!string.IsNullOrWhiteSpace(fullName))
            {
                FullNameLabel.Text = fullName;
            }

            var response = await _apiService.SendAsync(
                HttpMethod.Get,
                "Wallet");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load account information.",
                    "OK");

                return;
            }

            var wallet = await response.Content
                .ReadFromJsonAsync<WalletResponse>();

            if (wallet != null)
            {
                WalletNumberLabel.Text =
                    wallet.WalletNumber;

                BalanceLabel.Text =
                    $"Rs. {wallet.Balance:N2}";
            }

            var email =
                await SecureStorage.Default.GetAsync("email");

            if (!string.IsNullOrWhiteSpace(email))
            {
                EmailLabel.Text = email;
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnLogoutClicked(
        object sender,
        EventArgs e)
    {
        bool confirm = await DisplayAlert(
            "Logout",
            "Are you sure you want to logout?",
            "Yes",
            "No");

        if (!confirm)
            return;

        SecureStorage.Default.Remove("accessToken");
        SecureStorage.Default.Remove("refreshToken");
        SecureStorage.Default.Remove("userId");
        SecureStorage.Default.Remove("fullName");
        SecureStorage.Default.Remove("email");

        ((AppShell)Shell.Current).HideMenu();

        await Shell.Current.GoToAsync("//MainPage");
    }
}