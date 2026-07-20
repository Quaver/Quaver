using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanel : DraggableButton
    {
        private const int ExpandedHeight = 237;

        private const int HeaderHeight = 45;

        private const int BottomBorderHeight = 8;

        private NineSliceSprite CollapsedBackground { get; }

        /// <summary>
        /// </summary>
        public Container Header { get; private set; }

        /// <summary>
        /// </summary>
        protected SpriteTextPlus HeaderText { get; private set; }

        /// <summary>
        /// </summary>
        public Container Content { get; private set; }

        /// <summary>
        ///     The button used to collapse and expand the panel.
        /// </summary>
        protected IconButton CollapseButton { get; private set; }

        /// <summary>
        ///     Whether the panel is currently collapsed.
        /// </summary>
        public bool IsCollapsed { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="header"></param>
        public EditorPanel(string header) : base(UserInterface.EditorPanelBackground)
        {
            Size = new ScalableVector2(288, ExpandedHeight);

            CollapsedBackground = new NineSliceSprite(UserInterface.EditorPanelBackground,
                new SliceMargins(4, 4, HeaderHeight, BottomBorderHeight))
            {
                Parent = this,
                Size = Size,
                Visible = false,
                UpdateWhenInvisible = false
            };

            CreateHeader(header);
            CreateContent();
            CreateCollapseButton();

            SizeChanged += (sender, size) => CollapsedBackground.Size = size;
        }

        /// <inheritdoc />
        /// <summary>
        ///     Uses the original background rendering while expanded and nine-slice scaling while collapsed.
        /// </summary>
        public override void DrawToSpriteBatch()
        {
            if (!IsCollapsed)
                base.DrawToSpriteBatch();
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        private void CreateHeader(string text)
        {
            Header = new Container()
            {
                Parent = this,
                Size = new ScalableVector2(Width, HeaderHeight)
            };

            HeaderText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), text.ToUpper(), 18)
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                X = 18
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContent()
        {
            Content = new Container()
            {
                Parent = this,
                Y = Header.Height,
                Size = new ScalableVector2(Width, Height - Header.Height)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateCollapseButton()
        {
            CollapseButton = new IconButton(FontAwesome.Get(FontAwesomeIcon.fa_chevron_arrow_up))
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(20, 20),
                X = -HeaderText.X
            };

            CollapseButton.Clicked += (sender, args) => SetCollapsed(!IsCollapsed);
        }

        /// <summary>
        ///     Collapses or expands the panel while keeping its header in the same screen-space position.
        /// </summary>
        /// <param name="collapsed"></param>
        public void SetCollapsed(bool collapsed)
        {
            if (IsCollapsed == collapsed)
                return;

            var originalPosition = AbsolutePosition;

            IsCollapsed = collapsed;
            CollapsedBackground.Visible = collapsed;
            Content.Visible = !collapsed;
            Content.UpdateWhenInvisible = !collapsed;
            Height = collapsed ? HeaderHeight + BottomBorderHeight : ExpandedHeight;
            CollapseButton.Image = FontAwesome.Get(collapsed
                ? FontAwesomeIcon.fa_chevron_arrow_down
                : FontAwesomeIcon.fa_chevron_arrow_up);

            MoveToAbsolutePosition(originalPosition);

            if (!collapsed)
                ClampToWindow();
        }

        /// <summary>
        ///     Moves the panel's top-left corner to an absolute screen-space position without changing its alignment.
        /// </summary>
        /// <param name="position"></param>
        private void MoveToAbsolutePosition(Vector2 position)
        {
            var parentScale = Parent?.AbsoluteScale ?? Vector2.One;
            var delta = position - AbsolutePosition;

            Position = new ScalableVector2(
                X + delta.X / parentScale.X,
                Y + delta.Y / parentScale.Y,
                Position.X.Scale,
                Position.Y.Scale);
        }

        /// <summary>
        ///     Keeps the fully expanded panel reachable if it was moved while collapsed.
        /// </summary>
        private void ClampToWindow()
        {
            var position = AbsolutePosition;
            position.X = MathHelper.Clamp(position.X, 0, Math.Max(0, WindowManager.Width - AbsoluteSize.X));
            position.Y = MathHelper.Clamp(position.Y, 0, Math.Max(0, WindowManager.Height - AbsoluteSize.Y));
            MoveToAbsolutePosition(position);
        }
    }
}
