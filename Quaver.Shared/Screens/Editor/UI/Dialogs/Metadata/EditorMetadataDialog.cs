/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata
{
    public class EditorMetadataDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        private EditorMetadataChanger MetadataChanger { get; set; }

        /// <summary>
        /// </summary>
        private bool IsClosing { get; set; }

        /// <summary>
        /// </summary>
        public EditorMetadataDialog(EditorScreen screen) : base(0)
        {
            Screen = screen;
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 0.75f, 200));
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!GraphicsHelper.RectangleContains(MetadataChanger.ScreenRectangle,
                        MouseManager.CurrentState.Position) && !IsClosing)
                {
                    Close();
                    IsClosing = true;
                }
            };
        }

        /// <summary>
        /// </summary>
        public sealed override void CreateContent() => CreateMetadataChanger();

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape) && !IsClosing)
            {
                Close();
                IsClosing = true;
            }
        }

        /// <summary>
        /// </summary>
        private void CreateMetadataChanger()
        {
            MetadataChanger = new EditorMetadataChanger(this)
            {
                Parent = Container,
                Alignment = Alignment.MidLeft
            };

            MetadataChanger.X = -MetadataChanger.Width;

            MetadataChanger.MoveToX(0, Easing.OutQuint, 600);
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        public void Close()
        {
            if (IsClosing)
                return;

            IsClosing = true;
            MetadataChanger.Animations.Clear();
            MetadataChanger.MoveToX(-MetadataChanger.Width - 2, Easing.OutQuint, 400);

            Animations.Clear();
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0f, 200));
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }
    }
}
