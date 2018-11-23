using Microsoft.Xna.Framework.Graphics;
using Quaver.Database.Maps;

namespace Quaver.Graphics.Backgrounds
{
    public class BackgroundBlurredEventArgs
    {
        public Map Map { get; }
        public Texture2D Texture { get; }

        public BackgroundBlurredEventArgs(Map map, Texture2D tex)
        {
            Map = map;
            Texture = tex;
        }
    }
}