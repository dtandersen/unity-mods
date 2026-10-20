using OdinsAi.Entity;
using OdinsAi.Query;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: read one compendium page by its topic.</summary>
    public sealed class ReadCompendiumCommand
    {
        private readonly CompendiumIndex _index;

        public ReadCompendiumCommand(CompendiumIndex index)
        {
            _index = index;
        }

        public CompendiumEntry Execute(string topic)
        {
            return _index.Get(topic);
        }
    }
}