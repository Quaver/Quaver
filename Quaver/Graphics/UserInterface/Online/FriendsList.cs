using Microsoft.Xna.Framework;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.UserInterface.Online
{
    internal class FriendsList : Sprite
    {
        private Sprite Header { get; }

        private SpriteText HeaderText { get; }

        private Sprite ListContainer { get; }

        private SpriteText OnlineText { get; }

        internal FriendsList()
        {
            Size = new UDim2D(315, 275);
            Tint = Color.Black;
            Alpha = 0;

            Header = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(SizeX, 40),
                Tint = Colors.DarkGray,
                Alpha = 1f
            };

            HeaderText = new SpriteText()
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                Text = "Friends List",
                Font = Fonts.AllerBold16,
                TextScale = 0.75f,
                PosX = 20
            };

            ListContainer = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(SizeX, SizeY - Header.SizeY),
                PosY = Header.SizeY + 5,
                Tint = ColorHelper.HexToColor("#2B2B2B"),
                Alpha = 0.25f
            };

            OnlineText = new SpriteText()
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                TextAlignment = Alignment.MidRight,
                Text = "0 Online",
                Font = Fonts.AllerBold16,
                TextScale = 0.75f,
                PosX = -20
            };            
        }
    }
}