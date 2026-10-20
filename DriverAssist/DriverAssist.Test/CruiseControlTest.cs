using DriverAssist.ECS;
using DriverAssist.Localization;
using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.Cruise
{
    public class CruiseControlTest
    {
        private readonly CruiseControl cruiseControl;
        private readonly LocoEntity loco;
        private readonly Translation localization;
        private readonly FakeCruiseControlConfig config;
        private readonly LocoSettings de2settings;
        private readonly LocoSettings dh4settings;
        private readonly FakeTrainCarWrapper train;
        private readonly FakeClock clock;
        private readonly EntityManager entityManager;

        public CruiseControlTest()
        {
            TUnitLogger.Init();

            localization = TranslationManager.Current;
            config = new FakeCruiseControlConfig
            {
                Offset = -2.5f,
                Diff = 2.5f,
                Acceleration = "DriverAssist.Cruise.FakeAccelerator",
                Deceleration = "DriverAssist.Cruise.FakeDecelerator"
            };

            de2settings = new FakeLocoConfig();
            dh4settings = new FakeLocoConfig();
            config.LocoSettings[LocoType.DE2] = de2settings;
            config.LocoSettings[LocoType.DH4] = dh4settings;
            train = new FakeTrainCarWrapper();
            loco = new LocoEntity(1f / 60f);
            loco.UpdateLocomotive(train);
            train.Type = LocoType.DE2;
            loco.Reverser = 1;
            clock = new FakeClock();
            entityManager = new EntityManager
            {
                Loco = loco
            };
            cruiseControl = new CruiseControl(config, clock, entityManager)
            {
                // Enabled = true
            };
            loco.Components.LocoSettings = config.LocoSettings[LocoType.DE2];
            loco.Components.CruiseControl = new CruiseControlComponent();
        }

        [Test]
        public void ShouldAccelerate()
        {
            loco.Throttle = 0;

            // cruiseControl.DesiredSpeed = 30;
            loco.Components.CruiseControl = CruiseControlComponent.Make(30, config.Diff, config.Offset);

            train.SpeedKmh = 29.9f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Throttle.ShouldBe(0);

            train.SpeedKmh = 25;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Throttle.ShouldBe(0);

            train.SpeedKmh = 24.9f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 27.5 km/h");
            loco.Throttle.ShouldBe(0.1f);

            train.SpeedKmh = 27.4f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 27.5 km/h");
            loco.Throttle.ShouldBe(0.2f);

            train.SpeedKmh = 27.5f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Throttle.ShouldBe(0f);

            train.SpeedKmh = 27.4f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Throttle.ShouldBe(0f);

            train.SpeedKmh = 24;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 27.5 km/h");
            loco.Throttle.ShouldBe(0.1f);
        }

        [Test]
        public void AccelerationNearZero()
        {
            config.Offset = 0;
            config.Diff = 2.5f;
            // cruiseControl.DesiredSpeed = 5;
            loco.Components.CruiseControl = CruiseControlComponent.Make(5, config.Diff, config.Offset);


            train.SpeedKmh = 0;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 5 km/h");
            loco.Throttle.ShouldBe(0.1f);
        }

        [Test]
        public void ShouldDecelerate()
        {
            loco.TrainBrake = 0;
            // cruiseControl.DesiredSpeed = 20;
            loco.Components.CruiseControl = CruiseControlComponent.Make(20, config.Diff, config.Offset);

            train.SpeedKmh = 20.1f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Decelerating to 17.5 km/h");
            loco.TrainBrake.ShouldBe(0.1f);

            // still going above 17.5, should brake
            train.SpeedKmh = 17.6f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Decelerating to 17.5 km/h");
            loco.Throttle.ShouldBe(0);
            loco.TrainBrake.ShouldBe(0.2f);

            train.SpeedKmh = 17.5f;
            WhenCruise();

            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = 17.6f;
            WhenCruise();

            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = 20.1f;
            WhenCruise();

            loco.TrainBrake.ShouldBe(0.1f);
        }

        [Test]
        public void ApplyBrakesWhenZeroSetpoint()
        {
            // cruiseControl.DesiredSpeed = 0;
            loco.Components.CruiseControl = CruiseControlComponent.Make(0, config.Diff, config.Offset);

            train.SpeedKmh = 20;
            loco.Throttle = 1;

            WhenCruise();

            loco.TrainBrake.ShouldBe(1);
            loco.Throttle.ShouldBe(0);
            cruiseControl.Status.ShouldBe(localization.CC_STOPPING);
        }

        /// When the controls change
        /// Disable cruise control
        [Test]
        public void DisableWhenControlsChange()
        {
            loco.Components.ControlsChanged = true;
            WhenCruise();
            loco.Components.CruiseControl.ShouldBeNull();
        }

        [Test]
        public void DontDoAnythingIfDisabled()
        {
            loco.Components.CruiseControl = null;
            // cruiseControl.DesiredSpeed = 10;
            // loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);
            train.SpeedKmh = 20;
            WhenCruise();
            loco.IndBrake.ShouldBe(0);
            cruiseControl.Status.ShouldBe("Disabled");
        }

        [Test]
        public void HaltTrainIfDesiredSpeedIsNegativeAndTrainIsInForwardGear()
        {
            // cruiseControl.DesiredSpeed = -10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(-10, config.Diff, config.Offset);
            loco.Reverser = 1;
            loco.Throttle = 0.5f;
            WhenCruise();
            loco.Throttle.ShouldBe(0);
            loco.TrainBrake.ShouldBe(1);
            cruiseControl.Status.ShouldBe(localization.CC_CHANGING_DIRECTION);

            WhenCruise();
            loco.Components.CruiseControl.ShouldNotBeNull();
        }

        [Test]
        public void HaltTrainIfDesiredSpeedIsPositiveAndTrainIsInReverse()
        {
            // cruiseControl.DesiredSpeed = 10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);

            loco.Reverser = 0;
            loco.Throttle = 0.5f;
            WhenCruise();
            loco.Throttle.ShouldBe(0);
            loco.TrainBrake.ShouldBe(1);
            cruiseControl.Status.ShouldBe(localization.CC_CHANGING_DIRECTION);

            WhenCruise();
            loco.Components.CruiseControl.ShouldNotBeNull();
        }

        [Test]
        public void ChangeFromReverseToForward()
        {
            // cruiseControl.DesiredSpeed = 10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);

            loco.Reverser = 0;
            WhenCruise();
            loco.Reverser.ShouldBe(1);
            loco.Throttle.ShouldBe(0);
            loco.TrainBrake.ShouldBe(1);
            cruiseControl.Status.ShouldBe(localization.CC_CHANGING_DIRECTION);
        }

        /// In this case the train skips the coasting state
        [Test]
        public void ChangeFromForwardToReverse()
        {
            config.Offset = 0;
            train.SpeedKmh = 0;
            train.Reverser = 0;
            // cruiseControl.DesiredSpeed = -5;
            loco.Components.CruiseControl = CruiseControlComponent.Make(-5, config.Diff, config.Offset);

            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_ACCELERATING, 5));

            train.SpeedKmh = -8;
            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_DECELERATING, 5));

            train.SpeedKmh = -6;
            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_DECELERATING, 5));

            train.SpeedKmh = -4.9f;
            WhenCruise();
            cruiseControl.Status.ShouldBe(localization.CC_COASTING);
        }

        /// In this case the train skips the coasting state
        [Test]
        public void ChangeFromForwardToReverse2()
        {
            config.Offset = 0;
            train.SpeedKmh = 10;
            train.Reverser = 1;
            // cruiseControl.DesiredSpeed = 5;
            loco.Components.CruiseControl = CruiseControlComponent.Make(5, config.Diff, config.Offset);

            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_DECELERATING, 5));

            train.SpeedKmh = 2;
            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_ACCELERATING, 5));

            train.SpeedKmh = 4;
            WhenCruise();
            cruiseControl.Status.ShouldBe(string.Format(localization.CC_ACCELERATING, 5));

            train.SpeedKmh = 5.1f;
            WhenCruise();
            cruiseControl.Status.ShouldBe(localization.CC_COASTING);
        }

        [Test]
        public void AccelerateInReverse()
        {
            // cruiseControl.DesiredSpeed = -10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(-10, config.Diff, config.Offset);

            train.SpeedKmh = -4;
            loco.Reverser = 0;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 7.5 km/h");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0.1f);
            loco.TrainBrake.ShouldBe(0);

            train.SpeedKmh = -7.5f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = -4;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Accelerating to 7.5 km/h");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0.1f);
            loco.TrainBrake.ShouldBe(0);
        }

        [Test]
        public void DecelerateInReverse()
        {
            // cruiseControl.DesiredSpeed = -10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(-10, config.Diff, config.Offset);

            loco.Reverser = 0;

            train.SpeedKmh = -10f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = -9f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = -11;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Decelerating to 7.5 km/h");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0.1f);

            train.SpeedKmh = -7.5f;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Coasting");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0f);

            train.SpeedKmh = -11;
            WhenCruise();
            cruiseControl.Status.ShouldBe("Decelerating to 7.5 km/h");
            loco.Reverser.ShouldBe(0);
            loco.Throttle.ShouldBe(0f);
            loco.TrainBrake.ShouldBe(0.1f);
        }

        [Test]
        public void ZeroThrottleIfInNeutral()
        {
            // cruiseControl.DesiredSpeed = 10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);

            train.SpeedKmh = 0;
            loco.Throttle = 1;
            loco.Reverser = .5f;
            WhenCruise();
            loco.Throttle.ShouldBe(0);
            cruiseControl.Status.ShouldBe("Idle: Engage reverser to start");
        }

        [Test]
        public void UseTheSettingsForTheLocoType()
        {
            train.Type = LocoType.DH4;
            loco.Components.LocoSettings = config.LocoSettings[LocoType.DH4];

            // cruiseControl.DesiredSpeed = 10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);

            train.SpeedKmh = 0;

            WhenCruise();

            ((FakeAccelerator)cruiseControl.Accelerator).Settings.ShouldBe(dh4settings);
        }

        [Test]
        public void ReportIfLocoHasNoSettings()
        {
            train.Type = LocoType.DE6;
            loco.Components.LocoSettings = null;

            WhenCruise();

            cruiseControl.Status.ShouldBe(string.Format(localization.CC_UNSUPPORTED, LocoType.DE6));
        }

        [Test]
        public void PassTimeToContext()
        {
            train.Type = LocoType.DH4;
            loco.Components.LocoSettings = config.LocoSettings[LocoType.DH4];
            // cruiseControl.DesiredSpeed = 10;
            loco.Components.CruiseControl = CruiseControlComponent.Make(10, config.Diff, config.Offset);

            train.SpeedKmh = 0;
            clock.Time2 = 5;

            WhenCruise();

            ((FakeAccelerator)cruiseControl.Accelerator).Context.Time.ShouldBe(5);
        }

        void WhenCruise()
        {
            cruiseControl.OnUpdate();
        }

        //     public class FakeLocoController : LocoController
        //     {
        //         public string LocoType { get; set; }
        //         public float SpeedKmh { get; set; }

        //         public float RelativeSpeedKmh
        //         {
        //             get
        //             {
        //                 if (IsForward)
        //                     return SpeedKmh;
        //                 else
        //                     return -SpeedKmh;
        //             }
        //         }

        //         public float Throttle { get; set; }
        //         public float TrainBrake { get; set; }
        //         public float IndBrake { get; set; }
        //         public float Temperature { get; set; }
        //         public float Torque { get; set; }
        //         public float RelativeTorque
        //         {
        //             get
        //             {
        //                 if (IsForward) return Torque;
        //                 else return -Torque;
        //             }
        //         }
        //         public float Reverser { get; set; }
        //         public float AmpsRoc { get; set; }
        //         public float AverageAmps { get; set; }
        //         public float Amps { get; set; }
        //         public float Rpm { get; set; }
        //         public float AccelerationMs { get; set; }
        //         public bool IsElectric { get; set; }

        //         public bool IsForward
        //         {
        //             get
        //             {
        //                 return Reverser >= 0.5f;
        //             }
        //         }

        //         public bool IsReversing
        //         {
        //             get
        //             {
        //                 return !IsForward;
        //             }
        //         }

        //         public float RelativeAccelerationMs
        //         {
        //             get
        //             {
        //                 if (IsForward)
        //                     return AccelerationMs;
        //                 else
        //                     return -AccelerationMs;
        //             }
        //         }
        //     }
        // }

    }

}
