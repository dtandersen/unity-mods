using Shouldly;
using StationeersAi.Formatting;

namespace StationeersAi.Test
{
    /// <summary>Assistant naming and Markdown stripping for the plain-text console.</summary>
    public class FormattingTests
    {
        [Test]
        public void Naming_resolves_blank_to_default()
        {
            ChatNaming.Resolve(null).ShouldBe("Sage");
            ChatNaming.Resolve("  ").ShouldBe("Sage");
            ChatNaming.Resolve(" Ada ").ShouldBe("Ada");
            ChatNaming.Apply("Sage", "hi").ShouldBe("Sage: hi");
        }

        [Test]
        public void Markdown_strips_to_plain_text()
        {
            MarkdownToPlain.ToPlainText("**bold** and `code`").ShouldBe("bold and code");
            MarkdownToPlain.ToPlainText("# Title").ShouldBe("Title");
            MarkdownToPlain.ToPlainText("- item").ShouldBe("• item");
            MarkdownToPlain.ToPlainText("[link](http://example.com)").ShouldBe("link");
        }
    }
}
