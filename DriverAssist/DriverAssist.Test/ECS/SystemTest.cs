using DriverAssist.Test;
using Shouldly;

namespace DriverAssist.ECS
{
    public class SystemTest
    {
        private readonly SystemManager systemManager;
        private readonly FakeSystem system1;
        private readonly FakeSystem system2;

        public SystemTest()
        {
            TUnitLogger.Init();
            systemManager = new SystemManager();
            system1 = new FakeSystem();
            system2 = new FakeSystem();
        }

        [Test]
        public void UpdatesSystems()
        {
            systemManager.AddSystem(system1);
            systemManager.AddSystem(system2);

            WhenSystemsRun();

            system1.Updated.ShouldBeTrue();
            system2.Updated.ShouldBeTrue();
        }

        [Test]
        public void DoesntUpdateDisabledSystems()
        {
            systemManager.AddSystem(system1);
            systemManager.AddSystem(system2);
            system1.Enabled = false;
            system2.Enabled = false;

            WhenSystemsRun();

            system1.Updated.ShouldBeFalse();
            system2.Updated.ShouldBeFalse();
        }


        void WhenSystemsRun()
        {
            systemManager.Update();
        }
    }

    class FakeSystem : BaseSystem
    {
        public bool Updated;

        public override void OnUpdate()
        {
            Updated = true;
        }
    }
}
