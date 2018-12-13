using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Graphics;
using System.Diagnostics;

namespace Quaver.Shared.Profiling
{
    public class Profiler : Container
    {
        private Sprite ContentContainer { get; set; }

        /// <summary>
        ///     The amount of time elapsed so we can begin counting each second.
        /// </summary>
        private TimeSpan ElapsedTime { get; set; } = TimeSpan.Zero;

        /// <summary>
        ///     This displays current FPS.
        /// </summary>
        public SpriteText TextFps { get; }

        /// <summary>
        ///     This displays current Memory Usage.
        /// </summary>
        public SpriteText TextMemory { get; }

        /// <summary>
        ///     This displays current Memory Usage.
        /// </summary>
        public SpriteText TextCpu { get; }

        /// <summary>
        /// 
        /// </summary>
        public PerformanceCounter CpuCounter { get; }


        /// <summary>
        ///     The current frame rate.
        /// </summary>
        private int FrameRate { get; set; }

        /// <summary>
        ///     The amount of frames that we currently have.
        /// </summary>
        private int FrameCounter { get; set; }

        /// <summary>
        ///     The last FPS recorded, so we know if to update the counter.
        /// </summary>
        private int LastRecordedFps { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// Ctor
        /// </summary>
        /// <param name="ui"></param>
        public Profiler(GlobalUserInterface ui)
        {
            Parent = ui;
            CpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");

            ContentContainer = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(160, 60),
                Position = new ScalableVector2(-10, -55),
                Tint = Color.Black,
                Alpha = 0.4f,
                Alignment = Alignment.BotRight
            };

            TextFps = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(5, 0),
                WidthScale = 1,
                Height = 20
            };

            TextMemory = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(5, 20),
                WidthScale = 1,
                Height = 20
            };

            TextCpu = new SpriteText(BitmapFonts.Exo2SemiBold, " ", 12)
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopLeft,
                Position = new ScalableVector2(5, 40),
                WidthScale = 1,
                Height = 20,
                Text = "TEST"
            };
            ShowProfiler();
        }

        public void ShowProfiler()
        {
            FrameCounter = 0;
            Visible = true;
        }

        public void HideProfiler() => Visible = false;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            // Profiler will only update when visible
            if (!Visible)
                return;

            ElapsedTime += gameTime.ElapsedGameTime;
            FrameCounter++;
            if (ElapsedTime <= TimeSpan.FromSeconds(1))
            {
                base.Update(gameTime);
                return;
            }

            ElapsedTime -= TimeSpan.FromSeconds(1);
            FrameRate = FrameCounter;
            FrameCounter = 0;

            // After a fixed interval, update and display data
            UpdateVisuals();
            base.Update(gameTime);
        }

        private void UpdateVisuals()
        {
            TextFps.Text = $"FPS: {FrameRate}";
            TextMemory.Text = $"Memory: {Process.GetCurrentProcess().WorkingSet64/1000000}MB";
            TextCpu.Text = $"Cpu: {CpuCounter.NextValue()}%";
        }
    }
}
