using DriverAssist.ECS;
using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.Cruise
{
    public class AccelTest
    {
        private readonly FakeLocoController loco;
        private readonly FakeLocoConfig de2settings;
        private readonly FakeLocoConfig dh4settings;
        private readonly FakeTrainCarWrapper train;
        private readonly PredictiveAcceleration accelerator;
        private CruiseControlContext context;
        private const float STEP = 1 / 11f;

        public AccelTest()
        {
            TUnitLogger.Init();

            de2settings = new FakeLocoConfig
            {
                CruiseAccel = .05f,
                MaxAccel = .25f,
                BrakeReleaseFactor = .5f,
                BrakingTime = 10,
                HillClimbAccel = .025f,
                MaxAmps = 750,
                OperatingTemp = 105,
                HillClimbTemp = 118,
                MinBrake = 0,
                MinTorque = 22000,
                OverdriveEnabled = true
            };

            dh4settings = new FakeLocoConfig
            {
                CruiseAccel = .05f,
                MaxAccel = .25f,
                BrakeReleaseFactor = .5f,
                BrakingTime = 10,
                HillClimbAccel = .025f,
                MaxAmps = 0,
                OperatingTemp = 105,
                HillClimbTemp = 118,
                MinBrake = 0,
                MinTorque = 35000,
                OverdriveEnabled = true
            };
            train = new FakeTrainCarWrapper
            {
                Type = LocoType.DE2
            };
            loco = new FakeLocoController(1f / 60f);
            loco.UpdateLocomotive(train);
            train.Type = LocoType.DE2;
            loco.Reverser = 1;
            accelerator = new PredictiveAcceleration
            {
                LastThrottleChange = -3
            };
            context = new CruiseControlContext(de2settings, loco);
        }

        /// <summary> 
        /// The train is stopped.
        /// It should speed up.
        /// </summary>
        [Test]
        public void AccelerateFromStop()
        {
            context.Time = 5;

            context.DesiredSpeed = 5;

            train.SpeedKmh = 0;
            WhenAccel();
            loco.Throttle.ShouldBe(STEP);
        }

        /// <summary> 
        /// The train is accelerating rapidly,
        /// but the throttle is only in the first notch.
        /// It should maintain speed.
        /// </summary>
        [Test]
        public void DontExceedMaxAccel()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = 0.25f;
            train.Throttle = STEP;

            WhenAccel();

            loco.Throttle.ShouldBe(STEP);
        }

        /// <summary> 
        /// The train is accelerating rapidly,
        /// and the throttle is past the first notch.
        /// It should slow down.
        /// </summary>
        [Test]
        public void DontExceedMaxAccel2()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.MaxAccel;
            train.Throttle = 1.1f * STEP;
            // train.SpeedKmh = 0;

            WhenAccel();

            loco.Throttle.ShouldBe(STEP);
        }

        /// <summary> 
        /// Torque is below MinTorque.
        /// The locomotive should speed up.
        /// </summary>
        [Test]
        public void SpeedUpIfLowTorque()
        {
            context.DesiredSpeed = 5;
            train.Throttle = STEP;
            train.Torque = 10000;

            WhenAccel();

            loco.Throttle.ShouldBe(2 * STEP);

            WhenAccel();
            WhenAccel();
            WhenAccel();

            loco.Throttle.ShouldBe(3 * STEP);
        }

        /// <summary> 
        /// The train is above operating temperature,
        /// it is cruising,
        /// and the temperature is not decreasing.
        /// It should slow down.
        /// </summary>
        [Test]
        public void SlowDownIfHeatIsOverOperatingTemp()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Temperature = 105;
            loco.TemperatureChange = 0;

            WhenAccel();
            loco.Throttle.ShouldBe(2 * STEP);

            WhenAccel();
            loco.Throttle.ShouldBe(2 * STEP);

            WhenAccel();
            loco.Throttle.ShouldBe(2 * STEP);

            WhenAccel();
            loco.Throttle.ShouldBe(1 * STEP);
        }

        /// <summary> 
        /// The train is above operating temperature,
        /// it is cruising,
        /// and the temperature is decreasing.
        /// It should maintain the current throttle position.
        /// </summary>
        [Test]
        public void MaintainThrottleIfTemperatureIsFalling()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Temperature = 106;
            // train.SpeedKmh = 0;

            WhenAccel();

            loco.Throttle.ShouldBe(3 * STEP);
        }

        /// <summary> 
        /// The train is below operating temperature,
        /// it is cruising,
        /// torque is low,
        /// and the temperature is increasing.
        /// It should maintain the current throttle position.
        /// </summary>
        [Test]
        public void MaintainThrottleIfTemperatureIsFalling2()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            loco.TemperatureChange = 0.1f;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Temperature = 104;
            // train.SpeedKmh = 0;

            WhenAccel();

            loco.Throttle.ShouldBe(3 * STEP);
        }

        /// <summary> 
        /// The train well below operating temperature,
        /// it is cruising,
        /// torque is low,
        /// and the temperature is increasing.
        /// It should maintain the current throttle position.
        /// </summary>
        [Test]
        public void MaintainThrottleIfTemperatureIsFalling3()
        {
            context.DesiredSpeed = 5;
            // context.Time = 5;
            loco.AccelerationMs = de2settings.CruiseAccel;
            loco.TemperatureChange = 0f;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Temperature = de2settings.OperatingTemp - 5;
            // train.SpeedKmh = 0;

            WhenAccel();

            loco.Throttle.ShouldBe(4 * STEP);
        }

        /// <summary>
        /// The train is accelerating slowly
        /// and it has exceeded operating temperature.
        /// It should speed up.
        /// </summary>
        [Test]
        public void SpeedUpWhenAccelerationIsLow()
        {
            context.DesiredSpeed = 5;
            loco.AccelerationMs = de2settings.HillClimbAccel;
            train.Throttle = 3 * STEP;
            train.Temperature = de2settings.OperatingTemp;

            WhenAccel();

            loco.Throttle.ShouldBe(4 * STEP);
        }

        // /// <summary> 
        // /// The train is above operating temperature it should maintain the current throttle position if the temperature is decreasing.
        // /// </summary>
        // [Fact(Skip = "Duplicates MaintainThrottleIfTemperatureIsFalling")]
        // public void DontThrottleUpIfCruising()
        // {
        //     context.Time = 5;
        //     accelerator.lastShift = 0;
        //     context.DesiredSpeed = 5;
        //     loco.AccelerationMs = 0.05f;
        //     loco.TemperatureChange = -.1f;
        //     train.Throttle = 2 * step;
        //     train.Torque = 15000;
        //     train.Temperature = 106;

        //     WhenAccel();

        //     train.Throttle.ShouldBe(2 * step);
        // }

        /// <summary> 
        /// The traction motor is about to explode.
        /// The train should slow down.
        /// </summary>
        [Test]
        public void SlowDownIfTemperatureIsCritical()
        {
            context.DesiredSpeed = 5;
            loco.TemperatureChange = 0;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Temperature = 118;

            WhenAccel();

            loco.Throttle.ShouldBe(2 * STEP);
        }

        /// <summary> 
        /// The traction motor is about to explode.
        /// The train should slow down.
        /// </summary>
        [Test]
        public void SlowDownIfAmpsIsCritical()
        {
            context.DesiredSpeed = 5;
            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Amps = 750;

            WhenAccel();

            loco.Throttle.ShouldBe(2 * STEP);
        }

        /// <summary> 
        /// The wheels are slipping.
        /// The train should slow down.
        /// </summary>
        [Test]
        public void SlowsDownIfWheelsAreSlipping()
        {
            context.DesiredSpeed = 5;
            // loco.TemperatureChange = -.1f;
            train.Throttle = 3 * STEP;
            train.WheelSlip = 0.5f;
            // train.Torque = 10000;
            // train.Amps = 750;

            WhenAccel();

            loco.Throttle.ShouldBe(2 * STEP);
        }

        /// <summary> 
        /// The DE4 has no traction motor,
        /// It should speed up.
        /// </summary>
        [Test]
        public void DE4HasNoAmps()
        {
            context = new CruiseControlContext(dh4settings, loco)
            {
                DesiredSpeed = 5
            };

            loco.TemperatureChange = -.1f;
            train.Throttle = 3 * STEP;
            train.Torque = 10000;
            train.Type = LocoType.DH4;
            train.Amps = 0;

            WhenAccel();

            loco.Throttle.ShouldBe(4 * STEP);
        }

        /// <summary> 
        /// Torque is above MinTorque.
        /// The train should maintain speed.
        /// </summary>
        [Test]
        public void MaintainThrottleIfTorqueHigh()
        {
            context.DesiredSpeed = 5;
            train.Throttle = 2 * STEP;
            train.Torque = 25000;

            WhenAccel();

            loco.Throttle.ShouldBe(2 * STEP);
        }

        /// <summary> 
        /// The train did not shift.
        /// LastShift should not change.
        /// </summary>
        [Test]
        public void DontUpdateShiftTimeIfWeDontShift()
        {
            context.Time = 5;
            accelerator.LastThrottleChange = 0;
            context.DesiredSpeed = 5;
            train.Throttle = 2 * STEP;
            train.Torque = 25000;

            WhenAccel();

            accelerator.LastThrottleChange.ShouldBe(0);
        }


        void WhenAccel()
        {
            accelerator.Tick(context);
            context.Time += 1;
        }
    }

    class FakeLocoController : LocoEntity
    {
        override public float AccelerationMs { get; set; }
        override public float TemperatureChange { get; set; }

        public FakeLocoController(float fixedDeltaTime) : base(fixedDeltaTime)
        {
        }
    }
}
