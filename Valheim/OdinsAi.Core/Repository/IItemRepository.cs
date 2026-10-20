using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to the item "database".</summary>
    public interface IItemRepository
    {
        /// <summary>All known items. Order is not significant.</summary>
        IReadOnlyList<Item> GetAll();

        /// <summary>The item with the given id, or null if none exists.</summary>
        Item GetById(string id);
    }
}