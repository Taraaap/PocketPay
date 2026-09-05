using System.Net.Http.Headers;
using System.Net.Http.Json;
using PocketPay.Mobile.Services;
namespace PocketPay.Mobile;

public partial class DepositPage : ContentPage
{
    private readonly ApiService _apiService;

    public DepositPage()
    {
        InitializeComponent();

        _apiService = new ApiService();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var token = await SecureStorage.Default.GetAsync("accessToken");

        if (string.IsNullOrEmpty(token))
        {
            await Shell.Current.GoToAsync("//MainPage");
        }
    }

    private async void OnDepositClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            if (!decimal.TryParse(
                    AmountEntry.Text,
                    out decimal amount))
            {
                await DisplayAlert(
                    "Invalid Amount",
                    "Please enter a valid amount.",
                    "OK");

                return;
            }

            if (amount <= 0)
            {
                await DisplayAlert(
                    "Invalid Amount",
                    "Amount must be greater than zero.",
                    "OK");

                return;
            }

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

            var request = new
            {
                amount = amount
            };

            var response = await _apiService.SendAsync(
             HttpMethod.Post, "Wallet/deposit", request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlert(
                    "Deposit Failed",
                    error,
                    "OK");

                return;
            }

            await DisplayAlert(
                "Success",
                $"Rs. {amount:N2} deposited successfully.",
                "OK");

            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnCancelClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("//HomePage");
    }
}