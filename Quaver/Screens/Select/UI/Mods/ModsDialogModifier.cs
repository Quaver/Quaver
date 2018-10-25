using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Modifiers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Screens.Select.UI.Mods
{
    public abstract class ModsDialogModifier : Sprite
    {
        /// <summary>
        ///     Reference to the parent dialog
        /// </summary>
        public ModsDialog Dialog { get; }

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
        public List<ModsDialogOption> Options { get; }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="dialog"></param>
        ///  <param name="modifier"></param>
        public ModsDialogModifier(ModsDialog dialog, IGameplayModifier modifier)
        {
            Dialog = dialog;
            Modifier = modifier;

            Size = new ScalableVector2(dialog.Width, 50);
            Tint = Color.Black;

            CreateModifierName();
            CreateModifierDescription();

            // ReSharper disable once VirtualMemberCallInConstructor
            Options = CreateModsDialogOptions();

            // ReSharper disable once VirtualMemberCallInConstructor
            ChangeSelectedOptionButton();

            AlignOptions();

            ModManager.ModsChanged += OnModsChanged;
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
        private void CreateModifierName()
        {
            ModifierName = new SpriteText(BitmapFonts.Exo2BoldItalic, Modifier.Name, 14)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = -10
            };

            ModifierName.X += ModifierName.Width + 65;
        }

        /// <summary>
        ///     Creates the text that displays the modifier's description.
        /// </summary>
        private void CreateModifierDescription()
        {
            ModifierDescription = new SpriteText(BitmapFonts.Exo2BoldItalic, Modifier.Description, 12)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Y = ModifierName.Height + 2
            };

            ModifierDescription.X += ModifierDescription.Width + 65;
        }

        /// <summary>
        ///    Creates the dialog options for the mods.
        /// </summary>
        /// <returns></returns>
        public abstract List<ModsDialogOption> CreateModsDialogOptions();

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