using System;
using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.ECS
{
    public class LastControlsSystemTest
    {
        private readonly LocoEntity loco;
        private readonly EntityManager entityManager;
        private readonly LastControlsSystem system;

        public LastControlsSystemTest()
        {
            TUnitLogger.Init();

            entityManager = new();
            loco = entityManager.Loco = new LocoEntity(1f / 60f);
            entityManager.Loco.UpdateLocomotive(new FakeTrainCarWrapper());
            system = new LastControlsSystem(entityManager);
        }

        [Test]
        public void SavesStateOfBrakes()
        {
            loco.IndBrake = 0.5f;
            loco.TrainBrake = 0.6f;

            WhenSystemUpdates();

            loco.Components.LastControls.Value.IndBrake.ShouldBe(loco.IndBrake);
            loco.Components.LastControls.Value.TrainBrake.ShouldBe(loco.TrainBrake);
        }

        [Test]
        public void NullLoco()
        {
            entityManager.Loco = null;

            WhenSystemUpdates();
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
