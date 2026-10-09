namespace StationeersAi.View;

/// <summary>
/// The game's chat as a passive view: it posts one named line wherever chat
/// goes. It holds no rules — no formatting, no name fallback, no trimming — and
/// specs record the posts instead of sending them.
/// </summary>
public interface IChatView
{
    /// <summary>
    /// Posts <paramref name="text"/> to chat as <paramref name="name"/>. The game
    /// shows the name itself, so the text carries no prefix.
    /// </summary>
    void Post(string name, string text);
}
