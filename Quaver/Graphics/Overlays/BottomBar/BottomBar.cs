using System.Diagnostics;
using Microsoft.Xna.Framework;
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

        private Sprite SwanLogo { get; }

        private SpriteText SwanText { get; }

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
                Font = QuaverFonts.AssistantRegular16,
                Text = "Tip: If our knees were bent the other way, what would chairs look like?",
                TextScale = 0.75f
            };

            QuaverButton = new LogoButton
            {
                Parent = this,
                Alignment = Alignment.MidLeft
            };

            SwanText = new SpriteText()
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                TextAlignment = Alignment.MidCenter,
                Text = "Powered by Swan 2017-2018",
                Font = QuaverFonts.AssistantRegular16,
                TextScale = 0.75f
            };

            SwanText.PosX = (-SwanText.Font.MeasureString(SwanText.Text) * SwanText.TextScale).X;

            SwanLogo = new Sprite()
            {
                Image = GameBase.QuaverUserInterface.SwanLogo,
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new UDim2D(40, 40),
                PosX = SwanText.PosX - 100,
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