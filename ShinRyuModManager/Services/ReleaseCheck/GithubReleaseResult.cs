using System.Text.Json.Serialization;

namespace ShinRyuModManager.Services.ReleaseCheck;

public class GithubReleaseResult
{
    [JsonPropertyName("tag_name")]
    public string TagName { get; init; }
}
