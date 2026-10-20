using System;
using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Reads the target player's carried items from the game.</summary>
    public sealed class ValheimInventoryRepository
    {
        private readonly Player _player;

        /// <param name="player">
        /// The player to read. On a client this is the local player; on a server it
        /// is the player the question is about.
        /// </param>
        public ValheimInventoryRepository(Player player)
        {
            _player = player;
        }

        public IReadOnlyList<ItemStack> GetCarriedItems()
        {
            var inventory = _player != null ? _player.GetInventory() : null;
            if (inventory == null)
            {
                return Array.Empty<ItemStack>();
            }

            return inventory.GetAllItems().Select(ValheimItems.ToStack).ToArray();
        }
    }
}