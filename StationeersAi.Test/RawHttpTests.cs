using Shouldly;
using StationeersAi.Server;

namespace StationeersAi.Test;

/// <summary>Unit specs for the loopback server's minimal HTTP parsing.</summary>
public class RawHttpTests
{
    [Test]
    public void Parses_get_with_query()
    {
        var request = RawHttpRequest.TryParse("GET /sage/structures?q=vent&cableNetworkId=7 HTTP/1.1\r\nHost: x\r\n\r\n", null);

        request.ShouldNotBeNull();
        request.Method.ShouldBe("GET");
        request.Path.ShouldBe("/sage/structures");
        request.Query["q"].ShouldBe("vent");
        request.Query["cableNetworkId"].ShouldBe("7");
    }

    [Test]
    public void Parses_post_with_headers()
    {
        var request = RawHttpRequest.TryParse(
            "POST /sage/ic HTTP/1.1\r\nContent-Length: 10\r\nAuthorization: Bearer abc\r\n\r\n",
            "{\"a\":1}");

        request.ShouldNotBeNull();
        request.Method.ShouldBe("POST");
        request.Headers["Authorization"].ShouldBe("Bearer abc");
        request.Body.ShouldBe("{\"a\":1}");
    }

    [Test]
    public void Rejects_unknown_methods()
    {
        RawHttpRequest.TryParse("DELETE /sage/structures HTTP/1.1\r\n\r\n", null).ShouldBeNull();
        RawHttpRequest.TryParse("garbage", null).ShouldBeNull();
    }

    [Test]
    public void Strips_absolute_form_targets()
    {
        var request = RawHttpRequest.TryParse("GET http://127.0.0.1:27015/sage/status HTTP/1.1\r\n\r\n", null);

        request.ShouldNotBeNull();
        request.Path.ShouldBe("/sage/status");
    }
}
