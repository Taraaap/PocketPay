using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PocketPay.Mobile;

public partial class HomePage : ContentPage
{
    private readonly HttpClient _httpClient;

    public HomePage()
    {
        InitializeComponent();

        _httpClient = new HttpClient();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await LoadWallet();
    }

    private async Task LoadWallet()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync("accessToken");

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert(
                    "Error",
                    "Login token not found.",
                    "OK");

                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);

            var response = await _httpClient.GetAsync(
                "https://10.0.2.2:7225/api/Wallet");

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
}

public class WalletResponse
{
    public Guid Id { get; set; }

    public string WalletNumber { get; set; } = string.Empty;

    public decimal Balance { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}