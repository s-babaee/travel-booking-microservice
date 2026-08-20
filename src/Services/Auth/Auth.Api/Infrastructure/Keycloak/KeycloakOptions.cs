namespace Auth.Api.Infrastructure.Keycloak;

public sealed class KeycloakOptions
{
    public string BaseUrl { get; set; } = "http://localhost:8081";
    public string Realm { get; set; } = "travel";
    public string ClientId { get; set; } = "travel-auth-api";
    public string ClientSecret { get; set; } = string.Empty;
    public string AdminRealm { get; set; } = "master";
    public string AdminUsername { get; set; } = "admin";
    public string AdminPassword { get; set; } = string.Empty;
}
