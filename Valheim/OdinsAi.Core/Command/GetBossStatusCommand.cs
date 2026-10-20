using System.Collections.Generic;
using OdinsAi.Entity;
using OdinsAi.Repository;

namespace OdinsAi.Command
{
    /// <summary>Domain operation: look up the bosses and their defeat status.</summary>
    public sealed class GetBossStatusCommand
    {
        private readonly IBossRepository _bosses;

        public GetBossStatusCommand(IBossRepository bosses)
        {
            _bosses = bosses;
        }

        public IReadOnlyList<BossStatus> Execute()
        {
            return _bosses.GetBosses();
        }
    }
}