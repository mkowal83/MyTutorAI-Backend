using System.Net;
using System.Net.Http.Json;
using MyTutorAI.Api.Tests.Infrastructure;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Application.Llm.Contracts;

namespace MyTutorAI.Api.Tests.Sessions;

/// <summary>
/// EN: Data isolation between users: another user's session behaves as if it did not exist (404), and no LLM call is made on its behalf.
/// PL: Izolacja danych między użytkownikami: cudza sesja zachowuje się jak nieistniejąca (404) i nie jest wywoływany LLM w jej imieniu.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class SessionIsolationTests(ApiTestFactory factory)
{
    [Fact]
    public async Task Another_users_session_is_not_found()
    {
        var ct = TestContext.Current.CancellationToken;

        using var owner = await factory.CreateAuthorizedClientAsync(ct);
        using var stranger = await factory.CreateAuthorizedClientAsync(ct);

        var start = await owner.PostAsJsonAsync("/api/sessions", new StartSessionRequest("Small talk"), ct);
        start.StatusCode.Should().Be(HttpStatusCode.OK);
        var sessionId = (await start.Content.ReadFromJsonAsync<StartSessionResponse>(ct))!.SessionId;

        var tutorCalls = factory.Llm.CallCount<LiveTutorReplyResponse>();
        var episodeCalls = factory.Llm.CallCount<EpisodeResponse>();

        (await stranger.GetAsync($"/api/sessions/{sessionId}", ct)).StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await stranger.PostAsJsonAsync($"/api/sessions/{sessionId}/messages", new SendMessageRequest("Hello!"), ct))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);
        (await stranger.PostAsync($"/api/sessions/{sessionId}/close", content: null, ct))
            .StatusCode.Should().Be(HttpStatusCode.NotFound);

        factory.Llm.CallCount<LiveTutorReplyResponse>().Should().Be(tutorCalls);
        factory.Llm.CallCount<EpisodeResponse>().Should().Be(episodeCalls);
        (await owner.GetAsync($"/api/sessions/{sessionId}", ct)).StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
