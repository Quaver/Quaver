using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using Force.DeepCloner;
using IniFileParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Helpers;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Move;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Resnap;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Swap;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Remove;
using Quaver.Shared.Screens.Edit.Actions.TimingGroups.Rename;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Quaver.Shared.Screens.Edit.Input;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.Plugins.Timing;
using Quaver.Shared.Screens.Edit.UI;
using Quaver.Shared.Screens.Edit.UI.Panels.Layers.Dialogs;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Quaver.Shared.Screens.Edit.UI.Playfield.Waveform;
using Quaver.Shared.Screens.Editor.Timing;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
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
    public sealed class EditScreen : QuaverScreen, IHasLeftPanel
    {
        static readonly TimeSpan _backupInterval = TimeSpan.FromMinutes(5);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Editor;

        public Timer BackupScheduler { get; }

        /// <summary>
        /// </summary>
        public Map Map { get; }

        /// <summary>
        ///     The most recent backup of the map, used for comparing changes.
        /// </summary>
        public Qua BackupQua { get; private set; }

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
        ///     The index of the difficulty to reference (<see cref="UneditableMap"/>)
        /// </summary>
        public BindableInt ReferenceDifficultyIndex { get; set; } = new(0, 0, int.MaxValue);

        /// <summary>
        ///     The AudioTrack to be used during this edit session
        /// </summary>
        public IAudioTrack Track { get; private set; }

        /// <summary>
        ///     The background used for visual testing
        /// </summary>
        public EditorVisualTestBackground BackgroundStore { get; }

        /// <summary>
        ///     The currently active skin
        /// </summary>
        public Bindable<SkinStore> Skin { get; private set; }

        /// <summary>
        ///     The index of the hitobjects in which hitsounds are being played
        /// </summary>
        private int HitsoundObjectIndex { get; set; }

        /// <summary>
        /// </summary>
        public BindableInt BeatSnap { get; } = new BindableInt(4, 1, 48);

        /// <summary>
        ///     All of the available beat snaps to use in the editor.
        /// </summary>
        public static List<int> AvailableBeatSnaps { get; set; } = new List<int>
        {
            1,
            2,
            3,
            4,
            6,
            8,
            12,
            16
        };

        /// <summary>
        ///     In Menu -> Edit -> Resnap All/Selected Notes, the divisions to choose
        /// </summary>
        public static HashSet<int> CustomSnapDivisions { get; } = new HashSet<int> { 12, 16 };

        /// <summary>
        /// </summary>
        private int BeatSnapIndex => AvailableBeatSnaps.FindIndex(x => x == BeatSnap.Value);

        /// <summary>
        /// </summary>
        public BindableInt PlayfieldScrollSpeed { get; } =
            ConfigManager.EditorScrollSpeedKeys ?? new BindableInt(20, 1, 40);

        /// <summary>
        /// </summary>
        public Bindable<bool> AnchorHitObjectsAtMidpoint { get; } =
            ConfigManager.EditorHitObjectsMidpointAnchored ?? new Bindable<bool>(true);

        /// <summary>
        /// </summary>
        public BindableInt BackgroundBrightness { get; } =
            ConfigManager.EditorBackgroundBrightness ?? new BindableInt(40, 1, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableMetronome { get; } =
            ConfigManager.EditorPlayMetronome ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public Bindable<bool> MetronomePlayHalfBeats { get; } =
            ConfigManager.EditorMetronomePlayHalfBeats ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableHitsounds { get; } =
            ConfigManager.EditorEnableHitsounds ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public BindableInt HitsoundVolume { get; } = ConfigManager.EditorHitsoundVolume ?? new BindableInt(-1, -1, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> ScaleScrollSpeedWithRate { get; } =
            ConfigManager.EditorScaleSpeedWithRate ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public Bindable<bool> ShowWaveform { get; } =
            ConfigManager.EditorShowWaveform ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public Bindable<bool> ShowSpectrogram { get; } =
            ConfigManager.EditorShowSpectrogram ?? new Bindable<bool>(true) { Value = true };

        /// <summary>
        /// </summary>
        public BindableInt WaveformBrightness { get; } =
            ConfigManager.EditorWaveformBrightness ?? new BindableInt(50, 1, 100);

        /// <summary>
        /// </summary>
        public BindableInt SpectrogramBrightness { get; } =
            ConfigManager.EditorSpectrogramBrightness ?? new BindableInt(50, 1, 100);

        public BindableInt SpectrogramFftSize { get; } =
            ConfigManager.EditorSpectrogramFftSize ?? new BindableInt(256, 256, 16384);

        /// <summary>
        /// </summary>
        public Bindable<EditorPlayfieldWaveformAudioDirection> AudioDirection { get; } =
            ConfigManager.EditorAudioDirection ??
            new Bindable<EditorPlayfieldWaveformAudioDirection>(EditorPlayfieldWaveformAudioDirection.Both);

        /// <summary>
        /// </summary>
        public Bindable<EditorPlayfieldWaveformFilter> WaveformFilter { get; } = ConfigManager.EditorAudioFilter ??
            new Bindable<EditorPlayfieldWaveformFilter>(EditorPlayfieldWaveformFilter.None);

        /// <summary>
        /// </summary>
        public Bindable<EditorBeatSnapColor> BeatSnapColor { get; } = ConfigManager.EditorBeatSnapColorType ??
                                                                      new Bindable<EditorBeatSnapColor>(
                                                                          EditorBeatSnapColor.Default);

        /// <summary>
        /// </summary>
        public Bindable<HitObjectColoring> ObjectColoring { get; } = ConfigManager.EditorObjectColoring ?? new Bindable<HitObjectColoring>(HitObjectColoring.None);

        /// <summary>
        /// </summary>
        public Bindable<EditorCompositionTool> CompositionTool { get; } =
            new Bindable<EditorCompositionTool>(EditorCompositionTool.Select);

        /// <summary>
        /// </summary>
        public BindableInt LongNoteOpacity { get; } =
            ConfigManager.EditorLongNoteOpacity ?? new BindableInt(100, 30, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> DisplayGameplayPreview { get; } =
            ConfigManager.EditorDisplayGameplayPreview ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<bool> PlaceObjectsOnNearestTick { get; } =
            ConfigManager.EditorPlaceObjectsOnNearestTick ?? new Bindable<bool>(true);

        /// <summary>
        /// </summary>
        public Bindable<bool> LiveMapping { get; } = ConfigManager.EditorLiveMapping ?? new Bindable<bool>(true);

        /// <summary>
        /// </summary>
        public Bindable<bool> InvertBeatSnapScroll { get; } =
            ConfigManager.EditorInvertBeatSnapScroll ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        private Metronome Metronome { get; }

        /// <summary>
        /// </summary>
        public List<IEditorPlugin> Plugins { get; private set; }

        /// <summary>
        /// </summary>
        public Dictionary<EditorBuiltInPlugin, IEditorPlugin> BuiltInPlugins { get; private set; }

        /// <summary>
        /// </summary>
        public EditorActionManager ActionManager { get; }

        /// <summary>
        /// </summary>
        public BindableList<HitObjectInfo> SelectedHitObjects { get; set; } =
            new BindableList<HitObjectInfo>(new List<HitObjectInfo>());

        /// <summary>
        /// </summary>
        public Bindable<EditorLayerInfo> SelectedLayer { get; } = new Bindable<EditorLayerInfo>(null);

        /// <summary>
        ///     The fraction scaling of ImGui Windows.
        ///     Do NOT use bindable from this. We expect the user to leave and reenter the screen to see the effect
        ///     Since <see cref="Wobble.Graphics.ImGUI.ImGuiRenderer.RebuildFontAtlas"/> needs to be called to update font sizes
        /// </summary>
        public float ImGuiScale { get; } = ConfigManager.EditorImGuiScalePercentage?.Value / 100f ?? 1f;

        /// <summary>
        ///     Objects that are currently copied
        /// </summary>
        public List<HitObjectInfo> Clipboard { get; } = new List<HitObjectInfo>();

        /// <summary>
        ///     The default/top layer that objects are placed in
        /// </summary>
        public EditorLayerInfo DefaultLayer { get; } = new EditorLayerInfo
        {
            Name = "Default Layer",
            Hidden = false,
            ColorRgb = "255,255,255"
        };


        /// <summary>
        /// </summary>
        public EditorInputManager InputManager { get; private set; }

        /// <summary>
        /// </summary>
        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; set; } =
            new Bindable<SelectContainerPanel>(SelectContainerPanel.MapPreview);

        /// <summary>
        /// </summary>
        private FileSystemWatcher FileWatcher { get; set; }

        private double LastSeekDistance;

        /// <summary>
        ///     The amount of time that has elapsed since the playfield has zoomed.
        ///     Used to create a 'textbox-like' function to change playfield zoom when
        ///     holding down the key.
        /// </summary>
        private double TimeSinceLastPlayfieldZoom { get; set; }

        /// <summary>
        ///     The hit objects that are being placed, while the keybind is not released.
        ///     This is used to place LN in livemapping.
        ///     The 0th element is not used. Use 1-indexed lane number to index into this array.
        /// </summary>
        private readonly HitObjectInfo[] heldLivemapHitObjectInfos;

        /// <summary>
        ///     The scroll group id to place SVs to if the scroll group provided to <see cref="ActionManager"/> is null
        /// </summary>
        public string SelectedScrollGroupId { get; set; } = Qua.DefaultScrollGroupId;

        /// <summary>
        ///     The scroll group corresponding to <see cref="SelectedScrollGroupId"/>
        /// </summary>
        public ScrollGroup SelectedScrollGroup =>
            (WorkingMap.TimingGroups?.TryGetValue(SelectedScrollGroupId, out var timingGroup) ?? false) &&
            timingGroup is ScrollGroup scrollGroup
                ? scrollGroup
                : WorkingMap.DefaultScrollGroup;

        /// <summary>
        ///     Color Generator for colored things
        /// </summary>
        private PaulToulColorGenerator ColorGenerator { get; } = new();

        /// <summary>
        /// </summary>
        public EditScreen(Map map, IAudioTrack track = null, EditorVisualTestBackground visualTestBackground = null)
        {
            Map = map;
            BackgroundStore = visualTestBackground;

            try
            {
                OriginalQua = map.LoadQua();
                WorkingMap = OriginalQua.DeepClone();
                heldLivemapHitObjectInfos = new HitObjectInfo[WorkingMap.GetKeyCount() + 1];
            }
            catch (Exception e)
            {
                Exit(() => new SelectionScreen());

                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error,
                    "There was an issue while loading this map in the editor.");
                return;
            }

            if (map.Game is MapGame.Quaver)
                BackupScheduler = new(MakeScheduledMapBackup, null, _backupInterval, _backupInterval);

            SetAudioTrack(track);

            ActionManager = new EditorActionManager(this, WorkingMap);
            UneditableMap = new Bindable<Qua>(null);
            Metronome = new Metronome(WorkingMap, Track,
                ConfigManager.GlobalAudioOffset ?? new BindableInt(0, -500, 500), MetronomePlayHalfBeats);

            LoadSkin();
            SetHitSoundObjectIndex();
            LoadPlugins();

            if (ConfigManager.Pitched != null)
                ConfigManager.Pitched.ValueChanged += OnPitchedChanged;

            SkinManager.SkinLoaded += OnSkinLoaded;
            GameBase.Game.Window.FileDropped += OnFileDropped;
            ActionManager.TimingGroupRenamed += ActionManagerOnTimingGroupRenamed;
            ActionManager.TimingGroupDeleted += ActionManagerOnTimingGroupDeleted;

            ReferenceDifficultyIndex = new BindableInt(0, 0, Map.Mapset.Maps.Count - 1);
            ReferenceDifficultyIndex.ValueChanged += LoadReferenceDifficulty;

            InitializeDiscordRichPresence();
            AddFileWatcher();

            View = new EditScreenView(this);
            InputManager = new EditorInputManager(this);
        }

        public void ResetInputManager()
        {
            InputManager?.Destroy();
            InputManager = new EditorInputManager(this);
        }

        private void ActionManagerOnTimingGroupDeleted(object sender, EditorTimingGroupRemovedEventArgs e)
        {
            foreach (var hitObjectInfo in Clipboard)
            {
                if (hitObjectInfo.TimingGroup == e.Id)
                    hitObjectInfo.TimingGroup = Qua.DefaultScrollGroupId;
            }

            if (e.Id == SelectedScrollGroupId)
                SelectedScrollGroupId = Qua.DefaultScrollGroupId;
        }

        private void ActionManagerOnTimingGroupRenamed(object sender, EditorTimingGroupRenamedEventArgs e)
        {
            foreach (var hitObjectInfo in Clipboard)
            {
                if (hitObjectInfo.TimingGroup == e.OldId)
                    hitObjectInfo.TimingGroup = e.NewId;
            }

            if (e.OldId == SelectedScrollGroupId)
                SelectedScrollGroupId = e.NewId;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            GameBase.Game.IsMouseVisible = true;
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 0;

            if (Map.NewlyCreated)
            {
                DialogManager.Show(new EditorMetadataDialog(this));
                Map.NewlyCreated = false;
            }

            base.OnFirstUpdate();
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
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            ActionManager.TimingGroupRenamed -= ActionManagerOnTimingGroupRenamed;
            ActionManager.TimingGroupDeleted -= ActionManagerOnTimingGroupDeleted;

            BackupScheduler?.Dispose();
            Track?.Dispose();
            Skin?.Value?.Dispose();
            Skin?.Dispose();
            UneditableMap?.Dispose();
            BeatSnap?.Dispose();
            BackgroundStore?.Dispose();
            Metronome?.Dispose();
            CompositionTool?.Dispose();
            ActionManager.Dispose();
            SelectedHitObjects.Dispose();
            SelectedLayer.Dispose();
            ActiveLeftPanel.Dispose();
            FileWatcher?.Dispose();

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

            if (ObjectColoring != ConfigManager.EditorObjectColoring)
                ObjectColoring.Dispose();

            if (LongNoteOpacity != ConfigManager.EditorLongNoteOpacity)
                LongNoteOpacity.Dispose();

            if (PlaceObjectsOnNearestTick != ConfigManager.EditorPlaceObjectsOnNearestTick)
                PlaceObjectsOnNearestTick.Dispose();

            if (LiveMapping != ConfigManager.EditorLiveMapping)
                LiveMapping.Dispose();

            if (ShowWaveform != ConfigManager.EditorShowWaveform)
                ShowWaveform.Dispose();

            if (ShowSpectrogram != ConfigManager.EditorShowSpectrogram)
                ShowSpectrogram.Dispose();

            if (ConfigManager.Pitched != null)
                ConfigManager.Pitched.ValueChanged -= OnPitchedChanged;

            SkinManager.SkinLoaded -= OnSkinLoaded;

            Plugins.ForEach(x => x.Destroy());
            
            InputManager?.Destroy();

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
            {
                Track = track;
                Track.ApplyRate(ConfigManager.Pitched?.Value ?? true);

                if (AudioEngine.Track != null)
                {
                    lock (AudioEngine.Track)
                    {
                        if (!AudioEngine.Track.IsDisposed)
                            AudioEngine.Track.Dispose();

                        AudioEngine.Track = Track;
                        AudioEngine.Map = Map;
                    }
                }
            }

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
        private void HandleInput()
        {
            if (Exiting)
                return;

            InputManager.HandleInput();
        }

        /// <summary>
        ///     Sets the hitsounds object index, so we know which object to play sounds for.
        ///     This is generally used when seeking through the map.
        /// </summary>
        private void SetHitSoundObjectIndex()
        {
            HitsoundObjectIndex = WorkingMap.HitObjects.IndexAtTime((float)Track.Time);
            HitsoundObjectIndex++;
        }

        /// <summary>
        /// </summary>
        private void PlayHitsounds()
        {
            if (!Track.IsPlaying)
                return;

            for (var i = HitsoundObjectIndex; i < WorkingMap.HitObjects.Count; i++)
            {
                var obj = WorkingMap.HitObjects[i];

                if (Track.Time >= obj.StartTime)
                {
                    if (obj.EditorLayer == 0 && DefaultLayer.Hidden)
                        continue;

                    try
                    {
                        var layer = WorkingMap.EditorLayers[obj.EditorLayer - 1];

                        if (layer.Hidden)
                            continue;
                    }
                    catch (Exception)
                    {
                        // ignore and play
                    }

                    HitObjectManager.PlayObjectHitSounds(obj, Skin.Value, HitsoundVolume.Value);
                    HitsoundObjectIndex = i + 1;
                }
                else
                    break;
            }
        }

        #region SEEKING

        /// <summary>
        ///     Removes All Custom Beat Snaps added by the user in CustomBeatSnapDialog.
        /// </summary>
        private void RemoveCustomBeatSnaps() => AvailableBeatSnaps = new List<int>
        {
            1,
            2,
            3,
            4,
            6,
            8,
            12,
            16
        };

        /// <summary>
        /// </summary>
        public void TogglePlayPause()
        {
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
        /// <param name="direction"></param>
        /// <param name="snapFactor"></param>
        /// <param name="enableMoving"></param>
        /// <param name="enableSelection"></param>
        public double SeekInDirection(Direction direction, float snapFactor = 1, bool enableMoving = false,
            bool enableSelection = false)
        {
            var snap = BeatSnap.Value * snapFactor;

            var time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, direction, snap, Track.Time);

            if (Track.IsPlaying)
            {
                for (var i = 0; i < 3; i++)
                    time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, direction, snap, time);
            }

            SeekTo(time, enableMoving, enableSelection);

            return time;
        }

        public void SeekTo(double time, bool enableMoving = false, bool enableSelection = false)
        {
            if (Track == null || Track.IsDisposed || !CanSeek()) return;

            var originalTime = Track.Time;

            time = Math.Clamp(time, 0, Track.Length - 100);
            LastSeekDistance = time - Track.Time;

            Track.Seek(time);

            if (enableSelection)
                SelectObjectsInRange(originalTime, time);

            if (enableMoving)
            {
                var dt = (int)(time - originalTime);
                ActionManager.Perform(
                    new EditorActionMoveHitObjects(ActionManager, WorkingMap, SelectedHitObjects.Value, 0, dt));
            }
        }

        public void SeekToStart(bool enableSelection = false)
        {
            SeekTo(WorkingMap.HitObjects.Count == 0 ? 0 : WorkingMap.HitObjects.Min(h => h.StartTime),
                enableSelection: enableSelection);
        }

        public void SeekToEnd(bool enableSelection = false)
        {
            SeekTo(
                WorkingMap.HitObjects.Count == 0
                    ? Track.Length
                    : WorkingMap.HitObjects.Max(h => Math.Max(h.StartTime, h.EndTime)),
                enableSelection: enableSelection);
        }

        public void SeekToStartOfSelection(bool enableSelection = false)
        {
            if (SelectedHitObjects.Value.Count == 0) return;
            SeekTo(SelectedHitObjects.Value.Min(h => h.StartTime), enableSelection: enableSelection);
        }

        public void SeekToEndOfSelection(bool enableSelection = false)
        {
            if (SelectedHitObjects.Value.Count == 0) return;
            SeekTo(SelectedHitObjects.Value.Max(h => Math.Max(h.StartTime, h.EndTime)),
                enableSelection: enableSelection);
        }

        #endregion

        #region LAYERS

        public void ToggleViewLayers() => ToggleObjectColoring(ObjectColoring, HitObjectColoring.Layer);
        public void ToggleViewTimingGroups() => ToggleObjectColoring(ObjectColoring, HitObjectColoring.TimingGroup);

        private EditorLayerInfo GetNextLayerInDirection(Direction direction, EditorLayerInfo layer)
        {
            // Default layer will be handled as index -1
            var index = WorkingMap.EditorLayers.IndexOf(layer);

            var step = direction == Direction.Forward ? 1 : -1;
            var nextIndex = Math.Min(index + step, WorkingMap.EditorLayers.Count() - 1);
            var nextLayer = nextIndex < 0 ? DefaultLayer : WorkingMap.EditorLayers[nextIndex];

            return nextLayer;
        }

        public void ChangeSelectedLayer(Direction direction) =>
            SelectedLayer.Value = GetNextLayerInDirection(direction, SelectedLayer.Value);

        public void ToggleSelectedLayerVisibility() => ActionManager.ToggleLayerVisibility(SelectedLayer.Value);

        public void ToggleAllLayerVisibility()
        {
            foreach (var layer in WorkingMap.EditorLayers)
                ActionManager.ToggleLayerVisibility(layer);
            ActionManager.ToggleLayerVisibility(DefaultLayer);
        }

        public void MoveSelectedNotesToCurrentLayer() =>
            ActionManager.MoveHitObjectsToLayer(SelectedLayer.Value, SelectedHitObjects.Value);

        public void AddNewLayer()
        {
            var layer = new EditorLayerInfo
            {
                Name = $"Layer {WorkingMap.EditorLayers.Count + 1}", ColorRgb = "255,255,255"
            };

            // FindIndex() returns -1 when the default layer is selected
            int index = WorkingMap.EditorLayers.FindIndex(l => l == SelectedLayer.Value) + 2;
            ActionManager.Perform(new EditorActionCreateLayer(WorkingMap, ActionManager, SelectedHitObjects, layer,
                index));
        }

        public void DeleteLayer()
        {
            if (SelectedLayer.Value == DefaultLayer || SelectedLayer.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot delete the default layer!");
                return;
            }

            ActionManager.Perform(new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects,
                SelectedLayer.Value));
            NotificationManager.Show(NotificationLevel.Success, $"Deleted layer '{SelectedLayer.Value.Name}'");
        }

        public void RenameLayer()
        {
            if (SelectedLayer.Value == DefaultLayer || SelectedLayer.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot rename the default layer!");
                return;
            }
            DialogManager.Show(new DialogRenameLayer(SelectedLayer.Value, ActionManager, WorkingMap));
        }

        public void RecolorLayer()
        {
            if (SelectedLayer.Value == DefaultLayer || SelectedLayer.Value == null)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot recolor the default layer!");
                return;
            }     
            DialogManager.Show(new DialogChangeLayerColor(SelectedLayer.Value, ActionManager, WorkingMap));
        }

        #region TIMING_GROUPS

        public void MoveSelectedNotesToCurrentTimingGroup() => 
            ActionManager.MoveObjectsToTimingGroup(SelectedHitObjects.Value, SelectedScrollGroupId);

        
        public string AddNewTimingGroup()
        {
            var newGroupId = EditorPluginUtils.GenerateTimingGroupId();

            var rgb = ColorGenerator.NextColor(
                WorkingMap.TimingGroups.Select(t => 
                    ColorHelper.ToXnaColor(t.Value.GetColor())
                    ).ToHashSet());

            var timingGroup = new ScrollGroup
            {
                InitialScrollVelocity = 1,
                ScrollVelocities =
                    new List<SliderVelocityInfo> { new() { Multiplier = 1, StartTime = 0 } },
                ColorRgb = $"{rgb.R},{rgb.G},{rgb.B}"
            };

            ActionManager.CreateTimingGroup(newGroupId, timingGroup, SelectedHitObjects.Value);                
            SelectedScrollGroupId = newGroupId;

            return newGroupId;
        }

        public void DeleteTimingGroup()
        {
            if (SelectedScrollGroupId is Qua.DefaultScrollGroupId or Qua.GlobalScrollGroupId)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot delete the default timing groups!");
                return;
            }

            var timingGroupId = SelectedScrollGroupId;

            ActionManager.RemoveTimingGroup(timingGroupId);
            NotificationManager.Show(NotificationLevel.Success, $"Deleted layer '{timingGroupId}'");
        }

        public void RecolorTimingGroup() =>
            DialogManager.Show(new EditorChangeTimingGroupColorDialog(SelectedScrollGroupId, SelectedScrollGroup, ActionManager));

        
        #endregion

        #endregion

        #region DIALOGS

        public void OpenCustomSnapDialog() =>
            DialogManager.Show(new CustomBeatSnapDialog(BeatSnap, AvailableBeatSnaps));

        public void OpenMetadataDialog() => DialogManager.Show(new EditorMetadataDialog(this));

        public void OpenModifiersDialog() => DialogManager.Show(new EditorModifierMenuDialog());

        #endregion

        #region PLUGINS

        /// <summary>
        ///     Loads any plugins for the editor
        /// </summary>
        private void LoadPlugins()
        {
            Plugins = new List<IEditorPlugin>();

            LoadPluginsFromDirectory($"{WobbleGame.WorkingDirectory}/Plugins", false);

            if (ConfigManager.SteamWorkshopDirectory != null)
                LoadPluginsFromDirectory($"{ConfigManager.SteamWorkshopDirectory.Value}", true);

            LoadBuiltInPlugins();
        }

        /// <summary>
        /// </summary>
        private void LoadBuiltInPlugins()
        {
            var dir = $"Quaver.Resources/Scripts/Lua/Editor";

            BuiltInPlugins = new Dictionary<EditorBuiltInPlugin, IEditorPlugin>()
            {
                {EditorBuiltInPlugin.TimingPointEditor, new EditorTimingPointPanel(this)},
                {EditorBuiltInPlugin.ScrollVelocityEditor, new EditorScrollVelocityPanel(this)},
                {EditorBuiltInPlugin.ScrollSpeedFactorEditor, new EditorScrollSpeedFactorPanel(this)},
                {EditorBuiltInPlugin.TimingGroupEditor, new EditorTimingGroupPanel(this)},
                {EditorBuiltInPlugin.KeybindEditor, new EditorKeybindPanel(this)},
                {EditorBuiltInPlugin.BpmCalculator, new EditorPlugin(this, "BPM Calculator", "The Quaver Team", "",
                    $"{dir}/BpmCalculator/plugin.lua", true)},
                {EditorBuiltInPlugin.BpmDetector, new EditorPlugin(this, "BPM Detector", "The Quaver Team", "",
                    $"{dir}/BpmDetector/plugin.lua", true)},
                {EditorBuiltInPlugin.GoToObjects, new EditorPlugin(this, "Go To Objects", "The Quaver Team", "",
                    $"{dir}/GoToObjects/plugin.lua", true)}
            };

            foreach (var plugin in BuiltInPlugins)
                Plugins.Add(plugin.Value);

            // If the user has no timing points in their map, auto-open the bpm calculator
            if (WorkingMap.TimingPoints.Count == 0)
                BuiltInPlugins[EditorBuiltInPlugin.BpmCalculator].IsActive = true;
        }

        private void LoadPluginsFromDirectory(string dir, bool isWorkshop)
        {
            foreach (var directory in Directory.GetDirectories(dir))
            {
                var pluginPath = $"{directory}/plugin.lua";
                var settingsPath = $"{directory}/settings.ini";

                if (!File.Exists(pluginPath))
                {
                    Logger.Debug($"Skipping load on plugin: {directory} because there is no plugin.lua file", LogType.Runtime);
                    continue;
                }

                if (!File.Exists(settingsPath))
                {
                    Logger.Debug($"Skipping load on plugin: {directory} because there is no settings.ini file", LogType.Runtime);
                    continue;
                }

                try
                {
                    var data = new IniFileParser.IniFileParser(new ConcatenateDuplicatedKeysIniDataParser())
                        .ReadFile($"{directory}/settings.ini")["Settings"];

                    var plugin = new EditorPlugin(this, data["Name"] ?? "", data["Author"] ?? "",
                        data["Description"] ?? "", pluginPath, false, Path.GetFileName(directory), isWorkshop);

                    Plugins.Add(plugin);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }
        }

        public void ToggleBuiltinPlugin(EditorBuiltInPlugin plugin)
        {
            TogglePlugin(BuiltInPlugins[plugin]);
        }

        public void TogglePlugin(IEditorPlugin plugin)
        {
            plugin.IsActive = !plugin.IsActive;

            if (plugin.IsActive)
                plugin.Initialize();
        }

        #endregion

        #region MAP ACTIONS

        /// <summary>
        ///     Copies any objects that are currently selected to the clipboard
        /// </summary>
        public void CopySelectedObjects()
        {
            var cb = Wobble.Platform.Clipboard.NativeClipboard;

            // If no objects are selected, just select the time in the track instead
            if (SelectedHitObjects.Value.Count == 0)
            {
                cb.SetText($"{(int)Math.Round(Track.Time, MidpointRounding.AwayFromZero)}");
                return;
            }

            var copyString = "";

            Clipboard.Clear();

            foreach (var h in SelectedHitObjects.Value.OrderBy(x => x.StartTime))
            {
                copyString += $"{h.StartTime}|{h.Lane},";
                Clipboard.Add(h);
            }

            copyString = copyString.TrimEnd(',');

            cb.SetText(copyString);
        }

        /// <summary>
        ///     Pastes any objects that are currently selected and resnaps the pasted notes if desired
        /// </summary>
        public void PasteCopiedObjects(bool resnapObjects)
        {
            if (Clipboard.Count == 0)
                return;

            var clonedObjects = new List<HitObjectInfo>();

            var difference = (int)Math.Round(Track.Time - Clipboard.First().StartTime, MidpointRounding.AwayFromZero);

            foreach (var h in Clipboard)
            {
                var hitObject = new HitObjectInfo()
                {
                    StartTime = h.StartTime + difference,
                    EditorLayer = h.EditorLayer,
                    HitSound = h.HitSound,
                    Lane = h.Lane,
                    TimingGroup = WorkingMap.TimingGroups.ContainsKey(h.TimingGroup)
                        ? h.TimingGroup
                        : Qua.DefaultScrollGroupId
                };

                if (h.IsLongNote)
                    hitObject.EndTime = h.EndTime + difference;

                // Don't paste notes past the end
                if (hitObject.StartTime > Track.Length || hitObject.EndTime > Track.Length)
                    continue;

                clonedObjects.Add(hitObject);
            }

            if (resnapObjects)
            {
                // Don't add to undo stack
                var resnapAction = new EditorActionResnapHitObjects(ActionManager, WorkingMap, new List<int> { 16, 12 },
                    clonedObjects, false);
                resnapAction.Perform();
            }

            ActionManager.Perform(new EditorActionPlaceHitObjectBatch(ActionManager, WorkingMap, clonedObjects));

            SelectedHitObjects.Clear();
            SelectedHitObjects.AddRange(clonedObjects);
        }

        /// <summary>
        ///     Performs a cut operation on the selected objects
        /// </summary>
        public void CutSelectedObjects()
        {
            if (SelectedHitObjects.Value.Count == 0)
                return;

            CopySelectedObjects();
            DeleteSelectedObjects();
        }

        /// <summary>
        ///     Deletes any objects that are currently selected
        /// </summary>
        public void DeleteSelectedObjects()
        {
            if (SelectedHitObjects.Value.Count == 0)
                return;

            ActionManager.RemoveHitObjectBatch(SelectedHitObjects.Value);
        }

        /// <summary>
        ///     Select any object that are at the current time
        /// </summary>
        public void SelectObjectsAtCurrentTime()
        {
            SelectedHitObjects.AddRange(WorkingMap.HitObjects
                .Where(hitObject => Math.Abs(hitObject.StartTime - Track.Time) < 2
                                    && !SelectedHitObjects.Value.Contains(hitObject))
                .ToList());
        }

        public void SelectObjectsInRange(double time1, double time2)
        {
            const float toleranceMs = 2;
            if (time1 > time2)
                (time1, time2) = (time2, time1);

            SelectedHitObjects.AddRange(WorkingMap.HitObjects
                .Where(hitObject => hitObject.StartTime >= time1 - toleranceMs
                                    && hitObject.StartTime <= time2 + toleranceMs
                                    && !SelectedHitObjects.Value.Contains(hitObject)).ToList());
        }

        /// <summary>
        ///     Selects every single object in the map
        /// </summary>
        public void SelectAllObjects()
        {
            SelectedHitObjects.Value.Clear();
            SelectedHitObjects.AddRange(WorkingMap.HitObjects);
        }

        /// <summary>
        ///     Selects all objects in the currently selected layer
        /// </summary>
        public void SelectAllObjectsInLayer()
        {
            var layer = 0;

            if (SelectedLayer.Value != null)
                layer = WorkingMap.EditorLayers.IndexOf(SelectedLayer.Value) + 1;

            var objects =
                WorkingMap.HitObjects.FindAll(x => x.EditorLayer == layer && !SelectedHitObjects.Value.Contains(x));
            SelectedHitObjects.AddRange(objects);
        }

        /// <summary>
        ///     Selects all objects in the currently selected scroll group
        /// </summary>
        public void SelectAllObjectsInTimingGroup()
        {
            SelectedHitObjects.Clear();
            SelectedHitObjects.AddRange(WorkingMap.HitObjects
                .Where(note => note.TimingGroup == SelectedScrollGroupId).ToList());
        }

        /// <summary>
        ///     Flips all objects that are currently selected
        /// </summary>
        public void FlipSelectedObjects()
        {
            if (SelectedHitObjects.Value.Count == 0)
                return;

            ActionManager.Perform(new EditorActionFlipHitObjects(ActionManager, WorkingMap,
                new List<HitObjectInfo>(SelectedHitObjects.Value)));
        }

        /// <summary>
        ///     Places a timing point or scroll velocity at the current point in time.
        /// </summary>
        private void PlaceTimingPointOrScrollVelocity()
        {
            if (!KeyboardManager.IsShiftDown())
            {
                ActionManager.PlaceScrollVelocity(new SliderVelocityInfo
                {
                    StartTime = (float)Track.Time,
                    Multiplier = WorkingMap.GetScrollVelocityAt(Track.Time)?.Multiplier ?? 1.0f
                }, null);
            }
            else
            {
                if (WorkingMap.TimingPoints.Count != 0)
                {
                    ActionManager.PlaceTimingPoint(new TimingPointInfo
                    {
                        StartTime = (float)Track.Time,
                        Bpm = WorkingMap.GetTimingPointAt(Track.Time)?.Bpm ?? WorkingMap.TimingPoints.First().Bpm
                    });
                }
            }
        }

        /// <summary>
        ///     At current time and lane, either places a note, or adjusts the LN length of it,
        ///     depending on the state of <see cref="isUniquePress"/>.
        /// </summary>
        /// <param name="lane">1-indexed lane number</param>
        /// <param name="isUniquePress"></param>
        /// <param name="isRelease"></param>
        public void HandleHitObjectPlacement(int lane, bool isUniquePress, bool isRelease)
        {
            var time = (int)Math.Round(Track.Time, MidpointRounding.AwayFromZero);

            // Only snaps the time if the audio is playing
            if (ConfigManager.EditorLiveMapSnap.Value && AudioEngine.Track.IsPlaying)
            {
                time = ((EditScreenView)View).Playfield.GetNearestTickFromTime(
                    time + ConfigManager.EditorLiveMapOffset.Value, BeatSnap.Value);
            }

            if (isRelease)
            {
                if (heldLivemapHitObjectInfos[lane] != null && ConfigManager.EditorLiveMapLongNote.Value)
                {
                    if (time - heldLivemapHitObjectInfos[lane].StartTime >
                        ConfigManager.EditorLiveMapLongNoteThreshold.Value)
                    {
                        var heldLivemapHitObjectStartTime = heldLivemapHitObjectInfos[lane].StartTime;

                        // Remove the notes covered by this LN
                        var lnsAtTime = WorkingMap.HitObjects.Where(h =>
                                h != heldLivemapHitObjectInfos[lane]
                                && h.Lane == lane 
                                && heldLivemapHitObjectStartTime <= h.StartTime
                                && h.StartTime <= time)
                            .ToList();

                        if (lnsAtTime.Count > 0)
                            ActionManager.RemoveHitObjectBatch(lnsAtTime);

                        ActionManager.ResizeLongNote(heldLivemapHitObjectInfos[lane],
                            heldLivemapHitObjectInfos[lane].EndTime, time);
                    }
                }
                heldLivemapHitObjectInfos[lane] = null;
                return;
            }

            var layer = WorkingMap.EditorLayers.FindIndex(l => l == SelectedLayer.Value) + 1;

            if (isUniquePress)
            {
                // Can be multiple if overlap
                var hitObjectsAtTime = WorkingMap.HitObjects.Where(h => h.Lane == lane && h.StartTime == time)
                    .ToList();
                var lnsAtTime = WorkingMap.HitObjects.Where(h => h.Lane == lane && h.IsLongNote && h.StartTime <= time && time <= h.EndTime)
                    .ToList();

                if (hitObjectsAtTime.Count > 0)
                {
                    foreach (var note in hitObjectsAtTime)
                        ActionManager.RemoveHitObject(note);
                }
                else
                {
                    // Remove any long notes that this note would reside in before placing
                    ActionManager.RemoveHitObjectBatch(lnsAtTime);
                    heldLivemapHitObjectInfos[lane] = ActionManager.PlaceHitObject(lane, time, 0, layer, timingGroupId: SelectedScrollGroupId);
                }
            }
        }

        #endregion

        #region EDITOR ACTIONS

        public void AdjustZoom(int stepSize) => PlayfieldScrollSpeed.Value += stepSize;

        public void ChangeToolTo(EditorCompositionTool tool) => CompositionTool.Value = tool;

        public void ChangeTool(Direction direction)
        {
            var index = StepAndWrapNumber(direction, (int)CompositionTool.Value,
                Enum.GetValues(typeof(EditorCompositionTool)).Length);
            CompositionTool.Value = (EditorCompositionTool)index;
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
                    BeatSnap.Value = index + 1 < AvailableBeatSnaps.Count
                        ? AvailableBeatSnaps[index + 1]
                        : AvailableBeatSnaps.First();
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
                NotificationManager.Show(NotificationLevel.Warning,
                    "You cannot change the audio rate this way any further!");
                return;
            }

            Track.Rate = targetRate;
        }

        /// <summary>
        ///     Swap selected objects' lanes (2 of 4/7 lanes only)
        /// </summary>
        public void SwapSelectedObjects(int swapLane1, int swapLane2)
        {
            if (SelectedHitObjects.Value.Count == 0)
                return;

            ActionManager.Perform(new EditorActionSwapLanes(ActionManager, WorkingMap,
                new List<HitObjectInfo>(
                    SelectedHitObjects.Value.Where(h => h.Lane == swapLane1 || h.Lane == swapLane2)),
                swapLane1, swapLane2));
        }

        /// <summary>
        ///     Highlights notes and goes to a specific timestamp.
        ///     Acceptable inputs:
        ///         - `1234|1,1255|3,1300|4` (selects notes and goes to the first timestamp)
        ///         - `12540` (goes to that timestamp)
        /// </summary>
        /// <param name="input"></param>
        public void GoToObjects(string input)
        {
            SelectedHitObjects.Clear();

            input = input.Trim();

            // Only timestamp was given
            if (Regex.IsMatch(input, @"^\d+$"))
            {
                var value = int.Parse(input);

                if (value > 0 && value < Track.Length)
                    Track.Seek(value);

                return;
            }

            var foundObjects = new List<HitObjectInfo>();

            try
            {
                var split = input.Split(",");

                foreach (var obj in split)
                {
                    var splitObj = obj.Split("|");

                    if (splitObj.Length == 1)
                        continue;

                    var time = int.Parse(splitObj[0]);
                    var lane = int.Parse(splitObj[1]);

                    var found = WorkingMap.HitObjects.Find(x => x.StartTime == time && x.Lane == lane);

                    if (found == null)
                        continue;

                    foundObjects.Add(found);
                }

                if (foundObjects.Count == 0)
                    return;

                SelectedHitObjects.AddRange(foundObjects);
                Track.Seek(foundObjects.First().StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Saves the map either synchronously or in a separate threaad.
        /// </summary>
        /// <param name="synchronous"></param>
        /// <param name="forceSave"></param>
        public void Save(bool synchronous = false, bool forceSave = false)
        {
            if (!ActionManager.HasUnsavedChanges && !forceSave)
                return;

            if (Map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot save a map loaded from another game!");
                return;
            }

            try
            {
                if (synchronous)
                    SaveWorkingMap();
                else
                {
                    ThreadScheduler.Run(() =>
                    {
                        SaveWorkingMap();
                        NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");
                    });
                }

                if (ActionManager.UndoStack.Count != 0)
                    ActionManager.LastSaveAction = ActionManager.UndoStack.Peek();

                Map.DifficultyProcessorVersion = "Needs Update";
                MapDatabaseCache.UpdateMap(Map);

                if (!MapDatabaseCache.MapsToUpdate.Contains(MapManager.Selected.Value))
                    MapDatabaseCache.MapsToUpdate.Add(MapManager.Selected.Value);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while saving your map!");
            }
        }

        /// <summary>
        /// </summary>
        private void SaveWorkingMap()
        {
            if (Map.Game == MapGame.Quaver)
                FileWatcher.EnableRaisingEvents = false;

            var map = WorkingMap.DeepClone();
            map.Save($"{ConfigManager.SongDirectory}/{Map.Directory}/{Map.Path}");

            if (Map.Game == MapGame.Quaver)
                FileWatcher.EnableRaisingEvents = true;
        }

        /// <summary>
        ///     Schedule Refresh for an outdated .qua file
        /// </summary>
        public void RefreshFileCache()
        {
            if (!MapDatabaseCache.MapsToUpdate.Contains(MapManager.Selected.Value))
                MapDatabaseCache.MapsToUpdate.Add(MapManager.Selected.Value);

            NotificationManager.Show(NotificationLevel.Info,
                $"The cached data for this file will be updated when you leave the editor.");
        }

        /// <summary>
        ///     Exits the editor and returns to song select
        /// </summary>
        public void LeaveEditor()
        {
            if (ActionManager.HasUnsavedChanges)
            {
                DialogManager.Show(new SaveAndExitDialog(this));
                return;
            }

            ExitToSongSelect();
        }

        /// <summary>
        /// </summary>
        public void ExitToSongSelect()
        {
            GameBase.Game.IsMouseVisible = false;
            GameBase.Game.GlobalUserInterface.Cursor.Alpha = 1;

            ModManager.RemoveAllMods();
            RemoveCustomBeatSnaps();

            Exit(() => new SelectionScreen());
        }

        /// <summary>
        /// </summary>
        public void ExitToTestPlay(bool fromStart = false)
        {
            if (Exiting)
                return;

            if (WorkingMap.HitObjects.Count(x => x.StartTime >= Track.Time) == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "There aren't any hitobjects to play past this point!");
                return;
            }

            if (WorkingMap.TimingPoints.Count == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "A timing point must be added to your map before test playing!");
                return;
            }

            if (DialogManager.Dialogs.Count != 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Finish what you're doing before test playing!");
                return;
            }

            GameBase.Game.IsMouseVisible = false;

            Exit(() =>
            {
                if (ActionManager.HasUnsavedChanges)
                {
                    Save(true);
                    NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");
                }

                var map = WorkingMap.DeepClone();
                map.ApplyMods(ModManager.Mods);

                var startTime = fromStart ? 0 : Track.Time;

                return new GameplayScreen(map, "", new List<Score>(), null, true, startTime, false, null, null, false,
                    true);
            });
        }

        /// <summary>
        ///     Creates a new mapset from an audio file
        /// </summary>
        /// <param name="audioFile"></param>
        public static void CreateNewMapset(string audioFile)
        {
            try
            {
                var game = GameBase.Game as QuaverGame;
                var tagFile = TagLib.File.Create(audioFile);
                var audioFileName = "audio" + Path.GetExtension(audioFile);

                // Create a fresh .qua with the available metadata from the file
                var qua = new Qua()
                {
                    AudioFile = audioFileName,
                    Artist = tagFile.Tag.FirstPerformer ?? "",
                    Title = tagFile.Tag.Title ?? "",
                    Source = tagFile.Tag.Album ?? "",
                    Tags = string.Join(" ", tagFile.Tag.Genres) ?? "",
                    Creator = ConfigManager.Username.Value,
                    DifficultyName = "",
                    Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}",
                    BackgroundFile = "",
                    Mode = GameMode.Keys4,
                    BPMDoesNotAffectScrollVelocity = true,
                    InitialScrollVelocity = 1
                };

                // Create a new directory to house the map.
                var dir = $"{ConfigManager.SongDirectory.Value}/{TimeHelper.GetUnixTimestampMilliseconds()}";
                Directory.CreateDirectory(dir);

                // Copy over the audio file into the directory
                File.Copy(audioFile, $"{dir}/{audioFileName}");

                // Save the new .qua file into the directory
                var path =
                    $"{dir}/{StringHelper.FileNameSafeString($"{TimeHelper.GetUnixTimestampMilliseconds()}")}.qua";
                qua.Save(path);

                // Create a new database map
                var map = Map.FromQua(qua, path);
                map.Id = MapDatabaseCache.InsertMap(map);
                map.NewlyCreated = true;

                // Create a new mapset from the map
                var mapset = MapsetHelper.ConvertMapsToMapsets(new List<Map> { map }).First();
                map.Mapset = mapset;

                // Make sure the mapset is loaded
                MapManager.Mapsets.Add(mapset);

                // Update the cache the next time the user goes to song select
                if (!MapDatabaseCache.MapsToUpdate.Contains(map))
                    MapDatabaseCache.MapsToUpdate.Add(map);

                var track = AudioEngine.LoadMapAudioTrack(map);

                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                BackgroundHelper.Load(map);

                game?.CurrentScreen.Exit(() =>
                {
                    MapManager.Selected.Value = map;
                    return new EditScreen(map, track);
                });
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while creating a new mapset.");
            }
        }

        /// <summary>
        ///     Switch to another difficulty of the mapset
        /// </summary>
        /// <param name="map"></param>
        /// <param name="force"></param>
        public void SwitchToMap(Map map, bool force = false)
        {
            if (ActionManager.HasUnsavedChanges && !force)
            {
                DialogManager.Show(new UnsavedChangesSwitchMapDialog(this, map));
                return;
            }

            ThreadScheduler.Run(() =>
            {
                try
                {
                    MapManager.Selected.Value = map;
                    var track = AudioEngine.LoadMapAudioTrack(map);

                    Exit(() => new EditScreen(map, track));
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while switching difficulty.");
                }
            });
        }

        /// <summary>
        ///     Creates a brand new map and reloads the editor
        /// </summary>
        /// <param name="copyCurrent"></param>
        /// <param name="force"></param>
        public void CreateNewDifficulty(bool copyCurrent = true, bool force = false)
        {
            if (Map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "You cannot create new difficulties for maps from other games. Create a new set!");

                return;
            }

            if (ActionManager.HasUnsavedChanges && !force)
            {
                DialogManager.Show(new UnsavedChangesNewMapDialog(this, copyCurrent));
                return;
            }

            ThreadScheduler.Run(() =>
            {
                try
                {
                    var qua = WorkingMap.DeepClone();
                    qua.DifficultyName = "";
                    qua.MapId = -1;
                    qua.Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}";

                    if (!copyCurrent)
                        qua.HitObjects.Clear();

                    var dir = $"{ConfigManager.SongDirectory.Value}/{Map.Directory}";
                    var path =
                        $"{dir}/{StringHelper.FileNameSafeString($"{TimeHelper.GetUnixTimestampMilliseconds()}")}.qua";
                    qua.Save(path);

                    // Add the new map to the db.
                    var map = Map.FromQua(qua, path);
                    map.DateAdded = DateTime.Now;
                    map.Id = MapDatabaseCache.InsertMap(map);
                    map.Mapset = Map.Mapset;
                    map.NewlyCreated = true;
                    Map.Mapset.Maps.Add(map);
                    MapManager.Selected.Value = map;

                    if (!MapDatabaseCache.MapsToUpdate.Contains(map))
                        MapDatabaseCache.MapsToUpdate.Add(map);

                    var track = AudioEngine.LoadMapAudioTrack(map);

                    Exit(() => new EditScreen(map, track));
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                    NotificationManager.Show(NotificationLevel.Error,
                        "There was an issue while creating a new difficulty.");
                }
            });
        }

        /// <summary>
        /// </summary>
        public void UploadMapset()
        {
            if (!OnlineManager.Connected)
                NotificationManager.Show(NotificationLevel.Warning, "You must be logged in to upload your mapset!");
            else
                DialogManager.Show(new EditorUploadConfirmationDialog(this));
        }

        /// <summary>
        /// </summary>
        public void SubmitForRank()
        {
            if (!OnlineManager.Connected)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "You must be logged in to submit your mapset for rank!");
                return;
            }

            if (!EditorUploadConfirmationDialog.IsMapsetEligibleToUpload(Map))
                return;

            if (ActionManager.HasUnsavedChanges)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "Your map has unsaved changes. Please save & upload before submitting for rank.");
                return;
            }

            DialogManager.Show(new EditorSubmitForRankConfirmationDialog(this));
        }

        /// <summary>
        /// </summary>
        public void ExportToZip()
        {
            NotificationManager.Show(NotificationLevel.Info, "Please wait while the mapset is being exported...");

            ThreadScheduler.Run(() =>
            {
                MapManager.Selected.Value.Mapset.ExportToZip();
                NotificationManager.Show(NotificationLevel.Success, "The mapset has been successfully exported!");
            });
        }

        #endregion

        #region BOOKMARKS

        /// <summary>
        ///     Seeks to the nearest bookmark in a given direction
        /// </summary>
        /// <param name="direction"></param>
        public void SeekToNearestBookmark(Direction direction)
        {
            if (WorkingMap.Bookmarks.Count == 0)
                return;

            BookmarkInfo nextBookmark = null;

            var closest = WorkingMap.Bookmarks.OrderBy(x => Math.Abs(x.StartTime - Track.Time)).First();
            var index = WorkingMap.Bookmarks.IndexOf(closest);

            switch (direction)
            {
                case Direction.Forward:
                    if (closest.StartTime > Track.Time && Math.Abs(closest.StartTime - Track.Time) > 0.1)
                        nextBookmark = closest;
                    else if (index + 1 < WorkingMap.Bookmarks.Count)
                        nextBookmark = WorkingMap.Bookmarks[index + 1];
                    break;
                case Direction.Backward:
                    if (closest.StartTime < Track.Time && Math.Abs(closest.StartTime - Track.Time) > 0.1)
                        nextBookmark = closest;
                    else if (index - 1 >= 0)
                        nextBookmark = WorkingMap.Bookmarks[index - 1];
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (nextBookmark == null)
                return;

            Track.Seek(Math.Clamp(nextBookmark.StartTime, 0, Track.Length));
        }

        #endregion

        #region EVENT HANDLERS

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => SetHitSoundObjectIndex();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPitchedChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (Track == AudioEngine.Track)
                return;

            Track.ApplyRate(e.Value);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// <exception cref="NotImplementedException"></exception>
        private void OnSkinLoaded(object sender, SkinReloadedEventArgs e) => Skin.Value = SkinManager.Skin;

        /// <summary>
        ///     Dragging backgrounds into the window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnFileDropped(object sender, string e)
        {
            var file = e.ToLower();

            if (file.EndsWith(".yaml"))
            {
                if (Path.GetFullPath(file).Equals(Path.GetFullPath(EditorInputConfig.ConfigPath),
                        StringComparison.CurrentCultureIgnoreCase))
                {
                    NotificationManager.Show(NotificationLevel.Error, "You cannot import the keymap you are already using!");
                    return;
                }
                DialogManager.Show(new YesNoDialog("APPLY KEYMAP", 
                    "Are you sure you want to overwrite your keymap?\nYou might want to back up your keymap first.",
                    () =>
                    {
                        File.Copy(file, EditorInputConfig.ConfigPath, true);
                        ResetInputManager();
                        NotificationManager.Show(NotificationLevel.Success, "The keymap has been applied!");
                    }));
                return;
            }

            if (!file.EndsWith(".jpg") && !file.EndsWith(".jpeg") && !file.EndsWith(".png"))
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            if (Map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Warning,
                    "You cannot set a new background for a map loaded from another game.");
                return;
            }

            DialogManager.Show(new EditorChangeBackgroundDialog(this, e));
        }

        #endregion

        #region HELPERS

        /// <summary>
        /// </summary>
        private void AddFileWatcher()
        {
            if (Map.Game != MapGame.Quaver || ConfigManager.SongDirectory == null)
                return;

            var dir = $"{ConfigManager.SongDirectory}/{Map.Directory}";

            if (!Directory.Exists(dir))
                return;

            FileWatcher = new FileSystemWatcher(dir) { NotifyFilter = NotifyFilters.LastWrite, Filter = $"{Map.Path}" };

            FileWatcher.Changed += (sender, args) =>
            {
                if (DialogManager.Dialogs.Count != 0)
                    return;

                DialogManager.Show(new EditorManualChangesDialog(this));
            };

            FileWatcher.EnableRaisingEvents = true;
        }

        private int StepAndWrapNumber(Direction direction, int i, int max)
        {
            if (max == 0) return i;

            switch (direction)
            {
                case Direction.Forward:
                    i += 1;
                    break;
                case Direction.Backward:
                    i -= 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            i %= max;

            if (i == -1)
                i = max - 1;

            return i;
        }

        private void ToggleBindableBool(Bindable<bool> boolean, string name)
        {
            boolean.Value = !boolean.Value;
            NotificationManager.Show(NotificationLevel.Info, (boolean.Value ? "Enabled" : "Disabled") + " " + name);
        }

        private void ToggleObjectColoring(Bindable<HitObjectColoring> coloring, HitObjectColoring mask)
        {
            coloring.Value = coloring.Value == mask ? HitObjectColoring.None : mask;

            NotificationManager.Show(NotificationLevel.Info,
                (mask == coloring.Value ? "Enabled" : "Disabled") + " " + mask + " coloring");
        }

        /// <summary>
        ///     Returns if the user is able to seek through the track
        ///     If the user is hovering over a scroll container, it prevents them from seeking.
        /// </summary>
        /// <returns></returns>
        private bool CanSeek()
        {
            var view = (EditScreenView)View;
            return !view.Layers.IsHovered() && !view.AutoMod.Panel.IsHovered();
        }

        /// <summary>
        /// </summary>
        private void InitializeDiscordRichPresence()
        {
            try
            {
                DiscordHelper.Presence.StartTimestamp = (long)(TimeHelper.GetUnixTimestampMilliseconds() / 1000);
                DiscordHelper.Presence.EndTimestamp = 0;
                DiscordHelper.Presence.LargeImageText =
                    OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(WorkingMap.Mode).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(WorkingMap.Mode);

                RichPresenceHelper.UpdateRichPresence("Editing", WorkingMap.ToString());
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        #endregion

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Editing, Map.MapId, "",
            (byte)WorkingMap.Mode, $"{Map.Artist} - {Map.Title} [{Map.DifficultyName}]", 0);

        /// <summary>
        ///     Reload <see cref="UneditableMap"/>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void LoadReferenceDifficulty(object sender, BindableValueChangedEventArgs<int> e)
        {
            ShowReferenceDifficulty();
        }

        /// <summary>
        ///     Reload <see cref="UneditableMap"/>
        /// </summary>
        public void ShowReferenceDifficulty()
        {
            ThreadScheduler.Run(() =>
            {
                var map = Map.Mapset.Maps[ReferenceDifficultyIndex.Value];
                if (UneditableMap.Value != null)
                {
                    lock (UneditableMap.Value)
                        UneditableMap.Value = map.LoadQua();
                }
                else
                    UneditableMap.Value = map.LoadQua();
            });
        }

        void MakeScheduledMapBackup(object _)
        {
            // We likely need to clone this because we are in a different thread,
            // which could cause mutation during enumeration, and an exception thrown.
            var newBackup = WorkingMap.DeepClone();

            if (BackupQua?.EqualByValue(newBackup) ?? false)
                return;

            var mapDirectory = Path.Join(ConfigManager.MapBackupDirectory, Path.GetFileNameWithoutExtension(Map.Path));
            Directory.CreateDirectory(mapDirectory);
            var filePath = Path.Join(mapDirectory, $"{DateTime.Now.ToString("s").Replace(':', '_')}.qua");

            BackupQua = newBackup;
            BackupQua.Save(filePath);
        }
    }
}