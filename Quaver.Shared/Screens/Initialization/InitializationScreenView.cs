using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.Results.UI.Tabs.Overview.Graphs.Footer;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Initialization
{
    public class InitializationScreenView : ScreenView
    {
        private LoadingWheelText LoadingText { get; set; }

        public InitializationScreenView(Screen screen) : base(screen)
        {
        }

        public override void Update(GameTime gameTime)
        {
            var game = GameBase.Game as QuaverGame;

            if ((game?.FirstUpdateCalled ?? false) && LoadingText == null)
            {
                LoadingText = new LoadingWheelText(24, "Loading...")
                {
                    Parent = Container,
                    Alignment = Alignment.BotRight,
                    Position = new ScalableVector2(-20, -20)
                };
            }

            Container?.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.Black);
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}