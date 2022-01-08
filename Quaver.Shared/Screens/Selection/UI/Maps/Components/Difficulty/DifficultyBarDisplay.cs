using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Selection.UI.Maps.Components.Difficulty
{
    public class DifficultyBarDisplay : Sprite, IDrawableMapComponent
    {
        /// <summary>
        /// </summary>
        public Map Map { get; private set; }

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
            Size = halfSize ? new ScalableVector2(352, 21) : new ScalableVector2(704, 42);
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

            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Open() => SlideToDifficultyValue();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public void Close()
        {
        }

        /// <summary>
        ///     Performs a sliding animation to the difficulty value
        /// </summary>
        public void SlideToDifficultyValue(bool instant = false)
        {
            var diff = Map.DifficultyFromMods(ModManager.Mods);

            const int maxDiff = 50;

            var percent = diff / maxDiff;

            var width = (int) (Width * percent);

            Container.ClearAnimations();

            if (width == (int) Container.Width)
                return;

            Container.ChangeWidthTo(width, Easing.OutQuint, 2000);
        }

        /// <summary>
        ///     Called when the modifiers have changed. Used to reset the
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => SlideToDifficultyValue();

        /// <summary>
        ///     Changes the map this difficulty display represents
        /// </summary>
        /// <param name="map"></param>
        public void ChangeMap(Map map)
        {
            Map = map;
            SlideToDifficultyValue();
        }
    }
}
