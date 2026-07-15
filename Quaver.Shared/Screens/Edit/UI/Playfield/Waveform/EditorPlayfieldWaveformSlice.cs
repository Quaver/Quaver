using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Waveform
{
    public class EditorPlayfieldWaveformSlice : Sprite
    {
        private readonly EditorPlayfield _playfield;
        private readonly double _startTimeMilliseconds;
        private readonly double _lengthMilliseconds;

        private Sprite _sliceSprite;
        private Texture2D _sliceTexture;

        public EditorPlayfieldWaveformSlice(EditorPlayfield playfield, Color[] pixels, int textureWidth,
            int textureHeight, double startTime, double length)
        {
            _playfield = playfield;
            _startTimeMilliseconds = startTime;
            _lengthMilliseconds = length;

            _sliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, textureWidth, textureHeight);
            _sliceTexture.SetData(pixels);

            _sliceSprite = new Sprite
            {
                Alpha = 0,
                Image = _sliceTexture,
                Width = textureWidth,
                Height = (float)length
            };
            _sliceSprite.FadeTo(1, Easing.Linear, 250);
        }

        public override void Update(GameTime gameTime)
        {
            _sliceSprite?.PerformTransformations(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            if (_sliceSprite == null)
                return;

            _sliceSprite.X = _playfield.ScreenRectangle.X;
            _sliceSprite.Y = _playfield.HitPositionY -
                (float)(_startTimeMilliseconds + _lengthMilliseconds) * _playfield.TrackSpeed - Height;
            _sliceSprite.Height = (float)_lengthMilliseconds * _playfield.TrackSpeed;
            _sliceSprite.Tint = new Color(
                ConfigManager.EditorWaveformColorR.Value / 255f,
                ConfigManager.EditorWaveformColorG.Value / 255f,
                ConfigManager.EditorWaveformColorB.Value / 255f,
                ConfigManager.EditorWaveformBrightness.Value / 100f);
            _sliceSprite.Draw(gameTime);
        }

        public override void Destroy()
        {
            _sliceSprite?.Destroy();
            _sliceSprite = null;
            _sliceTexture?.Dispose();
            _sliceTexture = null;
            base.Destroy();
        }
    }
}
