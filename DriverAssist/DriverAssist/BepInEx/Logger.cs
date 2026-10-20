#if BEPINEX

using BepInEx.Logging;
using Crowbus.Common;

namespace DriverAssist.Implementation
{
    class BepInExLogger : Logger
    {
        private readonly ManualLogSource logger;
        private readonly string prefix;

        public BepInExLogger(ManualLogSource logger, string prefix = "")
        {
            this.logger = logger;
            this.prefix = prefix;
        }

        public void Info(string message)
        {
            logger.LogInfo($"{prefix}{message}");
        }

        public void Warn(string message)
        {
            logger.LogWarning($"{prefix}{message}");
        }

        public void Error(System.Exception error)
        {
            logger.LogError($"{prefix}{error}");
        }
    }
}

#endif
