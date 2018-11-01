using System;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Quaver.Resources;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.SongSelect.UI.Banner
{
    public class BannerRankedStatus : Sprite
    {
        /// <summary>
        ///     The text that displays the ranked status of the map.
        /// </summary>
        private SpriteText Status { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public BannerRankedStatus()
        {
            Size = new ScalableVector2(168, 26);
            Tint = Color.LimeGreen;

            Status = new SpriteText(BitmapFonts.Exo2Bold, "Not Submitted", 11, false)
            {
                Parent = this,
                Alignment = Alignment.MidCenter
            };

            Alpha = 0.85f;
            AddBorder(Color.DarkGreen, 2);
        }

        /// <summary>
        ///     Updates the ranked status with a new map.
        /// </summary>
        /// <param name="map"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        public void UpdateMap(Map map)
        {
            switch (map.RankedStatus)
            {
                case RankedStatus.NotSubmitted:
                    Status.Text = "Not Submitted";
                    Tint = ColorHelper.HexToColor("#A62639");
                    Border.Tint = ColorHelper.HexToColor("#511C29");
                    break;
                case RankedStatus.Unranked:
                    Status.Text = "Unranked";
                    Tint = ColorHelper.HexToColor("#594E36");
                    Border.Tint = ColorHelper.HexToColor("#2F2504");
                    break;
                case RankedStatus.Ranked:
                    Status.Text = "Ranked";
                    Tint = ColorHelper.HexToColor("#73BA9B");
                    Border.Tint = ColorHelper.HexToColor($"#003E1F");
                    break;
                case RankedStatus.DanCourse:
                    Status.Text = "Dan Course";
                    Tint = ColorHelper.HexToColor("#49306B");
                    Border.Tint = ColorHelper.HexToColor($"#635380");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}