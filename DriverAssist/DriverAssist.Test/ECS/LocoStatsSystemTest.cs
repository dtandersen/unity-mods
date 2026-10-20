using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.ECS
{
    public class LocoStatsSystemTest
    {
        private readonly LocoEntity loco;
        private readonly FakeTrainCarWrapper train;
        private readonly LocoStatsSystem system;

        public LocoStatsSystemTest()
        {
            TUnitLogger.Init();

            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoEntity(1f / 60f);
            loco.UpdateLocomotive(train);
            system = new LocoStatsSystem(loco, 1, .5f);
        }

        /// <summary>
        /// The train is a DM3 
        /// and a gear change has been requested.
        /// Proceed.
        /// </summary>
        [Test]
        public void UpdatesAcceleration()
        {
            loco.Components.LocoStats = new LocoStats()
            {
                SpeedMs = 1
            };
            train.SpeedMs = 2;
            WhenSystemUpdates();

            loco.Components.LocoStats.AccelerationMs2.ShouldBe(1);
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
