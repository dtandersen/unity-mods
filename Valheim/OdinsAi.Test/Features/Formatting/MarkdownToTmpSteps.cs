using OdinsAi.Formatting;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features.Formatting
{
    [Binding]
    public sealed class MarkdownToTmpSteps
    {
        private string _result;

        [When("I convert the markdown {string}")]
        public void WhenIConvertTheMarkdown(string markdown)
        {
            _result = MarkdownToTmp.ToTmpRichText(markdown);
        }

        [When("I convert the markdown:")]
        public void WhenIConvertTheMarkdownBlock(string markdown)
        {
            _result = MarkdownToTmp.ToTmpRichText(markdown);
        }

        [Then("the text is {string}")]
        public void ThenTheTextIs(string expected)
        {
            _result.ShouldBe(Normalize(expected));
        }

        [Then("the text is:")]
        public void ThenTheTextIsBlock(string expected)
        {
            _result.ShouldBe(Normalize(expected));
        }

        private static string Normalize(string value)
        {
            return value?.Replace("\r\n", "\n").Replace('\r', '\n');
        }
    }
}