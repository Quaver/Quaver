/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Skinning;

namespace Quaver.Shared.Screens.Gameplay.UI
{
    public class KeysPerSecond : NumberDisplay
    {
        /// <summary>
        ///     The list of all the click times.
        /// </summary>
        private List<double> Clicks { get; } = new List<double>();

        /// <summary>
        ///     The keys per second.
        /// </summary>
        private int Kps => Clicks.Count;

        /// <summary>
        ///     The amount of time
        /// </summary>
        private double Time { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="startingValue"></param>
        /// <param name="imageScale"></param>
        /// <param name="position"></param>
        internal KeysPerSecond(NumberDisplayType type, string startingValue, Vector2 imageScale, float position)
            : base(type, startingValue, imageScale, position)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Time += gameTime.ElapsedGameTime.TotalMilliseconds;

            CalculateKeysPerSecond();
            X = -TotalWidth + SkinManager.Skin.Keys[MapManager.Selected.Value.Mode].KpsDisplayPosX;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Calculates the keys per second every frame.
        /// </summary>
        private void CalculateKeysPerSecond()
        {
            for (var i = 0; i < Clicks.Count; i++)
            {
                if (Clicks[i] <= Time - 1000)
                {
                    Clicks.RemoveAt(i);
                    Value = Kps.ToString();
                }
            }
        }

        /// <summary>
        ///     Adds a click to the KPS times.
        /// </summary>
        public void AddClick() => Clicks.Add(Time);
    }
}
