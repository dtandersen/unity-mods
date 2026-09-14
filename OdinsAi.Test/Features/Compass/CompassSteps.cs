using Reqnroll;
using Shouldly;
using CompassPoints = OdinsAi.Query.Compass;

namespace OdinsAi.Test.Features.CompassDirections
{
    [Binding]
    public sealed class CompassSteps
    {
        private string _direction;

        [When("I compute the compass direction {float}, {float}")]
        public void WhenIComputeTheCompassDirection(float dx, float dz)
        {
            _direction = CompassPoints.Direction(dx, dz);
        }

        [Then("the compass direction is {string}")]
        public void ThenTheCompassDirectionIs(string expected)
        {
            _direction.ShouldBe(expected);
        }
    }
}