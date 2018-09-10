using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Online;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Quaver.Screens.Menu.UI.BottomToolbar
{
    public class BottomBar : Sprite
    {
        private Sprite TopLine { get; }

        private SpriteText MenuTip { get; }

        private LogoButton QuaverButton { get; }

        private Sprite Avatar { get; }

        private SpriteText UsernameText { get; }

        private ImageButton TwitterButton { get; }

        private ImageButton BlogButton { get; }

        private ImageButton DiscordButton { get; }

        internal BottomBar()
        {
            Size = new ScalableVector2(WindowManager.Width, 80);
            Tint = Color.Black;
            Y = 0;
            Alpha = 0f;
            Alignment = Alignment.BotLeft;

            TopLine = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Tint = Color.White,
                Width = Width - 160,
                Height = 1,
                Alpha = 0.3f,
            };

            MenuTip = new SpriteText(Fonts.Exo2Regular24, "Tip: If our knees were bent the other way, what would chairs look like?")
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
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
                Size = new ScalableVector2(120, 30),
                X = -150,
                Alpha = 0
            };

            Avatar = new Sprite()
            {
                Image = SteamManager.UserAvatar,
                Parent = nameContainer,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(30, 30),
            };

            UsernameText = new SpriteText(Fonts.Exo2Regular24, ConfigManager.Username.Value != "" ? ConfigManager.Username.Value : "Unknown Player")
            {
                Parent = nameContainer,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                TextScale = 0.55f,
                X = Avatar.Width + 8,
                Y = -2
            };


            TwitterButton = new ImageButton(FontAwesome.Twitter, (o, e) => Process.Start("https://twitter.com/QuaverGame"))
            {
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new ScalableVector2(15, 15),
                X = QuaverButton.X + QuaverButton.Width + 5,
                Tint = new Color(0, 172, 237)
            };

            BlogButton = new ImageButton(FontAwesome.Rss, (o, e) => Process.Start("https://blog.quavergame.com"))
            {
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new ScalableVector2(13, 13),
                X = TwitterButton.Width + TwitterButton.X + 20,
                Tint = new Color(242, 101, 34)
            };

            DiscordButton = new ImageButton(FontAwesome.Discord, (o, e) => Process.Start("https://discord.gg/nJa8VFr"))
            {
                Alignment = Alignment.MidLeft,
                Parent = this,
                Size = new ScalableVector2(20, 20),
                X = BlogButton.Width + BlogButton.X + 15,
                Tint = new Color(114, 137, 218)
            };
        }
    }
}
