using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.AutoMod.Issues;
using Quaver.API.Maps.AutoMod.Issues.Audio;
using Quaver.API.Maps.AutoMod.Issues.Autoplay;
using Quaver.API.Maps.AutoMod.Issues.HitObjects;
using Quaver.API.Maps.AutoMod.Issues.Images;
using Quaver.API.Maps.AutoMod.Issues.Map;
using Quaver.API.Maps.AutoMod.Issues.Mapset;
using Quaver.API.Maps.AutoMod.Issues.Metadata;
using Quaver.API.Maps.AutoMod.Issues.ScrollVelocities;
using Quaver.API.Maps.AutoMod.Issues.TimingPoints;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Dialogs.Metadata;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Platform;

namespace Quaver.Shared.Screens.Edit.UI.AutoMods
{
    public class EditorAutoModPanelContainer : Container
    {
        /// <summary>
        /// </summary>
        public Bindable<bool> IsActive { get; } = new Bindable<bool>(false);

        private EditScreen Screen { get; }

        public EditorAutoModPanel Panel { get; }

        private ScalableVector2 PanelPosition { get; set; }

        private bool DialogsOpen { get; set; }

        /// <summary>
        ///     Methods that will run for each issue type when it is clicked.
        /// </summary>
        private Dictionary<Type, Action<AutoModIssue>> IssueMethods { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorAutoModPanelContainer(EditScreen screen)
        {
            Screen = screen;

            var mapset = new List<Qua> {Screen.WorkingMap};

            Screen.Map.Mapset.Maps.ForEach(x =>
            {
                if (x == Screen.Map)
                    return;

                try
                {
                    mapset.Add(x.LoadQua());
                }
                catch (Exception e)
                {
                    // This can happen if e.g. a mapset in the database has a missing .qua file.
                    // Just ignore these, don't prevent editor from loading.
                    Logger.Warning($"Couldn't load difficulty `{x.DifficultyName}` for automod: {e}", LogType.Runtime);
                }
            });

            Panel = new EditorAutoModPanel(Screen.WorkingMap, mapset, this)
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            ChangePanelPosition();
            InitializeIssueClickMethods();

            IsActive.ValueChanged += OnActiveChanged;
            Panel.IssueClicked += OnIssueClicked;
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (IsActive.Value && Panel.Position.X.Value > 0)
                PanelPosition = Panel.Position;

            if (DialogManager.Dialogs.Count > 0 || DialogManager.Dialogs.Count > 0 != DialogsOpen)
                ChangePanelPosition();

            DialogsOpen = DialogManager.Dialogs.Count > 0;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            if (!IsActive.Value)
                return;

            base.Draw(gameTime);
        }

        /// <summary>
        /// </summary>
        public override void Dispose()
        {
            IsActive.Dispose();

            // Can happen if an exception occurs before Panel is created.
            if (Panel != null)
                Panel.IssueClicked -= OnIssueClicked;

            base.Dispose();
        }

        /// <summary>
        ///     Switches the position of the panel depending on if it is active or not.
        /// </summary>
        private void ChangePanelPosition()
        {
            if (!IsActive.Value || DialogManager.Dialogs.Count > 0)
                Panel.Position = new ScalableVector2(-10000, 0);
            else
                Panel.Position = PanelPosition;
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnAudioBitrate(AutoModIssueAudioBitrate issue)
        {
            if (Screen.Map.Game != MapGame.Quaver)
                return;

            var path = MapManager.GetAudioPath(Screen.Map);

            if (string.IsNullOrEmpty(path))
                return;

            Utils.NativeUtils.HighlightInFileManager(path);
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnAudioFormat(AutoModIssueAudioFormat issue)
        {
            if (Screen.Map.Game != MapGame.Quaver)
                return;

            var path = MapManager.GetAudioPath(Screen.Map);

            if (string.IsNullOrEmpty(path))
                return;

            Utils.NativeUtils.HighlightInFileManager(path);
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnAutoplayFailure(AutoModIssueAutoplayFailure issue) => Logger.Debug($"Autoplay Failure", LogType.Runtime, false);

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnImageResolution(AutoModIssueImageResolution issue) => HighlightImageFile(issue.Item);

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnImageTooLarge(AutoModIssueImageTooLarge issue) => HighlightImageFile(issue.Item);

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnNoBackground(AutoModIssueNoBackground issue)
            => NotificationManager.Show(NotificationLevel.Info, "To add a background image, drag a .png or .jpg into the window.");

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnExcessiveBreakTime(AutoModIssueExcessiveBreakTime issue)
        {
            Screen.SelectedHitObjects.Clear();
            Screen.SelectedHitObjects.Add(issue.HitObject);

            try
            {
                AudioEngine.Track.Seek(issue.HitObject.StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnObjectAfterAudioEnd(AutoModIssueObjectAfterAudioEnd issue)
        {
            Screen.SelectedHitObjects.Clear();
            Screen.SelectedHitObjects.Add(issue.HitObject);

            try
            {
                AudioEngine.Track.Seek(AudioEngine.Track.Length);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnObjectBeforeStart(AutoModIssueObjectBeforeStart issue)
        {
            Screen.SelectedHitObjects.Clear();
            Screen.SelectedHitObjects.Add(issue.HitObject);

            try
            {
                AudioEngine.Track.Seek(0);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnObjectInAllColumns(AutoModIssueObjectInAllColumns issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnOverlappingObjects(AutoModIssueOverlappingObjects issue)
        {
            Screen.SelectedHitObjects.Clear();
            Screen.SelectedHitObjects.AddRange(issue.HitObjects.ToList());

            try
            {
                AudioEngine.Track.Seek(issue.HitObjects.First().StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnShortLongNote(AutoModIssueShortLongNote issue)
        {
            Screen.SelectedHitObjects.Clear();
            Screen.SelectedHitObjects.Add(issue.HitObject);

            try
            {
                AudioEngine.Track.Seek(issue.HitObject.StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnMapLength(AutoModIssueMapLength issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnMapPreviewPoint(AutoModIssuePreviewPoint issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnMapsetSpreadLength(AutoModIssueMapsetSpreadLength issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnMismatchingMetadata(AutoModIssueMismatchingMetadata issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnMultiModeDiffName(AutoModIssueMultiModeDiffName issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnNonCommaSeparatedTags(AutoModIssueNonCommaSeparatedTags issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnNonStandardizedMetadata(AutoModIssueNonStandardizedMetadata issue)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnNonRomanized(AutoModIssueNonRomanized issue) => DialogManager.Show(new EditorMetadataDialog(Screen));

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnScrollVelocityAfterEnd(AutoModIssueScrollVelocityAfterEnd issue)
        {
            try
            {
                AudioEngine.Track.Seek(AudioEngine.Track.Length);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnScrollVelocityOverlap(AutoModIssueScrollVelocityOverlap issue)
        {
            try
            {
                AudioEngine.Track.Seek(issue.ScrollVelocities.First().StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnTimingPointOverlap(AutoModeIssueTimingPointOverlap issue)
        {
            try
            {
                AudioEngine.Track.Seek(issue.TimingPoints.First().StartTime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="issue"></param>
        private void OnTimingPointAfterAudioEnd(AutoModIssueTimingPointAfterAudioEnd issue)
        {
            try
            {
                AudioEngine.Track.Seek(AudioEngine.Track.Length);
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
        private void OnIssueClicked(object sender, EditorAutoModIssueClicked e)
        {
            if (IssueMethods.ContainsKey(e.Issue.GetType()))
                IssueMethods[e.Issue.GetType()]?.Invoke(e.Issue);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnActiveChanged(object sender, BindableValueChangedEventArgs<bool> e) => ChangePanelPosition();

        /// <summary>
        ///     When an issue is clicked, this initializes which method will run
        /// </summary>
        private void InitializeIssueClickMethods() => IssueMethods = new Dictionary<Type, Action<AutoModIssue>>
        {
            {typeof(AutoModIssueAudioBitrate), e => OnAudioBitrate((AutoModIssueAudioBitrate)e)},
            {typeof(AutoModIssueAudioFormat), e => OnAudioFormat((AutoModIssueAudioFormat)e)},
            {typeof(AutoModIssueAutoplayFailure), e => OnAutoplayFailure((AutoModIssueAutoplayFailure)e)},
            {typeof(AutoModIssueImageResolution), e => OnImageResolution((AutoModIssueImageResolution)e)},
            {typeof(AutoModIssueImageTooLarge), e => OnImageTooLarge((AutoModIssueImageTooLarge)e)},
            {typeof(AutoModIssueNoBackground), e => OnNoBackground((AutoModIssueNoBackground)e)},
            {typeof(AutoModIssueExcessiveBreakTime), e => OnExcessiveBreakTime((AutoModIssueExcessiveBreakTime)e)},
            {typeof(AutoModIssueObjectAfterAudioEnd), e => OnObjectAfterAudioEnd((AutoModIssueObjectAfterAudioEnd)e)},
            {typeof(AutoModIssueObjectBeforeStart), e => OnObjectBeforeStart((AutoModIssueObjectBeforeStart)e)},
            {typeof(AutoModIssueObjectInAllColumns), e => OnObjectInAllColumns((AutoModIssueObjectInAllColumns)e)},
            {typeof(AutoModIssueOverlappingObjects), e => OnOverlappingObjects((AutoModIssueOverlappingObjects)e)},
            {typeof(AutoModIssueShortLongNote), e => OnShortLongNote((AutoModIssueShortLongNote)e)},
            {typeof(AutoModIssueMapLength), e => OnMapLength((AutoModIssueMapLength)e)},
            {typeof(AutoModIssuePreviewPoint), e => OnMapPreviewPoint((AutoModIssuePreviewPoint)e)},
            {typeof(AutoModIssueMapsetSpreadLength), e => OnMapsetSpreadLength((AutoModIssueMapsetSpreadLength)e)},
            {typeof(AutoModIssueMismatchingMetadata), e => OnMismatchingMetadata((AutoModIssueMismatchingMetadata)e)},
            {typeof(AutoModIssueMultiModeDiffName), e => OnMultiModeDiffName((AutoModIssueMultiModeDiffName)e)},
            {typeof(AutoModIssueNonRomanized), e => OnNonRomanized((AutoModIssueNonRomanized)e)},
            {typeof(AutoModIssueScrollVelocityAfterEnd), e => OnScrollVelocityAfterEnd((AutoModIssueScrollVelocityAfterEnd)e)},
            {typeof(AutoModIssueScrollVelocityOverlap), e => OnScrollVelocityOverlap((AutoModIssueScrollVelocityOverlap)e)},
            {typeof(AutoModeIssueTimingPointOverlap), e => OnTimingPointOverlap((AutoModeIssueTimingPointOverlap)e)},
            {typeof(AutoModIssueTimingPointAfterAudioEnd), e => OnTimingPointAfterAudioEnd((AutoModIssueTimingPointAfterAudioEnd)e)},
            {typeof(AutoModIssueNonCommaSeparatedTags), e => OnNonCommaSeparatedTags((AutoModIssueNonCommaSeparatedTags)e)},
            {typeof(AutoModIssueNonStandardizedMetadata), e => OnNonStandardizedMetadata((AutoModIssueNonStandardizedMetadata)e)},
        };

        /// <summary>
        /// </summary>
        private void HighlightImageFile(string item)
        {
            if (Screen.Map.Game != MapGame.Quaver)
                return;

            string path = MapManager.GetBackgroundPath(Screen.Map);
            if (item.ToLower() == "banner")
                path = MapManager.GetBannerPath(Screen.Map);

            if (string.IsNullOrEmpty(path))
                return;

            Utils.NativeUtils.HighlightInFileManager(path);
        }
    }
}