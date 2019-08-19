using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Metadata;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Mapsets.Maps.Components.Difficulty
{
    public class DifficultyBarDisplay : Sprite, IDrawableMapComponent
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        public ScrollContainer Container { get; }

        /// <summary>
        /// </summary>
        private Sprite ProgressBar { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="displayBackground"></param>
        /// <param name="halfSize"></param>
        public DifficultyBarDisplay(Map map, bool displayBackground = true, bool halfSize = false)
        {
            Map = map;
            Image = UserInterface.DifficultyBarBackground;
            Size = halfSize ? new ScalableVector2(362, 21) : new ScalableVector2(724, 42);
            Alpha = displayBackground ? 1 : 0;

            Container = new ScrollContainer(new ScalableVector2(0, Height), Size)
            {
                Parent = this,
                Alpha = 0
            };

            ProgressBar = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Size = new ScalableVector2(Width, Height),
                Image = UserInterface.DifficultyBarColor
            };

            Container.AddContainedDrawable(ProgressBar);

            SlideToDifficultyValue();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open()
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        ///     Performs a sliding animation to the difficulty value
        /// </summary>
        public void SlideToDifficultyValue()
        {
            var diff = Map.DifficultyFromMods(ModManager.Mods);

            const int maxDiff = 35;

            var percent = diff / maxDiff;

            Container.ClearAnimations();
            Container.ChangeWidthTo((int) (Width * percent), Easing.OutQuint, 2000);
        }
    }
}