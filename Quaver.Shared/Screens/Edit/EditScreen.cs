using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImGuiNET;
using IniFileParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Editor.Timing;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Edit
{
    public sealed class EditScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Editor;

        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        ///     A copy of the original and unedited map
        /// </summary>
        public Qua OriginalQua { get; }

        /// <summary>
        ///     The map that is being worked on by the user
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        ///     The uneditable map that is currently being viewed. Usually used for
        ///     viewing multiple difficulties at a time.
        /// </summary>
        public Bindable<Qua> UneditableMap { get; }

        /// <summary>
        ///     The AudioTrack to be used during this edit session
        /// </summary>
        public IAudioTrack Track { get; private set; }

        /// <summary>
        ///     The background used for visual testing
        /// </summary>
        public EditorVisualTestBackground BackgroundStore { get; }

        /// <summary>
        ///     The cvrrently active skin
        /// </summary>
        public Bindable<SkinStore> Skin { get; private set; }

        /// <summary>
        ///     The index of the hitobjects in which hitsounds are being played
        /// </summary>
        private int HitsoundObjectIndex { get; set; }

        /// <summary>
        /// </summary>
        public BindableInt BeatSnap { get; } = new BindableInt(4, 1, 16);

        /// <summary>
        ///     All of the available beat snaps to use in the editor.
        /// </summary>
        public List<int> AvailableBeatSnaps { get; } = new List<int> {1, 2, 3, 4, 6, 8, 12, 16};

        /// <summary>
        /// </summary>
        private int BeatSnapIndex => AvailableBeatSnaps.FindIndex(x => x == BeatSnap.Value);

        /// <summary>
        /// </summary>
        public BindableInt PlayfieldScrollSpeed { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> AnchorHitObjectsAtMidpoint { get; private set; }

        /// <summary>
        /// </summary>
        public BindableInt BackgroundBrightness { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableMetronome { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> MetronomePlayHalfBeats { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableHitsounds { get; private set; }

        /// <summary>
        /// </summary>
        public BindableInt HitsoundVolume { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> ScaleScrollSpeedWithRate { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<EditorBeatSnapColor> BeatSnapColor { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<bool> ViewLayers { get; private set; }

        /// <summary>
        /// </summary>
        private Metronome Metronome { get; }

        /// <summary>
        /// </summary>
        public List<EditorPlugin> Plugins { get; private set; }

        /// <summary>
        /// </summary>
        public EditScreen(Map map, IAudioTrack track = null, EditorVisualTestBackground visualTestBackground = null)
        {
            Map = map;
            OriginalQua = map.LoadQua();
            WorkingMap = ObjectHelper.DeepClone(OriginalQua);
            BackgroundStore = visualTestBackground;

            SetAudioTrack(track);
            LoadSkin();
            InitializePlayfieldScrollSpeed();
            InitializeHitObjectMidpointAnchoring();
            InitializeBackgroundBrightness();
            InitializeMetronomeEnable();
            InitializeMetronomePlayHalfBeats();
            InitializeHitsoundsEnable();
            InitializeHitsoundVolume();
            InitializeScaleScrollSpeedWithRate();
            InitializeBeatSnapColor();
            InitializeViewLayers();

            SetHitSoundObjectIndex();
            UneditableMap = new Bindable<Qua>(null);
            Metronome = new Metronome(WorkingMap, Track,  ConfigManager.GlobalAudioOffset ?? new BindableInt(0, -500, 500), MetronomePlayHalfBeats);

            LoadPlugins();

            View = new EditScreenView(this);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (!Exiting)
            {
                HandleInput();

                if (EnableHitsounds.Value)
                    PlayHitsounds();

                if (EnableMetronome.Value)
                    Metronome?.Update(gameTime);
            }

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Track.Seeked -= OnTrackSeeked;
            Track?.Dispose();
            Skin?.Value?.Dispose();
            Skin?.Dispose();
            UneditableMap?.Dispose();
            BeatSnap?.Dispose();
            BackgroundStore?.Dispose();
            Metronome?.Dispose();

            if (PlayfieldScrollSpeed != ConfigManager.EditorScrollSpeedKeys)
                PlayfieldScrollSpeed.Dispose();

            if (AnchorHitObjectsAtMidpoint != ConfigManager.EditorHitObjectsMidpointAnchored)
                AnchorHitObjectsAtMidpoint.Dispose();

            if (BackgroundBrightness != ConfigManager.EditorBackgroundBrightness)
                BackgroundBrightness.Dispose();

            if (EnableMetronome != ConfigManager.EditorPlayMetronome)
                EnableMetronome.Dispose();

            if (MetronomePlayHalfBeats != ConfigManager.EditorMetronomePlayHalfBeats)
                MetronomePlayHalfBeats.Dispose();

            if (EnableHitsounds != ConfigManager.EditorEnableHitsounds)
                EnableHitsounds.Dispose();

            if (HitsoundVolume != ConfigManager.EditorHitsoundVolume)
                HitsoundVolume.Dispose();

            if (ScaleScrollSpeedWithRate != ConfigManager.EditorScaleSpeedWithRate)
                ScaleScrollSpeedWithRate.Dispose();

            if (BeatSnapColor != ConfigManager.EditorBeatSnapColorType)
                BeatSnapColor.Dispose();

            if (ViewLayers != ConfigManager.EditorViewLayers)
                ViewLayers.Dispose();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        /// <param name="track"></param>
        private void SetAudioTrack(IAudioTrack track)
        {
            if (track == null)
            {
                AudioEngine.LoadCurrentTrack();
                Track = AudioEngine.Track;
            }
            else
                Track = track;

            Track.Seeked += OnTrackSeeked;
        }

        /// <summary>
        /// </summary>
        private void LoadSkin()
        {
            Skin = new Bindable<SkinStore>(SkinManager.Skin) { Value = SkinManager.Skin };

            if (Skin.Value != null)
                return;

            Skin.Value = new SkinStore();
        }

        /// <summary>
        /// </summary>
        private void InitializePlayfieldScrollSpeed()
            => PlayfieldScrollSpeed = ConfigManager.EditorScrollSpeedKeys ?? new BindableInt(20, 1, 40);

        /// <summary>
        /// </summary>
        private void InitializeHitObjectMidpointAnchoring()
        {
            AnchorHitObjectsAtMidpoint = ConfigManager.EditorHitObjectsMidpointAnchored ?? new Bindable<bool>(true)
            {
                Value = true
            };
        }

        /// <summary>
        /// </summary>
        private void InitializeBackgroundBrightness()
            => BackgroundBrightness = ConfigManager.EditorBackgroundBrightness ?? new BindableInt(40, 1, 100);

        /// <summary>
        /// </summary>
        private void InitializeMetronomeEnable()
            => EnableMetronome = ConfigManager.EditorPlayMetronome ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        private void InitializeMetronomePlayHalfBeats()
            => MetronomePlayHalfBeats = ConfigManager.EditorMetronomePlayHalfBeats ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        private void InitializeHitsoundsEnable()
            => EnableHitsounds = ConfigManager.EditorEnableHitsounds ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        private void InitializeHitsoundVolume()
            => HitsoundVolume = ConfigManager.EditorHitsoundVolume ?? new BindableInt(-1, -1, 100);

        /// <summary>
        /// </summary>
        private void InitializeScaleScrollSpeedWithRate()
            => ScaleScrollSpeedWithRate = ConfigManager.EditorScaleSpeedWithRate ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        private void InitializeBeatSnapColor() => BeatSnapColor = ConfigManager.EditorBeatSnapColorType ?? new Bindable<EditorBeatSnapColor>(EditorBeatSnapColor.Default);

        /// <summary>
        /// </summary>
        private void InitializeViewLayers() => ViewLayers = ConfigManager.EditorViewLayers ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        private void HandleInput()
        {
            if (DialogManager.Dialogs.Count != 0)
                return;

            HandleKeyPressSpace();
            HandleKeyPressPageUp();
            HandleKeyPressPageDown();

            // To not conflict with the volume controller
            if (KeyboardManager.CurrentState.IsKeyUp(Keys.LeftAlt) && KeyboardManager.CurrentState.IsKeyUp(Keys.RightAlt) &&
                KeyboardManager.CurrentState.IsKeyUp(Keys.LeftControl) && KeyboardManager.CurrentState.IsKeyUp(Keys.RightControl))
            {
                HandleSeekingBackwards();
                HandleSeekingForwards();
            }

            HandleBeatSnapChanges();
            HandlePlaybackRateChanges();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressSpace()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Space))
                return;

            if (Track == null || Track.IsDisposed)
                return;

            if (Track.IsPlaying)
                Track.Pause();
            else
            {
                var wasStopped = Track.IsStopped;

                Track.Play();

                if (wasStopped)
                    SetHitSoundObjectIndex();
            }
        }

        /// <summary>
        /// </summary>
        private void HandleSeekingBackwards()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Left)
                && MouseManager.CurrentState.ScrollWheelValue <= MouseManager.PreviousState.ScrollWheelValue)
                return;

            if (Track == null || Track.IsDisposed)
                return;

            var time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, Direction.Backward, BeatSnap.Value, Track.Time);

            if (time < 0)
                return;

            Track.Seek(time);
        }

        /// <summary>
        /// </summary>
        private void HandleSeekingForwards()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Right)
                && MouseManager.CurrentState.ScrollWheelValue >= MouseManager.PreviousState.ScrollWheelValue)
                return;

            if (Track == null || Track.IsDisposed)
                return;

            var time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, Direction.Forward, BeatSnap.Value, Track.Time);

            if (time > Track.Length)
                return;

            Track.Seek(time);
        }

        /// <summary>
        /// </summary>
        private void HandleBeatSnapChanges()
        {
            if (KeyboardManager.CurrentState.IsKeyUp(Keys.LeftControl) && KeyboardManager.CurrentState.IsKeyUp(Keys.Right))
                return;

            if (MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue || KeyboardManager.IsUniqueKeyPress(Keys.Up))
                ChangeBeatSnap(Direction.Forward);

            if (MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue || KeyboardManager.IsUniqueKeyPress(Keys.Down))
                ChangeBeatSnap(Direction.Backward);
        }

        /// <summary>
        /// </summary>
        private void HandlePlaybackRateChanges()
        {
            if (KeyboardManager.CurrentState.IsKeyUp(Keys.LeftControl) && KeyboardManager.CurrentState.IsKeyUp(Keys.RightControl))
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemMinus))
                ChangeAudioPlaybackRate(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(Keys.OemPlus))
                ChangeAudioPlaybackRate(Direction.Forward);
        }

        /// <summary>
        ///     Sets the hitsounds object index, so we know which object to play sounds for.
        ///     This is generally used when seeking through the map.
        /// </summary>
        private void SetHitSoundObjectIndex()
        {
            HitsoundObjectIndex = WorkingMap.HitObjects.FindLastIndex(x => x.StartTime <= Track.Time);
            HitsoundObjectIndex++;
        }

        /// <summary>
        /// </summary>
        private void PlayHitsounds()
        {
            for (var i = HitsoundObjectIndex; i < WorkingMap.HitObjects.Count; i++)
            {
                var obj = WorkingMap.HitObjects[i];

                if (Track.Time >= obj.StartTime)
                {
                    HitObjectManager.PlayObjectHitSounds(obj, Skin.Value, HitsoundVolume.Value);
                    HitsoundObjectIndex = i + 1;
                }
                else
                    break;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="direction"></param>
        public void ChangeBeatSnap(Direction direction)
        {
            var index = BeatSnapIndex;

            switch (direction)
            {
                case Direction.Forward:
                    BeatSnap.Value = index + 1 < AvailableBeatSnaps.Count ? AvailableBeatSnaps[index + 1] : AvailableBeatSnaps.First();
                    break;
                case Direction.Backward:
                    BeatSnap.Value = index - 1 >= 0 ? AvailableBeatSnaps[index - 1] : AvailableBeatSnaps.Last();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressPageUp()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.PageUp))
                PlayfieldScrollSpeed.Value++;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressPageDown()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.PageDown))
                PlayfieldScrollSpeed.Value--;
        }

        /// <summary>
        /// </summary>
        /// <param name="direction"></param>
        public void ChangeAudioPlaybackRate(Direction direction)
        {
            float targetRate;

            switch (direction)
            {
                case Direction.Forward:
                    targetRate = Track.Rate + 0.25f;
                    break;
                case Direction.Backward:
                    targetRate = Track.Rate - 0.25f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (targetRate <= 0 || targetRate > 2.0f)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot change the audio rate this way any further!");
                return;
            }

            Track.Rate = targetRate;
        }

        /// <summary>
        ///     Loads any plugins for the editor
        /// </summary>
        private void LoadPlugins()
        {
            Plugins = new List<EditorPlugin>();

            var pluginDirectories = Directory.GetDirectories($"{WobbleGame.WorkingDirectory}/Plugins");

            foreach (var directory in pluginDirectories)
            {
                var pluginPath = $"{directory}/plugin.lua";
                var settingsPath = $"{directory}/settings.ini";

                if (!File.Exists(pluginPath))
                {
                    Logger.Important($"Skipping load on plugin: {directory} because there is no plugin.lua file", LogType.Runtime);
                    continue;
                }

                if (!File.Exists(settingsPath))
                {
                    Logger.Important($"Skipping load on plugin: {directory} because there is no settings.ini file", LogType.Runtime);
                    continue;
                }

                try
                {
                    var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser())
                        .ReadFile($"{directory}/settings.ini")["Settings"];

                    var plugin = new EditorPlugin(data["Name"] ?? "", data["Author"] ?? "",
                        data["Description"] ?? "", pluginPath);

                    Plugins.Add(plugin);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => SetHitSoundObjectIndex();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;
    }
}