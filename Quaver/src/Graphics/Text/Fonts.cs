using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;


namespace Quaver.Graphics.Text
{
    internal static class Fonts
    {
        public static SpriteFont Medium12 { get; } = GameBase.Content.Load<SpriteFont>("Medium12");
        public static SpriteFont Medium16 { get; } = GameBase.Content.Load<SpriteFont>("Medium16");
        public static SpriteFont Medium24 { get; } = GameBase.Content.Load<SpriteFont>("Medium24");
        public static SpriteFont Medium48 { get; } = GameBase.Content.Load<SpriteFont>("Medium48");
        public static SpriteFont Bold12 { get; } = GameBase.Content.Load<SpriteFont>("Bold12");
        public static SpriteFont Test { get; } = GameBase.Content.Load<SpriteFont>("Medium12");
    }
}
