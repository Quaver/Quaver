using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Online;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class ScoreboardOneVsOneWins : Sprite
    {
        private Scoreboard Scoreboard { get; }

        private SpriteTextPlus SelfWins { get; }

        private SpriteTextPlus OtherWins { get; }

        private int Spacing { get; } = 15;

        public ScoreboardOneVsOneWins(Scoreboard scoreboard)
        {
            Scoreboard = scoreboard;
            Size = new ScalableVector2(260, 34);
            Image = UserInterface.WinsPanel;

            var myWins = OnlineManager.CurrentGame.PlayerWins.Find(x => x.UserId == OnlineManager.Self.OnlineUser.Id);

            var myWinCount = 0;

            if (myWins != null)
                myWinCount = myWins.Wins;

            SelfWins = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), myWinCount.ToString())
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Spacing,
                FontSize = 18,
                Tint = new Color(249, 13, 63)
            };

            var otherPlayerScore = MapManager.Selected.Value.Scores.Value.First();

            var otherWins = 0;

            if (otherPlayerScore != null)
            {
                var otherPlayerWinCount = OnlineManager.CurrentGame.PlayerWins.Find(x => x.UserId == otherPlayerScore.PlayerId);

                if (otherPlayerWinCount != null)
                    otherWins = otherPlayerWinCount.Wins;
            }

            OtherWins = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), otherWins.ToString())
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Spacing,
                FontSize = 18,
                Tint = new Color(13, 148, 253)
            };
        }
    }
}