using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;
using Quaver.Shared.Scheduling;
using Wobble.Graphics.Shaders;
using System.Collections.Generic;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveformSlice : Sprite
    {
        private EditorPlayfield Playfield { get; }

        private RenderTarget2D Slice { get; set; }

        private Sprite SliceSprite { get; set; }

        private int SliceSize { get; }

        private double SliceTimeMilliSeconds { get; }


        public EditorPlayfieldWaveformSlice(EditorPlayfield playfield, int sliceSize, float[,] sliceData, double sliceTime)
        {
            SliceSprite = new Sprite();

            Playfield = playfield;
            SliceSize = sliceSize;
            SliceTimeMilliSeconds = sliceTime;

            var textureHeight = SliceSize / 2;
            ThreadScheduler.Run(() =>
            {
                var sliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, (int)playfield.Width, textureHeight);

                var dataColors = new Color[(int)playfield.Width * textureHeight];

                for (var y = 0; y < textureHeight; y += 1)
                {
                    var lengthRight = (int)Math.Abs(sliceData[y * 2, 0] * 127);
                    var lengthLeft = (int)Math.Abs(sliceData[y * 2, 1] * 127);

                    var pivotPoint = (int)playfield.Width / 2 - lengthLeft;

                    for (var x = 0; x < playfield.Width; x++)
                    {
                        var index  = (textureHeight - y - 1) * (int)playfield.Width + x;

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

                sliceTexture.SetData(dataColors);

                ScheduleUpdate(() =>
                {
                    SliceSprite.Image = sliceTexture;
                    SliceSprite.Width = (int)playfield.Width;
                    SliceSprite.Height = SliceSize;
                });
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public void Draw(GameTime gameTime)
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
            Slice = null;
            SliceSprite = null;

            base.Destroy();
        }
    }
}