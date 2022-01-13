using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;
using Wobble.Graphics.Animations;
using Quaver.Shared.Config;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveformSlice : Sprite
    {
        private EditorPlayfield Playfield { get; set; }

        private Sprite SliceSprite { get; set; }

        private int SliceSize { get; }

        private double SliceTimeMilliSeconds { get; }

        private Texture2D SliceTexture { get; set; }

        public EditorPlayfieldWaveformSlice(EditorPlayfield playfield, int sliceSize, float[,] sliceData, double sliceTime)
        {
            Playfield = playfield;
            SliceSize = sliceSize;
            SliceTimeMilliSeconds = sliceTime;

            CreateSlice(sliceData);
        }

        public override void Update(GameTime gameTime)
        {
            SliceSprite.PerformTransformations(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SliceSprite.X = Playfield.ScreenRectangle.X;
            SliceSprite.Y = Playfield.HitPositionY - (float)(SliceTimeMilliSeconds + SliceSize) * Playfield.TrackSpeed - Height;
            SliceSprite.Height = SliceSize * Playfield.TrackSpeed;
            SliceSprite.Tint = new Color(
                (float)ConfigManager.EditorWaveformColorR.Value / 255,
                (float)ConfigManager.EditorWaveformColorG.Value / 255,
                (float)ConfigManager.EditorWaveformColorB.Value / 255,
                (float)ConfigManager.EditorWaveformBrightness.Value / 100);
            SliceSprite.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SliceSprite?.Destroy();
            SliceSprite = null;
            SliceTexture?.Dispose();

            base.Destroy();
        }

        public void UpdatePlayfield(EditorPlayfield playfield) => Playfield = playfield;

        private void CreateSlice(float[,] sliceData)
        {
            SliceSprite = new Sprite { Alpha = 0 };

            var textureHeight = SliceSize;

            SliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, (int)Playfield.Width, textureHeight);

            var dataColors = new Color[(int)Playfield.Width * textureHeight];

            var LeftDirection = ConfigManager.EditorAudioDirection.Value == EditorPlayfieldWaveformAudioDirection.Right ? 1 : 0;
            var RightDirection = ConfigManager.EditorAudioDirection.Value == EditorPlayfieldWaveformAudioDirection.Left ? 0 : 1;

            for (var y = 0; y < textureHeight; y += 1)
            {
                var LeftPoint = (int)Math.Clamp((Playfield.Width / 2) - (Math.Abs(sliceData[y, LeftDirection]) * 254), 0, Playfield.Width / 2);
                var RightPoint = (int)Math.Clamp((Playfield.Width / 2) + (Math.Abs(sliceData[y, RightDirection]) * 254), Playfield.Width / 2, Playfield.Width);

                for (var x = LeftPoint; x < RightPoint; x++)
                {
                    var index = (textureHeight - y - 1) * (int)Playfield.Width + x;

                    dataColors[index].R = 255;
                    dataColors[index].G = 255;
                    dataColors[index].B = 255;
                    dataColors[index].A = 255;
                }
            }

            SliceTexture.SetData(dataColors);
            SliceSprite.Image = SliceTexture;
            SliceSprite.Width = (int)Playfield.Width;
            SliceSprite.Height = SliceSize;
            SliceSprite.FadeTo(1, Easing.Linear, 250);
        }
    }
}
