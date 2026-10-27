using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Screens.V2.UI.Filters;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Music.UI
{
    internal sealed class MusicQueuePanel : Container
    {
        private SkinV2MusicQueueDialogConfig Config { get; }

        private Bindable<string> Query { get; }

        private Sprite Background { get; }

        private V2FilterSearchTextbox Search { get; }

        private SpriteTextPlus TitleHeader { get; }

        private SpriteTextPlus ArtistHeader { get; }

        private SpriteTextPlus LengthHeader { get; }

        private MusicQueueList List { get; }

        private MusicPlaybackController Playback { get; set; }

        private bool ApplyingLayout { get; set; }

        public MusicQueuePanel(ScalableVector2 size, Bindable<string> query, SkinV2MusicQueueDialogConfig config)
        {
            Config = config;
            Query = query;
            Size = size;

            Background = new Sprite
            {
                Parent = this,
                Size = Size,
                Tint = SkinV2Color.Parse(Config.PanelColor)
            };

            var search = Config.Search;
            var searchStyle = new V2FilterFieldStyle
            {
                Height = search.Height,
                SearchIconSize = search.IconSize,
                SearchIconInset = search.IconInset,
                CornerRadius = search.CornerRadius,
                BackgroundColor = SkinV2Color.Parse(search.BackgroundColor),
                TextColor = SkinV2Color.Parse(search.TextColor),
                PlaceholderColor = SkinV2Color.Parse(search.PlaceholderColor),
                CursorColor = SkinV2Color.Parse(search.CursorColor)
            };
            Search = new V2FilterSearchTextbox(Query, LocalizationManager.Get("Screen_Download_Typetosearch"), FontManager.GetWobbleFont(search.Font), search.FontSize, searchStyle, 1)
            {
                Parent = this
            };

            var headerFont = FontManager.GetWobbleFont(Config.List.Font);
            TitleHeader = CreateHeader(headerFont, "Screen_Selection_Title", Alignment.TopLeft);
            ArtistHeader = CreateHeader(headerFont, "Screen_Selection_Artist", Alignment.TopLeft);
            LengthHeader = CreateHeader(headerFont, "Screen_Selection_Length", Alignment.TopRight);

            List = new MusicQueueList(new ScalableVector2(1, 1), Config.List, Config.Row, mapset => Playback?.Select(mapset))
            {
                Parent = this
            };

            Query.ValueChanged += OnQueryChanged;
            MapManager.Selected.ValueChanged += OnSelectedMapChanged;
            ApplyLayout();
            RefreshItems();
        }

        public void SetPlaybackController(MusicPlaybackController playback)
        {
            if (Playback != null)
                Playback.AvailableSongs.ValueChanged -= OnAvailableSongsChanged;

            Playback = playback;

            if (Playback != null)
                Playback.AvailableSongs.ValueChanged += OnAvailableSongsChanged;

            RefreshItems();
        }

        protected override void OnRectangleRecalculated()
        {
            base.OnRectangleRecalculated();
            if (Background != null && !ApplyingLayout && !IsDisposed)
                ApplyLayout();
        }

        public override void Destroy()
        {
            Query.ValueChanged -= OnQueryChanged;
            MapManager.Selected.ValueChanged -= OnSelectedMapChanged;
            if (Playback != null)
                Playback.AvailableSongs.ValueChanged -= OnAvailableSongsChanged;

            base.Destroy();
        }

        private SpriteTextPlus CreateHeader(WobbleFontStore font, string key, Alignment alignment) =>
            new SpriteTextPlus(font, LocalizationManager.Get(key), Config.List.FontSize)
            {
                Parent = this,
                Alignment = alignment,
                Tint = SkinV2Color.Parse(Config.List.TextColor)
            };

        private void ApplyLayout()
        {
            ApplyingLayout = true;
            try
            {
                Background.Size = Size;
                Background.Image = RoundedRectTextureCache.Get(Width, Height, Config.CornerRadius);

                var padding = Config.List.ContentPadding;
                Search.Position = new ScalableVector2(padding, padding);
                Search.Width = Math.Max(1, Width - padding * 2);

                var headerY = padding + Config.Search.Height;
                var listY = headerY + Config.List.HeaderHeight;
                var contentWidth = Math.Max(1, Width - padding * 2);
                List.Position = new ScalableVector2(padding, listY);
                List.ApplySize(new ScalableVector2(contentWidth, Math.Max(1, Height - listY - padding)));

                var rowWidth = V2Scrollbar.GetContentWidth(List);
                var artistX = padding + rowWidth * Config.Row.ArtistColumnStart;
                var headerTextY = Math.Max(headerY, listY - Config.List.HeaderGap - TitleHeader.Height);

                TitleHeader.Position = new ScalableVector2(padding + Config.Row.HorizontalPadding, headerTextY);
                ArtistHeader.Position = new ScalableVector2(artistX, headerTextY);
                LengthHeader.Position = new ScalableVector2(padding + rowWidth - Config.Row.HorizontalPadding - Width, headerTextY);
            }
            finally
            {
                ApplyingLayout = false;
            }
        }

        private void RefreshItems()
        {
            List<Mapset> items;
            var query = Query.Value?.Trim();
            var focusCurrent = false;
            if (Playback == null)
                items = new List<Mapset>();
            else if (string.IsNullOrEmpty(query))
            {
                items = Playback.GetQueueMapsets(14, 15);
                focusCurrent = true;
            }
            else
                items = Playback.GetOrderedMapsets()
                    .Where(mapset => mapset.Title?.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

            List.SetItems(items, focusCurrent);
            Search.SetResultsText(LocalizationManager.Get("Screen_Music_SongsFound", items.Count));
        }

        private void OnQueryChanged(object sender, BindableValueChangedEventArgs<string> args) =>
            RefreshItems();

        private void OnSelectedMapChanged(object sender, BindableValueChangedEventArgs<Map> args)
        {
            if (string.IsNullOrWhiteSpace(Query.Value))
                RefreshItems();
            else
                List.RefreshSelection();
        }

        private void OnAvailableSongsChanged(object sender, BindableValueChangedEventArgs<List<Mapset>> args) => RefreshItems();
    }
}
