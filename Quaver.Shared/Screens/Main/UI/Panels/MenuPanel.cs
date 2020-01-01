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
        public static ScalableVector2 PanelSize => new ScalableVector2(WindowManager.Width / 5, 970);

        /// <summary>
        ///     The width of a panel when it is expanded
        /// </summary>
        private const int ExpandedWidth = 620;

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
        private SpriteTextPlus Title { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Subtitle { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MenuPanel(MenuPanelContainer container, Texture2D image, string title, string subtitle, Action clickAction)
            : base(PanelSize, PanelSize)
        {
            Container = container;

            Background = new Sprite
            {
                Image = image,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(WindowManager.Width, WindowManager.Height),
                UsePreviousSpriteBatchOptions = true
            };

            AddContainedDrawable(Background);

            Button = new ImageButton(UserInterface.BlankBox)
            {
                Parent = this,
                Alpha = 0
            };

            Button.Clicked += (sender, args) => clickAction();

            Darkness = new Sprite()
            {
                Parent = Button,
                Tint = Color.Black,
                Alpha = 0.60f,
            };

            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), title, 30)
            {
                Parent = Button,
                Alignment = Alignment.BotCenter,
                Y = -150
            };

            Subtitle = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), subtitle, 22)
            {
                Parent = Button,
                Alignment = Alignment.BotCenter,
                Y = Title.Y + Title.Height + 4,
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
            Darkness.Size = Button.Size;

            base.Update(gameTime);
        }

        /// <summary>
        ///     Expands the panel to its full width
        /// </summary>
        public void Expand(GameTime gameTime)
        {
            const int animTime = 75;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Title.X = MathHelper.Lerp(Title.X, -ExpandedWidth / 2f + Title.Width / 2f + 20, (float) Math.Min(dt / animTime, 1));

            Width = MathHelper.Lerp(Width, ExpandedWidth, (float) Math.Min(dt / animTime, 1));
            Background.X = MathHelper.Lerp(Background.X, -ExpandedWidth / 2f, (float) Math.Min(dt / animTime, 1));

            Darkness.Alpha = MathHelper.Lerp(Darkness.Alpha, 0.45f, (float) Math.Min(dt / animTime, 1));

            Subtitle.Visible = true;
            Subtitle.Alpha = MathHelper.Lerp(Subtitle.Alpha, 1, (float) Math.Min(dt / (animTime + 50), 1));
            Subtitle.X = MathHelper.Lerp(Subtitle.X, -ExpandedWidth / 2f + Subtitle.Width / 2f + 20, (float) Math.Min(dt / animTime, 1));
        }

        /// <summary>
        ///     Condenses the panel to its original size
        /// </summary>
        public void Condense(GameTime gameTime)
        {
            const int animTime = 100;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Subtitle.Visible = false;
            Subtitle.X = 0;
            Subtitle.Alpha = 0;

            Title.X = MathHelper.Lerp(Title.X, 0, (float) Math.Min(dt / animTime, 1));
            Background.X = MathHelper.Lerp(Background.X, -450, (float) Math.Min(dt / animTime, 1));

            Width = MathHelper.Lerp(Width, (WindowManager.Width - ExpandedWidth) / 4, (float) Math.Min(dt / animTime, 1));
            Darkness.Alpha = MathHelper.Lerp(Darkness.Alpha, 0.6f, (float) Math.Min(dt / animTime, 1));
        }

        /// <summary>
        ///     Condenses the panel back to its original size
        /// </summary>
        public void CondenseToOriginalSize(GameTime gameTime)
        {
            const int animTime = 100;

            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            Title.X = MathHelper.Lerp(Title.X, 0, (float) Math.Min(dt / animTime, 1));

            Subtitle.Visible = false;
            Subtitle.X = 0;
            Subtitle.Alpha = 0;

            Width = MathHelper.Lerp(Width, PanelSize.X.Value, (float) Math.Min(dt / animTime, 1));
            Background.X = MathHelper.Lerp(Background.X, -450, (float) Math.Min(dt / animTime, 1));

            Darkness.Alpha = MathHelper.Lerp(Darkness.Alpha, 0.6f, (float) Math.Min(dt / animTime, 1));
        }
    }
}