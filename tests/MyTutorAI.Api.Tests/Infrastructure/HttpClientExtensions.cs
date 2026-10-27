using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using MyTutorAI.Application.Contracts.Auth;

namespace MyTutorAI.Api.Tests.Infrastructure;

/// <summary>
/// EN: Helpers for calling the API under test as a freshly registered user.
/// PL: Pomocniki do wywoływania testowanego API jako świeżo zarejestrowany użytkownik.
/// </summary>
public static class HttpClientExtensions
{
    private const string Password = "Integration-Tests-1";

    /// <summary>
    /// EN: Registers an account with a unique e-mail and logs it in; returns the token pair.
    /// PL: Rejestruje konto z unikalnym e-mailem i loguje je; zwraca parę tokenów.
    /// </summary>
    public static async Task<AuthResponse> RegisterAndLoginAsync(this HttpClient client, CancellationToken cancellationToken)
    {
        var email = $"it-{Guid.NewGuid():N}@example.com";

        var register = await client.PostAsJsonAsync("/api/auth/register", new RegisterRequest(email, Password, "Integration Tester"), cancellationToken);
        register.StatusCode.Should().Be(HttpStatusCode.Accepted);

        var login = await client.PostAsJsonAsync("/api/auth/login", new LoginRequest(email, Password), cancellationToken);
        login.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await login.Content.ReadFromJsonAsync<AuthResponse>(cancellationToken))!;
    }

    /// <summary>
    /// EN: Creates a client of a newly registered user with its access token in the <c>Authorization</c> header.
    /// PL: Tworzy klienta nowo zarejestrowanego użytkownika z jego access tokenem w nagłówku <c>Authorization</c>.
    /// </summary>
    public static async Task<HttpClient> CreateAuthorizedClientAsync(this ApiTestFactory factory, CancellationToken cancellationToken)
    {
        var client = factory.CreateClient();
        var auth = await client.RegisterAndLoginAsync(cancellationToken);
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth.AccessToken);
        return client;
    }
}
