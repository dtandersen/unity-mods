using System.Collections.Generic;
using System.Linq;
using OdinsAi.Entity;
using UnityEngine;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Reads items (and their food stats) from the game's <see cref="ObjectDB"/>.
    /// The snapshot is materialised on the calling thread, so it must be created on
    /// the Unity main thread; afterwards the data is plain C#.
    /// </summary>
    public sealed class ValheimItemRepository : IItemRepository
    {
        private readonly List<Item> _items = new List<Item>();

        public ValheimItemRepository(ObjectDB database)
        {
            if (database == null || database.m_items == null)
            {
                return;
            }

            foreach (var prefab in database.m_items)
            {
                if (prefab == null)
                {
                    continue;
                }

                var drop = prefab.GetComponent<ItemDrop>();
                var shared = drop != null && drop.m_itemData != null ? drop.m_itemData.m_shared : null;
                if (shared == null)
                {
                    continue;
                }

                FoodStats food = null;
                if (ValheimItems.IsFood(shared))
                {
                    food = new FoodStats(
                        shared.m_food,
                        shared.m_foodStamina,
                        shared.m_foodEitr,
                        shared.m_foodBurnTime,
                        shared.m_foodRegen);
                }

                _items.Add(new Item(prefab.name, Localize(shared.m_name), ValheimItems.TypeOf(shared), food));
            }
        }

        public IReadOnlyList<Item> GetAll()
        {
            return _items;
        }

        public Item GetById(string id)
        {
            return _items.FirstOrDefault(item => item.Id == id);
        }

        internal static string Localize(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return token;
            }

            var localization = Localization.instance;
            return localization != null ? localization.Localize(token) : token;
        }
    }
}