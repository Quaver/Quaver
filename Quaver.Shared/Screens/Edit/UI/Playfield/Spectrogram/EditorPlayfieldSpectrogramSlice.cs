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

        private Sprite SliceSprite { get; set; }

        private int SliceSize { get; }

        private double SliceTimeMilliSeconds { get; }

        private Texture2D SliceTexture { get; set; }
        
        private float LengthMs { get; set; }
        
        private int SampleRate { get; set; }

        private int ReferenceWidth { get; } = 1024;

        public EditorPlayfieldSpectrogramSlice(EditorPlayfield playfield, float lengthMs, int sliceSize,
            float[,] sliceData,
            double sliceTime, int sampleRate)
        {
            Playfield = playfield;
            SliceSize = sliceSize;
            SliceTimeMilliSeconds = sliceTime;
            LengthMs = lengthMs;
            SampleRate = sampleRate;

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

            var textureHeight = SliceSize;

            SliceTexture = new Texture2D(GameBase.Game.GraphicsDevice, ReferenceWidth, textureHeight);

            var dataColors = new Color[ReferenceWidth * textureHeight];
            Logger.Debug($"Slice {ReferenceWidth} x {textureHeight}", LogType.Runtime);

            for (var y = 0; y < textureHeight; y++)
            {
                for (var x = 0; x < EditorPlayfieldSpectrogram.FftCount; x++)
                {
                    var textureX = CalculateTextureXMel(x);
                    if (textureX == -1) continue;
                    var intensity = GetIntensity(sliceData, y, x);
                    var index = DataColorIndex(textureHeight, y, textureX);
                    var nextTextureX = CalculateTextureXMel(x + 1);
                    if (nextTextureX == -1) nextTextureX = ReferenceWidth - 1;
                    var nextIntensity = x == EditorPlayfieldSpectrogram.FftCount - 1
                        ? intensity
                        : GetIntensity(sliceData, y, x + 1);
                    var curColor = SpectrogramColormap.GetColor(intensity);
                    var nextColor = SpectrogramColormap.GetColor(nextIntensity);
                    var nextDataColorIndex = DataColorIndex(textureHeight, y, nextTextureX);
                    for (var i = index; i < nextDataColorIndex && i < dataColors.Length; i++)
                    {
                        // dataColors[i] = curColor;
                        dataColors[i] = Color.Lerp(curColor, nextColor, nextDataColorIndex == index ? 0 : (float)(i - index) / (nextDataColorIndex - index));
                    }
                    
                    // if (index + (int)Playfield.Width < dataColors.Length)
                    //     dataColors[index + (int)Playfield.Width] = new Color(intensity, 0, 0, 1);
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
            // var intensity = MathF.Sqrt(GetAverageData(sliceData, y, x)) * 3; // scale it (sqrt to make low values more visible)
            var intensity = Math.Clamp(1 + 40 * MathF.Log10(GetAverageData(sliceData, y, x)) / 100, 0f, 1f); // scale it (sqrt to make low values more visible)
            // intensity = Sigmoid(Math.Clamp(intensity, 0, 1));
            return intensity;
        }

        private int CalculateTextureXLog(int x)
        {
            var minFrequency = (float)SampleRate / EditorPlayfieldSpectrogram.FftCount;
            const float maxFrequency = 20000;
            var a = 1 / MathF.Log(maxFrequency / minFrequency);
            var b = -a * MathF.Log(minFrequency);
            var frequency = (float)x * SampleRate / EditorPlayfieldSpectrogram.FftCount;
            if (frequency < minFrequency || frequency > maxFrequency) return -1;
            var processedProgress = a * MathF.Log(frequency) + b;
            return (int)(processedProgress * ReferenceWidth);
        }
        
        private int CalculateTextureXMel(int x)
        {
            var maxMel = Mel(20000);
            var frequency = (float)x * SampleRate / EditorPlayfieldSpectrogram.FftCount;
            var mel = Mel(frequency);
            if (mel < 0 || mel > maxMel) return -1;
            var processedProgress = mel / maxMel;
            return (int)(processedProgress * ReferenceWidth);
        }
        
        private int CalculateTextureXLinear(int x)
        {
            var minFrequency = (float)SampleRate / EditorPlayfieldSpectrogram.FftCount;
            const float maxFrequency = 20000;
            var frequency = (float)x * SampleRate / EditorPlayfieldSpectrogram.FftCount;
            if (frequency < minFrequency || frequency > maxFrequency) return -1;
            
            return (int)((frequency - minFrequency) / (maxFrequency - minFrequency) * ReferenceWidth);
        }

        private float Mel(float frequency)
        {
            return 2595 * MathF.Log10(1 + frequency / 700);
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