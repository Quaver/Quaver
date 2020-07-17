using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;
using Quaver.Shared.Scheduling;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveformSlice : Sprite
    {
        private EditorPlayfield Playfield { get; }
        private RenderTarget2D Slice { get; set; }
        private Sprite SliceSprite { get; set; }

        private float[,] SliceData { get; }
        private int SliceSize { get; }

        private double SliceTimeMilliSeconds { get; }
        private double SliceTimeOffset { get; }

        public EditorPlayfieldWaveformSlice(EditorPlayfield playfield, int sliceSize, float[,] sliceData, double sliceTime)
        {
            SliceSprite = new Sprite();

            SliceTimeOffset = 0;

            Playfield = playfield;
            SliceSize = sliceSize;
            SliceData = sliceData;
            SliceTimeMilliSeconds = sliceTime + SliceTimeOffset;

            var (pixelWidth, pixelHeight) = new Vector2((int)playfield.Width, (int)SliceSize) * Wobble.Window.WindowManager.ScreenScale;

            Slice = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int)pixelWidth, (int)pixelHeight, false,
                                       GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);


            //multi threaded do not remove
            /*ThreadScheduler.Run(() =>
            {
                var sliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, (int)playfield.Width, (int)SliceSize);

                for (var i = 0; i < SliceSize; i += 2)
                {
                    var lengthRight = (int)Math.Abs(SliceData[i, 0] * 127);
                    var lengthLeft = (int)Math.Abs(SliceData[i, 1] * 127);

                    if (lengthRight != 0 || lengthLeft != 0)
                    {
                        var dataSize = lengthRight * 2 + lengthLeft * 2;

                        var dataColors = new Color[dataSize];
                        for (var c = 0; c < dataSize; c++)
                        {
                            dataColors[c] = new Color(0, 105, 155);
                        }

                        var y = SliceSize - i;
                        if (y < 2) y = 2;
                        if (y > SliceSize - 3) y = SliceSize - 3;

                        sliceTexture.SetData(0, new Rectangle((int)playfield.Width / 2 - lengthLeft, y, lengthLeft + lengthRight, 2), dataColors, 0, dataSize);
                    }
                }

                ScheduleUpdate(() =>
                {
                    SliceSprite.Image = sliceTexture;
                    SliceSprite.Width = (int)playfield.Width;
                    SliceSprite.Height = SliceSize;
                });
            });*/

            //single threaded do not remove
            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                var container = new Container { Size = new ScalableVector2(playfield.Width, SliceSize) };

                var gb = GameBase.Game.GraphicsDevice;

                gb.SetRenderTarget(Slice);
                gb.Clear(Color.TransparentBlack);

                for (var i = 0; i < SliceSize; i += 2)
                {
                    var lengthRight = Math.Abs(SliceData[i, 0] * 128f);
                    var lengthLeft = Math.Abs(SliceData[i, 1] * 128f);

                    var line = new Sprite
                    {
                        Parent = container,
                        Alignment = Alignment.TopLeft,
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(lengthLeft + lengthRight, 2),
                        Position = new ScalableVector2(playfield.Width / 2 - lengthLeft, SliceSize - i),
                        Tint = new Color(0.0f, 0.81f, 1f, 0.75f)
                    };
                }

                container.Draw(new GameTime());

                GameBase.Game.SpriteBatch.End();

                SliceSprite.Image = Slice;
                SliceSprite.Width = (int)playfield.Width;
                SliceSprite.Height = (int)SliceSize;

                gb.SetRenderTarget(null);
            });
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SliceSprite.X = Playfield.ScreenRectangle.X;
            SliceSprite.Y = Playfield.HitPositionY - (float)(SliceTimeMilliSeconds + SliceSize) * Playfield.TrackSpeed - Height;

            SliceSprite.Height = SliceSize * Playfield.TrackSpeed;

            SliceSprite?.Draw(gameTime);
        }
        public override void Destroy()
        {
            Slice = null;
            SliceSprite = null;

            base.Destroy();
        }
    }
}