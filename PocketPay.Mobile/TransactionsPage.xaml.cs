using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace PocketPay.Mobile;

public partial class TransactionsPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public TransactionsPage()
    {
        InitializeComponent();

        _httpClient = new HttpClient();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var token = await SecureStorage.Default.GetAsync("accessToken");

        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//MainPage");
            return;
        }

        await LoadTransactions();
    }

    private async Task LoadTransactions()
    {
        try
        {
            var token = await SecureStorage.Default.GetAsync(
                "accessToken");

            if (string.IsNullOrEmpty(token))
            {
                await DisplayAlert(
                    "Session Expired",
                    "Please login again.",
                    "OK");

                await Shell.Current.GoToAsync("//MainPage");

                return;
            }

            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue(
                    "Bearer",
                    token);

            var response = await _httpClient.GetAsync(
                "https://localhost:7225/api/Wallet/transactions");

            if (!response.IsSuccessStatusCode)
            {
                await DisplayAlert(
                    "Error",
                    "Unable to load transactions.",
                    "OK");

                return;
            }

            var transactions =
                await response.Content
                    .ReadFromJsonAsync<List<TransactionResponse>>();

            TransactionsList.ItemsSource =
                transactions ?? new List<TransactionResponse>();
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnBackClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}

public class TransactionResponse
{
    public Guid Id { get; set; }

    public decimal Amount { get; set; }

    public string Type { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? Reference { get; set; }

    public DateTime CreatedAt { get; set; }
}