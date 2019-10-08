using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Backgrounds;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class FooterJukeboxMapBackground : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Fade { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="jukebox"></param>
        public FooterJukeboxMapBackground(FooterJukebox jukebox) : base(jukebox.Size, new ScalableVector2(jukebox.Width - 5, jukebox.Height))
        {
            Alpha = 0;

            CreateBackground();
            CreateFade();

            MapManager.Selected.ValueChanged += OnMapChanged;
            BackgroundHelper.Loaded += OnBackgroundLoadded;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new Sprite
            {
                Alignment = Alignment.MidCenter,
                Y = 200,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Image = UserInterface.MenuBackgroundClear,
                Alpha = 0
            };

            if (BackgroundHelper.Map == MapManager.Selected.Value)
            {
                Background.Image = BackgroundHelper.RawTexture;
                Background.Alpha = 1;
            }

            AddContainedDrawable(Background);
        }

        /// <summary>
        /// </summary>
        private void CreateFade()
        {
            Fade = new Sprite
            {
                X = -2,
                Size = new ScalableVector2(Width + 10, Height),
                Image = UserInterface.JukeboxFade
            };

            AddContainedDrawable(Fade);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            Background.ClearAnimations();
            Background.FadeTo(0, Easing.Linear, 200);

            BackgroundHelper.Load(e.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundLoadded(object sender, BackgroundLoadedEventArgs e)
        {
            if (e.Map != MapManager.Selected.Value)
                return;

            Background.Image = e.Texture;

            Background.ClearAnimations();
            Background.FadeTo(1, Easing.Linear, 200);
        }
    }
}