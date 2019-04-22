using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Lobby.UI.Dialogs;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class LobbyMatchInfo : Sprite
    {
        /// <summary>
        /// </summary>
        private Sprite Background { get; set; }

        /// <summary>
        /// </summary>
        public SpriteMaskContainer Mask { get; set; }

        /// <summary>
        /// </summary>
        private LobbyScreen Screen { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap NoMatchSelected { get; set; }

        /// <summary>
        /// </summary>
        private Sprite ContentContainer { get; set; }

        /// <summary>
        /// </summary>
        public LobbyMatchInfo(LobbyScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(480, 638);
            Tint = Color.Black;
            Alpha = 0f;

            CreateBackground();
            CreateContentContainer();
            CreateNoMatchSelectedText();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            var container = new Container(0, 0, Width, 176)
            {
                Parent = this,
                Alignment = Alignment.TopCenter
            };

            container.AddBorder(Color.White, 2);
            container.Border.Alpha = 1;

            Mask = new SpriteMaskContainer
            {
                Parent = container,
                Size = new ScalableVector2(Width - 4, 172),
                Alignment = Alignment.TopCenter,
                Tint = Color.Black,
                Y = 2
            };

            Background = new Sprite
            {
                Parent = container,
                Size = new ScalableVector2(WindowManager.Width / 1.6f, WindowManager.Height / 1.6f),
                Image = UserInterface.MenuBackground,
                Y = -100,
                X = -100
            };

            Mask.AddContainedSprite(Background);
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer()
        {
            ContentContainer = new Sprite
            {
                Parent = this,
                Y = Mask.Y + Mask.Height + 20,
                Size = new ScalableVector2(Width, Height - Mask.Height - 20),
                Tint = Color.Black,
                Alpha = 0.60f
            };

            var test = new LabelledMetadata(Width - 60, "Room Name", "Testing")
            {
                Parent = ContentContainer,
                Alignment = Alignment.TopCenter,
                Y = 30
            };
        }

        /// <summary>
        /// </summary>
        private void CreateNoMatchSelectedText() => NoMatchSelected = new SpriteTextBitmap(FontsBitmap.GothamRegular,
            "No Match Selected!")
        {
            Parent = ContentContainer,
            Alignment = Alignment.MidCenter,
            FontSize = 16
        };
    }
}