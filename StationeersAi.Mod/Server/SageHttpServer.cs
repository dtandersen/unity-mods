using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading;
using StationeersAi.Server;

namespace StationeersAi.Mod.Server;

/// <summary>
/// Localhost HTTP host for <see cref="SageRouter"/>. Listens on
/// <c>127.0.0.1</c> only, runs each request on the Unity main thread through
/// <see cref="IMainThread"/>, and writes back the router's JSON. The plugin
/// starts this when <c>Server/Enabled</c> is on.
/// </summary>
public sealed class SageHttpServer : IDisposable
{
    private readonly Func<SageData> _data;
    private readonly IMainThread _threads;
    private readonly BepinExLogger _log;
    private readonly Func<int> _port;
    private readonly Func<string> _token;

    private HttpListener _listener;
    private Thread _loop;
    private volatile bool _running;

    public SageHttpServer(Func<SageData> data, IMainThread threads, BepinExLogger log, Func<int> port, Func<string> token)
    {
        _data = data;
        _threads = threads;
        _log = log;
        _port = port;
        _token = token;
    }

    public void Start()
    {
        if (_running)
        {
            return;
        }

        var port = _port();
        _listener = new HttpListener();
        _listener.Prefixes.Add("http://127.0.0.1:" + port + "/");
        _listener.Start();
        _running = true;
        _loop = new Thread(Loop);
        _loop.IsBackground = true;
        _loop.Start();
        _log.Log("Sage HTTP server on 127.0.0.1:" + port);
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

    private void Loop()
    {
        while (_running)
        {
            HttpListenerContext context = null;
            try
            {
                context = _listener.GetContext();
            }
            catch (Exception)
            {
                return;
            }

            var captured = context;
            ThreadPool.QueueUserWorkItem(_ => Handle(captured));
        }
    }

    private void Handle(HttpListenerContext context)
    {
        try
        {
            if (!Authorized(context))
            {
                Write(context, 401, "{\"error\":\"Bad or missing token.\"}");
                return;
            }

            var request = context.Request;
            var path = request.Url.AbsolutePath;
            if (!path.StartsWith("/sage", StringComparison.OrdinalIgnoreCase))
            {
                Write(context, 404, "{\"error\":\"No route serves " + path + ".\"}");
                return;
            }

            var query = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string key in request.QueryString.Keys)
            {
                if (key != null)
                {
                    query[key] = request.QueryString[key];
                }
            }

            string body = null;
            if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase) && request.HasEntityBody)
            {
                using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
                {
                    body = reader.ReadToEnd();
                }
            }

            var capturedBody = body;
            var response = _threads.RunSync(() => new SageRouter(_data()).Handle(request.HttpMethod, path, query, capturedBody));
            Write(context, response.Status, response.Body);
        }
        catch (Exception ex)
        {
            _log.Log(ex.ToString());
            try
            {
                Write(context, 500, "{\"error\":\"Server fell through.\"}");
            }
            catch (Exception)
            {
            }
        }
    }

    private bool Authorized(HttpListenerContext context)
    {
        var want = _token != null ? _token() : null;
        if (string.IsNullOrWhiteSpace(want))
        {
            return true;
        }

        var header = context.Request.Headers["Authorization"];
        return string.Equals(header, "Bearer " + want, StringComparison.Ordinal);
    }

    private static void Write(HttpListenerContext context, int status, string body)
    {
        var bytes = Encoding.UTF8.GetBytes(body ?? string.Empty);
        context.Response.StatusCode = status;
        context.Response.ContentType = "application/json";
        context.Response.ContentLength64 = bytes.Length;
        context.Response.OutputStream.Write(bytes, 0, bytes.Length);
        context.Response.OutputStream.Close();
    }
}
