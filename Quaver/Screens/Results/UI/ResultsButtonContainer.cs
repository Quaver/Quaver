using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Window;

namespace Quaver.Screens.Results.UI
{
    public class ResultsButtonContainer : Sprite
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

            Size = new ScalableVector2(WindowManager.Width - 100, 75);
            Tint = Color.Black;
            Alpha = 0.0f;

            InitializeButtons();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeButtons(gameTime.ElapsedGameTime.TotalMilliseconds);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates a button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        private static TextButton CreateButton(string text, EventHandler onClick)
        {
            var btn = new TextButton(UserInterface.BlankBox, BitmapFonts.Exo2Regular, text.ToUpper(), 16, onClick)
            {
                Text = {Tint = Color.White}
            };

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
                    SkinManager.Skin.SoundBack.CreateChannel().Play();
                    Screen.Exit(() => Screen.GoBackToMenu());
                }),
                // Export Replay Button
                CreateButton("Export Replay", (sender, args) =>
                {
                    SkinManager.Skin.SoundClick.CreateChannel().Play();
                    Screen.ExportReplay();
                }),
                // Retry Button
                CreateButton("Retry Map", (sender, args) =>
                {
                    SkinManager.Skin.SoundClick.CreateChannel().Play();
                    Screen.Exit(() => Screen.RetryMap());
                })
            };

            switch (Screen.ResultsScreenType)
            {
                case ResultsScreenType.FromGameplay:
                case ResultsScreenType.FromReplayFile:
                    // Watch Replay Button
                    Buttons.Add(CreateButton("Watch Replay", (sender, args) =>
                    {
                        SkinManager.Skin.SoundClick.CreateChannel().Play();

                        Screen.Exit(() => Screen.WatchReplay());
                    }));
                    break;
                case ResultsScreenType.FromLocalScore:
                    if (Screen.LocalReplayPath != null && File.Exists(Screen.LocalReplayPath))
                    {
                        // Watch Replay Button
                        Buttons.Add(CreateButton("Watch Replay", (sender, args) =>
                        {
                            SkinManager.Skin.SoundClick.CreateChannel().Play();

                            Screen.Exit(() => Screen.WatchReplay());
                        }));
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            // Go through each button and initialize the sprite further.
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];

                btn.Parent = this;
                btn.Size = new ScalableVector2(200, Height * 0.60f);
                btn.Alignment = Alignment.MidLeft;
                btn.Tint = Color.Black;
                btn.Alpha = 0.5f;

                var sizePer = Width / Buttons.Count;
                btn.X = sizePer * i + sizePer / 2f - btn.Width / 2f;
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
        public void ChangeSelected(Direction direction)
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
                SkinManager.Skin.SoundHover.CreateChannel().Play();
        }

        /// <summary>
        ///     Fires the event of the selected button.
        /// </summary>
        public void FireButtonEvent()
        {
            var btn = Buttons[SelectedButton];

            if (btn.IsClickable)
                btn.FireButtonClickEvent();
        }

        /// <summary>
        ///     Makes all the buttons in the container unclickable.
        /// </summary>
        public void MakeButtonsUnclickable() => Buttons.ForEach(x => x.IsClickable = false);

        /// <summary>
        ///     Makes all the buttons in the container clickable.
        /// </summary>
        public void MakeButtonsClickable() => Buttons.ForEach(x => x.IsClickable = false);
    }
}
