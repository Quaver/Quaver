using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Helpers;
using Wobble;
using Wobble.Graphics;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Tests.DifficultyGraph
{
    public class TestDifficultyGraphScreenView : ScreenView
    {
        public TestDifficultyGraphScreenView(Screen screen) : base(screen)
        {
            var map = Qua.Parse("");

            // ReSharper disable once ObjectCreationAsStatement
            var seekBar = new DifficultySeekBar(map, ModIdentifier.None, new ScalableVector2(70, WindowManager.Height), 150)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Tint = ColorHelper.HexToColor("#181818")
            };

            seekBar.AddBorder(Color.White, 2);

            // ReSharper disable once ObjectCreationAsStatement
            var seekBar2 = new DifficultySeekBar(map, ModIdentifier.Speed05X,
                new ScalableVector2(70, WindowManager.Height), 150)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = -200,
                Tint = ColorHelper.HexToColor("#181818")
            };

            seekBar2.AddBorder(Color.White, 2);

            // ReSharper disable once ObjectCreationAsStatement
            var seekBar3 = new DifficultySeekBar(map, ModIdentifier.Speed20X,
                new ScalableVector2(70, WindowManager.Height), 150)
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                X = 200,
                Tint = ColorHelper.HexToColor("#181818")
            };

            seekBar3.AddBorder(Color.White, 2);
        }

        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}