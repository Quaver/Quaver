using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.V2.UI
{
    internal static class V2Scrollbar
    {
        private const float DefaultGap = 10;

        private static readonly Color DefaultThumbColor = ColorHelper.HexToColor("#D9E3F4");
        private static readonly Color DefaultTrackColor = ColorHelper.HexToColor("#181E25");

        public static void Apply(ScrollContainer scroll, float width = 15, Color? thumbColor = null,
            Color? trackColor = null, float cornerRadius = 7)
        {
            if (scroll == null)
                throw new ArgumentNullException(nameof(scroll));

            scroll.Scrollbar.Width = width;
            scroll.Scrollbar.Tint = thumbColor ?? DefaultThumbColor;
            scroll.ScrollbarBackground.Tint = trackColor ?? DefaultTrackColor;
            scroll.ScrollbarBackground.UsePreviousSpriteBatchOptions = scroll.Scrollbar.UsePreviousSpriteBatchOptions;
            scroll.ScrollbarCornerRadius = cornerRadius;
            scroll.AllowScrollbarDragging = true;
            scroll.ShowScrollbarBackground = true;
            scroll.ShowScrollbarBackgroundWhenContentFits = true;
            scroll.Scrollbar.X = -DefaultGap;
            scroll.ScrollbarBackground.X = -DefaultGap;
        }

        public static float GetContentWidth(ScrollContainer scroll) =>
            Math.Max(1, scroll.Width - scroll.Scrollbar.Width - DefaultGap * 2);
    }
}
