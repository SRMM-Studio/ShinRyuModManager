using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;
using Utils;

namespace ShinRyuModManager.Services.ReleaseCheck;

public static class ReleaseCheck
{
    private const string SRMM_LATEST = "https://api.github.com/repos/SRMM-Studio/ShinRyuModManager/releases/latest";
    
    public static async Task<Version> CheckForUpdate()
    {
        var buildVersion = AssemblyVersion.GetBuildVersion();
        
        // Get latest version tag
        using var request = new HttpRequestMessage(HttpMethod.Get, SRMM_LATEST);
        
        request.Headers.Add("User-Agent", $"SRMM/{buildVersion}");
        request.Headers.Add("Accept", "application/vnd.github+json");
        request.Headers.Add("X-GitHub-Api-Version", "2026-03-10");
        
        using var response = await Utils.Client.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            Log.Warning("Unable to check for updates. Skipping...");
            
            return null;
        }
        
        var result = await response.Content.ReadAsStreamAsync();
        var parsedResult = await JsonSerializer.DeserializeAsync(result, SourceGenerationContext.Default.GithubReleaseResult);
        
        if (parsedResult == null || !Version.TryParse(parsedResult.TagName.TrimStart('v'), out var tagVersion))
        {
            Log.Warning("Unable to parse GitHub release tag: {Version}", parsedResult);
            
            return null;
        }
        
        return tagVersion > buildVersion ? tagVersion : null;
    }
}

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(GithubReleaseResult))]
internal partial class SourceGenerationContext : JsonSerializerContext;
