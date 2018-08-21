using System.Collections.Generic;
using Quaver.Database.Maps;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.DifficultySelection
{
    public class DifficultyButtonContainer : Sprite
    {
        /// <summary>
        ///     The parent DifficultySelector
        /// </summary>
        public DifficultySelector Selector { get; }

        /// <summary>
        ///     The mapset this selector refers to.
        /// </summary>
        public Mapset Mapset { get; }

        /// <summary>
        ///     The list of difficult select items in this container.
        /// </summary>
        public List<DifficultyButton> Items { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="set"></param>
        public DifficultyButtonContainer(DifficultySelector selector, Mapset set)
        {
            Selector = selector;
            Mapset = set;

            Width = Selector.Width;
            Height = Selector.Height;
            Alpha = 0;

            Items = new List<DifficultyButton>();

            for (var i = 0; i < Mapset.Maps.Count; i++)
            {
                Items.Add(new DifficultyButton(this, Mapset.Maps[i])
                {
                    UsePreviousSpriteBatchOptions = true,
                    Y = (DifficultyButton.HEIGHT + 3) * i
                });
            }
        }
    }
}