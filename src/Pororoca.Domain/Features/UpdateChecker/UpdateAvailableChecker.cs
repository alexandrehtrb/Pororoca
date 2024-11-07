using System.Text.Json;
using Pororoca.Domain.Features.Entities.GitHub;
using Pororoca.Domain.Features.Entities.Pororoca.Http;
using Pororoca.Domain.Features.Requester;
using static Pororoca.Domain.Features.Common.AvailablePororocaRequestSelectionOptions;
using static Pororoca.Domain.Features.Common.JsonConfiguration;

namespace Pororoca.Domain.Features.UpdateChecker;

public static class UpdateAvailableChecker
{
    internal static PororocaHttpRequest GenerateGetLatestReleaseReq(Version currentVersion) =>
        new(
            Name: string.Empty,
            HttpVersion: IsHttpVersionAvailableInOS(2.0m, out _) ? 2.0m : 1.1m,
            HttpMethod: "GET",
            Url: "https://api.github.com/repos/alexandrehtrb/Pororoca/releases/latest",
            Headers: [
                new(true, "User-Agent", "Pororoca v" + currentVersion.ToString(3)),
                new(true, "Accept", "application/vnd.github+json")
        ]);

    public static async Task CheckForUpdatesAsync(IPororocaRequester requester, Version currentVersion, Action<GitHubGetReleaseResponse> onUpdateAvailableCallback)
    {
        try
        {
            CancellationTokenSource cts = new(TimeSpan.FromSeconds(20));
            var req = GenerateGetLatestReleaseReq(currentVersion);
            var res = await requester.RequestAsync([], null, null, req, cts.Token);
            if (res.Successful && res.StatusCode == System.Net.HttpStatusCode.OK && res.HasBody)
            {
                using MemoryStream ms = new(res.GetBodyAsBinary()!);
                var body = JsonSerializer.Deserialize(ms, MinifyingJsonCtx.GitHubGetReleaseResponse)!;
                var latestVersion = Version.Parse(body.VersionName);
                if (latestVersion > currentVersion)
                {
                    onUpdateAvailableCallback(body);
                }
            }
        }
        catch
        {
        }
    }
}
