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
    internal class ResultsButtonContainer : Sprite
    {
        /// <summary>
        ///     Reference to the results screen itself.
        /// </summary>
        private ResultsScreen Screen { get; }

        /// <summary>
        ///     The list of buttons in this container.
        /// </summary>
        private List<TextButton> Buttons { get; set; }

        /// <summary>
        ///     The currently selected button
        /// </summary>
        private int SelectedButton { get; set; } = -1;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public ResultsButtonContainer(ResultsScreen screen)
        {
            Screen = screen;

            Size = new UDim2D(GameBase.WindowRectangle.Width - 100, 75);
            Tint = Color.Black;
            Alpha = 0.0f;

            InitializeButtons();
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
            btn.TextSprite.Font = Fonts.Exo2Regular24;
            btn.TextSprite.TextColor = Color.White;
            btn.TextSprite.TextScale = 0.50f;
            btn.TextSprite.Text = btn.TextSprite.Text.ToUpper();

            btn.OnUpdate += delegate(double dt) { btn.FadeToColor(btn.IsHovered ? Color.White : Color.Black, dt, 60); };
            return btn;
        }

        ///  <summary>
        ///  </summary>
        ///  <returns></returns>
        private void InitializeButtons()
        {
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
                CreateButton("Export Replay", (sender, args) =>
                {
                    Screen.ExportReplay();
                }),
                CreateButton("Retry Map", (sender, args) =>
                {
                    var scores = LocalScoreCache.FetchMapScores(GameBase.SelectedMap.Md5Checksum);
                    GameBase.GameStateManager.ChangeState(new GameplayScreen(Screen.Qua, GameBase.SelectedMap.Md5Checksum, scores));
                }),
            };

            // Go through each button and initialize the sprite further.
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];

                btn.Parent = this;
                btn.Size = new UDim2D(200, SizeY * 0.60f);
                btn.Alignment = Alignment.MidLeft;
                btn.Tint = Color.Black;
                btn.Alpha = 0.5f;

                var sizePer = SizeX / Buttons.Count;
                btn.PosX = sizePer * i + sizePer / 2f - btn.SizeX / 2f;
            }
        }
    }
}