using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>
    /// One player's state as seen by the tools: who they are, where they are, what
    /// they carry and know. Built per request for a target player — the local
    /// player on a client, the asker on a server.
    /// </summary>
    public interface IPlayerContext
    {
        /// <summary>The player's character name, or null when unavailable.</summary>
        string Name { get; }

        /// <summary>The player's position and biome, or null when unavailable.</summary>
        Location Location { get; }

        /// <summary>The items the player is carrying.</summary>
        IReadOnlyList<ItemStack> Inventory { get; }

        /// <summary>Containers within <paramref name="radius"/> of the player.</summary>
        IReadOnlyList<StorageContainer> GetNearbyStorage(float radius);

        /// <summary>The compendium pages visible to the player.</summary>
        IReadOnlyList<CompendiumEntry> Compendium { get; }

        /// <summary>Whether the player has discovered the recipe with this item id.</summary>
        bool IsRecipeKnown(string recipeId);

        /// <summary>Crafting stations the player can currently use.</summary>
        IReadOnlyList<StationInfo> AvailableStations { get; }
    }
}