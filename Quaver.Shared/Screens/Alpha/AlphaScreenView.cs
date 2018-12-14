using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Alpha
{
    public class AlphaScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText WelcomeText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText DiscordJoinText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText ThanksText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText ThingsWontBePerfectText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText PressToSkipText { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public AlphaScreenView(Screen screen) : base(screen)
        {
            GameBase.Game.GlobalUserInterface.Cursor.Hide(0);

            CreateBackgroundImage();

            var blackBg = new Sprite
            {
                Parent = Container,
                HeightScale = 1,
                WidthScale = 1,
                Tint = Color.Black,
                Alpha = 1,
                Alignment = Alignment.MidCenter
            };

            blackBg.AddBorder(Color.White, 3);

            CreateWelcomeText();
            CreateDiscordJoinText();
            CreateThanksText();
            CreateThingsWontBePerfectText();
            CreatePressToSkipText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        private void CreateBackgroundImage() => Background = new BackgroundImage(UserInterface.MenuBackgroundBlurred, 20, false) { Parent = Container };

        /// <summary>
        /// </summary>
        private void CreateWelcomeText() => WelcomeText = new SpriteText(BitmapFonts.Exo2Bold,
            "Welcome to Quaver!", 36)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = 250,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 600) }
        };

        /// <summary>
        /// </summary>
        private void CreateDiscordJoinText() => DiscordJoinText = new SpriteText(BitmapFonts.Exo2Medium,
            "If you are interested in developing or joining the community, check out the Offical Discord Server!", 14)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = WelcomeText.Y + WelcomeText.Height + 32,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1200) }
        };

        /// <summary>
        /// </summary>
        private void CreateThanksText() => ThanksText = new SpriteText(BitmapFonts.Exo2Medium,
            "Also be sure to report any bugs. Thank you for your time here!", 14)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = DiscordJoinText.Y + DiscordJoinText.Height + 8,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1800) }
        };

        /// <summary>
        /// </summary>
        private void CreateThingsWontBePerfectText() => ThingsWontBePerfectText = new SpriteText(BitmapFonts.Exo2Medium,
            "A lot of things will break since this game is still in development!", 12)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = ThanksText.Y + ThanksText.Height + 102,
            Tint = Colors.Negative,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 2400) }
        };

        /// <summary>
        /// </summary>
        private void CreatePressToSkipText() => PressToSkipText = new SpriteText(BitmapFonts.Exo2Medium,
            "Press [ ENTER ] to skip", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-20, 15),
            Tint = Colors.SecondaryAccent
        };
    }
}