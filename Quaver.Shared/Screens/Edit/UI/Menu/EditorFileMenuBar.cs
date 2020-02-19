using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Quaver.Shared.Graphics.Menu.Border;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Menu
{
    public class EditorFileMenuBar : SpriteImGui
    {
        /// <summary>
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        /// </summary>
        private BindableInt BackgroundBrightness { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> EnableMetronome { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> PlayMetronomeHalfBeats { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> EnableHitsounds { get; }

        /// <summary>
        /// </summary>
        private BindableInt HitsoundVolume { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> ScaleScrollSpeedWithRate { get; }

        /// <summary>
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsActive { get; private set; }

#if VISUAL_TESTS
        private static bool DestroyContext { get; } = false;
#else
        private static bool DestroyContext { get; } = false;
#endif

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        /// <param name="backgroundBrightness"></param>
        /// <param name="enableMetronome"></param>
        /// <param name="playMetronomeHalfBeats"></param>
        /// <param name="enableHitsounds"></param>
        /// <param name="hitsoundVolume"></param>
        /// <param name="scaleScrollSpeedWithRate"></param>
        public EditorFileMenuBar(IAudioTrack track, BindableInt backgroundBrightness, Bindable<bool> enableMetronome, Bindable<bool> playMetronomeHalfBeats,
            Bindable<bool> enableHitsounds, BindableInt hitsoundVolume, Bindable<bool> scaleScrollSpeedWithRate)
            : base(DestroyContext, GetOptions())
        {
            Track = track;
            BackgroundBrightness = backgroundBrightness;
            EnableMetronome = enableMetronome;
            PlayMetronomeHalfBeats = playMetronomeHalfBeats;
            EnableHitsounds = enableHitsounds;
            HitsoundVolume = hitsoundVolume;
            ScaleScrollSpeedWithRate = scaleScrollSpeedWithRate;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 10));
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4));
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 24, 0));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 24, 0));

            if (!ImGui.BeginMainMenuBar())
                return;

            Height = ImGui.GetWindowSize().Y;

            CreateFileSection();
            CreateEditSection();
            CreateViewSection();
            CreateAudioSection();
            CreateWebSection();
            CreatePluginsSection();
            CreateHelpSection();

            ImGui.EndMenuBar();
            Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            IsActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemHovered() || ImGui.IsAnyItemFocused();

            ImGui.PopStyleVar();
        }

        /// <summary>
        /// </summary>
        private void CreateFileSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("File"))
                return;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateEditSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Edit"))
                return;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateViewSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("View"))
                return;

            if (ImGui.BeginMenu("Background Brightness"))
            {
                for (var i = 0; i < 11; i++)
                {
                    var value = i * 10;

                    if (ImGui.MenuItem($"{value}%", "", BackgroundBrightness.Value == value))
                        BackgroundBrightness.Value = value;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Scale Scroll Speed w/ Audio Rate", "", ScaleScrollSpeedWithRate.Value))
                ScaleScrollSpeedWithRate.Value = !ScaleScrollSpeedWithRate.Value;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreatePluginsSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Plugins"))
                return;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateHelpSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Help"))
                return;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateWebSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Web"))
                return;

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        private void CreateAudioSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Audio"))
                return;

            if (ImGui.BeginMenu("Playback Speed"))
            {
                for (var i = 0; i < 4; i++)
                {
                    var value = (i + 1) * 0.25f;

                    if (ImGui.MenuItem($"{value * 100}%", "", Math.Abs(Track.Rate - value) < 0.001))
                        Track.Rate = value;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.BeginMenu("Hitsounds"))
            {
                if (ImGui.MenuItem("Enable", "", EnableHitsounds.Value))
                    EnableHitsounds.Value = !EnableHitsounds.Value;

                if (ImGui.BeginMenu("Volume"))
                {
                    if (ImGui.MenuItem($"Default ({(int) AudioSample.GlobalVolume}%)", "", HitsoundVolume.Value == -1))
                        HitsoundVolume.Value = -1;

                    for (var i = 0; i < 10; i++)
                    {
                        var val = (i + 1) * 10;

                        if (ImGui.MenuItem($"{val}%", "", HitsoundVolume.Value == val))
                            HitsoundVolume.Value = val;
                    }

                    ImGui.EndMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();
            
            if (ImGui.BeginMenu("Metronome"))
            {
                if (ImGui.MenuItem($"Enable", "", EnableMetronome.Value))
                    EnableMetronome.Value = !EnableMetronome.Value;

                if (ImGui.MenuItem("Play Half Beats", "", PlayMetronomeHalfBeats.Value))
                    PlayMetronomeHalfBeats.Value = !PlayMetronomeHalfBeats.Value;

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static ImGuiOptions GetOptions() => new ImGuiOptions(new List<ImGuiFont>
        {
            new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf"),
        }, false);
    }
}