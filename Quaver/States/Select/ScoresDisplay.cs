using System.Collections.Generic;
using System.Drawing;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.Text;
using Quaver.Main;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.States.Select
{
    internal class ScoresDisplay : Container
    {
        /// <summary>
        ///     All the currently displayed local scores.
        /// </summary>
        internal List<LocalScore> Scores { get; private set; }

        /// <summary>
        ///     All of the current score displays.
        /// </summary>
        internal List<Sprite> Displays { get; private set; } = new List<Sprite>();

        /// <summary>
        ///     Updates the display with new scores.
        /// </summary>
        /// <param name="scores"></param>
        internal void UpdateDisplay(List<LocalScore> scores)
        {
            Scores = scores;           
            
            // Get rid of all other displays.
            Displays.ForEach(x => x.Destroy());

            for (var i = 0; i < scores.Count && i < 8; i++)
            {
                var display = new Sprite()
                {
                    Parent = this,
                    Size = new UDim2D(320, 75),
                    Alignment = Alignment.TopCenter,
                    Tint = Color.White,
                    Alpha = 0.85f,
                    PosY = i * 80 + 100,
                    PosX = -20,
                    Image = GameBase.Skin.ScoreboardOther
                };
                
                // Create avatar
                var avatar = new Sprite()
                {
                    Parent = display,
                    Size = new UDim2D(display.SizeY, display.SizeY),
                    Alignment = Alignment.MidLeft,
                    Image = GameBase.QuaverUserInterface.UnknownAvatar,
                };
            
                // Create username text.
                var username = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Text = (i + 1) + ". " + scores[i].Name,
                    Alignment = Alignment.TopLeft,
                    Alpha = 1,
                    TextScale = 0.85f
                };

                // Set username position.
                var usernameTextSize = username.Font.MeasureString(username.Text);        
                username.PosX = avatar.SizeX + usernameTextSize.X * username.TextScale / 2f + 10;
                username.PosY = usernameTextSize.Y * username.TextScale / 2f - 2;
            
                // Create score text.
                var score = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.TopLeft,
                    Text = scores[i].Score.ToString("N0"),
                    TextScale = 0.78f,
                    Alpha = 1
                };
            
                var scoreTextSize = score.Font.MeasureString(score.Text);
                score.PosX = avatar.SizeX + scoreTextSize.X * score.TextScale / 2f + 12;
                score.PosY = username.PosY + scoreTextSize.Y * score.TextScale / 2f + 12;
                
                // Create score text.
                var maxCombo = new SpriteText()
                {
                    Parent = display,
                    Font = QuaverFonts.AssistantRegular16,
                    Alignment = Alignment.BotRight,
                    Text = $"{scores[i].MaxCombo:N0}x",
                    TextScale = 0.78f,
                    Alpha = 1
                };
                
                var comboTextSize = maxCombo.Font.MeasureString(maxCombo.Text);
                maxCombo.PosX = -comboTextSize.X * maxCombo.TextScale / 2f - 8;
                maxCombo.PosY = -comboTextSize.Y / 2f;

                var grade = new Sprite()
                {
                    Parent = display,
                    Image = GameBase.Skin.Grades[scores[i].Grade],
                    Alignment = Alignment.TopRight,
                    Size = new UDim2D(20, 20),
                    PosY = 2
                };
                Displays.Add(display);
            }
        }
    }
}