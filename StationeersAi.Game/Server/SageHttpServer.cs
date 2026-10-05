using StationeersAi.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StationeersAi.Server;

namespace StationeersAi.Game.Server;

/// <summary>
/// Loopback HTTP host for <see cref="SageRouter"/>. Mono's
/// <c>HttpListener</c> never holds a socket in this game, so this talks raw
/// TCP on <c>127.0.0.1</c> and parses just enough HTTP/1.1 (request line,
/// headers, <c>Content-Length</c> body) for Pi's calls. Each request runs on
/// the Unity main thread through <see cref="IMainThread"/>. The plugin
/// starts this when <c>Server/Enabled</c> is on.
/// </summary>
public sealed class SageHttpServer : IDisposable
{
    private readonly Func<SageData> _data;
    private readonly IMainThread _threads;
    private readonly Logger _log;
    private readonly Func<int> _port;
    private readonly Func<string> _token;

    private TcpListener? _listener;
    private Thread? _loop;
    private volatile bool _running;

    public SageHttpServer(Func<SageData> data, IMainThread threads, Logger log, Func<int> port, Func<string> token)
    {
        _data = data;
        _threads = threads;
        _log = log;
        _port = port;
        _token = token;
    }

    /// <summary>True while the socket binds and the accept loop runs.</summary>
    public bool IsAlive
    {
        get
        {
            try
            {
                return _running && _listener != null && _listener.Server.IsBound;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public void Start()
    {        if (_running)
        {
            return;
        }

        var port = _port();
        _listener = new TcpListener(IPAddress.Loopback, port);
        _listener.ExclusiveAddressUse = true;
        _listener.Start();
        _running = true;
        _loop = new Thread(Loop);
        _loop.IsBackground = true;
        _loop.Start();
        _log.Info("Sage HTTP server on " + _listener.LocalEndpoint + ". Bound=" + _listener.Server.IsBound);
        if (string.IsNullOrWhiteSpace(_token != null ? _token() : null))
        {
            _log.Info("Sage HTTP Server/Token is blank: only this machine can call.");
        }

        SelfCheck(port);
    }

    public void Dispose()
    {
        _running = false;
        try
        {
            _listener?.Stop();
        }
        catch (Exception)
        {
        }

        _listener = null;
    }

    /// <summary>
    /// Connects back to our own socket from inside the game. Tells socket
    /// failure apart from outside visibility failure.
    /// </summary>
    private void SelfCheck(int port)
    {
        try
        {
            using (var client = new TcpClient())
            {
                var result = client.BeginConnect(IPAddress.Loopback, port, null, null);
                if (result.AsyncWaitHandle.WaitOne(3000))
                {
                    client.EndConnect(result);
                    _log.Info("Sage HTTP self-check: connected to own socket.");
                }
                else
                {
                    _log.Info("Sage HTTP self-check: own socket did not answer in 3s.");
                }
            }
        }
        catch (Exception ex)
        {
            _log.Info("Sage HTTP self-check fell through: " + ex.Message);
        }
    }

    private void Loop()
    {
        while (_running)
        {
            var listener = _listener;
            if (listener == null)
            {
                return;
            }

            TcpClient? client = null;
            try
            {
                client = listener.AcceptTcpClient();
            }
            catch (Exception ex)
            {
                if (_running)
                {
                    _log.Info("Sage HTTP listener fell through: " + ex);
                }

                return;
            }

            if (client is null)
            {
                return;
            }

            var captured = client;
            ThreadPool.QueueUserWorkItem(_ => Handle(captured));
        }
    }

    private void Handle(TcpClient client)
    {
        using (client)
        {
            try
            {
                var stream = client.GetStream();
                stream.ReadTimeout = 10000;
                stream.WriteTimeout = 10000;

                var head = ReadHead(stream);
                if (head == null)
                {
                    return;
                }

                var request = RawHttpRequest.TryParse(head, null);
                if (request == null)
                {
                    Write(stream, 400, "{\"error\":\"Bad request.\"}");
                    return;
                }

                int length = 0;
                string lengthValue;
                if (request.Headers.TryGetValue("Content-Length", out lengthValue))
                {
                    int.TryParse(lengthValue.Trim(), out length);
                }

                // Small comfort for clients that wait for it.
                string expect;
                if (request.Headers.TryGetValue("Expect", out expect) &&
                    expect.IndexOf("100-continue", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    var go = Encoding.ASCII.GetBytes("HTTP/1.1 100 Continue\r\n\r\n");
                    stream.Write(go, 0, go.Length);
                }

                request.Body = length > 0 ? ReadBody(stream, length) : null;

                if (!request.Path.StartsWith("/sage", StringComparison.OrdinalIgnoreCase))
                {
                    Write(stream, 404, "{\"error\":\"No route serves " + request.Path + ".\"}");
                    return;
                }

                if (!Authorized(request))
                {
                    Write(stream, 401, "{\"error\":\"Bad or missing token.\"}");
                    return;
                }

                var captured = request;
                var response = _threads.RunSync(() => new SageRouter(_data()).Handle(captured.Method, captured.Path, captured.Query, captured.Body));
                Write(stream, response.Status, response.Body);
            }
            catch (Exception ex)
            {
                _log.Info(ex.ToString());
            }
        }
    }

    private bool Authorized(RawHttpRequest request)
    {
        // The socket binds loopback, so every caller is this machine.
        // A set token still guards all calls.
        var want = _token != null ? _token() : null;
        if (string.IsNullOrWhiteSpace(want))
        {
            return true;
        }

        string header;
        if (!request.Headers.TryGetValue("Authorization", out header))
        {
            return false;
        }

        return string.Equals(header, "Bearer " + want, StringComparison.Ordinal);
    }

    private static string? ReadHead(NetworkStream stream)
    {
        var bytes = new List<byte>(4096);
        var buffer = new byte[1024];
        while (bytes.Count < 65536)
        {
            int read;
            try
            {
                read = stream.Read(buffer, 0, buffer.Length);
            }
            catch (IOException)
            {
                return null;
            }

            if (read <= 0)
            {
                return null;
            }

            for (var i = 0; i < read; i++)
            {
                bytes.Add(buffer[i]);
            }

            if (EndsHead(bytes))
            {
                return Encoding.GetEncoding("iso-8859-1").GetString(bytes.ToArray());
            }
        }

        return null;
    }

    private static bool EndsHead(List<byte> bytes)
    {
        var n = bytes.Count;
        return n >= 4 &&
            bytes[n - 4] == '\r' && bytes[n - 3] == '\n' &&
            bytes[n - 2] == '\r' && bytes[n - 1] == '\n';
    }

    private static string? ReadBody(NetworkStream stream, int length)
    {
        if (length <= 0 || length > 1048576)
        {
            return null;
        }

        var bytes = new byte[length];
        var done = 0;
        while (done < length)
        {
            int read;
            try
            {
                read = stream.Read(bytes, done, length - done);
            }
            catch (IOException)
            {
                break;
            }

            if (read <= 0)
            {
                break;
            }

            done += read;
        }

        return Encoding.UTF8.GetString(bytes, 0, done);
    }

    private static void Write(NetworkStream stream, int status, string body)
    {
        var payload = Encoding.UTF8.GetBytes(body ?? string.Empty);
        var head = "HTTP/1.1 " + status + " " + Reason(status) + "\r\n" +
            "Content-Type: application/json\r\n" +
            "Content-Length: " + payload.Length + "\r\n" +
            "Connection: close\r\n\r\n";
        var headBytes = Encoding.ASCII.GetBytes(head);
        stream.Write(headBytes, 0, headBytes.Length);
        stream.Write(payload, 0, payload.Length);
    }

    private static string Reason(int status)
    {
        switch (status)
        {
            case 200: return "OK";
            case 400: return "Bad Request";
            case 401: return "Unauthorized";
            case 404: return "Not Found";
            case 405: return "Method Not Allowed";
            default: return "Error";
        }
    }
}
