using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.ECS
{
    public class ControlsChangedSystemTest
    {
        private readonly LocoEntity loco;
        private readonly EntityManager entityManager;
        private readonly ControlsChangedSystem system;

        public ControlsChangedSystemTest()
        {
            TUnitLogger.Init();

            entityManager = new();
            loco = entityManager.Loco = new LocoEntity(1f / 60f);
            entityManager.Loco.UpdateLocomotive(new FakeTrainCarWrapper());
            system = new ControlsChangedSystem(entityManager);
        }

        /// When the indepdentdant brake is applied
        /// Then the controls have changed
        [Test]
        public void SavesStateOfBrakes()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.LastControls = new LastControls()
            {
                IndBrake = 0
            };
            loco.IndBrake = 0.5f;

            WhenSystemUpdates();

            loco.Components.ControlsChanged.ShouldBe(true);
            loco.Components.CruiseControl.ShouldBeNull();
        }

        /// When the train brake is applied
        /// Then the controls have changed
        [Test]
        public void SavsesStateOfBrakes()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.LastControls = new LastControls()
            {
                TrainBrake = 0
            };
            loco.TrainBrake = 0.5f;

            WhenSystemUpdates();

            loco.Components.ControlsChanged.ShouldBe(true);
            loco.Components.CruiseControl.ShouldBeNull();
        }

        /// When the controls are the same
        /// Then the controls haven't changed
        [Test]
        public void SavsesStateOfBrakess()
        {
            loco.Components.CruiseControl = new CruiseControlComponent();
            loco.Components.ControlsChanged = true;
            loco.Components.LastControls = new LastControls()
            {
                TrainBrake = 0,
                IndBrake = 0
            };
            loco.TrainBrake = 0;
            loco.IndBrake = 0;

            WhenSystemUpdates();

            loco.Components.ControlsChanged.ShouldBeNull();
            loco.Components.CruiseControl.ShouldNotBeNull();
        }

        [Test]
        public void NullLoco()
        {
            entityManager.Loco = null;

            WhenSystemUpdates();
        }

        [Test]
        public void NullControls()
        {
            entityManager.Loco.Components.LastControls = null;

            WhenSystemUpdates();

            entityManager.Loco.Components.LastControls.ShouldBeNull();
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
