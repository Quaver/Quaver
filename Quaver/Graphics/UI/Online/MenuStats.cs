using Microsoft.Xna.Framework;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.Graphics.UI.Online
{
    internal class MenuStats : Sprite
    {
        private Sprite Header { get; }

        private SpriteText HeaderText { get; }
        
        private Playercard Player { get; }

        private Sprite ContentContainer { get; }

        private Sprite RankBadge { get; }

        internal MenuStats()
        {
            Size = new UDim2D(312, 150);
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
                Text = "Player Stats",
                Font = Fonts.AllerRegular16,
                TextScale = 0.75f,
                PosX = 20
            };

            var profileButton = new BasicButton()
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                Image = FontAwesome.Cog,
                Size = new UDim2D(15, 15),
                PosX = -10
            };
            
            ContentContainer = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(SizeX, SizeY - Header.SizeY + 5),
                PosY = Header.SizeY,
                Tint = Color.Black,
                Alpha = 0f
            };

            Player = new Playercard() {Parent = ContentContainer};

            var rankContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new UDim2D(SizeX, SizeY - ContentContainer.SizeY + 5),
                PosY = Player.SizeY,
                Tint = ColorHelper.HexToColor("#2B2B2B"),
                Alpha = 0.25f
            };
            
            RankBadge = new Sprite()
            {
                Parent = rankContainer,
                PosX = 15,
                Image = FontAwesome.Question,
                Size = new UDim2D(20, 20),
                Alignment = Alignment.MidLeft,
            };

            var rankText = new SpriteText()
            {
                Parent = rankContainer,
                Alignment = Alignment.MidLeft,
                TextAlignment = Alignment.MidLeft,
                Text = "Unranked",
                Font = Fonts.AllerRegular16,
                TextScale = 0.70f,
                PosX = RankBadge.PosX + RankBadge.SizeX + 10
            };

            var winCount = new SpriteText()
            {
                Parent = rankContainer,
                Alignment = Alignment.MidRight,
                TextAlignment = Alignment.MidRight,
                Text = "0",
                Font = Fonts.AllerRegular16,
                TextScale = 0.75f,
                PosX = -10
            };

            var trophy = new Sprite()
            {
                Parent = rankContainer, 
                Alignment = Alignment.MidRight, 
                Size = new UDim2D(15, 15),
                PosX = winCount.PosX - 10,
                Image = FontAwesome.Trophy,
                Tint = Color.Gold
            };

            trophy.PosX = -(winCount.Font.MeasureString(winCount.Text).X * winCount.TextScale) - 20;
        }
    }
}