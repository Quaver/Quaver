using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Music.UI.Controller;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Results.UI.Header
{
    public class ResultsScreenHeaderBackground : MusicControllerBackground
    {
        /// <summary>
        /// </summary>
        private Sprite DarknessFilter { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        public ResultsScreenHeaderBackground(ScalableVector2 size) : base(size, false)
        {
            Background.Y = 100;
            Background.Alignment = Alignment.MidCenter;
            Darkness.Alpha = 0f;

            Background.Image = AssetLoader.LoadTexture2D(GameBase.Game.Resources.Get($"Quaver.Resources/Maps/PrincessOfWinter/Princess of Winter.png"));
            CreateDarknessFilter();
        }

        /// <summary>
        /// </summary>
        private void CreateDarknessFilter()
        {
            DarknessFilter = new Sprite
            {
                Parent = this,
                Size = Size,
                Image = UserInterface.ResultsBackgroundFilter
            };
        }
    }
}