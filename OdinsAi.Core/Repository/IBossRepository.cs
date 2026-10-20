using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to the world's bosses and their defeat status.</summary>
    public interface IBossRepository
    {
        /// <summary>The bosses, or null when boss data is unavailable.</summary>
        IReadOnlyList<BossStatus> GetBosses();
    }
}