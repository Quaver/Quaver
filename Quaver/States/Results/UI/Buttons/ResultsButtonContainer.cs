using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.Database.Scores;
using Quaver.Graphics;
using Quaver.Graphics.Buttons;
using Quaver.Graphics.Sprites;
using Quaver.Main;
using Quaver.States.Gameplay;
using Quaver.States.Select;

namespace Quaver.States.Results.UI.Buttons
{
    internal class ResultsButtonContainer : HeaderedSprite
    {
        /// <summary>
        ///     Reference to the results screen itself.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     The list of buttons in this container.
        /// </summary>
        private List<TextButton> Buttons { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsButtonContainer(ResultsScreen screen)
                : base(new Vector2(GameBase.WindowRectangle.Width - 100, 125), "Actions", Fonts.AllerRegular16,
                    0.90f, Alignment.MidCenter, 50, Colors.DarkGray)
        {
            Screen = screen;

            Content = CreateContainer();
            Content.PosY = 50;
        }

        /// <summary>
        ///     Creates a button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        private static TextButton CreateButton(string text, EventHandler onClick)
        {
            var btn = new TextButton(new Vector2(), text);
            btn.Clicked += onClick;
            btn.TextSprite.Font = Fonts.AllerRegular16;
            btn.TextSprite.TextColor = Color.White;
            btn.TextSprite.TextScale = 0.85f;

            btn.OnUpdate += delegate(double dt)
            {
                if (btn.IsHovered)
                {

                }
            };

            return btn;
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <returns></returns>
        protected sealed override Sprite CreateContainer()
        {
            var content = new Sprite()
            {
                Parent = this,
                Size = new UDim2D(ContentSize.X, ContentSize.Y),
                Tint = Color.Black,
                Alpha = 0.45f
            };

            Buttons = new List<TextButton>
            {
                CreateButton("Back", (sender, args) =>
                {
                    GameBase.GameStateManager.ChangeState(new SongSelectState());
                }),
                CreateButton("Watch Replay", (sender, args) =>
                {
                    var scores = LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
                    GameBase.GameStateManager.ChangeState(new GameplayScreen(Screen.Qua, GameBase.SelectedMap.Md5Checksum, scores, Screen.Replay));
                }),
                CreateButton("Replay Map", (sender, args) =>
                {
                    var scores = LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
                    GameBase.GameStateManager.ChangeState(new GameplayScreen(Screen.Qua, GameBase.SelectedMap.Md5Checksum, scores));
                })
            };

            // Go through each button and initialize the sprite further.
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];

                btn.Parent = content;
                btn.Size = new UDim2D(200, content.SizeY * 0.60f);
                btn.Alignment = Alignment.MidLeft;
                btn.Tint = Color.White;
                btn.Alpha = 0.25f;

                var sizePer = SizeX / Buttons.Count;
                btn.PosX = sizePer * i + sizePer / 2f - btn.SizeX / 2f;
            }

            return content;
        }
    }
}