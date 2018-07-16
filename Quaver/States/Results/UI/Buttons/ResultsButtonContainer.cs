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
        ///
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            FadeButtons(dt);

            base.Update(dt);
        }

        /// <summary>
        ///     Creates a button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        private static TextButton CreateButton(string text, EventHandler onClick)
        {
            var btn = new TextButton(new Vector2(), text)
            {
                TextSprite =
                {
                    Font = Fonts.Exo2Regular24,
                    TextColor = Color.White,
                    TextScale = 0.50f,
                    Text = text.ToUpper()
                }
            };

            btn.Clicked += onClick;

            return btn;
        }

        ///  <summary>
        ///  </summary>
        ///  <returns></returns>
        private void InitializeButtons()
        {
            Buttons = new List<TextButton>
            {
                // Back Button.
                CreateButton("Back", (sender, args) =>
                {
                    GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundBack);
                    Screen.Exit(() => Screen.GoBackToMenu());
                }),
                // Watch Repaly Button
                CreateButton("Watch Replay", (sender, args) =>
                {
                    GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
                    Screen.Exit(() => Screen.WatchReplay());
                }),
                // Export Replay Button
                CreateButton("Export Replay", (sender, args) =>
                {
                    GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
                    Screen.ExportReplay();
                }),
                // Retry Button
                CreateButton("Retry Map", (sender, args) =>
                {
                    GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundClick);
                    Screen.Exit(() => Screen.RetryMap());
                })
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

            // If we actually do have buttons here, then we'll want to default the selected
            // button to the first.
            if (Buttons.Count > 0)
                SelectedButton = 0;
        }

        /// <summary>
        ///    Makes sure only hovered of selected buttons are faded to the correct colors.
        /// </summary>
        /// <param name="dt"></param>
        private void FadeButtons(double dt)
        {
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];
                btn.FadeToColor(btn.IsHovered || i == SelectedButton ? Color.White : Color.Black, dt, 60);
            }
        }

        /// <summary>
        ///     Changes the selected button to
        /// </summary>
        /// <param name="direction"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        internal void ChangeSelected(Direction direction)
        {
            var prevSelected = SelectedButton;

            switch (direction)
            {
                case Direction.Forward:
                    if (SelectedButton + 1 < Buttons.Count)
                        SelectedButton = SelectedButton + 1;
                    break;
                case Direction.Backward:
                    if (SelectedButton - 1 >= 0)
                        SelectedButton = SelectedButton - 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, null);
            }

            if (SelectedButton != prevSelected)
                GameBase.AudioEngine.PlaySoundEffect(GameBase.Skin.SoundHover);
        }

        /// <summary>
        ///     Fires the event of the selected button.
        /// </summary>
        internal void FireButtonEvent()
        {
            var btn = Buttons[SelectedButton];
            btn.Clicked?.Invoke(btn, EventArgs.Empty);
        }
    }
}