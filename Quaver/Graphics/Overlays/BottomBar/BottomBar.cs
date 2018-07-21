using System.Diagnostics;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;

namespace Quaver.Graphics.Overlays.BottomBar
{
    internal class BottomBar : Sprite
    {
        private Sprite TopLine { get; }

        private SpriteText MenuTip { get; }

        private LogoButton QuaverButton { get; }

        private Sprite Avatar { get; }

        private SpriteText UsernameText { get; }

        private BasicButton TwitterButton { get; }

        private BasicButton BlogButton { get; }

        private BasicButton DiscordButton { get; }

        internal BottomBar()
        {
            Size = new UDim2D(GameBase.WindowRectangle.Width, 80);
            Tint = Color.Black;
            PosY = 0;
            Alpha = 0f;
            Alignment = Alignment.BotLeft;

            TopLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Tint = Color.White,
                SizeX = SizeX - 160,
                SizeY = 1,
                Alpha = 0.3f,
            };

            MenuTip = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Font = Fonts.Exo2Regular24,
                Text = "Tip: If our knees were bent the other way, what would chairs look like?",
                TextScale = 0.45f
            };

            QuaverButton = new LogoButton
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            var nameContainer = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new UDim2D(120, 30),
                PosX = -150,
                Alpha = 0
            };

            Avatar = new Sprite()
            {
                Image = UserInterface.YouAvatar,
                Parent = nameContainer,
                Alignment = Alignment.TopLeft,
                Size = new UDim2D(30, 30),
            };

            UsernameText = new SpriteText
            {
                Parent = nameContainer,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                Text = ConfigManager.Username.Value != "" ? ConfigManager.Username.Value : "Unknown Player",
                Font = Fonts.Exo2Regular24,
                TextScale = 0.55f,
                PosX = Avatar.SizeX + 8,
                PosY = -2
            };


            TwitterButton = new BasicButton()
            {
                Image = FontAwesome.Twitter,
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new UDim2D(15, 15),
                PosX = QuaverButton.PosX + QuaverButton.SizeX + 5,
                Tint = new Color(0, 172, 237)
            };

            TwitterButton.Clicked += (o, e) => Process.Start("https://twitter.com/QuaverGame");

            BlogButton = new BasicButton()
            {
                Image = FontAwesome.Rss,
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new UDim2D(13, 13),
                PosX = TwitterButton.SizeX + TwitterButton.PosX + 20,
                Tint = new Color(242,101,34)
            };

            BlogButton.Clicked += (o, e) => Process.Start("https://blog.quavergame.com");

            DiscordButton = new BasicButton()
            {
                Image = FontAwesome.Discord,
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new UDim2D(20, 20),
                PosX = BlogButton.SizeX + BlogButton.PosX + 15,
                Tint = new Color(114,137,218)
            };

            DiscordButton.Clicked += (o, e) => Process.Start("https://discord.gg/nJa8VFr");
        }
    }
}