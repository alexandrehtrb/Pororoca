using System.Collections.Frozen;
using System.Net;
using System.Text.Json;
using NSubstitute;
using Pororoca.Domain.Features.Entities.GitHub;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using Xunit;
using static Pororoca.Domain.Features.Common.JsonConfiguration;
using static Pororoca.Domain.Features.UpdateChecker.UpdateAvailableChecker;

namespace Pororoca.Domain.Tests.Features.UpdateChecker;

public sealed class UpdateAvailableCheckerTests
{
    private readonly IPororocaRequester requester;

    public UpdateAvailableCheckerTests() =>
        this.requester = Substitute.For<IPororocaRequester>();

    [Fact]
    public void Request_should_contain_user_agent_and_accept_header()
    {
        // GIVEN
        var currentVersion = Version.Parse("1.0.0");

        // WHEN
        var res = GenerateGetLatestReleaseReq(currentVersion);

        // THEN
        Assert.NotNull(res.Headers);
        Assert.Equal(2, res.Headers.Count);
        Assert.Equal(new(true, "User-Agent", "Pororoca v1.0.0"), res.Headers[0]);
        Assert.Equal(new(true, "Accept", "application/vnd.github+json"), res.Headers[1]);
    }

    [Fact]
    public async Task If_request_fails_shouldnt_display_reminder()
    {
        // GIVEN
        var currentVersion = Version.Parse("1.0.0");
        var res = MockGetLatestReleaseResponseFailed();
        this.requester.RequestAsync([], null, null, Arg.Any<PororocaHttpRequest>(), Arg.Any<CancellationToken>())
                      .Returns(res);
        GitHubGetReleaseResponse? callbackBody = null;
        void callback(GitHubGetReleaseResponse x) => callbackBody = x;

        // WHEN
        await CheckForUpdatesAsync(this.requester, currentVersion, callback);

        // THEN
        Assert.Null(callbackBody);
        await this.requester.ReceivedWithAnyArgs().RequestAsync(null!, null, null, null!, default);
    }

    [Theory]
    [InlineData("0.8.1")]
    [InlineData("1.0.0")]
    public async Task If_latest_release_version_is_lower_or_equal_than_current_version_shouldnt_display_reminder(string latestVersion)
    {
        // GIVEN
        var currentVersion = Version.Parse("1.0.0");
        var res = MockGetLatestReleaseResponseSuccess(latestVersion);
        this.requester.RequestAsync([], null, null, Arg.Any<PororocaHttpRequest>(), Arg.Any<CancellationToken>())
                      .Returns(res);
        GitHubGetReleaseResponse? callbackBody = null;
        void callback(GitHubGetReleaseResponse x) => callbackBody = x;

        // WHEN
        await CheckForUpdatesAsync(this.requester, currentVersion, callback);

        // THEN
        Assert.Null(callbackBody);
        await this.requester.ReceivedWithAnyArgs().RequestAsync(null!, null, null, null!, default);
    }

    [Theory]
    [InlineData("1.0.1")]
    [InlineData("1.1.0")]
    [InlineData("2.0.0")]
    public async Task If_latest_release_version_is_higher_than_current_version_should_display_reminder(string latestVersion)
    {
        // GIVEN
        var currentVersion = Version.Parse("1.0.0");
        var res = MockGetLatestReleaseResponseSuccess(latestVersion);
        this.requester.RequestAsync([], null, null, Arg.Any<PororocaHttpRequest>(), Arg.Any<CancellationToken>())
                      .Returns(res);
        GitHubGetReleaseResponse? callbackBody = null;
        void callback(GitHubGetReleaseResponse x) => callbackBody = x;

        // WHEN
        await CheckForUpdatesAsync(this.requester, currentVersion, callback);

        // THEN
        Assert.NotNull(callbackBody);
        Assert.Equal("https://github.com/alexandrehtrb/Pororoca/releases/tag/" + latestVersion, callbackBody.HtmlUrl);
        Assert.Equal(latestVersion, callbackBody.VersionName);
        Assert.Equal("markdown description here", callbackBody.Description);
        await this.requester.ReceivedWithAnyArgs().RequestAsync(null!, null, null, null!, default);
    }

    private static PororocaHttpResponse MockGetLatestReleaseResponseSuccess(string version)
    {
        GitHubGetReleaseResponse bodyJson = new()
        {
            HtmlUrl = $"https://github.com/alexandrehtrb/Pororoca/releases/tag/{version}",
            VersionName = version,
            Description = "markdown description here"
        };
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(bodyJson, MinifyingJsonCtx.GitHubGetReleaseResponse);
        Dictionary<string, string> dict = new();
        var frozenDict = dict.ToFrozenDictionary();
        return new(null!, DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(250), HttpStatusCode.OK, frozenDict, frozenDict, bytes);
    }

    private static PororocaHttpResponse MockGetLatestReleaseResponseFailed()
    {
        Dictionary<string, string> dict = new();
        var frozenDict = dict.ToFrozenDictionary();
        return new(null!, DateTimeOffset.UtcNow, TimeSpan.FromMilliseconds(250), HttpStatusCode.Forbidden, frozenDict, frozenDict, []);
    }
}