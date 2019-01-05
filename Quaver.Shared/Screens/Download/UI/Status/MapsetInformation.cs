/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Newtonsoft.Json.Linq;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Download.UI.Drawable;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Quaver.Shared.Screens.Select.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Download.UI.Status
{
    public class MapsetInformation : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private DownloadScreenView View { get; }

        /// <summary>
        /// </summary>
        private SpriteText Status { get; set; }

        /// <summary>
        /// </summary>
        private Sprite MapBanner { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText Artist { get; set; }

        /// <summary>
        /// </summary>
        private SpriteText Creator { get; set; }

        /// <summary>
        /// </summary>
        private BorderedTextButton DownloadButton { get; set; }

        /// <summary>
        /// </summary>
        private BorderedTextButton CancelButton { get; set; }

        /// <summary>
        /// </summary>
        private BorderedTextButton ViewMapsetPageButton { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public MapsetInformation(DownloadScreenView view) : base(new ScalableVector2(400, 334), new ScalableVector2(400, 334))
        {
            View = view;
            Size = new ScalableVector2(400, 334);
            Tint = Color.Black;
            Alpha = 0.75f;
            AddBorder(Color.White);

            CreateTextDownloadStatus();
            CreateMapBanner();
            CreateTextTitle();
            CreateTextArtist();
            CreateTextCreator();
            CreateDownloadButton();
            CreateCancelButton();
            CreateViewMapsetPageButton();

            var screen = (DownloadScreen) View.Screen;
            screen.SelectedMapset.ValueChanged += OnSelectedMapsetChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            var screen = (DownloadScreen) View.Screen;

            // ReSharper disable once DelegateSubtraction
            screen.SelectedMapset.ValueChanged -= OnSelectedMapsetChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateTextDownloadStatus() => Status = new SpriteText(Fonts.Exo2Bold, "Mapset Information", 14)
        {
            Parent = this,
            Y = 15,
            X = 15,
            Tint = Colors.MainAccent
        };

        /// <summary>
        /// </summary>
        private void CreateMapBanner() => MapBanner = new Sprite
        {
            Parent = this,
            Alignment = Alignment.TopCenter,
            Y = Status.Y + Status.Height + 10,
            Size = new ScalableVector2(368, 80),
            SetChildrenAlpha = true,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateTextTitle() => Title = new SpriteText(Fonts.SourceSansProBold, "Title", 13)
        {
            Parent = this,
            X = Status.X,
            Y = MapBanner.Y + MapBanner.Height + 10,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateTextArtist() => Artist = new SpriteText(Fonts.SourceSansProBold, "Artist", 13)
        {
            Parent = this,
            X = Status.X,
            Y = Title.Y + Title.Height + 2,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateTextCreator() => Creator = new SpriteText(Fonts.SourceSansProSemiBold, "Created By: ", 13)
        {
            Parent = this,
            X = Status.X,
            Y = Artist.Y + Artist.Height + 2,
            Alpha = 0
        };

        /// <summary>
        /// </summary>
        private void CreateDownloadButton()
        {
            DownloadButton = new BorderedTextButton("Download", Colors.MainAccent)
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Y = Creator.Y + Creator.Height + 20,
                X = Status.X,
                Text = { Font = Fonts.SourceSansProSemiBold },
                SetChildrenAlpha = true,
                Alpha = 0
            };

            DownloadButton.Clicked += (o, e) =>
            {
                var screen = (DownloadScreen) View.Screen;
                screen.SelectedMapset.Value?.OnClicked();
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCancelButton()
        {
            CancelButton = new BorderedTextButton("Cancel", Color.Crimson)
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                Y = DownloadButton.Y,
                X = -Status.X,
                Text = {Font = Fonts.SourceSansProSemiBold},
                SetChildrenAlpha = true,
                Alpha = 0
            };

            CancelButton.Clicked += (o, e) =>
            {
                var screen = (DownloadScreen) View.Screen;

                if (screen.SelectedMapset.Value != null)
                {
                    if (screen.SelectedMapset.Value.Download != null)
                        NotificationManager.Show(NotificationLevel.Warning, "Cancelling downloads is not implemented yet!");

                    screen.SelectedMapset.Value = null;
                }
            };
        }

        /// <summary>
        /// </summary>
        private void CreateViewMapsetPageButton()
        {
            ViewMapsetPageButton = new BorderedTextButton("View Mapset Page", Color.LimeGreen)
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Y = DownloadButton.Y + DownloadButton.Height + 15,
                Size = new ScalableVector2(210, 35),
                Text = {Font = Fonts.SourceSansProSemiBold},
                SetChildrenAlpha = true,
                Alpha = 0
            };

            ViewMapsetPageButton.Clicked += (o, e) =>
            {
                var screen = (DownloadScreen) View.Screen;

                if (screen.SelectedMapset.Value != null)
                    BrowserHelper.OpenURL($"{OnlineClient.WEBSITE_URL}/mapsets/{screen.SelectedMapset.Value.MapsetId}");
            };
        }

        /// <summary>
        ///     Called when the user clicks on a new mapset, so we can display its banner and information.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedMapsetChanged(object sender, BindableValueChangedEventArgs<DownloadableMapset> e)
        {
            if (e.Value != null)
            {
                ShowPreview(e.Value.Mapset);
                return;
            }

            HidePreview();
        }

        /// <summary>
        /// </summary>
        private void ShowPreview(JToken mapset)
        {
            MapBanner.Alpha = 0;

            MapBanner.ClearAnimations();
            Title.ClearAnimations();
            Artist.ClearAnimations();
            Creator.ClearAnimations();
            DownloadButton.ClearAnimations();
            CancelButton.ClearAnimations();
            ViewMapsetPageButton.ClearAnimations();

            Title.Alpha = 0;
            Artist.Alpha = 0;
            Creator.Alpha = 0;
            DownloadButton.Alpha = 0;
            CancelButton.Alpha = 0;
            ViewMapsetPageButton.Alpha = 0;

            Title.Text = mapset["title"].ToString();
            Title.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));

            Artist.Text = mapset["artist"].ToString();
            Artist.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));

            Creator.Text = "Created By: " + mapset["creator_username"];
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));

            DownloadButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
            CancelButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
            ViewMapsetPageButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));

            Task.Run(() =>
            {
                MapBanner.Image = ImageDownloader.DownloadMapsetBanner((int) mapset["id"]);
                MapBanner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 300));
            });
        }

        /// <summary>
        /// </summary>
        private void HidePreview()
        {
            MapBanner.ClearAnimations();
            MapBanner.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, MapBanner.Alpha, 0, 300));

            Title.ClearAnimations();
            Title.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Title.Alpha, 0, 300));

            Artist.ClearAnimations();
            Artist.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Artist.Alpha, 0, 300));

            Creator.ClearAnimations();
            Creator.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Creator.Alpha, 0, 300));

            DownloadButton.ClearAnimations();
            DownloadButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, DownloadButton.Alpha, 0, 300));

            CancelButton.ClearAnimations();
            CancelButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, CancelButton.Alpha, 0, 300));

            ViewMapsetPageButton.ClearAnimations();
            ViewMapsetPageButton.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, ViewMapsetPageButton.Alpha, 0, 300));
        }
    }
}