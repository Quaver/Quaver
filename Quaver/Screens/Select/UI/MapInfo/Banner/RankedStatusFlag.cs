using System;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.MapInfo.Banner
{
    public class RankedStatusFlag : Sprite
    {
        /// <summary>
        ///     The text that displays the actual ranked status
        /// </summary>
        public SpriteText TextStatus { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public RankedStatusFlag()
        {
            Size = new ScalableVector2(125, 25);
            Position = new ScalableVector2(0.5f, 0.5f);
            Image = UserInterface.RankedStatusFlag;
            Y = 2;

            CreateTextStatus();
            ChangeColorAndText();
        }

        private void CreateTextStatus() => TextStatus = new SpriteText(BitmapFonts.Exo2BoldItalic, "", 14)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            X = 10,
        };

        /// <summary>
        ///     Changes the color and text to the associated ranked status/game.
        /// </summary>
        public void ChangeColorAndText()
        {
            var status = MapManager.Selected.Value.RankedStatus;

            if (MapManager.Selected.Value.Game != MapGame.Quaver)
            {
                Tint = ColorHelper.HexToColor("#9d84ec");
                TextStatus.Text = "Other Game";

                return;
            }

            switch (status)
            {
                case RankedStatus.NotSubmitted:
                    Tint = ColorHelper.HexToColor("#bb2424");
                    TextStatus.Text = "Not Submitted";
                    break;
                case RankedStatus.Unranked:
                    Tint = Color.Silver;
                    TextStatus.Text = "Unranked";
                    break;
                case RankedStatus.Ranked:
                    Tint = ColorHelper.HexToColor("#75e475");
                    TextStatus.Text = "Ranked";
                    break;
                case RankedStatus.DanCourse:
                    Tint = ColorHelper.HexToColor("#f6bc02");
                    TextStatus.Text = "Dan Course";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}