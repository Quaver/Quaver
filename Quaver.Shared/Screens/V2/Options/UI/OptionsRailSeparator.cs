using System;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.Options.UI
{
    /// <summary>
    ///     The line in the rail between the pinned entry and the category buttons.
    /// </summary>
    internal sealed class OptionsRailSeparator : Container
    {
        private Sprite Line { get; }

        internal OptionsRailSeparator(SkinV2OptionsCategoryNavigationConfig config)
        {
            Height = config.SearchSeparatorThickness;

            Line = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Tint = SkinV2Color.Parse(config.SearchSeparatorColor),
                UsePreviousSpriteBatchOptions = true
            };
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();

            Line?.SetSizeIfChanged(Math.Max(1, Width), Math.Max(1, Height));
        }
    }
}
