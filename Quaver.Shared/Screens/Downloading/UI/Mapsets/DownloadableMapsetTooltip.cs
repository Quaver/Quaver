using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online.API.MapsetSearch;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Mapsets
{
    public class DownloadableMapsetTooltip : Tooltip
    {
        public DownloadableMapsetTooltip(DownloadableMapset mapset) : base("", Colors.MainAccent)
        {
            var totalHeight = 6f;
            var maxWidth = 0f;
            const int padding = 6;

            foreach (var difficulty in mapset.Difficulties)
            {
                var text = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                    $"{difficulty.Key} - " + $"{StringHelper.RatingToString(difficulty.Value)}", 20)
                {
                    Parent = this,
                    UsePreviousSpriteBatchOptions = true,
                    Alignment = Alignment.TopCenter,
                    SetChildrenAlpha = true,
                    Tint = ColorHelper.DifficultyToColor((float) difficulty.Value)
                };

                text.Y = totalHeight + 4;
                totalHeight += text.Height + 4;
                maxWidth = Math.Max(maxWidth, text.Width);
            }

            Size = new ScalableVector2(maxWidth + 22, totalHeight + padding);
        }
    }
}