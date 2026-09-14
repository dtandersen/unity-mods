using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>In-memory bosses. Used by tests and as a stand-in.</summary>
    public sealed class InMemoryBossRepository : IBossRepository
    {
        private readonly List<BossStatus> _bosses = new List<BossStatus>();

        public void Add(BossStatus boss)
        {
            _bosses.Add(boss);
        }

        public IReadOnlyList<BossStatus> GetBosses()
        {
            return _bosses;
        }
    }
}