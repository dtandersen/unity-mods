using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using StationeersAi.Ai;
using UnityEngine.Networking;

namespace StationeersAi.Mod.Ai
{
    /// <summary>
    /// The Unity implementation of the HTTP boundary. This is the only part of the
    /// chain that cannot run outside Unity, so it is kept deliberately thin: build
    /// the request, await it, hand back the status and body.
    /// </summary>
    public sealed class UnityWebRequestTransport : IChatTransport
    {
        private readonly int _timeoutSeconds;

        public UnityWebRequestTransport(int timeoutSeconds)
        {
            _timeoutSeconds = timeoutSeconds;
        }

        public Task<HttpReply> PostAsync(HttpRequest request, CancellationToken cancellationToken = default)
        {
            var completion = new TaskCompletionSource<HttpReply>();

            var www = new UnityWebRequest(request.Url, UnityWebRequest.kHttpVerbPOST)
            {
                uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(request.Body)),
                downloadHandler = new DownloadHandlerBuffer(),
                timeout = _timeoutSeconds,
            };

            www.SetRequestHeader("Content-Type", "application/json");

            if (request.Headers != null)
            {
                foreach (var header in request.Headers)
                {
                    www.SetRequestHeader(header.Key, header.Value);
                }
            }

            www.SendWebRequest().completed += _ =>
            {
                try
                {
                    if (www.result != UnityWebRequest.Result.Success)
                    {
                        completion.TrySetException(new AiClientException(
                            $"Request failed: {www.error} (HTTP {www.responseCode})"));
                    }
                    else
                    {
                        completion.TrySetResult(new HttpReply(
                            (int)www.responseCode,
                            www.downloadHandler != null ? www.downloadHandler.text : string.Empty));
                    }
                }
                finally
                {
                    www.Dispose();
                }
            };

            if (cancellationToken.CanBeCanceled)
            {
                cancellationToken.Register(() => completion.TrySetCanceled(cancellationToken));
            }

            return completion.Task;
        }
    }
}
