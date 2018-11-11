using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Skinning;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Form;

namespace Quaver.Screens.Options
{
    public class OptionsItem : Sprite
    {
        /// <summary>
        ///     The name of the option's item
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     The form element to change the item's value.
        /// </summary>
        private Drawable FormElement { get; }

        /// <summary>
        ///     The parent dialog.
        /// </summary>
        private OptionsDialog Dialog { get; }

        /// <summary>
        ///     The text that states the name of the option
        /// </summary>
        private SpriteText NameText { get; set; }

        /// <summary>
        ///     The text that shows the value of the option. (Used only for some options.)
        /// </summary>
        private SpriteText ValueText { get; set; }

        /// <summary>
        ///     The callback function that runs when the option is changed.
        /// </summary>
        private Action OnChange { get; }

        /// <inheritdoc />
        /// <summary>
        ///     Create an OptionsItem with a Checkbox.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="checkbox"></param>
        /// <param name="onChange"></param>
        public OptionsItem(OptionsDialog dialog, string name, Checkbox checkbox, Action onChange = null)
        {
            Name = name;
            FormElement = checkbox ?? throw new ArgumentNullException(nameof(checkbox));
            Dialog = dialog;
            OnChange = onChange;

            // Style the actual container first
            InitializeContainer();

            // Style the checkbox afterwards
            checkbox.Parent = this;
            checkbox.Alignment = Alignment.MidRight;
            checkbox.Size = new ScalableVector2(Height * 0.50f, Height * 0.50f);
            checkbox.X = -checkbox.Width / 2f - 15f;

            // Add event handlers for when the value changes.
            checkbox.BindedValue.ValueChanged += OnValueChanged;
            checkbox.BindedValue.ValueChanged += PlayCheckboxClickSoundEffect;

            CreateNameText();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an OptionsItem with a HorizontalSelector
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="selector"></param>
        public OptionsItem(OptionsDialog dialog, string name, HorizontalSelector selector)
        {
            Name = name;
            FormElement = selector ?? throw new ArgumentNullException(nameof(selector));
            Dialog = dialog;

            // Style the actual container first
            InitializeContainer();

            // Style selector.
            selector.Parent = this;
            selector.Alignment = Alignment.MidRight;
            selector.Size = new ScalableVector2(200, 30);

            selector.ButtonSelectLeft.Size = new ScalableVector2(selector.Height, selector.Height);
            selector.ButtonSelectLeft.X -= 15;
            selector.ButtonSelectRight.Size = new ScalableVector2(selector.Height, selector.Height);
            selector.ButtonSelectRight.X += 15;

            selector.X -= selector.Width / 4f - 5;

            CreateNameText();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Creates an OptionsItem with a slider.
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="slider"></param>
        public OptionsItem(OptionsDialog dialog, string name, Slider slider)
        {
            Name = name;
            FormElement = slider ?? throw new ArgumentNullException(nameof(slider));
            Dialog = dialog;

            // Style the actual container first
            InitializeContainer();

            slider.Parent = this;
            slider.Alignment = Alignment.MidRight;
            slider.Width = 350;
            slider.Height = 2;
            slider.X -= 80;

            ValueText = new SpriteText(BitmapFonts.Exo2Regular, slider.BindedValue.Value.ToString(), 18)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
            };

            ValueText.X -= ValueText.Width + 20;

            // Set the OnChange function to just update the value of the text.
            OnChange = () => { ValueText.Text = slider.BindedValue.Value.ToString(); };

            // Subscribe to bindable change event.
            slider.BindedValue.ValueChanged += OnValueChanged;

            CreateNameText();
        }

        /// <inheritdoc />
        /// <summary>
        ///     Create an OptionsItem with a button
        /// </summary>
        /// <param name="dialog"></param>
        /// <param name="name"></param>
        /// <param name="button"></param>
        public OptionsItem(OptionsDialog dialog, string name, TextButton button)
        {
            Name = name;
            FormElement = button ?? throw new ArgumentNullException(nameof(button));
            Dialog = dialog;

            // Style the actual container first
            InitializeContainer();

            button.Parent = this;
            button.Alignment = Alignment.MidRight;
            button.Size = new ScalableVector2(200, 30);
            button.Text.Tint = Color.Black;
            button.Tint = Colors.SecondaryAccent;
            button.X -= 10;

            CreateNameText();
        }

        /// <summary>
        ///     Used in the constructors of the OptionsItem to not duplicate code.
        /// </summary>
        private void InitializeContainer()
        {
            Parent = Dialog;
            Tint = Color.Black;
            Alpha = 0.65f;
            Size = new ScalableVector2(Dialog.ContentContainer.Width - 160, 40);
            Y = Dialog.HeaderContainer.Height + 15;
            X = 140;
        }

        /// <summary>
        ///     Creates the actual item sprite.
        /// </summary>
        private void CreateNameText()
        {
            NameText = new SpriteText(BitmapFonts.Exo2Regular, Name, 18)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
            };

            NameText.X += NameText.Width + 15;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            var type = FormElement.GetType();

            if (type == typeof(Checkbox))
            {
                var checkbox = (Checkbox) FormElement;

                // ReSharper disable once DelegateSubtraction
                checkbox.BindedValue.ValueChanged -= OnValueChanged;

                // ReSharper disable once DelegateSubtraction
                checkbox.BindedValue.ValueChanged -= PlayCheckboxClickSoundEffect;
            }
            else if (type == typeof(Slider))
            {
                var slider = (Slider) FormElement;

                // ReSharper disable once DelegateSubtraction
                slider.BindedValue.ValueChanged -= OnValueChanged;
            }

            base.Destroy();
        }

        /// <summary>
        ///     When the value has changed, this will be called.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OnValueChanged<T>(object sender, BindableValueChangedEventArgs<T> args) => OnChange?.Invoke();

        /// <summary>
        ///     Play a sound effect when the checkbox is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private static void PlayCheckboxClickSoundEffect(object sender, BindableValueChangedEventArgs<bool> args)
        {
            if (args.Value)
                SkinManager.Skin.SoundClick.CreateChannel().Play();
            else
                SkinManager.Skin.SoundBack.CreateChannel().Play();
        }
    }
}
