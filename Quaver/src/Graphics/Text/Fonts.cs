using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Graphics
{
    internal class Fonts
    {
        public SpriteFont Medium12 { get; } = GameBase.Content.Load<SpriteFont>("Medium12");
        public SpriteFont Medium16 { get; } = GameBase.Content.Load<SpriteFont>("Medium16");
        public SpriteFont Medium24 { get; } = GameBase.Content.Load<SpriteFont>("Medium24");
        public SpriteFont Medium48 { get; } = GameBase.Content.Load<SpriteFont>("Medium48");
        public SpriteFont Bold12 { get; } = GameBase.Content.Load<SpriteFont>("Bold12");
        public SpriteFont Test { get; } = GameBase.Content.Load<SpriteFont>("testFont");
    }
}
