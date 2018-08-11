using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Database.Maps;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;

namespace Quaver.Screens.Select.UI.Selector
{
    public class DifficultySelectorContainer : Sprite
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
        public List<DifficultySelectorItem> Items { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="selector"></param>
        /// <param name="set"></param>
        public DifficultySelectorContainer(DifficultySelector selector, Mapset set)
        {
            Selector = selector;
            Mapset = set;

            Width = Selector.Width;
            Height = Selector.Height;
            Alpha = 0;

            Items = new List<DifficultySelectorItem>();

            for (var i = 0; i < Mapset.Maps.Count; i++)
            {
                Items.Add(new DifficultySelectorItem(this, Mapset.Maps[i])
                {
                    UsePreviousSpriteBatchOptions = true,
                    Y = (DifficultySelectorItem.HEIGHT + 3) * i
                });
            }
        }
    }
}