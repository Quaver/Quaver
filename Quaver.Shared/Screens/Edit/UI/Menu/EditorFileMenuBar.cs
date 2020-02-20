using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble;
using Wobble.Audio.Samples;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.Edit.UI.Menu
{
    public class EditorFileMenuBar : SpriteImGui
    {
        /// <summary>
        /// </summary>
        private Map Map { get; }

        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

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
        private Bindable<bool> AnchorHitObjectsAtMidpoint { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorBeatSnapColor> BeatSnapColor { get; }

        /// <summary>
        /// </summary>
        private BindableInt BeatSnap { get; }

        /// <summary>
        /// </summary>
        private List<int> AvailableBeatSnaps { get; }

        /// <summary>
        /// </summary>
        private Bindable<Qua> UneditableMap { get; }

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
        /// <param name="map"></param>
        /// <param name="workingMap"></param>
        /// <param name="track"></param>
        /// <param name="backgroundBrightness"></param>
        /// <param name="enableMetronome"></param>
        /// <param name="playMetronomeHalfBeats"></param>
        /// <param name="enableHitsounds"></param>
        /// <param name="hitsoundVolume"></param>
        /// <param name="scaleScrollSpeedWithRate"></param>
        /// <param name="anchorHitObjectsAtMidpoint"></param>
        /// <param name="beatSnapColor"></param>
        /// <param name="beatSnap"></param>
        /// <param name="availableBeatSnaps"></param>
        /// <param name="uneditableMap"></param>
        public EditorFileMenuBar(Map map, Qua workingMap, IAudioTrack track, BindableInt backgroundBrightness, Bindable<bool> enableMetronome,
            Bindable<bool> playMetronomeHalfBeats, Bindable<bool> enableHitsounds, BindableInt hitsoundVolume,
            Bindable<bool> scaleScrollSpeedWithRate, Bindable<bool> anchorHitObjectsAtMidpoint, Bindable<EditorBeatSnapColor> beatSnapColor,
            BindableInt beatSnap, List<int> availableBeatSnaps, Bindable<Qua> uneditableMap)
            : base(DestroyContext, GetOptions())
        {
            Map = map;
            WorkingMap = workingMap;
            Track = track;
            BackgroundBrightness = backgroundBrightness;
            EnableMetronome = enableMetronome;
            PlayMetronomeHalfBeats = playMetronomeHalfBeats;
            EnableHitsounds = enableHitsounds;
            HitsoundVolume = hitsoundVolume;
            ScaleScrollSpeedWithRate = scaleScrollSpeedWithRate;
            AnchorHitObjectsAtMidpoint = anchorHitObjectsAtMidpoint;
            BeatSnapColor = beatSnapColor;
            BeatSnap = beatSnap;
            AvailableBeatSnaps = availableBeatSnaps;
            UneditableMap = uneditableMap;
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

            if (ImGui.BeginMenu("Reference Difficulty"))
            {
                if (ImGui.MenuItem("None", "", UneditableMap.Value == null))
                    UneditableMap.Value = null;

                foreach (var map in Map.Mapset.Maps)
                {
                    var color = ColorHelper.DifficultyToColor((float) map.Difficulty10X);

                    ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(color.R / 255f, color.G / 255f, color.B / 255f, 1));

                    if (ImGui.MenuItem(map.DifficultyName, "",
                        UneditableMap?.Value?.DifficultyName == map.DifficultyName))
                    {
                        ThreadScheduler.Run(() =>
                        {
                            if (UneditableMap.Value != null)
                            {
                                lock (UneditableMap.Value)
                                    UneditableMap.Value = map.LoadQua();
                            }
                            else
                                UneditableMap.Value = map.LoadQua();
                        });
                    }

                    ImGui.PopStyleColor();
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

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

            if (ImGui.BeginMenu("Beat Snap Divisor"))
            {
                foreach (var snap in AvailableBeatSnaps)
                {
                    if (ImGui.MenuItem($"1/{StringHelper.AddOrdinal(snap)}", "", BeatSnap.Value == snap))
                        BeatSnap.Value = snap;
                }

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Beat Snap Color"))
            {
                foreach (EditorBeatSnapColor type in Enum.GetValues(typeof(EditorBeatSnapColor)))
                {
                    if (ImGui.MenuItem($"{type}", "", BeatSnapColor.Value == type))
                        BeatSnapColor.Value = type;
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Scale Scroll Speed", "", ScaleScrollSpeedWithRate.Value))
                ScaleScrollSpeedWithRate.Value = !ScaleScrollSpeedWithRate.Value;

            ImGui.Separator();

            if (ImGui.MenuItem("Center Objects", "", AnchorHitObjectsAtMidpoint.Value))
                AnchorHitObjectsAtMidpoint.Value = !AnchorHitObjectsAtMidpoint.Value;

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

            if (ImGui.MenuItem($"View Online Listing", WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{WorkingMap.MapId}");

            if (ImGui.MenuItem("Modding Discussion", WorkingMap.MapId != -1))
                BrowserHelper.OpenURL($"https://quavergame.com/mapsets/map/{WorkingMap.MapId}/mods");

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
                for (var i = 0; i < 8; i++)
                {
                    var value = (i + 1) * 0.25f;

                    if (ImGui.MenuItem($"{value * 100}%", "", Math.Abs(Track.Rate - value) < 0.001))
                        Track.Rate = value;
                }

                ImGui.EndMenu();
            }

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
            new ImGuiFont($@"{WobbleGame.WorkingDirectory}/Fonts/lato-black.ttf", 14),
        }, false);
    }
}