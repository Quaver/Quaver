using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyGamesFound : Container
    {
        /// <summary>
        ///    The amount of matches
        /// </summary>
        public SpriteTextPlus TextCount { get; private set; }

        /// <summary>
        ///     The text that displays "Matches Found"
        /// </summary>
        public SpriteTextPlus TextMatchesFound { get; private set; }

        /// <summary>
        ///     The amount of space between <see cref="TextCount"/> and <see cref="TextSpacing"/>
        /// </summary>
        private const int TextSpacing = 4;

        /// <summary>
        /// </summary>
        public MultiplayerLobbyGamesFound()
        {
            CreateTextCount();
            CreateTextMatchesFound();

            UpdateText();
        }

        /// <summary>
        /// </summary>
        private void CreateTextCount()
        {
            TextCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0", 22)
            {
                Parent = this,
                Tint = Colors.MainAccent
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextMatchesFound()
        {
            TextMatchesFound = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Matches Found", 22)
            {
                Parent = this,
                X = TextCount.Width + TextSpacing
            };
        }

        /// <summary>
        ///     Updates the text with the proper state and updates the container size
        /// </summary>
        private void UpdateText()
        {
            ScheduleUpdate(() =>
            {
                var count = 0;

                TextCount.Text = $"{count:n0}";

                if (count == 0 || count > 1)
                    TextMatchesFound.Text = "MATCHES FOUND";
                else
                    TextMatchesFound.Text = "MATCH FOUND";

                TextMatchesFound.X = TextCount.Width + TextSpacing;

                Size = new ScalableVector2((int) (TextCount.Width + TextSpacing + TextMatchesFound.Width),
                    (int) TextMatchesFound.Height);
            });
        }
    }
}