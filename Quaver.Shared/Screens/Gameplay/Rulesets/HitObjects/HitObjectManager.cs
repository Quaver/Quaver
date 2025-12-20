/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Skinning;
using Wobble;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class HitObjectManager
    {
        /// <summary>
        ///     If the map is complete and the results screen should show.
        /// </summary>
        public abstract bool IsComplete { get; }

        /// <summary>
        ///     The next object in the pool. Used for skipping.
        /// </summary>
        public abstract HitObjectInfo NextHitObject { get; }

        /// <summary>
        ///     Used to determine if the player is currently on a break in the song.
        /// </summary>
        public abstract bool OnBreak { get; }

        /// <summary>
        ///     The list of possible beat snaps.
        /// </summary>
        private static int[] BeatSnaps { get; } = { 48, 24, 16, 12, 8, 6, 4, 3 };

        /// <summary>
        ///     Precomputed beat snap indices at hitobject times
        /// </summary>
        public Dictionary<int, int> SnapIndices { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="map"></param>
        public HitObjectManager(Qua map)
        {
            SnapIndices = new Dictionary<int, int>();

            foreach (var hitObject in map.HitObjects)
            {
                if (!SnapIndices.ContainsKey(hitObject.StartTime))
                    SnapIndices.Add(hitObject.StartTime, GetBeatSnap(hitObject, hitObject.GetTimingPoint(map.TimingPoints)));
            }
        }

        /// <summary>
        ///     Updates all the containing HitObjects
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        public virtual void Destroy()
        {
        }

        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObjectPool.
        /// </summary>
        public static void PlayObjectHitSounds(HitObjectInfo hitObject, SkinStore skin = null, int volume = -1)
        {
            // Default to fallback skin if one wasn't provided
            if (skin == null)
                skin = SkinManager.Skin;

            var game = GameBase.Game as QuaverGame;

            // Disable hitsounds for left panel screens if the map preview isnt active
            if (game?.CurrentScreen is IHasLeftPanel screen)
            {
                if (screen.ActiveLeftPanel.Value != SelectContainerPanel.MapPreview)
                    return;
            }

            // Normal
            if (hitObject.HitSound == 0 || (HitSounds.Normal & hitObject.HitSound) != 0)
            {
                var chan = skin?.SoundHit?.CreateChannel();

                if (chan != null && volume != -1)
                    chan.Volume = volume;

                chan?.Play();
            }

            // Clap
            if ((HitSounds.Clap & hitObject.HitSound) != 0)
            {
                var chan = skin?.SoundHitClap?.CreateChannel();

                if (chan != null && volume != -1)
                    chan.Volume = volume;

                chan?.Play();
            }

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSound) != 0)
            {
                var chan = skin?.SoundHitWhistle?.CreateChannel();

                if (chan != null && volume != -1)
                    chan.Volume = volume;

                chan?.Play();
            }

            // Finish
            if ((HitSounds.Finish & hitObject.HitSound) != 0)
            {
                var chan = skin?.SoundHitFinish?.CreateChannel();

                if (chan != null && volume != -1)
                    chan.Volume = volume;

                chan?.Play();
            }
        }

        /// <summary>
        ///     Plays the correct keysounds based on the note index of the HitObjectPool.
        /// </summary>
        public static void PlayObjectKeySounds(HitObjectInfo hitObject)
        {
            var game = GameBase.Game as QuaverGame;

            // Disable hitsounds for left panel screens if the map preview isnt active
            if (game?.CurrentScreen is IHasLeftPanel screen)
            {
                if (screen.ActiveLeftPanel.Value != SelectContainerPanel.MapPreview)
                    return;
            }

            foreach (var keySound in hitObject.KeySounds)
                CustomAudioSampleCache.Play(keySound.Sample - 1, keySound.Volume);
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timingPoint"></param>
        /// <returns></returns>
        public static int GetBeatSnap(HitObjectInfo info, TimingPointInfo timingPoint)
        {
            // Get beat length
            var pos = info.StartTime - timingPoint.StartTime;
            var beatlength = 60000 / timingPoint.Bpm;

            // Calculate Note's snap index
            var index = Math.Round(48 * pos / beatlength, MidpointRounding.AwayFromZero);

            // Return Color of snap index
            for (var i = 0; i < 8; i++)
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (index % BeatSnaps[i] == 0)
                    return i;
            }

            // If it's not snapped to 1/16 or less, return 1/48 snap color
            return 8;
        }
    }
}
