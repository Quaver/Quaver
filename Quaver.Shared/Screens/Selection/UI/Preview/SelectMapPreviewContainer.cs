using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Replays;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Menu.Border;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Skinning;
using Wobble.Audio.Tracks;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
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
        private GameplayScreen LoadedGameplayScreen { get; set; }

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
        /// </summary>
        public SelectMapPreviewContainer(Bindable<bool> isPlayTesting, Bindable<SelectContainerPanel> activeLeftPanel, int height)
        {
            IsPlayTesting = isPlayTesting;
            ActiveLeftPanel = activeLeftPanel;
            Size = new ScalableVector2(564, height);
            Alpha = 0f;

            LoadGameplayScreenTask = new TaskHandler<Map, GameplayScreen>(LoadGameplayScreen);
            LoadGameplayScreenTask.OnCompleted += OnLoadedGameplayScreen;

            CreateLoadingWheel();
            CreateTestPlayPrompt();

            LoadGameplayScreenTask.Run(MapManager.Selected.Value, 400);
            MapManager.Selected.ValueChanged += OnMapChanged;
        }

        private void CreateTestPlayPrompt()
        {
            TestPlayPrompt = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack),
                "Press [TAB] to toggle autoplay on/off", 22)
            {
                Alignment = Alignment.TopCenter,
                Y = 175,
                DestroyIfParentIsNull = false
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            UpdateGameplayScreen(gameTime);

            TrackInPreviousFrame = AudioEngine.Track;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;

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

            lock (LoadedGameplayScreen)
            {
                TestPlayPrompt.Parent = null;
                LoadedGameplayScreen.Ruleset.Playfield.Container.Parent = null;
                LoadedGameplayScreen.Destroy();
                LoadedGameplayScreen = null;

                return HandleLoadGameplayScreen(map, token);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private GameplayScreen HandleLoadGameplayScreen(Map map, CancellationToken token)
        {
            var qua = map.LoadQua();
            map.Qua = qua;

            var autoplay = Replay.GeneratePerfectReplayKeys(new Replay(qua.Mode, "Autoplay", 0, map.Md5Checksum), qua);

            var gameplay = new GameplayScreen(qua, map.Md5Checksum, new List<Score>(), autoplay, true, 0,
                false, null, null, true);

            if (token.IsCancellationRequested)
                gameplay.Destroy();

            gameplay.HandleReplaySeeking();

            return gameplay;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnLoadedGameplayScreen(object sender, TaskCompleteEventArgs<Map, GameplayScreen> e)
        {
            if (MapManager.Selected.Value != e.Input)
                return;

            MapManager.Selected.Value.Qua = e.Result.Map;
            LoadedGameplayScreen = e.Result;

            ScheduleUpdate(() =>
            {
                var playfield = (GameplayPlayfieldKeys) LoadedGameplayScreen.Ruleset.Playfield;

                playfield.Stage.HealthBar.Visible = false;

                playfield.Container.Parent = this;
                playfield.Container.Size = Size;
                playfield.Container.X = 0;

                var scroll = LoadedGameplayScreen.Map.Mode == GameMode.Keys4
                    ? ConfigManager.ScrollDirection4K
                    : ConfigManager.ScrollDirection7K;

                var skin = SkinManager.Skin.Keys[e.Result.Map.Mode];

                const int filterPanelHeight = 88;

                switch (scroll.Value)
                {
                    case ScrollDirection.Down:
                    case ScrollDirection.Split:
                        playfield.Container.Alignment = Alignment.BotLeft;
                        playfield.Container.Y = -MenuBorder.HEIGHT - Y;
                        break;
                    case ScrollDirection.Up:
                        playfield.Container.Alignment = Alignment.TopLeft;
                        playfield.Stage.HitError.Y = -skin.HitErrorPosY - MenuBorder.HEIGHT;
                        playfield.Stage.OriginalComboDisplayY = -skin.ComboPosY - MenuBorder.HEIGHT - filterPanelHeight - Y;
                        playfield.Stage.ComboDisplay.Y = playfield.Stage.OriginalComboDisplayY;
                        playfield.Stage.JudgementHitBurst.OriginalPosY = skin.JudgementBurstPosY + MenuBorder.HEIGHT - filterPanelHeight - Y;
                        playfield.Stage.JudgementHitBurst.Y = playfield.Stage.JudgementHitBurst.OriginalPosY;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                TestPlayPrompt.Parent = this;
                TestPlayPrompt.Alpha = 0;

                if (!ShownTestPlayPrompt)
                {
                    TestPlayPrompt.FadeTo(1, Easing.Linear, 300);
                    TestPlayPrompt.Wait(2500);
                    TestPlayPrompt.FadeTo(0, Easing.Linear, 300);

                    ShownTestPlayPrompt = true;
                }
            });
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e)
        {
            if (e.OldValue != null)
                e.OldValue.Qua = null;

            if (LoadedGameplayScreen != null)
            {
                TestPlayPrompt.Parent = null;
                LoadedGameplayScreen.Ruleset.Playfield.Container.Parent = null;
                LoadedGameplayScreen.Destroy();
            }

            LoadGameplayScreenTask.Run(MapManager.Selected.Value, 400);
        }

        /// <summary>
        /// </summary>
        private void UpdateGameplayScreen(GameTime gameTime)
        {
            if (ActiveLeftPanel.Value != SelectContainerPanel.MapPreview ||
                LoadedGameplayScreen != null && LoadedGameplayScreen.IsDisposed)
                return;

            try
            {
                // Handle seeking when the track reloads
                if (AudioEngine.Track != TrackInPreviousFrame)
                    LoadedGameplayScreen?.HandleReplaySeeking();

                LoadedGameplayScreen?.Update(gameTime);
                IsPlayTesting.Value = !LoadedGameplayScreen?.InReplayMode ?? false;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}