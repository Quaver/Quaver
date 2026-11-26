using Microsoft.Xna.Framework;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.V2.Selection.UI.List
{
    /// <summary>
    ///     Provides selection and interaction behavior shared by mapset and difficulty rows
    /// </summary>
    internal abstract class SelectionListRow : PoolableSprite<SelectionListEntry>
    {
        private const int SelectionAnimationMilliseconds = 180;

        protected MapsetList List { get; }
        protected SkinV2SelectionRowConfig Config { get; }

        private NineSliceSprite Background { get; }
        private RowButton Button { get; }
        private Color NormalColor { get; set; }
        private Color HoverColor { get; set; }
        private Color SelectedColor { get; set; }
        private int TargetWidth { get; set; }
        private float BoundContentWidth { get; set; }
        private bool ContentBound { get; set; }

        public override int HEIGHT => Config.Height + List.RowGap;

        protected SelectionListRow(MapsetList list, SelectionListEntry item, int index, SkinV2SelectionRowConfig config) : base(list, item, index)
        {
            List = list;
            Config = config;
            SetChildrenAlpha = true;

            Background = new NineSliceSprite
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };
            Button = new RowButton(list, this)
            {
                Parent = this,
                UsePreviousSpriteBatchOptions = true
            };
            Button.Clicked += OnClicked;
            SizeChanged += OnSizeChanged;
        }

        public override void UpdateContent(SelectionListEntry item, int index)
        {
            var sameItem = ContentBound && Item.HasSameItem(item);
            Item = item;
            Index = index;
            List.AttachRow(this);
            var contentWidth = V2Scrollbar.GetContentWidth(List);

            if (sameItem && BoundContentWidth == contentWidth && Height == Config.Height)
            {
                UpdateRowLayout(System.Math.Min(Width, GetRowWidth()));
                return;
            }

            ContentBound = true;
            BoundContentWidth = contentWidth;
            ClearAnimations();
            Button.ResetInteractionState();

            NormalColor = SkinV2Color.Parse(Config.BackgroundColor);
            HoverColor = SkinV2Color.Parse(Config.HoveredBackgroundColor);
            SelectedColor = SkinV2Color.Parse(Config.SelectedBackgroundColor);
            Background.Image = List.GetRowBackground(Item);
            Background.Margins = new SliceMargins((int) System.Math.Ceiling(Config.CornerRadius));

            UpdateRowContent(sameItem);

            TargetWidth = GetRowWidth();
            Size = new ScalableVector2(TargetWidth, Config.Height);

            UpdateRowLayout(TargetWidth);
        }

        protected abstract void UpdateRowContent(bool sameItem);

        protected abstract void UpdateRowLayout(float rowWidth);

        protected virtual void UpdateRow(GameTime gameTime)
        {
        }

        internal abstract void RefreshDynamicContent();

        private int GetRowWidth() => System.Math.Min(List.IsSelected(Item) ? Config.SelectedWidth : Config.Width, (int) V2Scrollbar.GetContentWidth(List));

        private void OnSizeChanged(object sender, ScalableVector2 size)
        {
            if (IsDisposed || Button.IsDisposed)
                return;

            X = V2Scrollbar.GetContentWidth(List) - Width;
            Background.Size = Size;
            Button.Size = Size;

            UpdateRowLayout(Width);
        }

        public override void Update(GameTime gameTime)
        {
            if (!Visible)
                return;

            var targetWidth = GetRowWidth();
            if (TargetWidth != targetWidth)
            {
                TargetWidth = targetWidth;
                ClearAnimations();
                UpdateRowLayout(System.Math.Min(Width, TargetWidth));

                if (Width != TargetWidth)
                    ChangeWidthTo(TargetWidth, Easing.OutCubic, SelectionAnimationMilliseconds);
            }

            Button.IsInteractionEnabled = List.CanInteract && List.IsInteractive(Item);
            Background.Tint = List.IsSelected(Item) ? SelectedColor : Button.IsHovered ? HoverColor : NormalColor;
            UpdateRow(gameTime);

            base.Update(gameTime);
        }

        protected MarqueeSpriteText CreateText(string font, int size, string color)
        {
            var text = new MarqueeSpriteText(FontManager.GetWobbleFont(font), "", size, 1)
            {
                Parent = this,
                IsActive = true,
                UsePreviousSpriteBatchOptions = true
            };
            text.TextSprite.Tint = SkinV2Color.Parse(color);
            return text;
        }

        protected static void SetText(MarqueeSpriteText text, string value, int width)
        {
            var changed = text.TextSprite.Text != value || text.Width != width;

            text.TextSprite.Text = value;
            text.Size = new ScalableVector2(width, text.TextSprite.Height);

            if (changed)
                text.ResetPosition();
        }

        internal void SetActive(bool active)
        {
            if (!active)
                Button.ResetInteractionState();

            Visible = active;
        }

        private void OnClicked(object sender, System.EventArgs args) => List.Select(Item);

        public override void DrawToSpriteBatch()
        {
        }

        public override void Destroy()
        {
            SizeChanged -= OnSizeChanged;
            Button.Clicked -= OnClicked;

            base.Destroy();
        }

        /// <summary>
        ///     Restricts row click handling to the active Song Select viewport
        /// </summary>
        private sealed class RowButton(MapsetList list, SelectionListRow row) : ImageButton(WobbleAssets.WhiteBox)
        {
            protected override bool IsMouseInClickArea() => list.CanInteract && list.IsInteractive(row.Item) && list.IsHovered() && base.IsMouseInClickArea();

            public override void DrawToSpriteBatch()
            {
            }
        }
    }
}
