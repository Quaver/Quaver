using Microsoft.Xna.Framework;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerTeamPanel : Sprite
    {
        public ResultMultiplayerTeamPanel(ResultScreen screen)
        {
            Size = new ScalableVector2(WindowManager.Width - 56, 36);
            Image = UserInterface.ResultMultiplayerTeamPanel;

            var redScoreValue = screen.GetTeamAverage(MultiplayerTeam.Red);
            var blueScoreValue = screen.GetTeamAverage(MultiplayerTeam.Blue);

            var redTeamBackground = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(82, 30),
                X = 2,
                Alignment = Alignment.MidLeft,
                Alpha = 0
            };

            var redScore = new SpriteTextBitmap(FontsBitmap.GothamRegular,
                $"{redScoreValue:00.00}")
            {
                Parent = redTeamBackground,
                Alignment = Alignment.MidCenter,
                FontSize = 18,
            };

            var blueTeamBackground = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(82, 30),
                X = -2,
                Alignment = Alignment.MidRight,
                Alpha = 0
            };

            var blueScore = new SpriteTextBitmap(FontsBitmap.GothamRegular,
                $"{blueScoreValue:00.00}")
            {
                Parent = blueTeamBackground,
                Alignment = Alignment.MidCenter,
                FontSize = 18,
            };

            var redTeamWinCountBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(62, 30),
                X = -116,
                Alpha = 0
            };

            var redTeamWinCount = new SpriteTextBitmap(FontsBitmap.GothamRegular,
                OnlineManager.CurrentGame.RedTeamWins.ToString())
            {
                Parent = redTeamWinCountBackground,
                Alignment = Alignment.MidCenter,
                FontSize = 18,
            };

            var blueTeamWinCountBackground = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(62, 30),
                X = 116,
                Alpha = 0
            };

            var blueTeamWinCount = new SpriteTextBitmap(FontsBitmap.GothamRegular,
                OnlineManager.CurrentGame.BlueTeamWins.ToString())
            {
                Parent = blueTeamWinCountBackground,
                Alignment = Alignment.MidCenter,
                FontSize = 18,
            };

            var crown = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(16, 16),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_crown),
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                Visible = redScoreValue != blueScoreValue,
                X = blueScoreValue > redScoreValue ? 436 : -436,
                Y = -1,
                Tint = Color.Gold
            };
        }
    }
}