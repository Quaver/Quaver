using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Panels;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Main.UI.Panels
{
    public class MenuPanel : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private MenuPanelContainer Container { get; }

        /// <summary>
        ///     The initial size of the panel
        /// </summary>
        public static ScalableVector2 PanelSize { get; } = new ScalableVector2(324, 970);

        /// <summary>
        ///     The width of a panel when it is expanded
        /// </summary>
        private const int ExpandedWidth = 518;

        /// <summary>
        /// </summary>
        public Sprite Background { get; }

        /// <summary>
        /// </summary>
        private Sprite Darkness { get;  }

        /// <summary>
        /// </summary>
        public ImageButton Button { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Subtitle { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuPanel(MenuPanelContainer container, Texture2D image, string title, string subtitle)
            : base(PanelSize, PanelSize)
        {
            Container = container;

            Background = new Sprite
            {
                Image = image,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
            };

            AddContainedDrawable(Background);

            Darkness = new Sprite()
            {
                Tint = Color.Black,
                Size = Background.Size,
                Alpha = 0.60f
            };

            AddContainedDrawable(Darkness);

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0
            };

            Button.Hovered += (sender, args) =>
            {
                Expand();

                Container.Panels.ForEach(x =>
                {
                    if (x == this)
                        return;

                    x.Condense();
                });
            };

            Button.LeftHover += (sender, args) =>
            {
                // Run an update on ALL buttons to make sure we get the most recent hover state
                Container.Panels.ForEach(x => x.Update(new GameTime()));

                // All buttons are no longer hoevered, so return them to its original size
                if (!Container.Panels.Any(x => x.Button.IsHovered))
                    Container.Panels.ForEach(x => x.CondenseToOriginalSize());;
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), title, 26)
            {
                Parent = Button,
                Alignment = Alignment.BotCenter,
                Y = -200
            };

            Subtitle = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), subtitle, 20)
            {
                Parent = Button,
                Alignment = Alignment.BotCenter,
                Y = Title.Y + Title.Height + 18,
                Visible = false
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Button.Size = Size;
            base.Update(gameTime);
        }

        /// <summary>
        ///     Expands the panel to its full width
        /// </summary>
        public void Expand()
        {
            Subtitle.Visible = true;
            Subtitle.Alpha = 0;
            Subtitle.Animations.Clear();
            Subtitle.FadeTo(1, Easing.Linear, 200);

            Animations.Clear();
            ChangeWidthTo(ExpandedWidth, Easing.OutQuint, 400);

            Darkness.ClearAnimations();
            Darkness.FadeTo(0.45f, Easing.Linear, 200);
        }

        /// <summary>
        ///     Condenses the panel to its original size
        /// </summary>
        public void Condense()
        {
            Subtitle.Visible = false;

            Animations.Clear();
            ChangeWidthTo(260, Easing.OutQuint, 400);

            Darkness.ClearAnimations();
            Darkness.FadeTo(0.60f, Easing.Linear, 200);
        }

        /// <summary>
        ///     Condenses the panel back to its original size
        /// </summary>
        public void CondenseToOriginalSize()
        {
            Subtitle.Visible = false;

            Animations.Clear();
            ChangeWidthTo((int) PanelSize.X.Value, Easing.OutQuint, 400);

            Darkness.ClearAnimations();
            Darkness.FadeTo(0.60f, Easing.Linear, 200);
        }
    }
}