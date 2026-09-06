using System.Net.Http.Json;
using PocketPay.Mobile.Models;

namespace PocketPay.Mobile;

public partial class RegisterPage : ContentPage
{
    private readonly HttpClient _httpClient;

    public RegisterPage()
    {
        InitializeComponent();

        _httpClient = new HttpClient();
    }

    private async void OnRegisterClicked(
        object sender,
        EventArgs e)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(FullNameEntry.Text))
            {
                await DisplayAlert(
                    "Error",
                    "Please enter your full name.",
                    "OK");

                return;
            }

            if (string.IsNullOrWhiteSpace(EmailEntry.Text))
            {
                await DisplayAlert(
                    "Error",
                    "Please enter your email.",
                    "OK");

                return;
            }

            if (string.IsNullOrWhiteSpace(PasswordEntry.Text))
            {
                await DisplayAlert(
                    "Error",
                    "Please enter your password.",
                    "OK");

                return;
            }

            var request = new
            {
                fullName = FullNameEntry.Text.Trim(),
                email = EmailEntry.Text.Trim(),
                password = PasswordEntry.Text
            };

            var response = await _httpClient.PostAsJsonAsync(
                "https://localhost:7225/api/Auth/register",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var error =
                    await response.Content.ReadAsStringAsync();

                await DisplayAlert(
                    "Registration Failed",
                    error,
                    "OK");

                return;
            }

            await DisplayAlert(
                "Success",
                "Account created successfully. Please login.",
                "OK");

            await Shell.Current.GoToAsync("//MainPage");
        }
        catch (Exception ex)
        {
            await DisplayAlert(
                "Error",
                ex.Message,
                "OK");
        }
    }

    private async void OnLoginClicked(
        object sender,
        EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
