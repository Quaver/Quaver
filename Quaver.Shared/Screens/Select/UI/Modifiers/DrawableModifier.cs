/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Input;

namespace Quaver.Shared.Screens.Select.UI.Modifiers
{
    public abstract class DrawableModifier : Sprite
    {
        /// <summary>
        ///     Reference to the parent dialog
        /// </summary>
        public ModifiersDialog Dialog { get; }

        /// <summary>
        ///     Text that displays the name of the modifier.
        /// </summary>
        private SpriteText _nameText;
        protected string Name
        {
            set
            {
                _nameText.Text = value;

                // Modifier description and ranked icon depend on the height of the name.
                UpdateModifierDescription();
                UpdateRankedStatusIcon();
            }
        }

        /// <summary>
        ///     Text that displays the description of the modifier.
        /// </summary>
        private SpriteText _descriptionText;
        private string _description;
        protected string Description
        {
            set
            {
                _description = value;
                UpdateModifierDescription();
            }
        }

        /// <summary>
        ///     Displays the ranked status of the modifier.
        /// </summary>
        private Sprite _rankedStatusIcon;
        private bool _isRanked;
        protected bool IsRanked
        {
            set
            {
                _isRanked = value;
                UpdateRankedStatusIcon();
            }
        }

        /// <summary>
        ///     The list of options
        /// </summary>
        public List<DrawableModifierOption> Options { get; }

        /// <summary>
        ///     Creates a new DrawableModifier.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="description"></param>
        /// <param name="isRanked"></param>
        public DrawableModifier(ModifiersDialog dialog, string name, string description, bool isRanked)
        {
            Dialog = dialog;

            Size = new ScalableVector2(dialog.Width, 60);
            Tint = Color.Black;

            CreateModifierName();
            CreateModifierDescription();
            CreateRankedStatusIcon();

            Name = name;
            Description = description;
            IsRanked = isRanked;
            Options = new List<DrawableModifierOption>();

            UsePreviousSpriteBatchOptions = true;
            ModManager.ModsChanged += OnModsChanged;
        }

        /// <summary>
        ///     Creates a new DrawableModifier with name, description and ranked status taken from the given modifier.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="modifier"></param>
        public DrawableModifier(ModifiersDialog dialog, IGameplayModifier modifier)
            : this(dialog, modifier.Name, modifier.Description, modifier.Ranked)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            FadeToColor(GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position)
                ? ColorHelper.HexToColor("#313131")
                : Color.Black, gameTime.ElapsedGameTime.TotalMilliseconds, 30);

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            ModManager.ModsChanged -= OnModsChanged;
            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that displays the modifier's name.
        /// </summary>
        private void CreateModifierName() =>
            _nameText = new SpriteText(Fonts.Exo2Bold, "", 13)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -10,
                X = 65
            };


        /// <summary>
        ///     Creates the text that displays the modifier's description.
        /// </summary>
        private void CreateModifierDescription() =>
            _descriptionText = new SpriteText(Fonts.Exo2SemiBold, "", 12)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true
            };


        /// <summary>
        ///     Updates the text that displays the modifier's description.
        /// </summary>
        private void UpdateModifierDescription()
        {
            _descriptionText.Text = _description;
            _descriptionText.Y = _nameText.Height - 10;
            _descriptionText.X = _nameText.X;
        }

        /// <summary>
        ///     Creates the sprite that displays the ranked status of the mod.
        /// </summary>
        private void CreateRankedStatusIcon() =>
            _rankedStatusIcon = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                UsePreviousSpriteBatchOptions = true,
                Height = 30,
                Width = 30,
            };

        /// <summary>
        ///     Updates the sprite that displays the ranked status of the mod.
        /// </summary>
        private void UpdateRankedStatusIcon()
        {
            _rankedStatusIcon.Image = _isRanked ? UserInterface.NotificationSuccess : UserInterface.NotificationWarning;
            _rankedStatusIcon.Y = _nameText.Height - 20;
            _rankedStatusIcon.X = _nameText.X / 2f - _rankedStatusIcon.Width / 2f;
        }

        /// <summary>
        ///     Changes the selected option button (visually)
        /// </summary>
        public abstract void ChangeSelectedOptionButton();

        /// <summary>
        ///     Aligns the options. properly.
        /// </summary>
        protected void AlignOptions()
        {
            for (var i = Options.Count - 1; i >= 0; i--)
            {
                var option = Options[i];

                option.Alignment = Alignment.MidRight;

                if (i == Options.Count - 1)
                {
                    option.X = -25;
                }
                else
                {
                    var previousOption = Options[i + 1];
                    option.X = previousOption.X - previousOption.Width - 10;
                }
            }
        }

        private void OnModsChanged(object sender, ModsChangedEventArgs e) => ChangeSelectedOptionButton();
    }
}
