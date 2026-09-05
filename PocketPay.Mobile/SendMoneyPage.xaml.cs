using PocketPay.Mobile.Services;

namespace PocketPay.Mobile;

public partial class SendMoneyPage : ContentPage
{
    private readonly ApiService _apiService;

    public SendMoneyPage()
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
        }
    }

    private async void OnSendMoneyClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            var walletNumber =
                WalletNumberEntry.Text?.Trim();

            if (string.IsNullOrWhiteSpace(walletNumber))
            {
                await DisplayAlert(
                    "Invalid Wallet",
                    "Please enter the recipient wallet number.",
                    "OK");

                return;
            }

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

            var request = new
            {
                receiverWalletNumber = walletNumber,
                amount = amount
            };

            var response = await _apiService.SendAsync(
                HttpMethod.Post,
                "Wallet/transfer",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlert(
                    "Transfer Failed",
                    error,
                    "OK");

                return;
            }

            await DisplayAlert(
                "Success",
                $"Rs. {amount:N2} sent successfully.",
                "OK");

            WalletNumberEntry.Text = string.Empty;
            AmountEntry.Text = string.Empty;

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