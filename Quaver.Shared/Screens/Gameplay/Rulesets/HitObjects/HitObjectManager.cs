/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.HitObjects
{
    public abstract class HitObjectManager
    {
        /// <summary>
        ///     The number of objects left in the map
        ///     (Has to be implemented per game mode because pooling may be different.)
        /// </summary>
        public abstract int ObjectsLeft { get; }

        /// <summary>
        ///     The next object in the pool. Used for skipping.
        /// </summary>
        public abstract HitObjectInfo NextHitObject { get; }

        /// <summary>
        ///     Used to determine if the player is currently on a break in the song.
        /// </summary>
        public abstract bool OnBreak { get; }

        /// <summary>
        ///     If there are no more objects and the map is complete.
        /// </summary>
        public bool IsComplete => ObjectsLeft == 0;

        /// <summary>
        ///     The list of possible beat snaps.
        /// </summary>
        private static int[] BeatSnaps { get; } = { 48, 24, 16, 12, 8, 6, 4, 3 };

        /// <summary>
        ///     The beat snap index of each object in the map.
        /// </summary>
        public Dictionary<HitObjectInfo, int> SnapIndices { get; set; }

        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="map"></param>
        public HitObjectManager(Qua map)
        {
            SnapIndices = new Dictionary<HitObjectInfo, int>();

            foreach (var hitObject in map.HitObjects)
                SnapIndices.Add(hitObject, GetBeatSnap(hitObject, hitObject.GetTimingPoint(map.TimingPoints)));
        }

        /// <summary>
        ///     Updates all the containing HitObjects
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void Update(GameTime gameTime);

        /// <summary>
        ///     Plays the correct hitsounds based on the note index of the HitObjectPool.
        /// </summary>
        public static void PlayObjectHitSounds(HitObjectInfo hitObject)
        {
            if (!ConfigManager.EnableHitsounds.Value)
                return;

            // Normal
            if (hitObject.HitSound == 0 || (HitSounds.Normal & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHit.CreateChannel().Play();

            // Clap
            if ((HitSounds.Clap & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitClap.CreateChannel().Play();

            // Whistle
            if ((HitSounds.Whistle & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitWhistle.CreateChannel().Play();

            // Finish
            if ((HitSounds.Finish & hitObject.HitSound) != 0)
                SkinManager.Skin.SoundHitFinish.CreateChannel().Play();
        }

        /// <summary>
        ///     Returns color of note beatsnap
        /// </summary>
        /// <param name="info"></param>
        /// <param name="timingPoint"></param>
        /// <returns></returns>
        public static int GetBeatSnap(HitObjectInfo info, TimingPointInfo timingPoint)
        {
            // Add 2ms offset buffer space to offset and get beat length
            var pos = info.StartTime - timingPoint.StartTime + 0;
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
