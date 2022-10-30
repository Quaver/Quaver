using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Replays;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Graphs;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.HitObjects;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Logging;
using Wobble.Managers;
using Wobble.Scheduling;
using Wobble.Window;

namespace Quaver.Shared.Screens.Selection.UI.Preview
{
    public class SelectMapPreviewContainer : Sprite
    {
        /// <summary>
        /// </summary>
        private Bindable<bool> IsPlayTesting { get; }

        /// <summary>
        /// </summary>
        private Bindable<SelectContainerPanel> ActiveLeftPanel { get; }

        /// <summary>
        /// </summary>
        private LoadingWheel Wheel { get; set; }

        /// <summary>
        /// </summary>
        private TaskHandler<Map, GameplayScreen> LoadGameplayScreenTask { get; }

        /// <summary>
        ///     The gameplay screen instance that is currently loaded.
        /// </summary>
        protected GameplayScreen LoadedGameplayScreen { get; private set; }

        /// <summary>
        ///     Tells the user to press tab to toggle autoplay
        /// </summary>
        private SpriteTextPlus TestPlayPrompt { get; set; }

        /// <summary>
        ///     If true, it will never display the autoplay toggle more than once
        /// </summary>
        private bool ShownTestPlayPrompt { get; set; }

        /// <summary>
        ///     The audio track in the previous frame, so the replay can be seeked back if it changes
        /// </summary>
        private IAudioTrack TrackInPreviousFrame { get; set; }

        /// <summary>
        ///     The custom audio track for this container
        /// </summary>
        private IAudioTrack Track { get; }

        /// <summary>
        ///     The Qua that'll be used if one is passed in through the constructor
        /// </summary>
        protected Qua Qua { get; }

        /// <summary>
        /// </summary>
        private DifficultySeekBar SeekBar { get; set; }

        /// <summary>
        ///     If true, a difficulty seek bar will be created and displayed
        /// </summary>
        protected bool HasSeekBar { get; set; } = true;

        /// <summary>
        ///     The amount of delay before the task will run
        /// </summary>
        protected int DelayTime { get; set; } = 350;

        /// <summary>
        /// </summary>
        public SelectMapPreviewContainer(Bindable<bool> isPlayTesting, Bindable<SelectContainerPanel> activeLeftPanel, int height,
            IAudioTrack track = null, Qua qua = null)
        {
            IsPlayTesting = isPlayTesting;
            ActiveLeftPanel = activeLeftPanel;
            Qua = qua;
            Track = track;
            Size = new ScalableVector2(564, height);
            Alpha = 0f;

            LoadGameplayScreenTask = new TaskHandler<Map, GameplayScreen>(LoadGameplayScreen);
            LoadGameplayScreenTask.OnCompleted += OnLoadedGameplayScreen;

            CreateLoadingWheel();
            CreateTestPlayPrompt();

            RunLoadTask();

            MapManager.Selected.ValueChanged += OnMapChanged;
            ActiveLeftPanel.ValueChanged += OnLeftPanelChanged;
            SkinManager.SkinLoaded += OnSkinLoaded;

            ModManager.ModsChanged += OnModsChanged;

            if (Track != null)
                Track.Seeked += OnTrackSeeked;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateGameplayScreen(gameTime);

            TrackInPreviousFrame = Track ?? AudioEngine.Track;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable twice DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            ActiveLeftPanel.ValueChanged -= OnLeftPanelChanged;
            SkinManager.SkinLoaded -= OnSkinLoaded;

            ModManager.ModsChanged -= OnModsChanged;

            if (Track != null)
                Track.Seeked -= OnTrackSeeked;

            LoadGameplayScreenTask?.Dispose();
            LoadedGameplayScreen?.Destroy();
            TestPlayPrompt?.Destroy();

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => Wheel = new LoadingWheel
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(60, 60)
        };

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private GameplayScreen LoadGameplayScreen(Map map, CancellationToken token)
        {
            if (LoadedGameplayScreen == null)
                return HandleLoadGameplayScreen(map, token);

            TestPlayPrompt.Parent = null;
            LoadedGameplayScreen.Ruleset.Playfield.Container.Parent = null;
            LoadedGameplayScreen.Destroy();
            LoadedGameplayScreen = null;

            return HandleLoadGameplayScreen(map, token);
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private GameplayScreen HandleLoadGameplayScreen(Map map, CancellationToken token)
        {
            try
            {
                var qua = Qua ?? map.LoadQua();

                if (qua == Qua)
                    qua = ObjectHelper.DeepClone(qua);

                map.Qua = qua;
                map.Qua.ApplyMods(ModManager.Mods);

                var autoplay = Replay.GeneratePerfectReplayKeys(new Replay(qua.Mode, "Autoplay", 0, map.Md5Checksum), qua);

                var gameplay = new GameplayScreen(qua, map.Md5Checksum, new List<Score>(), autoplay, true, 0,
                    false, null, null, true);

                gameplay.HandleReplaySeeking();

                if (token.IsCancellationRequested)
                    gameplay.Destroy();

                return gameplay;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return null;
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadedGameplayScreen(object sender, TaskCompleteEventArgs<Map, GameplayScreen> e)
        {
            if (e.Result == null)
                return;

            if (MapManager.Selected.Value != e.Input)
                return;

            e.Input.Qua = e.Result.Map;

            LoadedGameplayScreen = e.Result;

            AddScheduledUpdate(() =>
            {
                var playfield = (GameplayPlayfieldKeys) LoadedGameplayScreen.Ruleset.Playfield;

                playfield.Stage.HealthBar.Visible = false;

                Wheel.ClearAnimations();
                Wheel.FadeTo(0, Easing.Linear, 250);

                playfield.Stage.FadeIn();
                playfield.Container.Parent = this;
                playfield.Container.Size = Size;
                playfield.Container.X = 0;
                playfield.ForegroundContainer.X = 0;
                playfield.BackgroundContainer.X = 0;
                playfield.Stage.HitLightingObjects.ForEach(x =>
                {
                    x.StopHolding();
                });

                var scroll = LoadedGameplayScreen.Map.Mode == GameMode.Keys4
                    ? ConfigManager.ScrollDirection4K
                    : ConfigManager.ScrollDirection7K;

                var skin = SkinManager.Skin.Keys[e.Result.Map.Mode];

                const int filterPanelHeight = 88;

                // Multiplier for preview to move the top half of the elements downwards by 11/15, as that is the amount that is covered by UI.
                const float previewMultiplier = 11 / 15f;

                switch (scroll.Value)
                {
                    case ScrollDirection.Down:
                    case ScrollDirection.Split:
                        playfield.Container.Alignment = Alignment.BotLeft;
                        playfield.Container.Y = -MenuBorder.HEIGHT - Y;

                        if (playfield.Stage.HitError.Y < 0)
                            playfield.Stage.HitError.Y *= previewMultiplier;

                        if (playfield.Stage.JudgementHitBursts[0].OriginalPosY < 0)
                            for (var i = 0; i < playfield.Stage.JudgementHitBursts.Count; i++)
                                playfield.Stage.JudgementHitBursts[i].OriginalPosY *= previewMultiplier;

                        if (playfield.Stage.OriginalComboDisplayY < 0)
                            playfield.Stage.OriginalComboDisplayY *= previewMultiplier;

                        playfield.Stage.ComboDisplay.Y = playfield.Stage.OriginalComboDisplayY;
                        break;
                    case ScrollDirection.Up:
                        playfield.Container.Alignment = Alignment.TopLeft;
                        playfield.Stage.HitError.Y -= filterPanelHeight + MenuBorder.HEIGHT;
                        for (var i = 0; i < playfield.Stage.JudgementHitBursts.Count; i++)
                            playfield.Stage.JudgementHitBursts[i].OriginalPosY -= filterPanelHeight + MenuBorder.HEIGHT;
                        playfield.Stage.OriginalComboDisplayY -= filterPanelHeight + MenuBorder.HEIGHT;

                        if (playfield.Stage.HitError.Y < 0)
                            playfield.Stage.HitError.Y *= previewMultiplier;

                        if (playfield.Stage.JudgementHitBursts[0].OriginalPosY < 0)
                            for (var i = 0; i < playfield.Stage.JudgementHitBursts.Count; i++)
                                playfield.Stage.JudgementHitBursts[i].OriginalPosY *= previewMultiplier;

                        if (playfield.Stage.OriginalComboDisplayY < 0)
                            playfield.Stage.OriginalComboDisplayY *= previewMultiplier;

                        playfield.Stage.ComboDisplay.Y = playfield.Stage.OriginalComboDisplayY;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                ShowTestPlayPrompt();
                CreateSeekBar(e.Input.Qua, playfield);
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (LoadedGameplayScreen != null)
            {
                if (e.OldValue != null)
                    e.OldValue.Qua = null;

                TestPlayPrompt.Parent = null;
                LoadedGameplayScreen.Ruleset.Playfield.Container.Parent = null;
                LoadedGameplayScreen?.Destroy();

                SeekBar?.Destroy();
            }

            RunLoadTask();
        }

        /// <summary>
        /// </summary>
        private void UpdateGameplayScreen(GameTime gameTime)
        {
            if (LoadedGameplayScreen != null && LoadedGameplayScreen.IsDisposed)
                return;

            if (MapManager.Selected.Value?.Qua == null)
                return;

            try
            {
                // Handle seeking when the track reloads
                if (AudioEngine.Track != TrackInPreviousFrame)
                {
                    if (SeekBar != null)
                        SeekBar.Track = AudioEngine.Track;

                    LoadedGameplayScreen?.HandleReplaySeeking();
                }

                if (ActiveLeftPanel.Value == SelectContainerPanel.MapPreview)
                    LoadedGameplayScreen?.HandleAutoplayTabInput(gameTime);

                LoadedGameplayScreen?.Update(gameTime);
                IsPlayTesting.Value = !LoadedGameplayScreen?.InReplayMode ?? false;

                if (LoadedGameplayScreen != null)
                {
                    var track = Track ?? AudioEngine.Track;
                    LoadedGameplayScreen.IsPaused = track.IsPaused || track.IsStopped;
                }
            }
            catch (Exception)
            {
                // ignored
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLeftPanelChanged(object sender, BindableValueChangedEventArgs<SelectContainerPanel> e)
        {
            if (e.Value != SelectContainerPanel.MapPreview)
                return;

            ShowTestPlayPrompt();
        }

        /// <summary>
        /// </summary>
        protected void RunLoadTask()
        {
            Wheel.ClearAnimations();
            Wheel.FadeTo(1, Easing.Linear, 150);

            LoadGameplayScreenTask.Run(MapManager.Selected.Value, DelayTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSkinLoaded(object sender, SkinReloadedEventArgs e) => RunLoadTask();

        /// <summary>
        /// </summary>
        private void CreateTestPlayPrompt()
        {
            TestPlayPrompt = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Press [TAB] to toggle play testing", 22)
            {
                Alignment = Alignment.TopCenter,
                Y = 175,
                DestroyIfParentIsNull = false
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e)
        {
            var isNone = e.ChangedMods.HasFlag(ModIdentifier.None);

            if (!isNone)
            {
                if (e.ChangedMods.HasFlag(ModIdentifier.Autoplay) || e.ChangedMods.HasFlag(ModIdentifier.Coop) || e.ChangedMods.HasFlag(ModIdentifier.Randomize))
                    return;
            }

            var isSpeedMod = e.ChangedMods >= ModIdentifier.Speed05X && e.ChangedMods <= ModIdentifier.Speed20X ||
                             e.ChangedMods >= ModIdentifier.Speed055X && e.ChangedMods <= ModIdentifier.Speed095X ||
                             e.ChangedMods >= ModIdentifier.Speed105X && e.ChangedMods <= ModIdentifier.Speed195X || isNone;

            if (isSpeedMod)
            {
                ScheduleUpdate(() =>
                {
                    CreateSeekBar(LoadedGameplayScreen?.Map, (GameplayPlayfieldKeys) LoadedGameplayScreen?.Ruleset?.Playfield, false);
                });
                return;
            }

            // Reload the entire
            RunLoadTask();
        }

        /// <summary>
        ///     Handles creating and initializing the seek bar that displays the map's difficulty
        /// </summary>
        private void CreateSeekBar(Qua qua, GameplayPlayfieldKeys playfield, bool animate = true)
        {
            if (!HasSeekBar)
                return;

            var oldSeekBar = SeekBar;

            if (playfield == null || qua == null)
            {
                oldSeekBar?.Destroy();
                return;
            }

            var stageRightWidth = (int) MathHelper.Clamp(playfield.Stage.StageRight.Width, 0, 8);

            SeekBar = new DifficultySeekBar(qua, ModManager.Mods, new ScalableVector2(56, Height), 200)
            {
                Alignment = Alignment.BotRight,
                X =  stageRightWidth - 8,
                Tint = ColorHelper.HexToColor("#181818"),
                SetChildrenAlpha = true,
            };

            if (animate)
                SeekBar.Alpha = 0;

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = SeekBar,
                Size = new ScalableVector2(2, SeekBar.Height),
                Tint = ColorHelper.HexToColor("#808080")
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = SeekBar,
                Alignment = Alignment.BotRight,
                Size = new ScalableVector2(2, SeekBar.Height),
                Tint = ColorHelper.HexToColor("#808080")
            };

            SeekBar.AudioSeeked += (o, args) => RefreshScreen();

            if (qua != MapManager.Selected.Value.Qua)
            {
                oldSeekBar?.Destroy();
                SeekBar.Destroy();
                return;
            }

            AddScheduledUpdate(() =>
            {
                oldSeekBar?.Destroy();
                SeekBar.Parent = this;

                if (animate)
                {
                    SeekBar.FadeTo(1, Easing.Linear, 300);

                    playfield.Container.X -= SeekBar.X;
                    playfield.Container.X -= SeekBar.Width / 3f;

                    if (qua.HasScratchKey)
                        SeekBar.X += SkinManager.Skin.Keys[qua.Mode].ScratchLaneSize / 4f;

                    playfield.Container.X += 2;
                }

                TestPlayPrompt.X = -SeekBar.Width / 2f + 2;
            });
        }

        /// <summary>
        /// </summary>
        private void ShowTestPlayPrompt()
        {
            if (ShownTestPlayPrompt || ActiveLeftPanel.Value != SelectContainerPanel.MapPreview)
                return;

            if (LoadedGameplayScreen == null)
                return;

            TestPlayPrompt.DestroyIfParentIsNull = false;
            TestPlayPrompt.Parent = this;
            TestPlayPrompt.Alpha = 0;

            if (!ShownTestPlayPrompt)
            {
                TestPlayPrompt.FadeTo(1, Easing.Linear, 300);
                TestPlayPrompt.Wait(3000);
                TestPlayPrompt.FadeTo(0, Easing.Linear, 300);

                ShownTestPlayPrompt = true;
            }
        }

        /// <summary>
        /// </summary>
        protected void RefreshScreen()
        {
            if (LoadedGameplayScreen == null)
                return;

            if (LoadedGameplayScreen.InReplayMode)
                LoadedGameplayScreen.HandleReplaySeeking();
            else
            {
                var hitobjectManager = (HitObjectManagerKeys) LoadedGameplayScreen.Ruleset.HitObjectManager;
                hitobjectManager.HandleSkip();
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnTrackSeeked(object sender, TrackSeekedEventArgs e) => RefreshScreen();
    }
}
