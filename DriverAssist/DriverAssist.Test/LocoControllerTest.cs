using DriverAssist.ECS;
using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.Cruise
{
    public class LocoEntityTest
    {
        private readonly LocoEntity loco;
        private readonly FakeTrainCarWrapper train;

        public LocoEntityTest()
        {
            TUnitLogger.Init();

            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2,
                Reverser = 1
            };

            loco = new LocoEntity(1f / 60f);
            loco.UpdateLocomotive(train);
        }

        /// The train is a DM3
        /// and a throttle change has been requested,
        /// but a gear change is in progress.
        /// Ignore it.
        [Test]
        public void PreventsThrottleChangeDuringShift()
        {
            train.Type = LocoType.DM3;
            train.Throttle = 0;

            // train is now changing gears
            loco.Components.GearChangeRequest = new GearChangeRequest();

            // should ignore this
            loco.Throttle = 1;

            train.Throttle.ShouldBe(0);
        }

        /// The train is a DE2
        /// and Gear is set.
        /// Ignore it.
        [Test]
        public void DoesntWriteToGearboxes()
        {
            train.GearboxA = 0;
            train.GearboxB = 0;

            loco.Gear = 2;

            train.GearboxA.ShouldBe(0);
            train.GearboxB.ShouldBe(0);
        }

        /// The train is a DE2
        /// and Gear is read.
        /// Return -1.
        [Test]
        public void DoesntReadFromGearboxes()
        {
            train.GearboxA = 1;
            train.GearboxB = 1;

            loco.Gear = 0;

            loco.Gear.ShouldBe(-1);
        }

        /// The train is a DE2
        /// and a gear change is requested.
        /// Deny it.
        [Test]
        public void DenyDe2Shift()
        {
            loco.Gear = 0;
            loco.ChangeGear(1);

            loco.Components.GearChangeRequest.ShouldBeNull();
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Test]
        public void AllowDm3GearShift()
        {
            train.Type = LocoType.DM3;

            // loco.Gear = 0;
            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 1;
            loco.ChangeGear(1);

            loco.Components.GearChangeRequest.Value.RequestedGear.ShouldBe(1);
            loco.Components.GearChangeRequest.Value.RestoreThrottle.ShouldBe(1f);
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Test]
        public void DontRestoreThrottleIfThrottleIsZero()
        {
            train.Type = LocoType.DM3;

            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 0;
            loco.ChangeGear(1);

            loco.Components.GearChangeRequest.Value.RequestedGear.ShouldBe(1);
            loco.Components.GearChangeRequest.Value.RestoreThrottle.ShouldBeNull();
        }

        /// The train is a DM3
        /// and a gear change is requested for gear 1 and throttle 1.
        /// Request a gear change to gear 1 and original throttle of 1.
        [Test]
        public void DoesntShiftIfShifting()
        {
            train.Type = LocoType.DM3;

            train.GearboxA = 0;
            train.GearboxB = 0;
            train.Throttle = 0;
            var request = new GearChangeRequest
            {
                RequestedGear = 8,
                RestoreThrottle = 1
            };
            loco.Components.GearChangeRequest = request;
            loco.ChangeGear(1);

            loco.Components.GearChangeRequest.Value.RequestedGear.ShouldBe(8);
            loco.Components.GearChangeRequest.Value.RestoreThrottle.ShouldBe(1);
        }

        /// The train is a DM3
        /// and Gear is 0
        /// and the gear -1 is requested.
        /// Ignore it.
        [Test]
        public void CantShiftBelowMinGear()
        {
            train.Type = LocoType.DM3;

            loco.Gear = 0;
            loco.ChangeGear(-1);

            loco.Components.GearChangeRequest.ShouldBeNull();
        }

        /// The train is a DM3
        /// and Gear is 8
        /// and the gear 9 is requested.
        /// Ignore it.
        [Test]
        public void CantShiftAboveMaxGear()
        {
            train.Type = LocoType.DM3;

            loco.Gear = 8;
            loco.ChangeGear(9);

            loco.Components.GearChangeRequest.ShouldBeNull();
        }

        /// The train is a DM3
        /// and Gear is 1
        /// and the gear 1 is requested.
        /// Ignore it.
        [Test]
        public void DoesntShiftToSameGear()
        {
            train.Type = LocoType.DM3;

            loco.Gear = 1;
            loco.ChangeGear(1);

            loco.Components.GearChangeRequest.ShouldBeNull();
        }
    }
}
