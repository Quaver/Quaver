using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Music.UI.Controller
{
    public class MusicControllerBackground : ScrollContainer
    {
        /// <summary>
        /// </summary>
        protected Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        protected Sprite Darkness { get; set; }

        /// <summary>
        /// </summary>
        protected float DarknessVisibleAlpha { get; set; } = 0.7f;

        /// <summary>
        /// </summary>
        private bool LoadBackground { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="size"></param>
        /// <param name="loadBackground"></param>
        public MusicControllerBackground(ScalableVector2 size, bool loadBackground = true) : base(size, size)
        {
            Alpha = 0;
            InputEnabled = false;
            LoadBackground = loadBackground;

            CreateBackground();
            CreateDarkness();
            InitializeBackgroundAndDarkness();

            MapManager.Selected.ValueChanged += OnMapChanged;
            BackgroundHelper.Loaded += OnBackgroundLoaded;

            if (LoadBackground)
                BackgroundHelper.Load(MapManager.Selected.Value);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            BackgroundHelper.Loaded -= OnBackgroundLoaded;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new Sprite
            {
                Alignment = Alignment.MidCenter,
                Y = -100,
                Size = new ScalableVector2(1920, 1080),
                Image = BackgroundHelper.RawTexture == null || BackgroundHelper.RawTexture.IsDisposed ? UserInterface.MenuBackgroundClear : BackgroundHelper.RawTexture
            };

            AddContainedDrawable(Background);
        }

        /// <summary>
        /// </summary>
        private void CreateDarkness()
        {
            Darkness = new Sprite
            {
                Parent = this,
                Size = Size,
                Tint = Color.Black,
            };
        }

        /// <summary>
        /// </summary>
        private void InitializeBackgroundAndDarkness()
        {
            if (BackgroundHelper.RawTexture == null || BackgroundHelper.Map != MapManager.Selected.Value)
                return;

            Background.Image = BackgroundHelper.RawTexture;
            Darkness.Alpha = DarknessVisibleAlpha;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            Darkness.ClearAnimations();
            Darkness.FadeTo(1, Easing.Linear, 150);

            if (LoadBackground)
                BackgroundHelper.Load(e.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoaded(object sender, BackgroundLoadedEventArgs e)
        {
            Background.Image = e.Texture;

            Darkness.ClearAnimations();
            Darkness.FadeTo(DarknessVisibleAlpha, Easing.Linear, 150);
        }
    }
}