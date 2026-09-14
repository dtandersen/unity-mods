using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Read-only access to the player's carried items.</summary>
    public interface IInventoryRepository
    {
        IReadOnlyList<ItemStack> GetCarriedItems();
    }
}