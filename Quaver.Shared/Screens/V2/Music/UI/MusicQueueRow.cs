using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Music.UI
{
    internal sealed class MusicQueueRow : PoolableSprite<Mapset>
    {
        private SkinV2MusicQueueRowConfig Config { get; }

        private Action<Mapset> Selected { get; }

        private RoundedButton Button { get; }

        private MarqueeSpriteText Title { get; }

        private MarqueeSpriteText Artist { get; }

        private SpriteTextPlus Length { get; }

        public override int HEIGHT => (int) Math.Round(Config.Height);

        public MusicQueueRow(PoolableScrollContainer<Mapset> container, Mapset item, int index, SkinV2MusicQueueRowConfig config, Action<Mapset> selected) : base(container, item, index)
        {
            Config = config;
            Selected = selected;
            Size = new ScalableVector2(V2Scrollbar.GetContentWidth(container), Config.Height);
            Tint = Color.Transparent;

            Button = new RoundedButton((sender, args) =>
            {
                if (Container.ContentViewport.IsHovered())
                    Selected?.Invoke(Item);
            })
            {
                Parent = this,
                Size = Size,
                CornerRadius = Config.CornerRadius,
                PerformHoverFade = true,
                UsePreviousSpriteBatchOptions = true
            };

            var font = FontManager.GetWobbleFont(Config.Font);
            Title = new MarqueeSpriteText(font, string.Empty, Config.FontSize, 1)
            {
                Parent = Button,
                IsActive = true,
                UsePreviousSpriteBatchOptions = true
            };
            Title.TextSprite.Tint = SkinV2Color.Parse(Config.TextColor);

            Artist = new MarqueeSpriteText(font, string.Empty, Config.FontSize, 1)
            {
                Parent = Button,
                IsActive = true,
                UsePreviousSpriteBatchOptions = true
            };
            Artist.TextSprite.Tint = SkinV2Color.Parse(Config.TextColor);

            Length = new SpriteTextPlus(font, string.Empty, Config.FontSize)
            {
                Parent = Button,
                Alignment = Alignment.MidRight,
                Tint = SkinV2Color.Parse(Config.LengthColor),
                UsePreviousSpriteBatchOptions = true
            };

            ApplyLayout();
        }

        public override void UpdateContent(Mapset item, int index)
        {
            Item = item;
            Index = index;

            var map = item?.Maps?.Count > 0 ? item.Maps[0] : null;
            Title.TextSprite.Text = map?.Title ?? string.Empty;
            Artist.TextSprite.Text = map?.Artist ?? string.Empty;
            Length.Text = FormatLength(map?.SongLength ?? 0);
            Title.ResetPosition();
            Artist.ResetPosition();
            RefreshSelection();
            ApplyLayout();
        }

        public void RefreshSelection()
        {
            var selected = MapManager.Selected?.Value;
            Button.Tint = selected != null && Item?.Maps?.Contains(selected) == true
                ? SkinV2Color.Parse(Config.SelectedColor) : SkinV2Color.Parse(Config.BackgroundColor);
        }

        private void ApplyLayout()
        {
            Button.Size = Size;

            var titleX = Config.HorizontalPadding;
            var artistX = Width * Config.ArtistColumnStart;
            var lengthX = Width * Config.LengthColumnStart;
            var gap = Config.HorizontalPadding;

            Title.Position = new ScalableVector2(titleX, 0);
            Title.Size = new ScalableVector2(Math.Max(1, artistX - titleX - gap), Height);
            Artist.Position = new ScalableVector2(artistX, 0);
            Artist.Size = new ScalableVector2(Math.Max(1, lengthX - artistX - gap), Height);
            Length.X = -Config.HorizontalPadding;
        }

        private static string FormatLength(double milliseconds)
        {
            var duration = TimeSpan.FromMilliseconds(Math.Max(0, milliseconds));
            return duration.TotalHours >= 1
                ? $"{(int) duration.TotalHours}:{duration.Minutes:00}:{duration.Seconds:00}"
                : $"{duration.Minutes:00}:{duration.Seconds:00}";
        }
    }
}
