/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Drawing;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Window;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Shared.Screens.Download.UI.Drawable
{
    public class CurrentlySearchingInterface : Sprite
    {
        /// <summary>
        /// </summary>
        private DownloadScreenView View { get; }

        /// <summary>
        /// </summary>
        private Sprite LoadingWheel { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText Header { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="container"></param>
        public CurrentlySearchingInterface(DownloadScreenView view, DownloadScrollContainer container)
        {
            View = view;
            Size = new ScalableVector2(container.Width, 100);
            Tint = Color.Black;
            Alpha = 0.85f;
            SetChildrenAlpha = false;

            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass),
                Size = new ScalableVector2(18, 18),
                Y = 18,
            };

            Header = new SpriteText(Fonts.Exo2SemiBold, "Searching For Mapsets...", 14)
            {
                Parent = Icon,
                Alignment = Alignment.MidLeft,
                X = Icon.Width + 10
            };

            Icon.X = Width / 2f - Header.Width / 2f - 10 - Icon.Width / 2f;

            CreateLoadingWheel();
            View.SearchBox.IsSearching.ValueChanged += OnIsSearchingValueChanged;
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
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            View.SearchBox.IsSearching.ValueChanged -= OnIsSearchingValueChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateLoadingWheel() => LoadingWheel = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(34, 34),
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

        /// <summary>
        ///     Called when the user is searching/finishes searching.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnIsSearchingValueChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            ClearAnimations();

            var val = e.Value ? 0.85f : 0;
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, val, 150));

            LoadingWheel.ClearAnimations();
            LoadingWheel.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, (e.Value ? 1 : 0), 150));

            Icon.ClearAnimations();
            Icon.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, e.Value ? 1 : 0, 150));

            Header.ClearAnimations();
            Header.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Alpha, e.Value ? 1 : 0, 150));
        }
    }
}