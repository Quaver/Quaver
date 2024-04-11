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
        private EditorPlayfield Playfield { get; set; }
        private EditorPlayfieldSpectrogram Spectrogram { get; set; }

        private Sprite SliceSprite { get; set; }

        private int SliceSize { get; }

        private double SliceTimeMilliSeconds { get; }

        private Texture2D SliceTexture { get; set; }

        private float LengthMs { get; set; }

        private int SampleRate { get; set; }

        private int ReferenceWidth { get; }

        private Func<float, float> FrequencyTransform =>
            ConfigManager.EditorSpectrogramFrequencyScale.Value switch
            {
                EditorPlayfieldSpectrogramFrequencyScale.Mel => Mel,
                EditorPlayfieldSpectrogramFrequencyScale.Erb1 => Erb1,
                EditorPlayfieldSpectrogramFrequencyScale.Erb2 => Erb2,
                EditorPlayfieldSpectrogramFrequencyScale.Linear => Linear,
                _ => throw new ArgumentOutOfRangeException()
            };

        public EditorPlayfieldSpectrogramSlice(EditorPlayfieldSpectrogram spectrogram, EditorPlayfield playfield,
            float lengthMs, int sliceSize,
            float[,] sliceData,
            double sliceTime, int sampleRate)
        {
            Spectrogram = spectrogram;
            Playfield = playfield;
            SliceSize = sliceSize;
            SliceTimeMilliSeconds = sliceTime;
            LengthMs = lengthMs;
            SampleRate = sampleRate;
            ReferenceWidth = spectrogram.FftCount;

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
            SliceSprite.Y = Playfield.HitPositionY - (float)(SliceTimeMilliSeconds + LengthMs) * Playfield.TrackSpeed -
                            Height;
            SliceSprite.Height = LengthMs * Playfield.TrackSpeed;
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

        public void UpdatePlayfield(EditorPlayfield playfield) => Playfield = playfield;

        private void CreateSlice(float[,] sliceData)
        {
            SliceSprite = new Sprite { Alpha = 0 };

            SliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, ReferenceWidth, SliceSize);

            var dataColors = new Color[ReferenceWidth * SliceSize];

            for (var y = 0; y < SliceSize; y++)
            {
                for (var x = 0; x < Spectrogram.FftCount; x++)
                {
                    var textureX = CalculateTextureX(x, FrequencyTransform);
                    if (textureX == -1) continue;
                    var intensity = GetIntensity(sliceData, y, x);
                    var index = DataColorIndex(SliceSize, y, textureX);
                    var nextTextureX = CalculateTextureX(x + 1, FrequencyTransform);
                    if (nextTextureX == -1) nextTextureX = ReferenceWidth - 1;
                    var nextIntensity = x == Spectrogram.FftCount - 1
                        ? intensity
                        : GetIntensity(sliceData, y, x + 1);
                    var curColor = SpectrogramColormap.GetColor(intensity);
                    var nextDataColorIndex = DataColorIndex(SliceSize, y, nextTextureX);
                    for (var i = index; i < nextDataColorIndex && i < dataColors.Length; i++)
                    {
                        dataColors[i] = curColor;
                    }
                }
            }

            SliceTexture.SetData(dataColors);
            SliceSprite.Image = SliceTexture;
            SliceSprite.Width = (int)Playfield.Width;
            SliceSprite.Height = LengthMs;
            SliceSprite.FadeTo(1, Easing.Linear, 250);
        }

        private float GetIntensity(float[,] sliceData, int y, int x)
        {
            var rawIntensity = GetAverageData(sliceData, y, x);
            var db = MathF.Abs(rawIntensity) < 1e-4f ? -100 : 20 * MathF.Log10(rawIntensity);
            var intensity = Math.Clamp(1 + db / 100, 0f, 1f);

            var cutoffFactor = ConfigManager.EditorSpectrogramCutoffFactor.Value;
            intensity = MathF.Max(intensity, cutoffFactor);
            intensity = (intensity - cutoffFactor) * (1 - cutoffFactor);

            intensity *= intensity * ConfigManager.EditorSpectrogramIntensityFactor.Value;
            intensity = Sigmoid(Math.Clamp(intensity, 0, 1));
            return intensity;
        }

        private int CalculateTextureX(int x, Func<float, float> transform)
        {
            var minFrequency = transform(ConfigManager.EditorSpectrogramMinimumFrequency.Value);
            var maxFrequency = transform(ConfigManager.EditorSpectrogramMaximumFrequency.Value);
            var frequency = (float)x * SampleRate / Spectrogram.FftCount;
            var transformedFrequency = transform(frequency);
            if (transformedFrequency > maxFrequency || transformedFrequency < minFrequency) return -1;
            return (int)((transformedFrequency - minFrequency) / (maxFrequency - minFrequency) * ReferenceWidth);
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


        private float GetAverageData(float[,] data, int y, int x)
        {
            return data[y, x];
        }

        private int DataColorIndex(int textureHeight, int y, int x)
        {
            return (textureHeight - y - 1) * ReferenceWidth + x;
        }
    }
}