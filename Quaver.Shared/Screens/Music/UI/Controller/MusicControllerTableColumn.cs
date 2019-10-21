using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerTableColumn
    {
        /// <summary>
        ///     The data the column represents
        /// </summary>
        public MusicControllerTableColumnType Type { get; }

        /// <summary>
        ///     The column container itself
        /// </summary>
        public Sprite Container { get; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Header { get; }

        /// <summary>
        /// </summary>
        public Sprite DividerLine { get; }

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="title"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        public MusicControllerTableColumn(MusicControllerTableColumnType type, string title, float width, float height)
        {
            Type = type;

            Container = new Sprite
            {
                Size = new ScalableVector2(width, height),
                Alpha = 0
            };

            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), title, 20)
            {
                Parent = Container,
                X = 36,
                Y = 16,
                //Tint = ColorHelper.HexToColor("#9177DD")
            };

            DividerLine = new Sprite()
            {
                Parent = Container,
                Y = 54,
                Size = new ScalableVector2(width, 2),
                Alpha = 0.75f
            };
        }
    }
}