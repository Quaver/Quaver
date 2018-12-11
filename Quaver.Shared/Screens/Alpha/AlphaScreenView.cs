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
        private Sprite QuaverLogo { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText WelcomeText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText KeepInMindText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText ThingsWontBePerfectText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText WantToContributeText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText ReportViaGithubText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText OrJoinDiscordText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText SpecialThanksText { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Heart { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText ToAllContributorsText { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText AndAllSupportersText { get; set; }

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
                Size = new ScalableVector2(1200, 640),
                Tint = Color.Black,
                Alpha = 0.85f,
                Alignment = Alignment.MidCenter
            };

            blackBg.AddBorder(Color.White, 3);

            CreateQuaverLogo();
            CreateWelcomeText();
            CreateKeepInMindText();
            CreateThingsWontBePerfectText();
            CreateWantToContributeText();
            CreateReportViaGithubText();
            CreateOrJoinDiscordText();
            CreateSpecialThanksText();
            CreateHeart();
            CreateToAllContributorsText();
            CreateAndAllSupportersText();
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
        private void CreateQuaverLogo() => QuaverLogo = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(220, 220),
            Alignment = Alignment.TopCenter,
            Image = UserInterface.QuaverLogoStylish,
            Y = 90
        };

        /// <summary>
        /// </summary>
        private void CreateWelcomeText() => WelcomeText = new SpriteText(BitmapFonts.Exo2Bold,
            "Welcome, and thanks for trying out Quaver!", 16)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = QuaverLogo.Y + QuaverLogo.Height + 15,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 600) }
        };

        /// <summary>
        /// </summary>
        private void CreateKeepInMindText() => KeepInMindText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "Keep in mind that this is an alpha/development build of the game", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = WelcomeText.Y + WelcomeText.Height + 10,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1200) }
        };

        /// <summary>
        /// </summary>
        private void CreateThingsWontBePerfectText() => ThingsWontBePerfectText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "A lot of features are still incomplete, and not everything will work properly", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = KeepInMindText.Y + KeepInMindText.Height + 10,
            Alpha = 0,
            Animations = { new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 1800) }
        };

        /// <summary>
        /// </summary>
        private void CreateWantToContributeText() => WantToContributeText = new SpriteText(BitmapFonts.Exo2Bold,
            "Want to contribute/report bugs?", 16)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = KeepInMindText.Y + KeepInMindText.Height + 60,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 2400)}
        };

        /// <summary>
        /// </summary>
        private void CreateReportViaGithubText() => ReportViaGithubText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "You can report bugs or request new features on GitHub", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = WantToContributeText.Y + WantToContributeText.Height + 10,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 3000)}
        };

        /// <summary>
        /// </summary>
        private void CreateOrJoinDiscordText() => OrJoinDiscordText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "Be sure to also join us on Discord to discuss further with the community", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = ReportViaGithubText.Y + ReportViaGithubText.Height + 10,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 3600)}
        };

        /// <summary>
        /// </summary>
        private void CreateSpecialThanksText() => SpecialThanksText = new SpriteText(BitmapFonts.Exo2Bold, "Special Thanks", 16)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = OrJoinDiscordText.Y + OrJoinDiscordText.Height + 30,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 4200)}
        };

        /// <summary>
        /// </summary>
        private void CreateHeart()
        {
            Heart = new Sprite
            {
                Parent = SpecialThanksText,
                Alignment = Alignment.MidLeft,
                X = SpecialThanksText.Width + 2,
                Size = new ScalableVector2(SpecialThanksText.Height - 6, SpecialThanksText.Height - 6),
                Tint = Color.Red,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_heart_shape_silhouette),
                Alpha = 0,
                Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 4200)}
            };

            SpecialThanksText.X -= Heart.Width / 2f - 1;
        }

        /// <summary>
        /// </summary>
        private void CreateToAllContributorsText() => ToAllContributorsText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "To all of the early contributors over the past year", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = SpecialThanksText.Y + SpecialThanksText.Height + 10,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 4800)}
        };

        /// <summary>
        /// </summary>
        private void CreateAndAllSupportersText() => AndAllSupportersText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "And everyone who has supported us throughout this journey", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopCenter,
            Y = ToAllContributorsText.Y + ToAllContributorsText.Height + 10,
            Alpha = 0,
            Animations = {new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 5400)}
        };

        /// <summary>
        /// </summary>
        private void CreatePressToSkipText() => PressToSkipText = new SpriteText(BitmapFonts.Exo2SemiBold,
            "Press [ Space ] to skip", 13)
        {
            Parent = Container,
            Alignment = Alignment.TopRight,
            Position = new ScalableVector2(-20, 15),
            Tint = Colors.SecondaryAccent
        };
    }
}