using System;
using Crowbus.Common;

namespace DriverAssist.Test
{
    public class TUnitLogger : Logger
    {
        public static void Init()
        {
            LogFactory.Factory.Value = (scope) => new TUnitLogger(scope);
        }

        private readonly string scope;

        public TUnitLogger(string scope = "")
        {
            this.scope = scope;
        }

        public void Info(string message)
        {
            WriteLog(message, "INFO");
        }

        public void Warn(string message)
        {
            WriteLog(message, "WARN");
        }

        public void Error(Exception error)
        {
            WriteLog(error.ToString(), "ERROR");
        }

        private void WriteLog(string message, string severity)
        {
            // TUnit intercepts Console output per test.
            Console.WriteLine($"{severity} [{scope}] {message}");
        }
    }
}
