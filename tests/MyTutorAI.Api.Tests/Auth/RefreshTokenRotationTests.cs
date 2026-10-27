using System.Net;
using System.Net.Http.Json;
using MyTutorAI.Api.Tests.Infrastructure;
using MyTutorAI.Application.Contracts.Auth;

namespace MyTutorAI.Api.Tests.Auth;

/// <summary>
/// EN: Refresh token rotation: each use issues a new token, and presenting an already rotated token revokes the user's whole token chain.
/// PL: Rotacja refresh tokenu: każde użycie wydaje nowy token, a przedstawienie już zrotowanego tokenu unieważnia cały łańcuch tokenów użytkownika.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class RefreshTokenRotationTests(ApiTestFactory factory)
{
    [Fact]
    public async Task Reusing_rotated_refresh_token_revokes_the_whole_chain()
    {
        var ct = TestContext.Current.CancellationToken;

        using var client = factory.CreateClient();
        var login = await client.RegisterAndLoginAsync(ct);

        var rotate = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(login.RefreshToken), ct);
        rotate.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotated = (await rotate.Content.ReadFromJsonAsync<AuthResponse>(ct))!;
        rotated.RefreshToken.Should().NotBe(login.RefreshToken);
        rotated.UserId.Should().Be(login.UserId);

        var reuse = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(login.RefreshToken), ct);
        reuse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        var afterRevocation = await client.PostAsJsonAsync("/api/auth/refresh", new RefreshRequest(rotated.RefreshToken), ct);
        afterRevocation.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
