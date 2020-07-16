using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveformSlice : Sprite
    {
        private EditorPlayfield Playfield { get; }
        private RenderTarget2D Slice { get; set; }
        private Sprite SliceSprite { get; set; }

        private float[,] SliceData { get; }
        private int SliceSize { get; }

        private double SliceTimeMillieSeconds { get; }
        private double SliceTimeOffset { get; }

        public EditorPlayfieldWaveformSlice(EditorPlayfield playfield, int sliceSize, float[,] sliceData, double sliceTime)
        {
            SliceSprite = new Sprite();

            SliceTimeOffset = 0;

            Playfield = playfield;
            SliceSize = sliceSize;
            SliceData = sliceData;
            SliceTimeMillieSeconds = sliceTime + SliceTimeOffset;

            var (pixelWidth, pixelHeight) = new Vector2((int)playfield.Width, (int)SliceSize) * Wobble.Window.WindowManager.ScreenScale;

            Slice = new RenderTarget2D(GameBase.Game.GraphicsDevice, (int)pixelWidth, (int)pixelHeight, false,
                                       GameBase.Game.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.None);

            GameBase.Game.ScheduledRenderTargetDraws.Add(() =>
            {
                var container = new Container { Size = new ScalableVector2(playfield.Width, SliceSize) };

                var gb = GameBase.Game.GraphicsDevice;

                gb.SetRenderTarget(Slice);
                gb.Clear(Color.TransparentBlack);

                for (var i = 0; i < SliceSize; i++)
                {
                    var lengthRight = Math.Abs(SliceData[i, 0] * 128f);
                    var lengthLeft = Math.Abs(SliceData[i, 1] * 128f);
                    var lineRight = new Sprite
                    {
                        Parent = container,
                        Alignment = Alignment.TopLeft,
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(lengthRight, 2),
                        Position = new ScalableVector2(playfield.Width / 2, SliceSize - i),
                        Tint = new Color(0.0f, 0.81f, 1f, 0.75f)
                    };

                    var lineLeft = new Sprite
                    {
                        Parent = container,
                        Alignment = Alignment.TopLeft,
                        Image = UserInterface.BlankBox,
                        Size = new ScalableVector2(lengthLeft, 2),
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

        public override void Draw(GameTime gameTime)
        {
            SliceSprite.X = Playfield.ScreenRectangle.X;
            SliceSprite.Y = Playfield.HitPositionY - (float)(SliceTimeMillieSeconds + SliceSize) * Playfield.TrackSpeed - Height;

            SliceSprite.Height = SliceSize * Playfield.TrackSpeed;

            SliceSprite.Draw(gameTime);
        }
        public override void Destroy()
        {
            Slice = null;
            SliceSprite = null;

            base.Destroy();
        }
    }
}