using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Assets;
using Quaver.Screens.Menu.UI.Tips;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Screens.Menu
{
    public class MenuScreenView : ScreenView
    {
        /// <summary>
        ///     The main menu background.
        /// </summary>
        public BackgroundImage Background { get; set; }

        /// <summary>
        ///     The line on the top.
        /// </summary>
        public Line TopLine { get; set; }

        /// <summary>
        ///     The line on the bottom.
        /// </summary>
        public Line BottomLine { get; set; }

        /// <summary>
        ///     The container for the screen content.
        /// </summary>
        public ScrollContainer MiddleContainer { get; set; }

        /// <summary>
        ///     The menu tip displayed at the bottom.
        /// </summary>
        public MenuTip Tip { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MenuScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreateLines();
            CreateMiddleContainer();
            CreateMenuTip();
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
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();
        }

        /// <summary>
        ///     Create
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.ConnectingBackground, 0, false)
        {
            Parent = Container,
            SpriteBatchOptions = new SpriteBatchOptions() { BlendState = BlendState.NonPremultiplied }
        };

        /// <summary>
        ///     Creates the top and bottom lines.
        /// </summary>
        private void CreateLines()
        {
            TopLine = new Line(Vector2.Zero, Color.White, 1)
            {
                Parent = Container,
                Position = new ScalableVector2(65, 62),
                UsePreviousSpriteBatchOptions = true
            };

            TopLine.EndPosition = new Vector2(WindowManager.Width - TopLine.X, TopLine.AbsolutePosition.Y);

            BottomLine = new Line(Vector2.Zero, Color.White, 1)
            {
                Parent = Container,
                Position = new ScalableVector2(65, WindowManager.Height - 62),
                UsePreviousSpriteBatchOptions = true
            };

            BottomLine.EndPosition = new Vector2(WindowManager.Width - BottomLine.X, BottomLine.AbsolutePosition.Y);
        }

        /// <summary>
        ///     Creates a ScrollContainer for all of the content int he middle of the screen.
        /// </summary>
        private void CreateMiddleContainer() => MiddleContainer = new ScrollContainer(
                new ScalableVector2(TopLine.EndPosition.X - TopLine.X, BottomLine.Y - TopLine.Y),
                new ScalableVector2(TopLine.EndPosition.X - TopLine.X, BottomLine.Y - TopLine.Y))
        {
            Parent = Container,
            Position = new ScalableVector2(TopLine.X, TopLine.Y),
            Alpha = 0,
            SpriteBatchOptions =
            {
                BlendState = BlendState.NonPremultiplied
            },
        };

        /// <summary>
        ///     Creates the menu tip
        /// </summary>
        private void CreateMenuTip()
        {
            Tip = new MenuTip()
            {
                X = 0,
                Alignment = Alignment.BotLeft,
                Y = -5
            };

            MiddleContainer.AddContainedDrawable(Tip);
        }
    }
}