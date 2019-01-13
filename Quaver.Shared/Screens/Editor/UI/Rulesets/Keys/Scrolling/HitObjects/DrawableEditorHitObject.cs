/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects
{
    public class DrawableEditorHitObject : Sprite
    {
        /// <summary>
        /// </summary>
        protected EditorScrollContainerKeys Container { get; }

        /// <summary>
        ///     The HitObjectInfo as defined in the .qua file
        /// </summary>
        public HitObjectInfo Info { get; }

        /// <summary>
        ///     Keeps track of if the object is on screen.
        ///     Separate property because this is handled on another thread.
        /// </summary>
        public bool IsInView { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="info"></param>
        /// <param name="texHead"></param>
        public DrawableEditorHitObject(EditorScrollContainerKeys container, HitObjectInfo info, Texture2D texHead)
        {
            Container = container;
            Info = info;
            Image = texHead;
            Height = (float) (Container.LaneSize - Container.DividerLineWidth * 2) * Image.Height / Image.Width;
            DestroyIfParentIsNull = false;
            SetPositionY();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Don't do anything except draw it to spritebatch.
        ///     It's handled separately
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime) => DrawToSpriteBatch();

        /// <summary>
        ///     Sets the Y position of the HitObject in accordance to the track
        /// </summary>
        public void SetPositionY() => Y = Container.HitPositionY - Info.StartTime * Container.TrackSpeed - Height;

        /// <summary>
        ///     Checks if the object is indeed on-screen and should be drawn.
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckIfOnScreen() => Info.StartTime * Container.TrackSpeed >= Container.TrackPositionY - Container.Height &&
                                                Info.StartTime * Container.TrackSpeed <= Container.TrackPositionY + Container.Height;
    }
}