#if BEPINEX

using BepInEx;
using DriverAssist.Implementation;

namespace DriverAssist.BepInEx
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class DriverAssistBepInExPlugin : BaseUnityPlugin
    {
        private UnityPresenter presenter;
        private Crowbus.Common.Logger log = new NullLogger();

        private void Awake()
        {
            LogFactory.Factory.Value = (scope) => new BepInExLogger(Logger, "--------------------------------------------------> ");
            log = LogFactory.GetLogger(typeof(DriverAssistBepInExPlugin));

            BepInExDriverAssistSettings config = new BepInExDriverAssistSettings(Config);

            presenter = new UnityPresenter(config);
            presenter.Init();
        }

        private void OnDestroy()
        {
            log.Info($"OnDestroy");
            presenter.Unload();
            presenter.OnDestroy();
        }

        private void Update()
        {
            presenter.OnUpdate();
        }

        private void FixedUpdate()
        {
            presenter.OnFixedUpdate();
        }

        private void OnGUI()
        {
            presenter.OnGui();
        }
    }
}

#endif
