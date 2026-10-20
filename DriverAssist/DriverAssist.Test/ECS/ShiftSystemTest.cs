using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.ECS
{
    public class ShiftSystemTest
    {
        private readonly LocoEntity loco;
        private readonly FakeTrainCarWrapper train;
        private readonly ShiftSystem system;

        public ShiftSystemTest()
        {
            TUnitLogger.Init();

            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoEntity(1f / 60f);
            loco.UpdateLocomotive(train);
            system = new ShiftSystem(loco);
        }

        /// The train is a DM3
        /// and a gear change has been requested.
        /// Proceed.
        [Test]
        public void ShiftGearsInDm3()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 1;
            train.GearboxA = 0;
            train.GearboxB = 0;

            train.GearChangeInProgress = false;
            loco.ChangeGear(3);
            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0);
            train.GearboxB.ShouldBe(0);
            train.Throttle.ShouldBe(0);

            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0.5f);
            train.GearboxB.ShouldBe(0.5f);
            train.Throttle.ShouldBe(0);

            train.GearChangeInProgress = true;
            WhenSystemUpdates();
            train.Throttle.ShouldBe(0);

            train.GearChangeInProgress = false;
            WhenSystemUpdates();
            train.Throttle.ShouldBe(1f);
            loco.Components.GearChangeRequest.ShouldBeNull();
        }

        /// A gear change has been requested,
        /// but the train has not completely throttled down.
        /// Wait.
        [Test]
        public void WaitForZeroThrottleBeforeShifting()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 1;

            loco.ChangeGear(3);

            // throttle is not yet 0
            train.Throttle = 0.1f;
            train.GearChangeInProgress = true;

            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0);
            train.GearboxB.ShouldBe(0);
            train.Throttle.ShouldBe(0);

            // throttle is now 0
            train.Throttle = 0f;

            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0.5f);
            train.GearboxB.ShouldBe(0.5f);
        }

        /// RPM >= 750.
        /// Dont restore throttle.
        [Test]
        public void WaitForLowRpmBeforeThrottle()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 1;

            loco.ChangeGear(3);
            loco.Gear = 3;
            train.Rpm = 750;
            train.Throttle = 0;
            train.GearChangeInProgress = false;

            WhenSystemUpdates();
            train.Throttle.ShouldBe(0);
        }

        /// The train is a DM3
        /// and Throttle is 0.
        /// and a gear change has been requested.
        /// The gear change should be instant.
        [Test]
        public void ChangeGearInstantlyIfZeroThrottle()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 0;
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.ChangeGear(3);
            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0.5f);
            train.GearboxB.ShouldBe(0.5f);
            train.Throttle.ShouldBe(0);
            loco.Components.GearChangeRequest.ShouldBeNull();
        }

        /// The train is a DM3
        /// and Throttle is 1
        /// and no gear change has been requested.
        /// Leave Throttle at 1.
        [Test]
        public void DoesntAdjustThrottleIfNotShifting()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 1;
            train.GearboxA = 0;
            train.GearboxB = 0;

            train.GearChangeInProgress = false;
            loco.Gear = 0;
            loco.ChangeGear(0);

            WhenSystemUpdates();
            train.GearboxA.ShouldBe(0);
            train.GearboxB.ShouldBe(0);
            train.Throttle.ShouldBe(1);
        }

        /// The train is a DM3
        /// and Throttle is 1
        /// and no gear change has been requested.
        /// Leave Throttle at 1.
        [Test]
        public void DoesntRestoreThrottleIfNotRequested()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 0;

            loco.ChangeGear(1);
            train.Throttle = 1;
            loco.Components.GearChangeRequest.Value.RestoreThrottle.ShouldBeNull();

            WhenSystemUpdates();
            loco.Components.GearChangeRequest.ShouldNotBeNull();
            loco.Components.GearChangeRequest.Value.RestoreThrottle.ShouldBeNull();
            train.Throttle.ShouldBe(0);

            WhenSystemUpdates();
            loco.Components.GearChangeRequest.ShouldBeNull();
            train.Throttle.ShouldBe(0);
        }

        /// The train is a DE2
        /// and a gear change has been requested.
        /// Ignore it.
        [Test]
        public void DoesntAutoShiftIfTrainNotMechanical()
        {
            train.Throttle = 1;
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.ChangeGear(2);
            WhenSystemUpdates();

            train.GearboxA.ShouldBe(0);
            train.GearboxB.ShouldBe(0);
            train.Throttle.ShouldBe(1);
        }

        private void WhenSystemUpdates()
        {
            system.OnUpdate();
        }
    }
}
