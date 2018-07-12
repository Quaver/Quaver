using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
    internal class ResultsButtonContainer : Sprite
    {
        /// <summary>
        ///     Reference to the results screen itself.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     The list of buttons in this container.
        /// </summary>
        private List<BasicButton> Buttons { get; set; }

        /// <summary>
        ///     Ctor
        /// </summary>
        /// <param name="screen"></param>
        internal ResultsButtonContainer(ResultsScreen screen)
        {
            Screen = screen;

            Alpha = 0.35f;
            Size = new UDim2D(GameBase.WindowRectangle.Width - 100, 75);
            Tint = Color.Black;

            InitializeButtons();
        }

        private void InitializeButtons()
        {
            Buttons = new List<BasicButton>
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

            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];

                btn.Parent = this;
                btn.Size = new UDim2D(200, SizeY * 0.75f);
                btn.Alignment = Alignment.MidLeft;

                var sizePer = SizeX / Buttons.Count;
                btn.PosX = sizePer * i + sizePer / 2f - btn.SizeX / 2f;
            }
        }

        /// <summary>
        ///     Creates a button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        private static BasicButton CreateButton(string text, EventHandler onClick)
        {
            var btn = new BasicButton();
            btn.Clicked += onClick;

            return btn;
        }
    }
}