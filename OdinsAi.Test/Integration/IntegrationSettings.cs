using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace OdinsAi.Test.Integration
{
    /// <summary>
    /// Credentials for the opt-in integration tests. Read from environment variables
    /// (<c>ODINSAI_ENDPOINT</c>, <c>ODINSAI_API_KEY</c>, <c>ODINSAI_MODEL</c>) or from a
    /// git-ignored <c>integration.settings.json</c> copied next to the test assembly.
    /// </summary>
    public sealed record IntegrationSettings(string Endpoint, string ApiKey, string Model)
    {
        private const string FileName = "integration.settings.json";

        public string ChatCompletionsUrl => Endpoint.TrimEnd('/') + "/chat/completions";

        public static IntegrationSettings Load()
        {
            var endpoint = Environment.GetEnvironmentVariable("ODINSAI_ENDPOINT");
            var apiKey = Environment.GetEnvironmentVariable("ODINSAI_API_KEY");
            var model = Environment.GetEnvironmentVariable("ODINSAI_MODEL");

            if (!string.IsNullOrWhiteSpace(endpoint) && !string.IsNullOrWhiteSpace(apiKey))
            {
                return new IntegrationSettings(endpoint, apiKey, model ?? "qwen38");
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
                (string)json["model"] ?? "qwen38");
        }
    }
}