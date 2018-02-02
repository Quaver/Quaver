using Quaver.GameState;
using Quaver.Graphics;
using Quaver.Graphics.Sprite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.GameState.Gameplay.PlayScreen
{
    class HitBurst
    {
        private Boundary Boundary { get; set; }
        private Sprite HitBurstSprite { get; set; }

        public HitBurst(DrawRectangle rect, Drawable parent, int keyLane)
        {
            Boundary = new Boundary()
            {
                Size = new UDim2(rect.Width, rect.Height),
                Position = new UDim2(rect.X, rect.Y),
                Parent = parent
            };

            /*
            HitBurstSprite = new Sprite()
            {
                //Image = GameBase.LoadedSkin.
            }*/
        }

        public void Draw()
        {
            throw new NotImplementedException();
        }

        public void UnloadContent()
        {
            throw new NotImplementedException();
        }

        public void Update(double dt)
        {
            throw new NotImplementedException();
        }
    }
}
