using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Assets;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI.Jukebox
{
    public class FooterJukeboxSongInfo : Sprite
    {
        /// <summary>
        /// </summary>
        public bool Active { get; private set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus NowPlaying { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; }

        /// <summary>
        /// </summary>
        private IAudioTrack PreviousAudioTrack { get; set; }

        /// <summary>
        /// </summary>
        private double TimeSinceActivated { get; set; }

        /// <summary>
        /// </summary>
        public FooterJukeboxSongInfo()
        {
            Size = new ScalableVector2(480, 39);
            SetChildrenAlpha = true;
            Image = UserInterface.JukeboxInfoBackground;

            Alpha = 0;

            NowPlaying = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Now Playing:", 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Position = new ScalableVector2(10, 0),
                Tint = Color.White,
                Alpha = 0
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "", 20)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Position = new ScalableVector2(NowPlaying.X + NowPlaying.Width + 16, NowPlaying.Y),
                Alpha = 0
            };

            if (MapManager.Selected.Value != null)
                SetText();

            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (AudioEngine.Track != PreviousAudioTrack && PreviousAudioTrack != null)
                Activate();

            if (Active)
            {
                TimeSinceActivated += gameTime.ElapsedGameTime.TotalMilliseconds;

                if (TimeSinceActivated >= 2500)
                    Deactivate();
            }

            PreviousAudioTrack = AudioEngine.Track;

            base.Update(gameTime);
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
        public void Activate()
        {
            Active = true;
            TimeSinceActivated = 0;

            ClearAnimations();
            FadeTo(1, Easing.Linear, 200);
            MoveToY(-60, Easing.OutQuint, 400);
        }

        /// <summary>
        /// </summary>
        public void Deactivate()
        {
            Active = false;
            TimeSinceActivated = 0;

            ClearAnimations();
            FadeTo(0, Easing.Linear, 150);
            MoveToY(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void SetText()
        {
            ScheduleUpdate(() =>
            {
                Title.Text = MapManager.Selected.Value != null ? $"{MapManager.Selected.Value.Artist} - {MapManager.Selected.Value.Title}" : "";
                Title.TruncateWithEllipsis(330); 
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => SetText();
    }
}