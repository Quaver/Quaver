using Microsoft.Xna.Framework;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI
{
    internal sealed class SelectionMapsetRow : PoolableSprite<Mapset>
    {
        private SelectionMapsetList List { get; }
        private SkinV2SelectionMapsetConfig Config { get; }
        private RowButton Button { get; }
        private SpriteTextPlus Title { get; }
        private SpriteTextPlus Artist { get; }
        private Color NormalColor { get; }
        private Color HoverColor { get; }
        private Color SelectedColor { get; }

        public override int HEIGHT => List.RowStride;

        internal SelectionMapsetRow(SelectionMapsetList list, SkinV2SelectionMapsetConfig config, Mapset item, int index) : base(list, item, index)
        {
            List = list;
            Config = config;

            NormalColor = SkinV2Color.Parse(config.BackgroundColor);
            HoverColor = SkinV2Color.Parse(config.HoveredBackgroundColor);
            SelectedColor = SkinV2Color.Parse(config.SelectedBackgroundColor);

            Button = new RowButton(list)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };
            Button.Clicked += OnClicked;

            Title = CreateText(config.TitleFontSize, config.TextColor);
            Artist = CreateText(config.ArtistFontSize, config.SecondaryTextColor);
        }

        public override void UpdateContent(Mapset item, int index)
        {
            Item = item;
            Index = index;

            Width = V2Scrollbar.GetContentWidth(List);
            Height = Config.Height;
            Button.Size = Size;

            var padding = System.Math.Min(Config.Padding, Width / 4f);
            var width = (int)System.Math.Max(1, Width - padding * 2);

            Title.Text = item.Title ?? "";
            Title.TruncateWithEllipsis(width);

            Artist.Text = item.Artist ?? "";
            Artist.TruncateWithEllipsis(width);

            var y = System.Math.Max(0, (Height - Title.Height - Artist.Height - 4) / 2f);
            Title.Position = new ScalableVector2(padding, y);
            Artist.Position = new ScalableVector2(padding, y + Title.Height + 4);
        }

        public override void Update(GameTime gameTime)
        {
            Button.IsInteractionEnabled = List.CanInteract;
            Button.Tint = Index == List.SelectedIndex ? SelectedColor : Button.IsHovered ? HoverColor : NormalColor;

            base.Update(gameTime);
        }

        public override void DrawToSpriteBatch()
        {
        }

        private SpriteTextPlus CreateText(int size, string color) => new SpriteTextPlus(FontManager.GetWobbleFont(Config.Font), "", size)
            {
                Parent = this,
                Tint = SkinV2Color.Parse(color),
                UsePreviousSpriteBatchOptions = true
            };

        private void OnClicked(object sender, System.EventArgs args) => List.Select(Item);

        private sealed class RowButton(SelectionMapsetList list) : ImageButton(WobbleAssets.WhiteBox)
        {
            protected override bool IsMouseInClickArea() => list.CanInteract && list.IsHovered() && base.IsMouseInClickArea();
        }

        public override void Destroy()
        {
            Button.Clicked -= OnClicked;
            base.Destroy();
        }
    }
}
