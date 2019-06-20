using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Result.UI.Multiplayer
{
    public class ResultMultiplayerScoreboardTableHeader : Sprite
    {
        /// <summary>
        /// </summary>
        private SpriteTextBitmap Ruleset { get; }

        /// <summary>
        /// </summary>
        public Dictionary<string, SpriteTextBitmap> Headers { get; private set; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        public ResultMultiplayerScoreboardTableHeader(int width)
        {
            Size = new ScalableVector2(width, 46);
            Tint = Color.Black;
            Alpha = 0.45f;
            Y = 2;
            X = 2;

            Ruleset = new SpriteTextBitmap(FontsBitmap.GothamRegular,
                OnlineManager.CurrentGame.Ruleset.ToString().Replace("_", " "))
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = 20,
                FontSize = 16,
                Tint = Colors.MainAccent,
            };

            CreateHeaders();

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(Width, 1),
                Alpha = 0.6f
            };
        }

        /// <summary>
        /// </summary>
        private void CreateHeaders()
        {
            var headers = new List<string>()
            {
                "Rating",
                "Grade",
                "Accuracy",
                "Max Combo",
                "Marv",
                "Perf",
                "Great",
                "Good",
                "Okay",
                "Miss",
                "Mods"
            };

            var lastWidth = 0f;
            var lastX = 0f;

            Headers = new Dictionary<string, SpriteTextBitmap>();

            for (var i = headers.Count - 1; i >= 0; i--)
            {
                // ReSharper disable once ObjectCreationAsStatement
                var txt = new SpriteTextBitmap(FontsBitmap.GothamRegular, headers[i])
                {
                    Parent = this,
                    Alignment = Alignment.MidRight,
                    FontSize = 16
                };

                if (i != headers.Count - 1)
                {
                    txt.X = lastX - lastWidth - 45;
                }
                else
                {
                    txt.X = -35;
                }

                lastWidth = txt.Width;
                lastX = txt.X;

                Headers.Add(headers[i], txt);
            }
        }
    }
}