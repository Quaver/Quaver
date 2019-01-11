using System;
using System.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Discord;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects;
using Quaver.Shared.Screens.Menu;
using Quaver.Shared.Screens.Select;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor
{
    public sealed class EditorScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Editor;

        /// <summary>
        ///    The original map that the user wants to edit.
        /// </summary>
        public Qua OriginalMap { get; }

        /// <summary>
        ///    The version of the map that is currently being worked on.
        /// </summary>
        public Qua WorkingMap { get; }

        /// <summary>
        ///     The game mode/ruleset used for the editor.
        /// </summary>
        public EditorRuleset Ruleset { get; private set; }

        /// <summary>
        ///     The index of the object who had its hitsounds played.
        /// </summary>
        private int HitSoundObjectIndex { get; set; }

        /// <summary>
        /// </summary>
        public EditorScreen(Qua map)
        {
            OriginalMap = map;
            WorkingMap = ObjectHelper.DeepClone(OriginalMap);

            DiscordHelper.Presence.Details = WorkingMap.ToString();
            DiscordHelper.Presence.State = "Editing";
            DiscordHelper.Presence.StartTimestamp = (long) (TimeHelper.GetUnixTimestampMilliseconds() / 1000);
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            if (!LoadAudioTrack())
                return;

            SetHitSoundObjectIndex();
            CreateRuleset();
            View = new EditorScreenView(this);
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PlayHitsounds();

            if (AudioEngine.Track.IsDisposed)
                AudioEngine.LoadCurrentTrack();

            HandleInput(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="gameTime"></param>
        private void HandleInput(GameTime gameTime)
        {
            if (Exiting || DialogManager.Dialogs.Count != 0)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                HandleKeyPressEscape();

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorPausePlay.Value))
                HandleKeyPressSpace();

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorDecreaseAudioRate.Value))
                ChangeAudioPlaybackRate(Direction.Backward);

            if (KeyboardManager.IsUniqueKeyPress(ConfigManager.KeyEditorIncreaseAudioRate.Value))
                ChangeAudioPlaybackRate(Direction.Forward);

            HandleAudioSeeking();
        }

        /// <summary>
        ///     Changes the audio playback rate either up or down.
        /// </summary>
        /// <param name="direction"></param>
        private void ChangeAudioPlaybackRate(Direction direction)
        {
            float targetRate;

            switch (direction)
            {
                case Direction.Forward:
                    targetRate = AudioEngine.Track.Rate + 0.25f;
                    break;
                case Direction.Backward:
                    targetRate = AudioEngine.Track.Rate - 0.25f;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (targetRate <= 0 || targetRate > 2.0f)
            {
                NotificationManager.Show(NotificationLevel.Error, "You cannot change the audio rate this way any further!");
                return;
            }

            var playAfterRateChange = false;

            if (AudioEngine.Track.IsPlaying)
            {
                AudioEngine.Track.Pause();
                playAfterRateChange = true;
            }

            AudioEngine.Track.Rate = targetRate;

            if (Ruleset is EditorRulesetKeys ruleset)
                ruleset.ScrollContainer.ResetObjectPositions();

            if (AudioEngine.Track.IsPaused && playAfterRateChange)
                AudioEngine.Track.Play();

            NotificationManager.Show(NotificationLevel.Info, $"Audio playback rate changed to: {targetRate * 100}%");
        }

        /// <summary>
        /// </summary>
        private void CreateRuleset()
        {
            switch (WorkingMap.Mode)
            {
                case GameMode.Keys4:
                case GameMode.Keys7:
                    Ruleset = new EditorRulesetKeys(this);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        ///     Attempts to load the audio track for the current map.
        ///     If it can't, it'll send the user back to the menu screen.
        /// </summary>
        /// <returns></returns>
        private bool LoadAudioTrack()
        {
            try
            {
                if (AudioEngine.Track != null && AudioEngine.Track.IsPaused)
                    return true;

                AudioEngine.LoadCurrentTrack();
                return true;
            }
            catch (Exception e)
            {
                NotificationManager.Show(NotificationLevel.Error, "Audio track was unable to be loaded for this map.");
                Exit(() => new MenuScreen());
                return false;
            }
        }

        /// <summary>
        /// </summary>
        private void HandleKeyPressEscape() => Exit(() =>
        {
            DiscordHelper.Presence.StartTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);

            if (AudioEngine.Track != null)
                AudioEngine.Track.Rate = 1.0f;

            AudioEngine.Track?.Fade(0, 100);

            return new SelectScreen();
        });

        /// <summary>
        /// </summary>
        private void HandleKeyPressSpace() => PlayPauseTrack();

        /// <summary>
        ///     Handles seeking through the audio whether with the scroll wheel or
        ///     arrow keys
        /// </summary>
        private void HandleAudioSeeking()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
                return;

            // Seek backwards
            if (KeyboardManager.IsUniqueKeyPress(Keys.Left) || MouseManager.CurrentState.ScrollWheelValue >
                MouseManager.PreviousState.ScrollWheelValue)
            {
                AudioEngine.SeekTrackToNearestSnap(WorkingMap, Direction.Backward, 4);
                SetHitSoundObjectIndex();
            }
            // Seek Forwards
            else if (KeyboardManager.IsUniqueKeyPress(Keys.Right) || MouseManager.CurrentState.ScrollWheelValue <
                MouseManager.PreviousState.ScrollWheelValue)
            {

                AudioEngine.SeekTrackToNearestSnap(WorkingMap, Direction.Forward, 4);
                SetHitSoundObjectIndex();
            }
        }

        /// <summary>
        ///     Completely stops the AudioTrack.
        /// </summary>
        public static void StopTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
                return;

            if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();

            AudioEngine.Track.Seek(0);
            AudioEngine.Track.Stop();
        }

        /// <summary>
        ///     Pauses/Plays the AudioTrack.
        /// </summary>
        public void PlayPauseTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
            {
                AudioEngine.LoadCurrentTrack();
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPlaying)
                AudioEngine.Track.Pause();
            else if (AudioEngine.Track.IsPaused)
                AudioEngine.Track.Play();
        }

        /// <summary>
        ///     Restarts the audio track from the beginning
        /// </summary>
        public void RestartTrack()
        {
            if (AudioEngine.Track.IsStopped || AudioEngine.Track.IsDisposed)
            {
                AudioEngine.LoadCurrentTrack();
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPlaying)
            {
                AudioEngine.Track.Pause();
                AudioEngine.Track.Seek(0);
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
            else if (AudioEngine.Track.IsPaused)
            {
                AudioEngine.Track.Seek(0);
                SetHitSoundObjectIndex();

                AudioEngine.Track.Play();
            }
        }

        /// <summary>
        ///     Keeps track of and plays object hitsounds.
        /// </summary>
        private void PlayHitsounds()
        {
            for (var i = HitSoundObjectIndex; i < WorkingMap.HitObjects.Count; i++)
            {
                if (Exiting)
                    return;

                var obj = WorkingMap.HitObjects[i];

                if (AudioEngine.Track.Time >= obj.StartTime)
                {
                    HitObjectManager.PlayObjectHitSounds(obj);
                    HitSoundObjectIndex = i + 1;
                }
                else
                    break;
            }
        }

        /// <summary>
        ///     Sets the hitsounds object index, so we know which object to play sounds for.
        ///     This is generally used when seeking through the map.
        /// </summary>
        public void SetHitSoundObjectIndex()
        {
            HitSoundObjectIndex = WorkingMap.HitObjects.FindLastIndex(x => x.StartTime <= AudioEngine.Track.Time);
            HitSoundObjectIndex++;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Editing, -1, "", (byte) GameMode.Keys4, "", 0);
    }
}