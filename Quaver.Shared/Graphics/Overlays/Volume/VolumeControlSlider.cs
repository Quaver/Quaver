using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Graphics.Overlays.Volume
{
    public class VolumeControlSlider : Container
    {
        /// <summary>
        /// </summary>
        public readonly BindableInt BindedValue;

        /// <summary>
        /// </summary>
        private Sprite Icon { get; set; }

        /// <summary>
        /// </summary>
        public Slider Slider{ get; private set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Percentage { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Name { get; set; }

        /// <summary>
        /// </summary>
        private const int Spacing = 18;

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public VolumeControlSlider(float width, Texture2D icon, string name, BindableInt value)
        {
            BindedValue = value;

            CreateIcon(icon);
            CreateSlider(width);
            CreatePercentage();
            CreateName(name);

            Size = new ScalableVector2(0, Name.Height + -Name.Y + Percentage.Height);

            BindedValue.ValueChanged += OnValueChanged;
            BindedValue.TriggerChangeEvent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            BindedValue.ValueChanged -= OnValueChanged;
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        public void Select()
        {
            Icon.Tint = ColorHelper.HexToColor("#45D6F5");
            Percentage.Tint = Icon.Tint;
        }

        /// <summary>
        /// </summary>
        public void Deselect()
        {
            Icon.Tint = Color.White;
            Percentage.Tint = Color.White;
        }

        /// <summary>
        /// </summary>
        /// <param name="icon"></param>
        private void CreateIcon(Texture2D icon)
        {
            Icon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                Image = icon,
                Size = new ScalableVector2(icon.Width, icon.Height)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateSlider(float width)
        {
            var ballTexture = UserInterface.VolumeSliderProgressBall;

            Slider = new Slider(BindedValue, new Vector2(width, 4), ballTexture)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Icon.X + Icon.Width + Spacing
            };

            Slider.ActiveColor.Image = UserInterface.VolumeSliderActive;
            Slider.Image = UserInterface.VolumeSliderInactive;
        }

        /// <summary>
        /// </summary>
        private void CreatePercentage()
        {
            Percentage = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "100%", 22)
            {
                Parent = this,
                Alignment = Alignment.MidLeft,
                X = Slider.X + Slider.Width + Spacing
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        private void CreateName(string name)
        {
            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name.ToUpper(), 20)
            {
                Parent = Slider,
                Y = -36
            };
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnValueChanged(object sender, BindableValueChangedEventArgs<int> e)
            => ScheduleUpdate(() => Percentage.Text = $"{e.Value}%");
    }
}