using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace StationeersAi.Test.Integration;

/// <summary>
/// Credentials for the opt-in integration tests. Read from environment variables
/// (<c>SAGEAI_ENDPOINT</c>, <c>SAGEAI_API_KEY</c>, <c>SAGEAI_MODEL</c>) or from a
/// git-ignored <c>integration.settings.json</c> copied next to the test assembly.
/// </summary>
public sealed record IntegrationSettings(string Endpoint, string ApiKey, string Model)
{
    private const string FileName = "integration.settings.json";

    public string ChatCompletionsUrl => Endpoint.TrimEnd('/') + "/chat/completions";

    public static IntegrationSettings Load()
    {
        var endpoint = Environment.GetEnvironmentVariable("SAGEAI_ENDPOINT");
        var apiKey = Environment.GetEnvironmentVariable("SAGEAI_API_KEY");
        var model = Environment.GetEnvironmentVariable("SAGEAI_MODEL");

        if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
        {
            return new IntegrationSettings(endpoint, apiKey, model ?? "gpt-4o-mini");
        }

        var path = Path.Combine(AppContext.BaseDirectory, FileName);
        if (!File.Exists(path))
        {
            return null;
        }

        var json = JObject.Parse(File.ReadAllText(path));
        return new IntegrationSettings(
            (string)json["endpoint"],
            (string)json["apiKey"],
            (string)json["model"] ?? "gpt-4o-mini");
    }
}
