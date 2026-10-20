namespace OdinsAi.Entity
{
    /// <summary>
    /// A page in the in-game compendium: a lore passage or Munin's hint about the
    /// world. <see cref="Topic"/> is the page title, <see cref="Text"/> the body.
    /// </summary>
    public sealed record CompendiumEntry(string Topic, string Text);
}