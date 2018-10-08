using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Screens.Connecting;
using Quaver.Screens.Menu;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Input;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Splash
{
    public class SplashScreenView : ScreenView
    {
        /// <summary>
        ///     Godly tbh.
        /// </summary>
        private Sprite QuaverLogo { get; }

        /// <summary>
        ///     Heading text that displays that this is a test build.
        /// </summary>
        private SpriteText Thanks { get; }

        /// <summary>
        ///     Text that shows that there are tons of bugs.
        /// </summary>
        private SpriteText Bugs { get; }

        /// <summary>
        ///     Text that asks if they want to contribute to the development
        /// </summary>
        private SpriteText Contribute { get; }

        /// <summary>
        ///     Text that states they can join the discord.
        /// </summary>
        private SpriteText JoinDiscord { get; }

        /// <summary>
        ///     Text that displays the discord link.
        /// </summary>
        private SpriteText DiscordLink { get; }

        /// <summary>
        ///     With love.
        /// </summary>
        private SpriteText MadeWith { get; }

        /// <summary>
        ///     Heart sprite.
        /// </summary>
        private Sprite Heart { get; }

        /// <summary>
        ///     le team of quaver.
        /// </summary>
        private SpriteText QuaverTeam { get; }

        /// <summary>
        ///     The sprite that handles fade effect for screen transitions.
        /// </summary>
        private Sprite ScreenTransitioner { get; }

        /// <summary>
        ///     If the screen is currently fading out.
        /// </summary>
        private bool IsFadingOut { get; set; }

        /// <summary>
        ///     The amount of time since the screen is fading out.
        /// </summary>
        private double TimeSinceFadingOut { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public SplashScreenView(Screen screen) : base(screen)
        {
            QuaverLogo = new Sprite()
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Image = UserInterface.QuaverLogo,
                Size = new ScalableVector2(150, 150),
                Y = -90
            };
            Thanks = new SpriteText(Fonts.Exo2Regular24, "Thanks for trying out our pre-alpha development build!")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.65f
            };

            Bugs = new SpriteText(Fonts.Exo2Regular24, "Expect there to be tons of bugs and things to not work properly.")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.45f,
                Y = Thanks.MeasureString().Y + 10
            };

            Contribute = new SpriteText(Fonts.Exo2Regular24, "Want to help contribute to the project with code, ideas, designs, or bug reports?")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.55f,
                Y = Bugs.Y + Bugs.MeasureString().Y + 35
            };

            JoinDiscord = new SpriteText(Fonts.Exo2Regular24, "You can join our Discord server at @ discord.gg/nJa8VFr")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.45f,
                Y = Contribute.Y + Contribute.MeasureString().Y + 10
            };

            MadeWith = new SpriteText(Fonts.Exo2Regular24, "Made with")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.45f,
                Y = JoinDiscord.Y + JoinDiscord.MeasureString().Y + 30
            };

            Heart = new Sprite()
            {
                Parent = Container,
                Image = FontAwesome.Heart,
                Size = new ScalableVector2(15, 15),
                Alignment = Alignment.MidCenter,
                Tint = Color.Red,
                Y = MadeWith.Y,
                X = MadeWith.MeasureString().X / 2f + 15
            };

            QuaverTeam = new SpriteText(Fonts.Exo2Regular24, "- The Quaver Team")
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextScale = 0.45f,
                Y = MadeWith.Y + MadeWith.MeasureString().Y + 5
            };

            ScreenTransitioner = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                Tint = Color.Black,
                Image = UserInterface.BlankBox,
                Alpha = 1,
                Transformations =
                {
                    new Transformation(TransformationProperty.Alpha, Easing.Linear, 1, 0, 2000)
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var screen = (SplashScreen) Screen;

            // Start fading out screen.
            if ((screen.TimeActive > 6500 || KeyboardManager.IsUniqueKeyPress(Keys.Space)) && !IsFadingOut)
            {
                IsFadingOut = true;
                ScreenTransitioner.Transformations.Clear();
                ScreenTransitioner.Transformations.Add(new Transformation(TransformationProperty.Alpha, Easing.Linear, 0, 1, 500));
            }

            // Start counting the time since fading out
            if (IsFadingOut)
                TimeSinceFadingOut += gameTime.ElapsedGameTime.TotalMilliseconds;

            // Change screens after fading for a bit.
            if (TimeSinceFadingOut > 700)
                QuaverScreenManager.ChangeScreen(new ConnectingScreen());

            Container?.Update(gameTime);
        }

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
    }
}
