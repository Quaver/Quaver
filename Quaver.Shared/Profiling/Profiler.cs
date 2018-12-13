using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using System;
using System.Collections.Generic;
using System.Text;
using Wobble;
using Wobble.Graphics.Sprites;
using Wobble.Graphics;
using System.Diagnostics;
using Quaver.Shared.Config;

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

            GraphFps = new ProfilerGraph(150, Color.LimeGreen)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(0, 0)
            };

            GraphMemory = new ProfilerGraph(516, Color.Blue)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-(ProfilerGraph.WIDTH + 5), 0)
            };

            GraphCpu = new ProfilerGraph(30, Color.Red)
            {
                Parent = ContentContainer,
                Alignment = Alignment.BotRight,
                Position = new ScalableVector2(-(ProfilerGraph.WIDTH + 5) * 2, 0)
            };

            if (ConfigManager.DisplayProfiler.Value)
                ShowProfiler();

            else
                HideProfiler();
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

            // Keep track of FPS.
            ElapsedTime += gameTime.ElapsedGameTime;
            FrameCounter++;
            if (ElapsedTime <= TimeSpan.FromSeconds(0.5))
            {
                base.Update(gameTime);
                return;
            }
            // After a fixed interval, update FPS accordingly and display data
            ElapsedTime -= TimeSpan.FromSeconds(0.5);
            FrameRate = FrameCounter;
            FrameCounter = 0;
            UpdateVisuals();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Update Graph Data and Visuals
        /// </summary>
        private void UpdateVisuals()
        {
            // Get CPU and Memory Usage
            var curMemory = Process.GetCurrentProcess().WorkingSet64 / 1000000;
            var curCpu = CpuCounter.NextValue();

            // Update Graphs
            GraphFps.UpdateData(FrameRate, $"FPS: {FrameRate}");
            GraphMemory.UpdateData(curMemory, $"Memory: {curMemory}MB");
            GraphCpu.UpdateData(curCpu, $"Cpu: {string.Format("{0:0.##}", curCpu)}%");
        }
    }
}
