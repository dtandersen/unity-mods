using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Read-only access to the compendium. Implementations decide which pages are
    /// visible (for example, only the ones the player has uncovered).
    /// </summary>
    public interface ICompendiumRepository
    {
        /// <summary>All visible pages. Order is not significant.</summary>
        IReadOnlyList<CompendiumEntry> GetEntries();
    }
}