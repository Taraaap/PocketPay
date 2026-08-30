using System.Net.Http.Json;

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
                "https://10.0.2.2:7225/api/Auth/login",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content
                    .ReadFromJsonAsync<LoginResponse>();

                if (result != null && !string.IsNullOrEmpty(result.AccessToken))
                {
                    await SecureStorage.Default.SetAsync(
                        "accessToken",
                        result.AccessToken);

                    await SecureStorage.Default.SetAsync(
                        "userId",
                        result.UserId);

                    await Navigation.PushAsync(
                        new HomePage());
                }

                await DisplayAlert(
                    "Success",
                    $"Welcome {result?.FullName}!",
                    "OK");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();

                await DisplayAlert(
                    "Login Failed",
                    error,
                    "OK");
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

public class LoginResponse
{
    public string UserId { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? AccessToken { get; set; }

    public string? RefreshToken { get; set; }

    public DateTime? AccessTokenExpiresAt { get; set; }

    public string Message { get; set; } = string.Empty;
}