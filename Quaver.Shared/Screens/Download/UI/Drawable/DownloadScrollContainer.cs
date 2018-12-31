/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Download.UI.Drawable
{
    public class DownloadScrollContainer : ScrollContainer
    {
        /// <summary>
        /// </summary>
        public DownloadScreenView View { get; }

        /// <summary>
        ///     The mapsets that are currently available for download
        /// </summary>
        public List<DownloadableMapset> Mapsets { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public DownloadScrollContainer(DownloadScreenView view): base(new ScalableVector2(900, 635), new ScalableVector2(900, 635))
        {
            View = view;
            Tint = Color.Black;
            Alpha = 0.35f;
            Mapsets = new List<DownloadableMapset>();

            InputEnabled = true;
            Scrollbar.Tint = Color.White;
            Scrollbar.Width = 5;
            Scrollbar.X += 10;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0
                && !View.SearchBox.IsSearching.Value;

            HandleInfiniteScrolling();
            base.Update(gameTime);
        }

        /// <summary>
        ///     Clears all mapsets in the container
        /// </summary>
        public void ClearMapsets()
        {
            ContentContainer.Height = Height;
            ScrollToTop();

            Mapsets.ForEach(x =>
            {
                if (x.Parent == ContentContainer)
                    RemoveContainedDrawable(x);

                x.Destroy();
            });

            Mapsets.Clear();
        }

        /// <summary>
        /// </summary>
        /// <param name="mapsets"></param>
        /// <param name="addToBottom"></param>
        public void AddMapsets(List<DownloadableMapset> mapsets, bool addToBottom)
        {
            Mapsets = addToBottom ? Mapsets.Concat(mapsets).ToList() : mapsets;

            for (var i = 0; i < Mapsets.Count; i++)
            {
                if (Mapsets[i].Parent == ContentContainer)
                    continue;

                Mapsets[i].X = -Mapsets[i].Width;
                Mapsets[i].Y = i * DownloadableMapset.HEIGHT + i * 5;

                if (addToBottom)
                    Mapsets[i].MoveToX(0, Easing.OutQuint, 300 + mapsets.FindIndex(x => x == Mapsets[i]) * 100);
                else
                    Mapsets[i].MoveToX(0, Easing.OutQuint, 300 + i * 75);

                AddContainedDrawable(Mapsets[i]);
            }

            var totalUserHeight = DownloadableMapset.HEIGHT * Mapsets.Count + Mapsets.Count * 5;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;

            if (!addToBottom)
                ScrollToTop();
        }

        /// <summary>
        ///     When a mapset is downloaded and we need to realign them to move it up.
        /// </summary>
        public void RealignMapsets()
        {
            for (var i = 0; i < Mapsets.Count; i++)
            {
                var yAnimation = Mapsets[i].Animations.Find(x => x.Properties == AnimationProperty.Y);

                if (yAnimation != null)
                    Mapsets[i].Animations.Remove(yAnimation);

                Mapsets[i].MoveToY(i * DownloadableMapset.HEIGHT + i * 5, Easing.OutQuint, 500);
            }

            var totalUserHeight = DownloadableMapset.HEIGHT * Mapsets.Count + Mapsets.Count * 5;

            if (totalUserHeight > Height)
                ContentContainer.Height = totalUserHeight;
            else
                ContentContainer.Height = Height;
        }

        /// <summary>
        /// </summary>
        private void ScrollToTop()
        {
            ContentContainer.Animations.Clear();
            ContentContainer.Y = 0;
            PreviousTargetY = 0;
            TargetY = 0;
        }

        /// <summary>
        ///     Handles when the user scrolls all the way down and we want to load more sets.
        /// </summary>
        private void HandleInfiniteScrolling()
        {
            var screen = (DownloadScreen) View.Screen;

            if (ContentContainer.Height - Math.Abs(ContentContainer.Y) - Height > 100 || View.SearchBox.IsSearching.Value ||
                ContentContainer.Height == Height || screen.LastSearchedCount == 0)
                return;

            View.SearchBox.SearchForMapsets(View.SearchBox.SearchBox.RawText, screen.CurrentGameMode, screen.CurrentRankedStatus, true);
        }
    }
}