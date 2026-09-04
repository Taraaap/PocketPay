using System.Net.Http.Json;
using PocketPay.Mobile.Models;
namespace PocketPay.Mobile;

public partial class MainPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public MainPage()
    {
        InitializeComponent();

        _httpClient = new HttpClient();
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        try
        {
            var request = new
            {
                email = EmailEntry.Text,
                password = PasswordEntry.Text
            };

           

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7225/api/Auth/login",
                request);

            

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();

                await DisplayAlert(
                    "Login Failed",
                    error,
                    "OK");

                return;
            }

            var result = await response.Content
                .ReadFromJsonAsync<LoginResponse>();

            if (result == null)
            {
                await DisplayAlert(
                    "Error",
                    "API returned no login data.",
                    "OK");

                return;
            }

           

            if (string.IsNullOrEmpty(result.AccessToken))
            {
                await DisplayAlert(
                    "Error",
                    "Access token is missing.",
                    "OK");

                return;
            }

            await SecureStorage.Default.SetAsync(
                "accessToken",
                result.AccessToken);

            await SecureStorage.Default.SetAsync(
                "userId",
                result.UserId);

            if (!string.IsNullOrEmpty(result.RefreshToken))
            {
                await SecureStorage.Default.SetAsync(
                    "refreshToken",
                    result.RefreshToken);
            }


            await Shell.Current.GoToAsync("//HomePage");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.ToString(),
                "OK");
        }
    }
}

