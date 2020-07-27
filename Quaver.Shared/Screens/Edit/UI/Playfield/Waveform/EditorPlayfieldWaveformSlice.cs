using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;

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

        public override void Draw(GameTime gameTime)
        {
            SliceSprite.X = Playfield.ScreenRectangle.X;
            SliceSprite.Y = Playfield.HitPositionY - (float)(SliceTimeMilliSeconds + SliceSize) * Playfield.TrackSpeed - Height;
            SliceSprite.Height = SliceSize * Playfield.TrackSpeed;
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
            SliceSprite = new Sprite();

            var textureHeight = SliceSize / 2;

            SliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, (int) Playfield.Width, textureHeight);

            var dataColors = new Color[(int) Playfield.Width * textureHeight];

            for (var y = 0; y < textureHeight; y += 1)
            {
                var lengthRight = (int)Math.Abs(sliceData[y * 2, 0] * 127);
                var lengthLeft = (int)Math.Abs(sliceData[y * 2, 1] * 127);

                var pivotPoint = (int) Playfield.Width / 2 - lengthLeft;

                for (var x = 0; x < Playfield.Width; x++)
                {
                    var index  = (textureHeight - y - 1) * (int)Playfield.Width + x;

                    switch (x >= pivotPoint && x <= pivotPoint + lengthRight + lengthLeft)
                    {
                        case true:
                            dataColors[index].R = 0;
                            dataColors[index].G = 200;
                            dataColors[index].B = 255;
                            dataColors[index].A = 128;
                            break;

                        default:
                            dataColors[index].R = 0;
                            dataColors[index].G = 0;
                            dataColors[index].B = 0;
                            dataColors[index].A = 0;
                            break;
                    }
                }
            }

            SliceTexture.SetData(dataColors);
            SliceSprite.Image = SliceTexture;
            SliceSprite.Width = (int)Playfield.Width;
            SliceSprite.Height = SliceSize;
        }
    }
}