using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StationeersAi.Test.Fakes;

/// <summary>A request captured by <see cref="FakeOpenAiServer"/>.</summary>
public sealed record CapturedHttpRequest(
    string Method,
    string Path,
    IReadOnlyDictionary<string, string> Headers,
    string Body);

/// <summary>
/// Minimal HTTP/1.1 server on a loopback socket, standing in for an
/// OpenAI-compatible provider. Built on <see cref="TcpListener"/> so it needs no
/// URL ACL / admin rights. Scripted replies are served in order.
/// </summary>
public sealed class FakeOpenAiServer : IDisposable
{
    private readonly TcpListener _listener;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();
    private readonly Task _acceptLoop;
    private readonly Queue<(int Status, string Body)> _responses = new Queue<(int, string)>();
    private readonly List<CapturedHttpRequest> _requests = new List<CapturedHttpRequest>();
    private readonly object _gate = new object();

    public FakeOpenAiServer()
    {
        _listener = new TcpListener(IPAddress.Loopback, 0);
        _listener.Start();
        Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
        _acceptLoop = Task.Run(AcceptLoopAsync);
    }

    public int Port { get; }

    public string ChatCompletionsUrl => $"http://127.0.0.1:{Port}/v1/chat/completions";

    public IReadOnlyList<CapturedHttpRequest> Requests
    {
        get
        {
            lock (_gate)
            {
                return _requests.ToArray();
            }
        }
    }

    public void RespondWith(int status, string body)
    {
        lock (_gate)
        {
            _responses.Enqueue((status, body));
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        _listener.Stop();

        try
        {
            _acceptLoop.Wait(TimeSpan.FromSeconds(1));
        }
        catch
        {
            // shutting down
        }

        _cts.Dispose();
    }

    private async Task AcceptLoopAsync()
    {
        while (!_cts.IsCancellationRequested)
        {
            TcpClient client;
            try
            {
                client = await _listener.AcceptTcpClientAsync(_cts.Token);
            }
            catch
            {
                break;
            }

            _ = Task.Run(() => HandleAsync(client));
        }
    }

    private async Task HandleAsync(TcpClient client)
    {
        try
        {
            using (client)
            using (var stream = client.GetStream())
            {
                var request = await ReadRequestAsync(stream);
                lock (_gate)
                {
                    _requests.Add(request);
                }

                (int Status, string Body) response;
                lock (_gate)
                {
                    response = _responses.Count > 0
                        ? _responses.Dequeue()
                        : (500, "{\"error\":\"no scripted response\"}");
                }

                await WriteResponseAsync(stream, response.Status, response.Body);
            }
        }
        catch
        {
            // Test server; ignore transport-level noise.
        }
    }

    private static async Task<CapturedHttpRequest> ReadRequestAsync(NetworkStream stream)
    {
        var buffer = new byte[8192];
        var data = new List<byte>();
        var headerEnd = -1;

        while (headerEnd < 0)
        {
            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                break;
            }

            for (var i = 0; i < read; i++)
            {
                data.Add(buffer[i]);
            }

            headerEnd = FindHeaderEnd(data);
        }

        if (headerEnd < 0)
        {
            return new CapturedHttpRequest(string.Empty, "/", new Dictionary<string, string>(), string.Empty);
        }

        var headerText = Encoding.ASCII.GetString(data.ToArray(), 0, headerEnd);
        var bodyStart = headerEnd + 4;
        var body = data.GetRange(bodyStart, data.Count - bodyStart);

        var lines = headerText.Split(new[] { "\r\n" }, StringSplitOptions.None);
        var requestLine = lines[0].Split(' ');
        var method = requestLine.Length > 0 ? requestLine[0] : string.Empty;
        var path = requestLine.Length > 1 ? requestLine[1] : "/";

        var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in lines.Skip(1))
        {
            var colon = line.IndexOf(':');
            if (colon > 0)
            {
                headers[line.Substring(0, colon).Trim()] = line.Substring(colon + 1).Trim();
            }
        }

        var contentLength = headers.TryGetValue("Content-Length", out var value) ? int.Parse(value) : 0;
        while (body.Count < contentLength)
        {
            var read = await stream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                break;
            }

            for (var i = 0; i < read; i++)
            {
                body.Add(buffer[i]);
            }
        }

        var bodyText = Encoding.UTF8.GetString(body.ToArray(), 0, Math.Min(body.Count, contentLength));
        return new CapturedHttpRequest(method, path, headers, bodyText);
    }

    private static int FindHeaderEnd(List<byte> data)
    {
        for (var i = 3; i < data.Count; i++)
        {
            if (data[i - 3] == 13 && data[i - 2] == 10 && data[i - 1] == 13 && data[i] == 10)
            {
                return i - 3;
            }
        }

        return -1;
    }

    private static async Task WriteResponseAsync(NetworkStream stream, int status, string body)
    {
        var bodyBytes = Encoding.UTF8.GetBytes(body);
        var header = "HTTP/1.1 " + status + " " + Reason(status) + "\r\n" +
                     "Content-Type: application/json\r\n" +
                     "Content-Length: " + bodyBytes.Length + "\r\n" +
                     "Connection: close\r\n\r\n";
        var headerBytes = Encoding.ASCII.GetBytes(header);

        await stream.WriteAsync(headerBytes, 0, headerBytes.Length);
        await stream.WriteAsync(bodyBytes, 0, bodyBytes.Length);
        await stream.FlushAsync();
    }

    private static string Reason(int status)
    {
        switch (status)
        {
            case 200: return "OK";
            case 401: return "Unauthorized";
            case 404: return "Not Found";
            case 429: return "Too Many Requests";
            case 500: return "Internal Server Error";
            default: return "Status";
        }
    }
}
