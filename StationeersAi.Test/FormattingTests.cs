using Shouldly;
using StationeersAi.Formatting;

namespace StationeersAi.Test;

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
    [Test]
    public void Log_text_folds_outside_latin1()
    {
        LogText.Sanitize("a\u2014b\u2022c\u2026d\U0001F44Be\u00B0f").ShouldBe("a-b-c...d??e\u00B0f");
        LogText.Sanitize(null).ShouldBeNull();
    }

    [Test]
    public void Code_hash_is_stable_short_hex()
    {
        CodeHash.Hash(string.Empty).ShouldBe("811c9dc5");
        CodeHash.Hash("a").ShouldBe("e40c292c");
        CodeHash.Hash("yield").ShouldBe("6c96f2ae");
        CodeHash.Hash(null).ShouldBe(CodeHash.Hash(string.Empty));
    }
}
