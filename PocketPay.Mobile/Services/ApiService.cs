using System.Net.Http.Headers;
using System.Net.Http.Json;
using PocketPay.Mobile;
using PocketPay.Mobile.Models;
namespace PocketPay.Mobile.Services;

public class ApiService
{
    private readonly HttpClient _httpClient;

    private const string BaseUrl =
        "https://localhost:7225/api";

    public ApiService()
    {
        _httpClient = new HttpClient();
    }

    public async Task<HttpResponseMessage> SendAsync(
        HttpMethod method,
        string endpoint,
        object? body = null)
    {
        var accessToken =
            await SecureStorage.Default.GetAsync("accessToken");

        if (string.IsNullOrEmpty(accessToken))
        {
            throw new Exception("User is not logged in.");
        }

        var response = await SendRequestAsync(
            method,
            endpoint,
            accessToken,
            body);

      
        if (response.StatusCode ==
            System.Net.HttpStatusCode.Unauthorized)
        {
            var refreshed = await RefreshTokenAsync();

            if (!refreshed)
            {
                throw new Exception(
                    "Session expired. Please login again.");
            }

          
            accessToken =
                await SecureStorage.Default
                    .GetAsync("accessToken");

            response = await SendRequestAsync(
                method,
                endpoint,
                accessToken!,
                body);
        }

        return response;
    }

    private async Task<HttpResponseMessage> SendRequestAsync(
        HttpMethod method,
        string endpoint,
        string accessToken,
        object? body)
    {
        var request = new HttpRequestMessage(
            method,
            $"{BaseUrl}/{endpoint}");

        request.Headers.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                accessToken);

        if (body != null)
        {
            request.Content = JsonContent.Create(body);
        }

        return await _httpClient.SendAsync(request);
    }

    private async Task<bool> RefreshTokenAsync()
    {
        var refreshToken =
            await SecureStorage.Default
                .GetAsync("refreshToken");

        if (string.IsNullOrEmpty(refreshToken))
            return false;

        var request = new
        {
            refreshToken = refreshToken
        };

        var response = await _httpClient.PostAsJsonAsync(
            $"{BaseUrl}/Auth/refresh",
            request);

        if (!response.IsSuccessStatusCode)
        {
            SecureStorage.Default.Remove("accessToken");
            SecureStorage.Default.Remove("refreshToken");
            SecureStorage.Default.Remove("userId");

            return false;
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponse>();

        if (result == null ||
            string.IsNullOrEmpty(result.AccessToken))
        {
            return false;
        }

        await SecureStorage.Default.SetAsync(
            "accessToken",
            result.AccessToken);

        if (!string.IsNullOrEmpty(result.RefreshToken))
        {
            await SecureStorage.Default.SetAsync(
                "refreshToken",
                result.RefreshToken);
        }

        return true;
    }
}