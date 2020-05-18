using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation.Metadata;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;

namespace Quaver.Shared.Screens.Selection.UI.Playlists
{
    public class PlaylistKeyValueDisplay : TextKeyValue
    {
        /// <summary>
        /// </summary>
        public Sprite DividerLine { get; }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <param name="keyColor"></param>
        public PlaylistKeyValueDisplay(string key, string value, Color keyColor, bool displayDividerLine = true) : base(key, value, 20, keyColor)
        {
            UsePreviousSpriteBatchOptions = true;
            Key.UsePreviousSpriteBatchOptions = true;
            Value.UsePreviousSpriteBatchOptions = true;

            Key.Tint = ColorHelper.HexToColor("#808080");

            DividerLine = new SpriteTextPlus(Key.Font, "|", Key.FontSize)
            {
                Parent = this,
                Position = new ScalableVector2(Value.X + Value.Width + 4, 0),
                Tint = ColorHelper.HexToColor("#808080"),
                UsePreviousSpriteBatchOptions = true,
                Visible = displayDividerLine,
                Y = Value.Y
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void UpdateSize()
        {
            base.UpdateSize();

            if (DividerLine.Visible)
            {
                DividerLine.X = Value.X + Value.Width + 4;
                Width += DividerLine.Width + 4;
            }
        }
    }
}