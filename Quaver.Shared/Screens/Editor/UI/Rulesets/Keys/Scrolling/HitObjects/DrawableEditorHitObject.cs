/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
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

        /// <summary>
        ///     The color when the object is shown as selected.
        /// </summary>
        protected Color SelectedColor { get; } = Color.LimeGreen;

        /// <summary>
        /// </summary>
        protected Sprite SelectionSprite { get; private set; }

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

            CreateSelectionSprite();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Don't do anything except draw it to spritebatch.
        ///     It's handled separately
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            DrawToSpriteBatch();

            if (SelectionSprite.Visible)
                SelectionSprite.DrawToSpriteBatch();
        }

        /// <summary>
        ///     Sets the Y position of the HitObject in accordance to the track
        /// </summary>
        public void SetPositionY()
        {
            Y = Container.HitPositionY - Info.StartTime * Container.TrackSpeed - Height;

            if (ConfigManager.EditorHitObjectsMidpointAnchored.Value)
                Y += Height / 2;
        }

        /// <summary>
        ///     Checks if the object is indeed on-screen and should be drawn.
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckIfOnScreen() => Info.StartTime * Container.TrackSpeed >= Container.TrackPositionY - Container.Height &&
                                                Info.StartTime * Container.TrackSpeed <= Container.TrackPositionY + Container.Height;

        /// <summary>
        ///     Determines if the HitObject is hovered.
        /// </summary>
        /// <param name="mousePos"></param>
        /// <returns></returns>
        public virtual bool IsHovered(Vector2 mousePos) => ScreenRectangle.Contains(mousePos);

        /// <summary>
        ///    Resets the tint of the long note and makes it appear as if it is active.
        /// </summary>
        public virtual void AppearAsActive()
        {
            var view = (EditorScreenView) Container.Ruleset.Screen.View;
            Tint = ColorHelper.ToXnaColor(view.LayerCompositor.ScrollContainer.AvailableItems[Info.EditorLayer].GetColor());

            SelectionSprite.Visible = false;
        }

        /// <summary>
        ///     Displays the object as selected.
        /// </summary>
        public virtual void AppearAsSelected()
        {
            SelectionSprite.Size = new ScalableVector2(Width, Width);
            SelectionSprite.Visible = true;
        }

        /// <summary>
        ///     Makes the object appear as if it is hidden in the layer
        /// </summary>
        public virtual void AppearAsHiddenInLayer()
        {
            Tint = new Color(40, 40, 40);
            SelectionSprite.Visible = false;
        }

        /// <summary>
        ///
        /// </summary>
        private void CreateSelectionSprite() => SelectionSprite = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Image = UserInterface.BlankBox,
            Tint = Colors.MainAccent,
            Alpha = 0.70f,
            Visible = false
        };
    }
}
