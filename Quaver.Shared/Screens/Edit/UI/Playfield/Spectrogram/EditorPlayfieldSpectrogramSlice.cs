using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble.Graphics.Sprites;
using Wobble;
using Wobble.Graphics.Animations;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit.UI.Playfield.Spectrogram
{
    public class EditorPlayfieldSpectrogramSlice : Sprite
    {
        private readonly EditorPlayfield _playfield;
        private readonly EditorPlayfieldSpectrogram _spectrogram;

        private Sprite SliceSprite { get; set; }

        private readonly int _sliceSize;

        private readonly double _sliceTimeMilliSeconds;

        private Texture2D SliceTexture { get; set; }

        private readonly float _lengthMs;

        private readonly int _sampleRate;

        private readonly int _referenceWidth;

        private readonly int _trackDataYOffset;

        private readonly Func<float, float> _frequencyTransform;

        private readonly int _minimumFrequency;
        private readonly int _maximumFrequency;

        private readonly float _cutoffFactor;
        private readonly float _intensityFactor;

        public EditorPlayfieldSpectrogramSlice(EditorPlayfieldSpectrogram spectrogram, EditorPlayfield playfield,
            float lengthMs, int sliceSize,
            float[][] sliceData,
            int trackDataYOffset,
            double sliceTime, int sampleRate)
        {
            _spectrogram = spectrogram;
            _playfield = playfield;
            _sliceSize = sliceSize;
            _sliceTimeMilliSeconds = sliceTime;
            _lengthMs = lengthMs;
            _sampleRate = sampleRate;
            _referenceWidth = spectrogram.FftCount;
            _trackDataYOffset = trackDataYOffset;
            _cutoffFactor = ConfigManager.EditorSpectrogramCutoffFactor.Value;
            _intensityFactor = ConfigManager.EditorSpectrogramIntensityFactor.Value;

            _frequencyTransform = ConfigManager.EditorSpectrogramFrequencyScale.Value switch
            {
                EditorPlayfieldSpectrogramFrequencyScale.Mel => Mel,
                EditorPlayfieldSpectrogramFrequencyScale.Erb1 => Erb1,
                EditorPlayfieldSpectrogramFrequencyScale.Erb2 => Erb2,
                EditorPlayfieldSpectrogramFrequencyScale.Linear => Linear,
                _ => throw new ArgumentOutOfRangeException()
            };
            
            
            _minimumFrequency = (int)_frequencyTransform(ConfigManager.EditorSpectrogramMinimumFrequency.Value);
            _maximumFrequency = (int)_frequencyTransform(ConfigManager.EditorSpectrogramMaximumFrequency.Value);

            CreateSlice(sliceData);
        }


        public override void Update(GameTime gameTime)
        {
            SliceSprite.PerformTransformations(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            SliceSprite.X = _playfield.ScreenRectangle.X;
            SliceSprite.Y = _playfield.HitPositionY - (float)(_sliceTimeMilliSeconds + _lengthMs) * _playfield.TrackSpeed -
                            Height;
            SliceSprite.Height = _lengthMs * _playfield.TrackSpeed;
            SliceSprite.Tint = new Color(
                1.0f,
                1.0f,
                1.0f,
                (float)ConfigManager.EditorSpectrogramBrightness.Value / 100);
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
        
        private void CreateSlice(float[][] sliceData)
        {
            SliceSprite = new Sprite { Alpha = 0 };

            SliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, _referenceWidth, _sliceSize);

            var dataColors = new Color[_referenceWidth * _sliceSize];

            for (var y = 0; y < _sliceSize; y++)
            {
                for (var x = 0; x < _spectrogram.FftCount; x++)
                {
                    var textureX = CalculateTextureX(x);
                    if (textureX == -1) continue;
                    var intensity = GetIntensity(sliceData, y, x);
                    var index = DataColorIndex(_sliceSize, y, textureX);
                    var nextTextureX = CalculateTextureX(x + 1);
                    if (nextTextureX == -1) nextTextureX = _referenceWidth - 1;
                    var nextIntensity = x == _spectrogram.FftCount - 1
                        ? intensity
                        : GetIntensity(sliceData, y, x + 1);
                    var curColor = SpectrogramColormap.GetColor(intensity);
                    var nextDataColorIndex = DataColorIndex(_sliceSize, y, nextTextureX);
                    for (var i = index; i < nextDataColorIndex && i < dataColors.Length; i++)
                    {
                        dataColors[i] = curColor;
                    }
                }
            }

            SliceTexture.SetData(dataColors);
            SliceSprite.Image = SliceTexture;
            SliceSprite.Width = (int)_playfield.Width;
            SliceSprite.Height = _lengthMs;
            SliceSprite.FadeTo(1, Easing.Linear, 250);
        }

        private float GetIntensity(float[][] sliceData, int y, int x)
        {
            var rawIntensity = GetAverageData(sliceData, y, x);
            var db = MathF.Abs(rawIntensity) < 1e-4f ? -100 : 20 * MathF.Log10(rawIntensity);
            var intensity = Math.Clamp(1 + db / 100, 0f, 1f);

            intensity = MathF.Max(intensity, _cutoffFactor);
            intensity = (intensity - _cutoffFactor) * (1 - _cutoffFactor);

            intensity *= intensity * _intensityFactor;
            intensity = Sigmoid(Math.Clamp(intensity, 0, 1));
            return intensity;
        }

        private int CalculateTextureX(int x)
        {
            var frequency = (float)x * _sampleRate / _spectrogram.FftCount;
            var transformedFrequency = _frequencyTransform(frequency);
            if (transformedFrequency > _maximumFrequency || transformedFrequency < _minimumFrequency) return -1;
            return (int)((transformedFrequency - _minimumFrequency) / (_maximumFrequency - _minimumFrequency) * _referenceWidth);
        }

        private float Linear(float x) => x;

        private float Mel(float frequency)
        {
            return 2595 * MathF.Log10(1 + frequency / 700);
        }

        private float Erb1(float frequency)
        {
            return 6.23f * frequency * frequency / 1000 / 1000 + 93.39f * frequency / 1000 + 28.52f;
        }

        private float Erb2(float frequency)
        {
            return 24.7f * (4.37f * frequency / 1000 + 1);
        }

        private float Sigmoid(float x)
        {
            return x < 0.2f ? 0 : (MathF.Tanh(x * 2 - 1) + 1) / 2;
        }


        private float GetAverageData(float[][] data, int y, int x)
        {
            if (_trackDataYOffset + y >= data.Length) return 0;
            return data[_trackDataYOffset + y][x];
        }

        private int DataColorIndex(int textureHeight, int y, int x)
        {
            return (textureHeight - y - 1) * _referenceWidth + x;
        }
    }
}