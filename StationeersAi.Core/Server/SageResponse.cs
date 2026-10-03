using Newtonsoft.Json;

namespace StationeersAi.Server;

/// <summary>One HTTP answer from <see cref="SageRouter"/>: status plus JSON.</summary>
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
        return new SageResponse(200, JsonConvert.SerializeObject(payload));
    }

    public static SageResponse Error(int status, string message)
    {
        return new SageResponse(status, JsonConvert.SerializeObject(new ErrorBody(message)));
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
