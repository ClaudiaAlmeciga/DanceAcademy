using Microsoft.JSInterop;

namespace DanceAcademy.Public.Auth;

public class TokenStorageService(IJSRuntime js)
{
    private const string TokenKey = "da_public_token";

    public ValueTask SaveAsync(string token) =>
        js.InvokeVoidAsync("localStorage.setItem", TokenKey, token);

    public ValueTask<string?> GetAsync() =>
        js.InvokeAsync<string?>("localStorage.getItem", TokenKey);

    public ValueTask RemoveAsync() =>
        js.InvokeVoidAsync("localStorage.removeItem", TokenKey);
}
