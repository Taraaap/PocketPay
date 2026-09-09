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

        var token = await SecureStorage.Default
            .GetAsync("accessToken");

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
            var fullName = await SecureStorage.Default
                .GetAsync("fullName");

            var email = await SecureStorage.Default
                .GetAsync("email");

            FullNameLabel.Text =
                string.IsNullOrWhiteSpace(fullName)
                    ? "Unknown"
                    : fullName;

            EmailLabel.Text =
                string.IsNullOrWhiteSpace(email)
                    ? "Unknown"
                    : email;

            var response = await _apiService.SendAsync(
                HttpMethod.Get,
                "Wallet");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load wallet information.",
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
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }
}