/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI
{
    public class DownloadingReplayDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        /// </summary>
        private Line TopLine { get; set; }

        /// <summary>
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="backgroundAlpha"></param>
        public DownloadingReplayDialog() : base(0)
        {
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, 0.35f, 200));
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContainingBox();
            CreateTopLine();
            CreateHeader();
            CreateLoadingWheel();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformLoadingWheelRotation();
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
        }

        /// <summary>
        /// </summary>
        private void CreateContainingBox() => ContainingBox = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, 124),
            Alignment = Alignment.MidCenter,
            Tint = Color.Black,
            Alpha = 0.85f,
        };

        /// <summary>
        /// </summary>
        private void CreateTopLine() => TopLine = new Line(new Vector2(WindowManager.Width, ContainingBox.AbsolutePosition.Y),
            Colors.MainAccent, 2)
        {
            Parent = ContainingBox,
            Alpha = 0.75f,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///     Creates the heading text
        /// </summary>
        private void CreateHeader()
        {
            var icon = new Sprite()
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_download_to_storage_drive),
                Size = new ScalableVector2(24, 24),
                Y = 18
            };

            var header = new SpriteText(Fonts.Exo2SemiBold, "Downloading Replay", 16)
            {
                Parent = icon,
                Alignment = Alignment.MidLeft,
                X = icon.Width + 10
            };

            icon.X = WindowManager.Width / 2f - header.Width / 2f - 10 - icon.Width / 2f;
        }

        /// <summary>
        ///     Creates the loading wheel that signifies we're downloading
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = ContainingBox,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(50, 50),
            Image = UserInterface.LoadingWheel,
            Y = 18
        };

        /// <summary>
        ///     Rotates the loading wheel endlessly
        /// </summary>
        private void PerformLoadingWheelRotation()
        {
            if (LoadingWheel.Animations.Count != 0)
                return;

            var rotation = MathHelper.ToDegrees(LoadingWheel.Rotation);
            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Rotation, Easing.Linear, rotation, rotation + 360, 1000));
        }
    }
}
