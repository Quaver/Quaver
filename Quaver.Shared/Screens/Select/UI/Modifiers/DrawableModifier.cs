using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Modifiers;
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
        ///     The modifier that this is for.
        /// </summary>
        public IGameplayModifier Modifier { get; }

        /// <summary>
        ///     Text that displays the name of the modifier.
        /// </summary>
        public SpriteText ModifierName { get; private set; }

        /// <summary>
        ///     Text that displays the description of the modifier.
        /// </summary>
        public SpriteText ModifierDescription { get; private set; }

        /// <summary>
        ///     The list of options
        /// </summary>
        public List<DrawableModifierOption> Options { get; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="dialog"></param>
        ///  <param name="modifier"></param>
        public DrawableModifier(ModifiersDialog dialog, IGameplayModifier modifier)
        {
            Dialog = dialog;
            Modifier = modifier;

            Size = new ScalableVector2(dialog.Width, 60);
            Tint = Color.Black;

            CreateModifierName();
            CreateModifierDescription();

            // ReSharper disable once VirtualMemberCallInConstructor
            Options = CreateModsDialogOptions();

            // ReSharper disable once VirtualMemberCallInConstructor
            ChangeSelectedOptionButton();

            AlignOptions();

            UsePreviousSpriteBatchOptions = true;
            ModManager.ModsChanged += OnModsChanged;
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
        ///     Creates the text that states the modifier's name.
        /// </summary>
        private void CreateModifierName() => ModifierName = new SpriteText(BitmapFonts.Exo2Bold, Modifier.Name , 13)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Y = -10,
            X = 65
        };

        /// <summary>
        ///     Creates the text that displays the modifier's description.
        /// </summary>
        private void CreateModifierDescription() => ModifierDescription = new SpriteText(BitmapFonts.Exo2SemiBold, Modifier.Description, 12, false)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Y = ModifierName.Height - 10,
            X = ModifierName.X,
            UsePreviousSpriteBatchOptions = true
        };

        /// <summary>
        ///    Creates the dialog options for the mods.
        /// </summary>
        /// <returns></returns>
        public abstract List<DrawableModifierOption> CreateModsDialogOptions();

        /// <summary>
        ///     Changes the selected option button (visually)
        /// </summary>
        public abstract void ChangeSelectedOptionButton();

        /// <summary>
        ///     Aligns the options. properly.
        /// </summary>
        private void AlignOptions()
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