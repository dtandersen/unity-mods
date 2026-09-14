using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>Maps a game item instance to an <see cref="ItemStack"/>.</summary>
    internal static class ValheimItems
    {
        public static ItemStack ToStack(ItemDrop.ItemData item)
        {
            var shared = item.m_shared;
            var name = shared != null ? ValheimItemRepository.Localize(shared.m_name) : null;
            var id = item.m_dropPrefab != null ? item.m_dropPrefab.name : name;
            return new ItemStack(id ?? name, name, item.m_stack, item.m_quality, TypeOf(shared));
        }

        public static bool IsFood(ItemDrop.ItemData.SharedData shared)
        {
            return shared != null &&
                   (shared.m_food > 0f || shared.m_foodStamina > 0f || shared.m_foodEitr > 0f);
        }

        public static string TypeOf(ItemDrop.ItemData.SharedData shared)
        {
            if (shared == null)
            {
                return "misc";
            }

            return IsFood(shared) ? "food" : shared.m_itemType.ToString().ToLowerInvariant();
        }
    }
}