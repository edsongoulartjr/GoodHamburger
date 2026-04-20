using System.Net.Http.Headers;
using System.Net.Http.Json;
using GoodHamburger.Web.Models;
using Microsoft.JSInterop;

namespace GoodHamburger.Web.Services;

public class AuthStateService(HttpClient http, IJSRuntime js)
{
    public string? UserName { get; private set; }
    public bool IsAuthenticated => !string.IsNullOrEmpty(_token);
    public event Action? OnChange;

    private string? _token;

    public async Task InitializeAsync()
    {
        _token = await js.InvokeAsync<string?>("localStorage.getItem", "jwt_token");
        UserName = await js.InvokeAsync<string?>("localStorage.getItem", "user_name");
        ApplyToken();
    }

    public async Task<string?> RegisterAsync(RegisterModel model)
    {
        var response = await http.PostAsJsonAsync("api/auth/register", model);
        return await HandleAuthResponse(response);
    }

    public async Task<string?> LoginAsync(LoginModel model)
    {
        var response = await http.PostAsJsonAsync("api/auth/login", model);
        return await HandleAuthResponse(response);
    }

    public async Task LogoutAsync()
    {
        _token = null;
        UserName = null;
        await js.InvokeVoidAsync("localStorage.removeItem", "jwt_token");
        await js.InvokeVoidAsync("localStorage.removeItem", "user_name");
        http.DefaultRequestHeaders.Authorization = null;
        OnChange?.Invoke();
    }

    private async Task<string?> HandleAuthResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<AuthResponseModel>(ApiJsonOptions.Default);
            if (result is not null)
            {
                _token = result.Token;
                UserName = result.Name;
                await js.InvokeVoidAsync("localStorage.setItem", "jwt_token", _token);
                await js.InvokeVoidAsync("localStorage.setItem", "user_name", UserName);
                ApplyToken();
                OnChange?.Invoke();
                return null; // success
            }
        }

        // Error
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ProblemModel>(ApiJsonOptions.Default);
            return error?.Detail ?? "Erro desconhecido.";
        }
        catch
        {
            return "Erro de comunicação com o servidor.";
        }
    }

    private void ApplyToken()
    {
        if (!string.IsNullOrEmpty(_token))
            http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
    }

    private record ProblemModel(string? Detail);
}
