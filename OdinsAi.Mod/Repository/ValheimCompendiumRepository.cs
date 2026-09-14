using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using OdinsAi.Entity;

namespace OdinsAi.Repository
{
    /// <summary>
    /// Reads the in-game compendium — the "Valheim Compendium" shown in
    /// <see cref="TextsDialog"/>, backed by <see cref="Player.GetKnownTexts"/> — plus,
    /// when <c>includeUnseen</c> is set, every Munin hint from
    /// <see cref="Tutorial.instance"/>. Text is localized on construction, so the
    /// snapshot must be taken on the Unity main thread; the result is plain C#.
    /// </summary>
    public sealed class ValheimCompendiumRepository : ICompendiumRepository
    {
        private static readonly Regex RichTextTags = new Regex("<[^>]+>", RegexOptions.Compiled);

        private readonly List<CompendiumEntry> _entries = new List<CompendiumEntry>();
        private readonly HashSet<string> _topics = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly HashSet<string> _texts = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ValheimCompendiumRepository(bool includeUnseen)
        {
            var player = Player.m_localPlayer;
            AddKnownTexts(player);
            if (includeUnseen)
            {
                AddAllHints(player);
            }
        }

        public IReadOnlyList<CompendiumEntry> GetEntries()
        {
            return _entries;
        }

        private void AddKnownTexts(Player player)
        {
            if (player == null)
            {
                return;
            }

            foreach (var known in player.GetKnownTexts())
            {
                Add(Clean(known.Key), Clean(known.Value));
            }
        }

        private void AddAllHints(Player player)
        {
            var tutorial = Tutorial.instance;
            if (tutorial == null || tutorial.m_texts == null)
            {
                return;
            }

            foreach (var text in tutorial.m_texts)
            {
                var topic = Clean(FirstNonEmpty(text.m_topic, text.m_label, text.m_name));
                var seen = player != null && player.HaveSeenTutorial(text.m_name);
                if (!seen)
                {
                    topic = topic + " (unseen)";
                }

                Add(topic, Clean(text.m_text));
            }
        }

        private void Add(string topic, string text)
        {
            if (string.IsNullOrWhiteSpace(topic) || string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            // The known-texts list and the tutorial list overlap; dedupe on both so a
            // page unlocked by the player is not repeated as an unseen hint.
            if (_topics.Contains(topic) || _texts.Contains(text))
            {
                return;
            }

            _topics.Add(topic);
            _texts.Add(text);
            _entries.Add(new CompendiumEntry(topic, text));
        }

        private static string FirstNonEmpty(params string[] values)
        {
            foreach (var value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }

            return string.Empty;
        }

        /// <summary>Localizes a game token and strips rich-text markup.</summary>
        private static string Clean(string token)
        {
            if (string.IsNullOrEmpty(token))
            {
                return token;
            }

            var localized = ValheimItemRepository.Localize(token.Replace("\u0016", string.Empty));
            return string.IsNullOrEmpty(localized)
                ? localized
                : RichTextTags.Replace(localized, string.Empty).Trim();
        }
    }
}