using System.Net.Http.Json;
using PocketPay.Mobile.Services;

namespace PocketPay.Mobile;

public partial class HomePage : ContentPage
{
    private readonly ApiService _apiService;

    public HomePage()
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

        await LoadWallet();
    }

    private async Task LoadWallet()
    {
        try
        {
            var response = await _apiService.SendAsync(
                HttpMethod.Get,
                "Wallet");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load wallet.",
                    "OK");

                return;
            }

            var wallet = await response.Content
                .ReadFromJsonAsync<WalletResponse>();

            if (wallet != null)
            {
                BalanceLabel.Text =
                    $"Rs. {wallet.Balance:N2}";

                WalletNumberLabel.Text =
                    $"Wallet: {wallet.WalletNumber}";
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

    private async void OnDepositClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//DepositPage");
    }

    private async void OnSendMoneyClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//SendMoneyPage");
    }


    private async void OnTransactionsClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync(
            "//TransactionsPage");
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

        await Shell.Current.GoToAsync("//MainPage");
    }
}

public class WalletResponse
{
    public Guid Id { get; set; }

    public string WalletNumber { get; set; }
        = string.Empty;

    public decimal Balance { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}