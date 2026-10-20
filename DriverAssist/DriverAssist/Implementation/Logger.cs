using System;
using Logger = Crowbus.Common.Logger;
using UnityEngine;

namespace DriverAssist.Implementation
{
    class UnityLogger : Logger
    {
        private readonly string prefix;

        UnityLogger()
        {
            prefix = "";
        }

        public void Info(string message)
        {
            UnityEngine.Debug.Log($"{prefix}{message}");
        }

        public void Warn(string message)
        {
            UnityEngine.Debug.Log($"{prefix}{message}");
        }

        public void Error(Exception error)
        {
            UnityEngine.Debug.Log($"{prefix}{error}");
        }
    }
}
