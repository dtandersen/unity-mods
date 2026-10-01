using Shouldly;
using StationeersAi.Ai;
using StationeersAi.Test.Fakes;

namespace StationeersAi.Test;

/// <summary>Mention answers reach the Sage window and public chat identical and in order.</summary>
public class FanoutTests
{
    [Test]
    public void Write_reaches_every_sink()
    {
        var first = new RecordingChatSink();
        var second = new RecordingChatSink();
        var fanout = new FanoutChatSink(first, second);

        fanout.Write("Sage: hi");

        first.Lines.ShouldBe(new[] { "Sage: hi" });
        second.Lines.ShouldBe(new[] { "Sage: hi" });
    }

    [Test]
    public void Write_keeps_order_on_every_sink()
    {
        var first = new RecordingChatSink();
        var second = new RecordingChatSink();
        var fanout = new FanoutChatSink(first, second);

        fanout.Write("one");
        fanout.Write("two");

        first.Lines.ShouldBe(new[] { "one", "two" });
        second.Lines.ShouldBe(new[] { "one", "two" });
    }
}
