using System;
using System.Collections.Generic;
using System.Linq;
using ManagedBass;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Helpers;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Main.UI.Visualizer
{
    public class NoteVisualizer : Container
    {
        private Texture2D Tex { get; }

        public List<Sprite> Sprites { get; } = new List<Sprite>();

        private const int ANIM_TIME = 20000;

        public NoteVisualizer()
        {
            Tex = UserInterface.NoteVisualizer;
            CreateSprites(null);
        }

        public override void Update(GameTime gameTime)
        {
            AddSprites();
            base.Update(gameTime);
        }

        private void CreateSprites(int? y)
        {
            const float scale = 0.65f;

            var tex = Tex;

            var sprite = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Size = new ScalableVector2(tex.Width * scale, tex.Height * scale),
                Alpha = SkinManager.Skin?.MainMenu.NoteVisualizerOpacity ?? 0.60f,
                Image = tex,
            };

            if (y.HasValue)
                sprite.Y = y.Value;
            else
                sprite.Y = -sprite.Height;

            sprite.MoveToY((int) WindowManager.Height, Easing.Linear, ANIM_TIME);

            Sprites.Add(sprite);
        }

        private void AddSprites()
        {
            var firstSprite = Sprites.First();

            if (firstSprite.Y >= 0 && Sprites.Count == 1)
                CreateSprites((int) -firstSprite.Height + 100);

            if (!RectangleF.Intersects(firstSprite.ScreenRectangle, new RectangleF(0, 0, WindowManager.Width, WindowManager.Height))
                && firstSprite.Y > 0)
            {
                firstSprite.Destroy();
                Sprites.Remove(firstSprite);
            }
        }
    }
}