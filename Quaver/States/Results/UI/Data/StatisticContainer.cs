using Quaver.Graphics.Sprites;

namespace Quaver.States.Results.UI.Data
{
    /// <summary>
    ///     Contains an individual statistic "screen" inside of the
    /// </summary>
    internal class StatisticContainer : Sprite
    {
        internal string Name { get; }

        internal Sprite Content { get; }

        internal StatisticContainer(string name, Sprite content = null)
        {
            Name = name;
            Content = content;
        }
    }
}