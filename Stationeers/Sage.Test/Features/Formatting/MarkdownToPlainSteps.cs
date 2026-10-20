using Reqnroll;
using Shouldly;
using Sage.Formatting;

namespace Sage.Test.Features.Formatting;

[Binding]
public sealed class MarkdownToPlainSteps
{
    private string _text;

    [When("I convert the markdown {string}")]
    public void WhenIConvertTheMarkdown(string markdown)
    {
        _text = MarkdownToPlain.ToPlainText(markdown);
    }

    [When("I convert the markdown:")]
    public void WhenIConvertTheMarkdownBlock(string markdown)
    {
        _text = MarkdownToPlain.ToPlainText(markdown);
    }

    [Then("the text is {string}")]
    public void ThenTheTextIs(string expected)
    {
        _text.ShouldBe(expected);
    }

    [Then("the text is:")]
    public void ThenTheTextIsBlock(string expected)
    {
        _text.ShouldBe(expected.Replace("\r\n", "\n").Trim());
    }
}
