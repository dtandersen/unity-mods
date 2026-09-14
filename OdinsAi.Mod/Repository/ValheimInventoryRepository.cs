using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Reads the local player's carried items from the game.</summary>
    public sealed class ValheimInventoryRepository : IInventoryRepository
    {
        public IReadOnlyList<ItemStack> GetCarriedItems()
        {
            var player = Player.m_localPlayer;
            var inventory = player != null ? player.GetInventory() : null;
            if (inventory == null)
            {
                return Array.Empty<ItemStack>();
            }

            return inventory.GetAllItems().Select(ValheimItems.ToStack).ToArray();
        }
    }
}