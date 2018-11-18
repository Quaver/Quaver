using System;
using Quaver.API.Enums;
using Quaver.Database.Maps;
using Quaver.Helpers;
using Quaver.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.SongSelect.UI.Banner
{
    public class BannerRankedStatus : Sprite
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public BannerRankedStatus() => Size = new ScalableVector2(105, 38);

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
                    Image = UserInterface.StatusNotSubmitted;
                    break;
                case RankedStatus.Unranked:
                    Image = UserInterface.StatusUnranked;
                    break;
                case RankedStatus.Ranked:
                    Image = UserInterface.StatusRanked;
                    break;
                case RankedStatus.DanCourse:
                    Image = UserInterface.StatusDanCourse;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}