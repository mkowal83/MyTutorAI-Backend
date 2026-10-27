using System.Net;
using System.Net.Http.Json;
using MyTutorAI.Api.Tests.Infrastructure;
using MyTutorAI.Application.Contracts.Memory;
using MyTutorAI.Application.Contracts.Sessions;
using MyTutorAI.Domain.Memory.Session;

namespace MyTutorAI.Api.Tests.Sessions;

/// <summary>
/// EN: Full session life cycle through the API (start → message → close → episode) on SQL Server and Redis in Testcontainers, with the LLM replaced by a fake.
/// PL: Pełny cykl życia sesji przez API (start → wiadomość → zamknięcie → epizod) na SQL Server i Redis w Testcontainers, z LLM zastąpionym fake'iem.
/// </summary>
[Collection(ApiTestCollection.Name)]
public sealed class SessionLifecycleTests(ApiTestFactory factory)
{
    [Fact]
    public async Task Session_started_messaged_and_closed_is_stored_as_episode()
    {
        var ct = TestContext.Current.CancellationToken;

        using var client = await factory.CreateAuthorizedClientAsync(ct);

        var start = await client.PostAsJsonAsync("/api/sessions", new StartSessionRequest("Job interview practice"), ct);
        start.StatusCode.Should().Be(HttpStatusCode.OK);
        var session = (await start.Content.ReadFromJsonAsync<StartSessionResponse>(ct))!;
        session.Phase.Should().Be(SessionPhase.Practice);
        session.Topic.Should().Be("Job interview practice");
        session.Opener.Should().NotBeNullOrWhiteSpace();

        var message = await client.PostAsJsonAsync(
            $"/api/sessions/{session.SessionId}/messages", new SendMessageRequest("I am working here since 2020."), ct);
        message.StatusCode.Should().Be(HttpStatusCode.OK);
        var reply = (await message.Content.ReadFromJsonAsync<SendMessageResponse>(ct))!;
        reply.Reply.Should().Be(FakeStructuredOutputClient.Reply);
        reply.WorkingMemoryCount.Should().Be(3);

        var close = await client.PostAsync($"/api/sessions/{session.SessionId}/close", content: null, ct);
        close.StatusCode.Should().Be(HttpStatusCode.OK);
        var closed = (await close.Content.ReadFromJsonAsync<CloseSessionResponse>(ct))!;
        closed.Episode.Summary.Should().Be(FakeStructuredOutputClient.Summary);
        closed.NextOpenPhase.Should().Be(SessionPhase.HomeworkCheck);

        var episodes = (await client.GetFromJsonAsync<EpisodesResponse>("/api/memory/episodes", ct))!;
        episodes.Count.Should().Be(1);
        episodes.Episodes.Should().ContainSingle().Which.Should().BeEquivalentTo(new
        {
            FakeStructuredOutputClient.Summary,
            PersonalFacts = new[] { FakeStructuredOutputClient.PersonalFact },
            FakeStructuredOutputClient.NextStep,
        });

        var closeAgain = await client.PostAsync($"/api/sessions/{session.SessionId}/close", content: null, ct);
        closeAgain.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }
}
