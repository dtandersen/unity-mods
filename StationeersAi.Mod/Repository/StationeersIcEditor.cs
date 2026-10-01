using System;
using StationeersAi.Entity;
using StationeersAi.Mod;
using StationeersAi.Repository;

namespace StationeersAi.Repository
{
    /// <summary>
    /// Writes MIPS source to the IC editor of a named computer or laptop. Only
    /// fills the editor buffer (mirroring what the stationeer types); it never
    /// Exports to housings or chips. The write runs on Unity's main thread
    /// because the editor owns Unity UI text.
    /// </summary>
    public sealed class StationeersIcEditor : IIcEditor
    {
        public IcWriteResult Write(string computer, string code)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => WriteInner(computer, code ?? string.Empty));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new IcWriteResult(false, null, 0, "Editor write failed.");
            }
        }

        private static IcWriteResult WriteInner(string computer, string code)
        {
            var matches = ComputerBoards.Match(computer);
            if (matches.Count == 0)
            {
                return new IcWriteResult(false, null, 0, $"Computer '{computer}' not found. Available: {ComputerBoards.Available()}");
            }

            if (matches.Count > 1)
            {
                return new IcWriteResult(false, null, 0, $"Several computers match '{computer}': {ComputerBoards.Names(matches)}");
            }

            matches[0].Motherboard.SetSourceCode(code);
            try
            {
                matches[0].Motherboard.SendUpdate();
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
            }

            return new IcWriteResult(true, matches[0].Name, code.Split('\n').Length, null);
        }
        public IcReadResult Read(string computer)
        {
            try
            {
                return BepInExPlugin.RunOnMainThreadSync(() => ReadInner(computer));
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new IcReadResult(false, null, null, "Editor read failed.");
            }
        }

        private static IcReadResult ReadInner(string computer)
        {
            var matches = ComputerBoards.Match(computer);
            if (matches.Count == 0)
            {
                return new IcReadResult(false, null, null, $"Computer '{computer}' not found. Available: {ComputerBoards.Available()}");
            }

            if (matches.Count > 1)
            {
                return new IcReadResult(false, null, null, $"Several computers match '{computer}': {ComputerBoards.Names(matches)}");
            }

            string code;
            try
            {
                var source = matches[0].Motherboard.GetSourceCode();
                code = source != null ? source.ToString() : string.Empty;
            }
            catch (Exception ex)
            {
                BepInExPlugin.Log.Log(ex.ToString());
                return new IcReadResult(false, null, null, "Editor read failed.");
            }

            return new IcReadResult(true, matches[0].Name, code ?? string.Empty, null);
        }
    }
}
