namespace PocketPay.Mobile.Services;

public class SessionService
{
    public async Task<bool> IsLoggedInAsync()
    {
        var token = await SecureStorage.Default.GetAsync("accessToken");

        return !string.IsNullOrWhiteSpace(token);
    }

    public async Task LogoutAsync()
    {
        SecureStorage.Default.Remove("accessToken");
        SecureStorage.Default.Remove("userId");
        SecureStorage.Default.Remove("refreshToken");
    }
}