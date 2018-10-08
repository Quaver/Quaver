using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Screens.Edit;
using Quaver.Screens.Options;
using Quaver.Screens.Select.UI.Mods;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Screens;

namespace Quaver.Screens.Select.UI.MapInfo.Actions
{
    public class ActionsBanner : Sprite
    {
        public SelectScreen Screen { get; }

        public SelectScreenView View { get; }

        public MapInfoContainer Container { get; }

        /// <summary>
        ///     The list of buttons in this container.
        /// </summary>
        private List<TextButton> Buttons { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="view"></param>
        /// <param name="container"></param>
        public ActionsBanner(SelectScreen screen, SelectScreenView view, MapInfoContainer container)
        {
            Screen = screen;
            View = view;
            Container = container;

            Size = new ScalableVector2(container.Width, 50);
            Y = container.Leaderboard.Y + container.Leaderboard.Height + 5;
            Alignment = Alignment.TopCenter;
            Tint = Color.Black;
            Alpha = 0f;

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

        ///  <summary>
        ///  </summary>
        ///  <returns></returns>
        private void InitializeButtons()
        {
            Buttons = new List<TextButton>
            {
                // Back Button.
                CreateButton("Game Settings", (sender, args) =>
                {
                    var dialog = new OptionsDialog(0.75f);
                    dialog.ChangeSection(dialog.Sections[2]);
                    DialogManager.Show(dialog);
                }),
                // Watch Repaly Button
                CreateButton("Mods", (sender, args) =>
                {
                    DialogManager.Show(new ModsDialog());
                }),
                // Export Replay Button
                CreateButton("Edit Map", (sender, args) =>
                {
                    QuaverScreenManager.ChangeScreen(new EditorScreen(MapManager.Selected.Value.LoadQua()));
                }),
            };

            // Go through each button and initialize the sprite further.
            for (var i = 0; i < Buttons.Count; i++)
            {
                var btn = Buttons[i];

                btn.Parent = this;
                btn.Size = new ScalableVector2(200, Height * 0.60f);
                btn.Alignment = Alignment.MidLeft;
                btn.Alpha = 1f;
                btn.Tint = Colors.DarkGray;

                var sizePer = Width / Buttons.Count;
                btn.X = sizePer * i + sizePer / 2f - btn.Width / 2f;
            }
        }

        /// <summary>
        ///     Creates a button
        /// </summary>
        /// <param name="text"></param>
        /// <param name="onClick"></param>
        /// <returns></returns>
        private static TextButton CreateButton(string text, EventHandler onClick)
        {
            var btn = new TextButton(UserInterface.SearchBar, Fonts.Exo2Regular24, text.ToUpper(), 0.50f, onClick)
            {
                Text = {TextColor = Color.White}
            };

            return btn;
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
                btn.FadeToColor(btn.IsHovered ? Color.White : Colors.DarkGray, dt, 60);
            }
        }
    }
}