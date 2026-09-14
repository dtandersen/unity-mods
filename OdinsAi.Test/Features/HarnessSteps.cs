using OdinsAi.Core;
using Reqnroll;
using Shouldly;

namespace OdinsAi.Test.Features
{
    /// <summary>
    /// Temporary smoke test proving the Reqnroll + TUnit + Shouldly pipeline works.
    /// Replace with real Odin's Eye requirements.
    /// </summary>
    [Binding]
    public class HarnessSteps
    {
        private string _actualName;

        [Given("the Odin's Eye core is loaded")]
        public void GivenTheCoreIsLoaded()
        {
            _actualName = CoreAssembly.Name;
        }

        [Then("the core assembly name should be {string}")]
        public void ThenTheCoreAssemblyNameShouldBe(string expected)
        {
            _actualName.ShouldBe(expected);
        }
    }
}