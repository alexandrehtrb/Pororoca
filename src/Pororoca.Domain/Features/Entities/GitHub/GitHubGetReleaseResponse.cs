using System.Text.Json.Serialization;

namespace Pororoca.Domain.Features.Entities.GitHub;

public sealed class GitHubGetReleaseResponse
{
#nullable disable warnings

    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }

    [JsonPropertyName("name")]
    public string VersionName { get; set; }

    [JsonPropertyName("body")]
    public string Description { get; set; }

#nullable restore warnings
}