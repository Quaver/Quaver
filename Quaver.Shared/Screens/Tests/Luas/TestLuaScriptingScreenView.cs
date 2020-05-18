using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scripting;
using Wobble;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Tests.Luas
{
    public class TestLuaScriptingScreenView : ScreenView
    {
        private List<LuaImGui> Scripts { get; } = new List<LuaImGui>();

        public TestLuaScriptingScreenView(Screen screen) : base(screen)
        {
            Scripts.Add(new LuaImGui(@"Quaver.Resources/Scripts/Lua/test.lua", true));
        }

        public override void Update(GameTime gameTime)
        {
            Container?.Update(gameTime);

            foreach (var script in Scripts)
                script.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));

            Container?.Draw(gameTime);

            foreach (var script in Scripts)
                script.Draw(gameTime);
        }

        public override void Destroy() => Container?.Destroy();
    }
}