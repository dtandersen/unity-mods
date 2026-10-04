using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StationeersAi.Formatting;

namespace StationeersAi.Server;

/// <summary>
/// One HTTP answer from <see cref="SageRouter"/>: status plus JSON.
/// Every string value ships with Unity rich-text tags stripped, so outside
/// tools never see the <c>color</c>/<c>size</c>/<c>link</c> markers the game
/// UI needs. IC10 has no angle-bracket syntax, so editor text is safe.
/// </summary>
public sealed class SageResponse
{
    public SageResponse(int status, string body)
    {
        Status = status;
        Body = body;
    }

    public int Status { get; }

    public string Body { get; }

    public static SageResponse Ok(object payload)
    {
        return new SageResponse(200, Serialize(payload));
    }

    public static SageResponse Error(int status, string message)
    {
        return new SageResponse(status, Serialize(new ErrorBody(message)));
    }

    private static string Serialize(object payload)
    {
        // NB: JToken.ToString(Formatting) is missing from the Newtonsoft copy
        // the game loads, so round-trip through the oldest stable call.
        var token = JToken.FromObject(payload);
        StripTags(token);
        return JsonConvert.SerializeObject(token);
    }

    private static void StripTags(JToken token)
    {
        if (token is JObject obj)
        {
            foreach (var property in obj.Properties())
            {
                StripTags(property.Value);
            }

            return;
        }

        if (token is JArray list)
        {
            foreach (var item in list)
            {
                StripTags(item);
            }

            return;
        }

        if (token is JValue value && value.Type == JTokenType.String)
        {
            var text = (string)value.Value;
            var plain = UnityRichText.Strip(text);
            if (!ReferenceEquals(plain, text))
            {
                value.Value = plain;
            }
        }
    }

    private sealed class ErrorBody
    {
        public ErrorBody(string message)
        {
            Error = message;
        }

        [JsonProperty("error")]
        public string Error { get; }
    }
}
