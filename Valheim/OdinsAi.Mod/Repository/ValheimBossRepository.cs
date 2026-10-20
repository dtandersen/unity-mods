using System.Collections.Generic;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Finds the world's bosses by scanning <see cref="ZNetScene"/> prefabs for
    /// characters with a <c>m_defeatSetGlobalKey</c>, then reports whether each
    /// key is set in <see cref="ZoneSystem"/>. This picks up bosses the
    /// <see cref="GlobalKeys"/> enum does not list (e.g. The Queen, Fader).
    /// </summary>
    public sealed class ValheimBossRepository : IBossRepository
    {
        public IReadOnlyList<BossStatus> GetBosses()
        {
            var scene = ZNetScene.instance;
            if (scene == null || scene.m_prefabs == null)
            {
                return null;
            }

            var zone = ZoneSystem.instance;
            var bosses = new List<BossStatus>();
            var seen = new HashSet<string>();

            foreach (var prefab in scene.m_prefabs)
            {
                if (prefab == null)
                {
                    continue;
                }

                var character = prefab.GetComponent<Character>();
                var key = character != null ? character.m_defeatSetGlobalKey : null;
                if (string.IsNullOrEmpty(key) || !seen.Add(key))
                {
                    continue;
                }

                var name = ValheimItemRepository.Localize(character.m_name);
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = key;
                }

                bosses.Add(new BossStatus(name, zone != null && zone.GetGlobalKey(key)));
            }

            return bosses;
        }
    }
}