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
        public ProfilerGraph GraphFps { get; }

        /// <summary>
        ///     This displays current Memory Usage.
        /// </summary>
        public ProfilerGraph GraphMemory { get; }

        /// <summary>
        ///     This displays current Cpu Usage.
        /// </summary>
        public ProfilerGraph GraphCpu { get; }

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
                Size = new ScalableVector2((ProfilerGraph.WIDTH + 5) * 3, 60),
                Tint = Color.Black,
                Alpha = 0,
                Alignment = Alignment.BotRight
            };

            //graphs
            GraphFps = new ProfilerGraph(100, Color.Green)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(0, 0)
            };

            GraphMemory = new ProfilerGraph(5, Color.Yellow)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-(ProfilerGraph.WIDTH + 5), 0)
            };

            GraphCpu = new ProfilerGraph(10, Color.Red)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-(ProfilerGraph.WIDTH + 5) * 2, 0)
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
            var curMemory = Process.GetCurrentProcess().WorkingSet64 / 1000000;
            var curCpu = CpuCounter.NextValue();

            GraphFps.UpdateData(FrameRate, $"FPS: {FrameRate}");
            GraphMemory.UpdateData(curMemory, $"Memory: {curMemory}MB");
            GraphCpu.UpdateData(curCpu, $"Cpu: {string.Format("{0:0.##}", curCpu)}%");
        }
    }
}
