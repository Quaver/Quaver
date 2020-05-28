using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ImGuiNET;
using IniFileParser;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Helpers;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Online;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.Flip;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.PlaceBatch;
using Quaver.Shared.Screens.Edit.Actions.HitObjects.RemoveBatch;
using Quaver.Shared.Screens.Edit.Components;
using Quaver.Shared.Screens.Edit.Dialogs;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Quaver.Shared.Screens.Edit.Plugins;
using Quaver.Shared.Screens.Edit.Plugins.Timing;
using Quaver.Shared.Screens.Editor;
using Quaver.Shared.Screens.Editor.Timing;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Edit
{
    public sealed class EditScreen : QuaverScreen, IHasLeftPanel
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
        public BindableInt BeatSnap { get; } = new BindableInt(4, 1, 48);

        /// <summary>
        ///     All of the available beat snaps to use in the editor.
        /// </summary>
        public List<int> AvailableBeatSnaps { get; } = new List<int> {1, 2, 3, 4, 6, 8, 12, 16};

        /// <summary>
        /// </summary>
        private int BeatSnapIndex => AvailableBeatSnaps.FindIndex(x => x == BeatSnap.Value);

        /// <summary>
        /// </summary>
        public BindableInt PlayfieldScrollSpeed { get; } = ConfigManager.EditorScrollSpeedKeys ?? new BindableInt(20, 1, 40);

        /// <summary>
        /// </summary>
        public Bindable<bool> AnchorHitObjectsAtMidpoint { get; } = ConfigManager.EditorHitObjectsMidpointAnchored ?? new Bindable<bool>(true);

        /// <summary>
        /// </summary>
        public BindableInt BackgroundBrightness { get; } = ConfigManager.EditorBackgroundBrightness ?? new BindableInt(40, 1, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableMetronome { get; } = ConfigManager.EditorPlayMetronome ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        public Bindable<bool> MetronomePlayHalfBeats { get; } = ConfigManager.EditorMetronomePlayHalfBeats ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<bool> EnableHitsounds { get; } = ConfigManager.EditorEnableHitsounds ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        public BindableInt HitsoundVolume { get; } = ConfigManager.EditorHitsoundVolume ?? new BindableInt(-1, -1, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> ScaleScrollSpeedWithRate { get; } = ConfigManager.EditorScaleSpeedWithRate ?? new Bindable<bool>(true) {Value = true};

        /// <summary>
        /// </summary>
        public Bindable<EditorBeatSnapColor> BeatSnapColor { get; } = ConfigManager.EditorBeatSnapColorType ?? new Bindable<EditorBeatSnapColor>(EditorBeatSnapColor.Default);

        /// <summary>
        /// </summary>
        public Bindable<bool> ViewLayers { get; } = ConfigManager.EditorViewLayers ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<EditorCompositionTool> CompositionTool { get; } = new Bindable<EditorCompositionTool>(EditorCompositionTool.Select);

        /// <summary>
        /// </summary>
        public BindableInt LongNoteOpacity { get; } = ConfigManager.EditorLongNoteOpacity ?? new BindableInt(100, 30, 100);

        /// <summary>
        /// </summary>
        public Bindable<bool> DisplayGameplayPreview { get; } = ConfigManager.EditorDisplayGameplayPreview ?? new Bindable<bool>(false);

        /// <summary>
        /// </summary>
        public Bindable<bool> PlaceObjectsOnNearestTick { get; } = ConfigManager.EditorPlaceObjectsOnNearestTick ?? new Bindable<bool>(true);

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
        public BindableList<HitObjectInfo> SelectedHitObjects { get; } = new BindableList<HitObjectInfo>(new List<HitObjectInfo>());

        /// <summary>
        /// </summary>
        public Bindable<EditorLayerInfo> SelectedLayer { get; } = new Bindable<EditorLayerInfo>(null);

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
        public Bindable<SelectContainerPanel> ActiveLeftPanel { get; set; } = new Bindable<SelectContainerPanel>(SelectContainerPanel.MapPreview);

        /// <summary>
        /// </summary>
        public EditScreen(Map map, IAudioTrack track = null, EditorVisualTestBackground visualTestBackground = null)
        {
            Map = map;
            BackgroundStore = visualTestBackground;

            try
            {
                OriginalQua = map.LoadQua();
                WorkingMap = ObjectHelper.DeepClone(OriginalQua);
            }
            catch (Exception e)
            {
                Exit(() => new SelectionScreen());

                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "There was an issue while loading this map in the editor.");
                return;
            }

            SetAudioTrack(track);

            ActionManager = new EditorActionManager(this, WorkingMap);
            UneditableMap = new Bindable<Qua>(null);
            Metronome = new Metronome(WorkingMap, Track,  ConfigManager.GlobalAudioOffset ?? new BindableInt(0, -500, 500), MetronomePlayHalfBeats);

            LoadSkin();
            SetHitSoundObjectIndex();
            LoadPlugins();

            if (ConfigManager.Pitched != null)
                ConfigManager.Pitched.ValueChanged += OnPitchedChanged;

            SkinManager.SkinLoaded += OnSkinLoaded;
            GameBase.Game.Window.FileDropped += OnFileDropped;

            InitializeDiscordRichPresence();
            View = new EditScreenView(this);
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

            if (LongNoteOpacity != ConfigManager.EditorLongNoteOpacity)
                LongNoteOpacity.Dispose();

            if (PlaceObjectsOnNearestTick != ConfigManager.EditorPlaceObjectsOnNearestTick)
                PlaceObjectsOnNearestTick.Dispose();

            if (ConfigManager.Pitched != null)
                ConfigManager.Pitched.ValueChanged -= OnPitchedChanged;

            SkinManager.SkinLoaded -= OnSkinLoaded;

            Plugins.ForEach(x => x.Destroy());

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

                lock (AudioEngine.Track)
                {
                    if (!AudioEngine.Track.IsDisposed)
                        AudioEngine.Track.Dispose();

                    AudioEngine.Track = Track;
                    AudioEngine.Map = Map;
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
            if (DialogManager.Dialogs.Count != 0)
                return;

            var view = (EditScreenView) View;

            if (view.IsImGuiHovered)
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
                HandleKeyPressUp();
                HandleKeyPressDown();
            }

            HandleBeatSnapChanges();
            HandlePlaybackRateChanges();
            HandleCtrlInput();
            HandleKeyPressDelete();
            HandleKeyPressEscape();
            HandleKeyPressF1();
            HandleKeyPressF5();
            HandleKeyPressF6();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressEscape()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                return;

            LeaveEditor();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF1()
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.F1))
                DialogManager.Show(new EditorMetadataDialog(this));
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF5()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F5))
                return;

            var plugin = BuiltInPlugins[EditorBuiltInPlugin.TimingPointEditor];
            plugin.IsActive = !plugin.IsActive;

            if (plugin.IsActive)
                plugin.Initialize();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressF6()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.F6))
                return;

            var plugin = BuiltInPlugins[EditorBuiltInPlugin.ScrollVelocityEditor];
            plugin.IsActive = !plugin.IsActive;

            if (plugin.IsActive)
                plugin.Initialize();
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressUp()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Up))
                return;

            var index = (int) CompositionTool.Value;

            if (index - 1 >= 0)
                CompositionTool.Value = (EditorCompositionTool) index - 1;
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressDown()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Down))
                return;

            var index = (int) CompositionTool.Value;

            // - 1 because mines aren't implemented yet
            if (index + 1 < Enum.GetNames(typeof(EditorCompositionTool)).Length - 1)
                CompositionTool.Value = (EditorCompositionTool) index + 1;
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
        /// <param name="direction"></param>
        private void HandleSeeking(Direction direction)
        {
            var time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, direction, BeatSnap.Value, Track.Time);

            if (Track.IsPlaying)
            {
                for (var i = 0; i < 3; i++)
                    time = AudioEngine.GetNearestSnapTimeFromTime(WorkingMap, direction, BeatSnap.Value, time);
            }

            if (time < 0)
                time = 0;

            if (time > Track.Length)
                time = Track.Length - 100;

            Track.Seek(time);
        }

        /// <summary>
        /// </summary>
        private void HandleSeekingBackwards()
        {
            var leftPressed = KeyboardManager.IsUniqueKeyPress(Keys.Left);

            if (!leftPressed && MouseManager.CurrentState.ScrollWheelValue <= MouseManager.PreviousState.ScrollWheelValue)
                return;

            if (Track == null || Track.IsDisposed || (!CanSeek() && !leftPressed))
                return;

            HandleSeeking(Direction.Backward);
        }

        /// <summary>
        /// </summary>
        private void HandleSeekingForwards()
        {
            var rightPressed = KeyboardManager.IsUniqueKeyPress(Keys.Right);

            if (!rightPressed && MouseManager.CurrentState.ScrollWheelValue >= MouseManager.PreviousState.ScrollWheelValue)
                return;

            if (Track == null || Track.IsDisposed || (!CanSeek() && !rightPressed))
                return;

            HandleSeeking(Direction.Forward);
        }

        /// <summary>
        /// </summary>
        private void HandleBeatSnapChanges()
        {
            var ctrlPressed = KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) ||
                              KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl);

            var scrolledForward = MouseManager.CurrentState.ScrollWheelValue > MouseManager.PreviousState.ScrollWheelValue;
            var scrolledBackward = MouseManager.CurrentState.ScrollWheelValue < MouseManager.PreviousState.ScrollWheelValue;

            if (ctrlPressed && (scrolledForward || KeyboardManager.IsUniqueKeyPress(Keys.Down)))
                ChangeBeatSnap(Direction.Forward);

            if (ctrlPressed && (scrolledBackward || KeyboardManager.IsUniqueKeyPress(Keys.Up)))
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
        /// </summary>
        private void HandleCtrlInput()
        {
            if (!KeyboardManager.CurrentState.IsKeyDown(Keys.LeftControl) &&
                !KeyboardManager.CurrentState.IsKeyDown(Keys.RightControl))
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Z))
                ActionManager.Undo();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Y))
                ActionManager.Redo();

            if (KeyboardManager.IsUniqueKeyPress(Keys.C))
                CopySelectedObjects();

            if (KeyboardManager.IsUniqueKeyPress(Keys.V))
                PasteCopiedObjects();

            if (KeyboardManager.IsUniqueKeyPress(Keys.X))
                CutSelectedObjects();

            if (KeyboardManager.IsUniqueKeyPress(Keys.U))
                UploadMapset();

            if (KeyboardManager.IsUniqueKeyPress(Keys.E))
                ExportToZip();

            if (KeyboardManager.IsUniqueKeyPress(Keys.Q))
                Map.OpenFile();

            if (KeyboardManager.IsUniqueKeyPress(Keys.W))
                Map.OpenFolder();

            if (KeyboardManager.IsUniqueKeyPress(Keys.A))
            {
                if (KeyboardManager.CurrentState.IsKeyDown(Keys.LeftAlt) || KeyboardManager.CurrentState.IsKeyDown(Keys.RightAlt))
                    SelectAllObjectsInLayer();
                else
                    SelectAllObjects();
            }

            if (KeyboardManager.IsUniqueKeyPress(Keys.H))
                FlipSelectedObjects();

            if (KeyboardManager.IsUniqueKeyPress(Keys.S))
                Save();

            if (KeyboardManager.IsUniqueKeyPress(Keys.N))
                DialogManager.Show(new EditorNewSongDialog());
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressDelete()
        {
            if (!KeyboardManager.IsUniqueKeyPress(Keys.Delete))
                return;

            DeleteSelectedObjects();
        }

        /// <summary>
        /// </summary>
        private void HandleTemporaryHitObjectPlacement()
        {
            // Clever way of handing key input with num keys since the enum values are 1 after each other.
            for (var i = 0; i < WorkingMap.GetKeyCount(); i++)
            {
                if (!KeyboardManager.IsUniqueKeyPress(Keys.D1 + i))
                    continue;

                var time = (int) Math.Round(Track.Time, MidpointRounding.AwayFromZero);
                ActionManager.PlaceHitObject(i + 1, time);
            }
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
            Plugins = new List<IEditorPlugin>();

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

                    var plugin = new EditorPlugin(this, data["Name"] ?? "", data["Author"] ?? "",
                        data["Description"] ?? "", pluginPath);

                    Plugins.Add(plugin);
                }
                catch (Exception e)
                {
                    Logger.Error(e, LogType.Runtime);
                }
            }

            LoadBuiltInPlugins();

            // If the user has no timing points in their map, auto-open the bpm calculator
            if (WorkingMap.TimingPoints.Count == 0)
                BuiltInPlugins[EditorBuiltInPlugin.BpmCalculator].IsActive = true;
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
                {EditorBuiltInPlugin.BpmCalculator, new EditorPlugin(this, "BPM Calculator", "The Quaver Team", "",
                    $"{dir}/BpmCalculator/plugin.lua", true)},
                {EditorBuiltInPlugin.BpmDetector, new EditorPlugin(this, "BPM Detector", "The Quaver Team", "",
                    $"{dir}/BpmDetector/plugin.lua", true)},
                {EditorBuiltInPlugin.GoToObjects, new EditorPlugin(this, "Go To Objects", "The Quaver Team", "",
                    $"{dir}/GoToObjects/plugin.lua", true)}
            };

            foreach (var plugin in BuiltInPlugins)
                Plugins.Add(plugin.Value);
        }

        /// <summary>
        ///     Copies any objects that are currently selected to the clipboard
        /// </summary>
        public void CopySelectedObjects()
        {
            var cb = Wobble.Platform.Clipboard.NativeClipboard;

            // If no objects are selected, just select the time in the track instead
            if (SelectedHitObjects.Value.Count == 0)
            {
                cb.SetText($"{(int) Math.Round(Track.Time, MidpointRounding.AwayFromZero)}");
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
        ///     Pastes any objects that are currently selected
        /// </summary>
        public void PasteCopiedObjects()
        {
            if (Clipboard.Count == 0)
                return;

            var clonedObjects = new List<HitObjectInfo>();

            var difference = (int) Math.Round(Track.Time - Clipboard.First().StartTime, MidpointRounding.AwayFromZero);

            foreach (var h in Clipboard)
            {
                var hitObject = new HitObjectInfo()
                {
                    StartTime = h.StartTime + difference,
                    EditorLayer = h.EditorLayer,
                    HitSound = h.HitSound,
                    Lane = h.Lane
                };

                if (h.IsLongNote)
                    hitObject.EndTime = h.EndTime + difference;

                // Don't paste notes past the end
                if (hitObject.StartTime > Track.Length || hitObject.EndTime > Track.Length)
                    continue;

                clonedObjects.Add(hitObject);
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

            ActionManager.Perform(new EditorActionRemoveHitObjectBatch(ActionManager, WorkingMap, new List<HitObjectInfo>(SelectedHitObjects.Value)));
            SelectedHitObjects.Clear();
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

            var objects = WorkingMap.HitObjects.FindAll(x => x.EditorLayer == layer && !SelectedHitObjects.Value.Contains(x));
            SelectedHitObjects.AddRange(objects);
        }

        /// <summary>
        ///     Flips all objects that are currently selected
        /// </summary>
        public void FlipSelectedObjects()
        {
            if (SelectedHitObjects.Value.Count == 0)
                return;

            ActionManager.Perform(new EditorActionFlipHitObjects(ActionManager, WorkingMap, new List<HitObjectInfo>(SelectedHitObjects.Value)));
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
                var path = $"{ConfigManager.SongDirectory}/{Map.Directory}/{Map.Path}";

                if (synchronous)
                    WorkingMap.Save(path);
                else
                {
                    ThreadScheduler.Run(() =>
                    {
                        WorkingMap.Save(path);
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
        ///     Exits the enditor and returns to song select
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

            Exit(() => new SelectionScreen());
        }

        /// <summary>
        /// </summary>
        public void ExitToTestPlay()
        {
            if (Exiting)
                return;

            GameBase.Game.IsMouseVisible = false;

            if (WorkingMap.HitObjects.Count(x => x.StartTime >= Track.Time) == 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "There aren't any hitobjects to play past this point!");
                return;
            }

            if (DialogManager.Dialogs.Count != 0)
            {
                NotificationManager.Show(NotificationLevel.Warning, "Finish what you're doing before test playing!");
                return;
            }

            Exit(() =>
            {
                if (ActionManager.HasUnsavedChanges)
                {
                    Save(true);
                    NotificationManager.Show(NotificationLevel.Success, "Your map has been successfully saved!");
                }

                var map = ObjectHelper.DeepClone(WorkingMap);
                map.ApplyMods(ModManager.Mods);

                return new GameplayScreen(map, "", new List<Score>(), null, true, Track.Time, false, null, null, false, true);
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

                // Create a fresh .qua with the available metadata from the file
                var qua = new Qua()
                {
                    AudioFile = Path.GetFileName(audioFile),
                    Artist = tagFile.Tag.FirstPerformer ?? "",
                    Title = tagFile.Tag.Title ?? "",
                    Source = tagFile.Tag.Album ?? "",
                    Tags = string.Join(" ", tagFile.Tag.Genres) ?? "",
                    Creator = ConfigManager.Username.Value,
                    DifficultyName = "",
                    Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}",
                    BackgroundFile = "",
                    Mode = GameMode.Keys4
                };

                // Create a new directory to house the map.
                var dir = $"{ConfigManager.SongDirectory.Value}/{TimeHelper.GetUnixTimestampMilliseconds()}";
                Directory.CreateDirectory(dir);

                // Copy over the audio file into the directory
                File.Copy(audioFile, $"{dir}/{Path.GetFileName(audioFile)}");

                // Save the new .qua file into the directory
                var path = $"{dir}/{StringHelper.FileNameSafeString($"{qua.Artist} - {qua.Title} [{qua.DifficultyName}] - {TimeHelper.GetUnixTimestampMilliseconds()}")}.qua";
                qua.Save(path);

                // Create a new database map
                var map = Map.FromQua(qua, path);
                map.Id = MapDatabaseCache.InsertMap(map, path);
                map.NewlyCreated = true;

                // Create a new mapset from the map
                var mapset = MapsetHelper.ConvertMapsToMapsets(new List<Map> {map}).First();
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
                    var qua = ObjectHelper.DeepClone(WorkingMap);
                    qua.DifficultyName = "";
                    qua.MapId = -1;
                    qua.Description = $"Created at {TimeHelper.GetUnixTimestampMilliseconds()}";

                    if (!copyCurrent)
                        qua.HitObjects.Clear();

                    var dir = $"{ConfigManager.SongDirectory.Value}/{Map.Directory}";
                    var path = $"{dir}/{StringHelper.FileNameSafeString($"{qua.Artist} - {qua.Title} [{qua.DifficultyName}] - {TimeHelper.GetUnixTimestampMilliseconds()}")}.qua";
                    qua.Save(path);

                    // Add the new map to the db.
                    var map = Map.FromQua(qua, path);
                    map.DateAdded = DateTime.Now;
                    map.Id = MapDatabaseCache.InsertMap(map, path);
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
                    NotificationManager.Show(NotificationLevel.Error, "There was an issue while creating a new difficulty.");
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
        public void ExportToZip()
        {
            NotificationManager.Show(NotificationLevel.Info, "Please wait while the mapset is being exported...");

            ThreadScheduler.Run(() =>
            {
                MapManager.Selected.Value.Mapset.ExportToZip();
                NotificationManager.Show(NotificationLevel.Success, "The mapset has been successfully exported!");
            });
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
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Editing, Map.MapId, "",
            (byte) WorkingMap.Mode, $"{Map.Artist} - {Map.Title} [{Map.DifficultyName}]", 0);

        /// <summary>
        ///     Returns if the user is able to seek through the track
        ///     If the user is hovering over a scroll container, it prevents them from seeking.
        /// </summary>
        /// <returns></returns>
        private bool CanSeek()
        {
            var view = (EditScreenView) View;
            return !view.Layers.IsHovered();
        }

        /// <summary>
        /// </summary>
        private void InitializeDiscordRichPresence()
        {
            try
            {
                DiscordHelper.Presence.Details = WorkingMap.ToString();
                DiscordHelper.Presence.State = "Editing";
                DiscordHelper.Presence.StartTimestamp = (long) (TimeHelper.GetUnixTimestampMilliseconds() / 1000);
                DiscordHelper.Presence.EndTimestamp = 0;
                DiscordHelper.Presence.LargeImageText = OnlineManager.GetRichPresenceLargeKeyText(ConfigManager.SelectedGameMode.Value);
                DiscordHelper.Presence.SmallImageKey = ModeHelper.ToShortHand(WorkingMap.Mode).ToLower();
                DiscordHelper.Presence.SmallImageText = ModeHelper.ToLongHand(WorkingMap.Mode);
                DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

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
            // If e is a file:// URI (for example, on Wayland), it needs to be converted to a local path. If it's
            // already a local path, this function leaves it as is.
            e = new Uri(e).LocalPath;

            var file = e.ToLower();

            if (!file.EndsWith(".jpg") && !file.EndsWith(".jpeg") && !file.EndsWith(".png"))
                return;

            if (DialogManager.Dialogs.Count != 0)
                return;

            if (Map.Game != MapGame.Quaver)
            {
                NotificationManager.Show(NotificationLevel.Warning, "You cannot set a new background for a map loaded from another game.");
                return;
            }

            DialogManager.Show(new EditorChangeBackgroundDialog(this, file));
        }
    }
}