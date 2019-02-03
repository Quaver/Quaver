/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerEditor : Sprite
    {
        /// <summary>
        /// </summary>
        private JukeboxButton ReturnButton { get; set; }

        /// <summary>
        /// </summary>
        private Sprite HeaderBackground { get; set; }

        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        private EditorScreenView View { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextLayerName { get; set; }

        /// <summary>
        /// </summary>
        private Textbox NameTextbox { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextColor { get; set; }

        /// <summary>
        /// </summary>
        private Sprite SelectedColor { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton ButtonSelectRandomColor { get; set; }

        /// <summary>
        /// </summary>
        private Random RNG { get; } = new Random();

        /// <summary>
        /// </summary>
        private SpriteTextBitmap TextSelectedColor { get; set; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        public EditorLayerEditor(EditorScreen screen, EditorScreenView view)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorEditLayerPanel;

            CreateHeader();
            CreateColorEditor();

            View = view;
            // ReSharper disable once PossibleNullReferenceException
            View.LayerCompositor.SelectedLayerIndex.ValueChanged += OnSelectedLayerChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once PossibleNullReferenceException
            // ReSharper disable once DelegateSubtraction
            View.LayerCompositor.SelectedLayerIndex.ValueChanged -= OnSelectedLayerChanged;

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 34),
                Tint = Color.Transparent,
            };

            CreateReturnButton();
            CreateNameTextbox();
        }

        /// <summary>
        /// </summary>
        private void CreateReturnButton() => ReturnButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_long_arrow_pointing_to_the_right),
            (sender, args) =>
            {
                if (string.IsNullOrWhiteSpace(NameTextbox.RawText))
                {
                    NotificationManager.Show(NotificationLevel.Error, "Your layer name must not be empty ");
                    return;
                }

                var selectedLayer = View.LayerCompositor.ScrollContainer.AvailableItems[View.LayerCompositor.SelectedLayerIndex.Value];
                var colorString = $"{SelectedColor.Tint.R},{SelectedColor.Tint.G},{SelectedColor.Tint.B}";

                // Only change/add add an action if it was actually changed.
                if (NameTextbox.RawText != selectedLayer.Name || selectedLayer.ColorRgb != colorString && selectedLayer.ColorRgb != null)
                    Screen.Ruleset.ActionManager.EditLayer(View.LayerCompositor, selectedLayer, NameTextbox.RawText, colorString);

                Screen.ActiveLayerInterface.Value = EditorLayerInterface.Composition;
            })
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = Color.White,
            X = -8
        };

        /// <summary>
        /// </summary>
        private void CreateNameTextbox()
        {
            TextLayerName = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Layer Name")
            {
                Parent = this,
                X = 10,
                Y = HeaderBackground.Y + HeaderBackground.Height + 10,
                FontSize = 16
            };

            var selectedLayer = View?.LayerCompositor.ScrollContainer.AvailableItems[View.LayerCompositor.SelectedLayerIndex.Value];

            NameTextbox = new Textbox(new ScalableVector2(Width - 20, 26),
                Fonts.Exo2SemiBold, 13, selectedLayer?.Name)
            {
                Parent = this,
                X = TextLayerName.X,
                Y = TextLayerName.Y + TextLayerName.Height + 10,
                Tint = Color.Transparent,
                AllowSubmission = false
            };

            NameTextbox.AddBorder(Color.White, 2);
        }

        /// <summary>
        /// </summary>
        private void CreateColorEditor()
        {
            TextColor = new SpriteTextBitmap(FontsBitmap.AllerRegular, "Layer Color")
            {
                Parent = this,
                X = TextLayerName.X,
                Y = NameTextbox.Y + NameTextbox.Height + 20,
                FontSize = 16
            };

            SelectedColor = new Sprite
            {
                Parent = this,
                X = TextLayerName.X,
                Y = TextColor.Y + TextColor.Height + 10,
                Size = new ScalableVector2(24, 24),
                Tint = Color.White
            };

            ButtonSelectRandomColor = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_refresh_page_option), (sender, args) =>
            {
                SelectedColor.Tint = new Color(RNG.Next(255), RNG.Next(255), RNG.Next(255));
                TextSelectedColor.Text = $"(R:{SelectedColor.Tint.R}, G:{SelectedColor.Tint.G}, B:{SelectedColor.Tint.B})";
            })
            {
                Parent = this,
                Size = new ScalableVector2(18, 18),
                Alignment = Alignment.TopRight,
                Y = SelectedColor.Y,
                X = -10,
                Tint = Color.White
            };

            TextSelectedColor = new SpriteTextBitmap(FontsBitmap.AllerRegular, $"(R:{SelectedColor.Tint.R}, G:{SelectedColor.Tint.G}, B:{SelectedColor.Tint.B})")
            {
                Parent = SelectedColor,
                Alignment = Alignment.MidLeft,
                X = SelectedColor.Width + 10,
                FontSize = 16
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectedLayerChanged(object sender, BindableValueChangedEventArgs<int> e)
        {
            var selectedLayer = View.LayerCompositor.ScrollContainer.AvailableItems[e.Value];

            NameTextbox.RawText = selectedLayer.Name;
            NameTextbox.InputText.Text = selectedLayer.Name;

            SelectedColor.Tint = ColorHelper.ToXnaColor(selectedLayer.GetColor());
            TextSelectedColor.Text = $"(R:{SelectedColor.Tint.R}, G:{SelectedColor.Tint.G}, B:{SelectedColor.Tint.B})";
        }
    }
}
