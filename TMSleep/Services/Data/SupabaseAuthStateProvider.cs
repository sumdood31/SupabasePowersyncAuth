using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;

public class SupabaseAuthStateProvider : AuthenticationStateProvider
{
    private readonly SupabaseConnector _connector;

    public SupabaseAuthStateProvider(SupabaseConnector connector)
    {
        _connector = connector;

        // Listen to the state changes we established inside the connector
        _connector.Client.Auth.AddStateChangedListener((sender, state) =>
        {
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        });
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var user = _connector.Client.Auth.CurrentUser;

        if (user != null && _connector.Client.Auth.CurrentSession != null)
        {
            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Id ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? "")
            };
            var identity = new ClaimsIdentity(claims, "SupabaseAuth");
            return new AuthenticationState(new ClaimsPrincipal(identity));
        }

        return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }
}