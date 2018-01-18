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
        public static SpriteFont Medium12 { get; } = GameBase.Content.Load<SpriteFont>("font-medium12");
        public static SpriteFont Medium16 { get; } = GameBase.Content.Load<SpriteFont>("font-medium16");
        public static SpriteFont Medium24 { get; } = GameBase.Content.Load<SpriteFont>("font-medium24");
        public static SpriteFont Medium48 { get; } = GameBase.Content.Load<SpriteFont>("font-medium48");
        public static SpriteFont Bold12 { get; } = GameBase.Content.Load<SpriteFont>("font-bold12");
    }
}
