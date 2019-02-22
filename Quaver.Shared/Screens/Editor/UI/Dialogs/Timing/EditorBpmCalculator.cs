using System;
using System.Globalization;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Server.Common.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Wobble.Graphics;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.Sprites;
using Wobble.Input;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Timing
{
    public class EditorBpmCalculator : SpriteImGui
    {
        /// <summary>
        /// </summary>
        private int TapCountNumber { get; set; } = 0;

        /// <summary>
        ///
        /// </summary>
        private long TapStart { get; set; }

        /// <summary>
        /// </summary>
        private double BpmAverage { get; set; }

        /// <summary>
        /// </summary>
        private double FirstTapAudioTime { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.T))
                HandleTaps();

            if (KeyboardManager.IsUniqueKeyPress(Keys.R))
                ResetCounter();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            SetWindowSize();

            ImGui.Begin("BPM Calculator", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse |
                                         ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.AlwaysAutoResize);

            ImGui.TextWrapped($"Start pressing \"T\" on your keyboard in sync with the beat to find the BPM.\n\n" +
                              $"Afterwards, make a timing point with the suggested " +
                              $"offset and BPM " +
                              $"and adjust the time offset.\n\n" +
                              $"Press the Reset button or \"R\" to start over.");

            ImGui.Dummy(new Vector2(0, 5));
            ImGui.Separator();

            ImGui.Dummy(new Vector2(0, 5));

            if (ImGui.Button("Tap Here"))
                HandleTaps();

            ImGui.SameLine();

            if (ImGui.Button("Reset"))
                ResetCounter();

            ImGui.Dummy(new Vector2(0, 15));

            ImGui.Text($"BPM: {BpmAverage:0.00}");

            ImGui.Dummy(new Vector2(0, 5));

            ImGui.Text($"First Tap Time: {FirstTapAudioTime:0.00}");

            ImGui.Dummy(new Vector2(0, 5));

            ImGui.Text($"# of Taps: {TapCountNumber}");

            if (TapCountNumber > 4)
            {
                ImGui.Dummy(new Vector2(0, 5));

                ImGui.TextWrapped($"We suggest placing a Timing Point at offset: \"{(int)FirstTapAudioTime}\" with " +
                           $"{Math.Round(BpmAverage, MidpointRounding.AwayFromZero)} bpm and adjusting the time from there.");
            }

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetSize()
        {
            var nativeSize = new Vector2(390, 323);

            var scale = WindowManager.DetermineDrawScaling();
            return new Vector2(nativeSize.X * scale.X, nativeSize.Y * scale.Y);
        }

        /// <summary>
        /// </summary>
        private static void SetWindowSize()
        {
            var size = GetSize();

            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(new Vector2(0, ConfigManager.WindowHeight.Value / 2f - size.Y / 2f));
        }

        /// <summary>
        /// </summary>
        private void HandleTaps()
        {
            var tapTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            if (TapCountNumber == 0)
            {
                TapStart = tapTimer;
                TapCountNumber = 1;
                FirstTapAudioTime = AudioEngine.Track.Time;
            }
            else
            {
                BpmAverage = 60000f * TapCountNumber / (tapTimer - TapStart);
                TapCountNumber++;
            }
        }

        /// <summary>
        /// </summary>
        private void ResetCounter()
        {
            BpmAverage = 0;
            FirstTapAudioTime = 0;
            TapCountNumber = 0;
            TapStart = 0;
        }
    }
}