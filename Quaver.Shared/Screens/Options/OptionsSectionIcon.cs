using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Graphics;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Options
{
    public class OptionsSectionIcon : Button
    {
        /// <summary>
        ///     The actual icon sprite.
        /// </summary>
        private Sprite Icon { get; }

        /// <summary>
        ///     A flag that displays to the left of the button if it is selected.
        /// </summary>
        private Sprite SelectedFlag { get; }

        /// <summary>
        ///     If the button is currently selected.
        /// </summary>
        public bool IsSelected { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="iconImage"></param>
        /// <param name="isSelected"></param>
        public OptionsSectionIcon(Texture2D iconImage, bool isSelected = false)
        {
            IsSelected = isSelected;
            Size = new ScalableVector2(65, 65);

            Icon = new Sprite()
            {
                Parent = this,
                Image = iconImage,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(25, 25)
            };

            SelectedFlag = new Sprite()
            {
                Parent = this,
                Visible = false,
                Size = new ScalableVector2(5, Height),
                Tint = Colors.MainAccent,
                X = 0
            };

            ChangeSelectedStyle();
        }

        public override void Update(GameTime gameTime)
        {
            if (!IsSelected)
                FadeToColor(IsHovered ? Color.LightGray : Colors.DarkGray, gameTime.ElapsedGameTime.TotalMilliseconds, 60);

            base.Update(gameTime);
        }

        /// <summary>
        ///     Changes/Creates the button's style based on if it is selected or not.
        /// </summary>
        private void ChangeSelectedStyle()
        {
            if (IsSelected)
            {
                Tint = Color.White;
                Icon.Tint = Color.Black;
                SelectedFlag.Visible = true;
            }
            else
            {
                Tint = Colors.DarkGray;
                Icon.Tint = Color.White;
                SelectedFlag.Visible = false;
            }
        }

        /// <summary>
        ///     Sets the button in a selected state.
        /// </summary>
        public void Select()
        {
            IsSelected = true;
            ChangeSelectedStyle();
        }

        /// <summary>
        ///     Sets the button in a deselected state
        /// </summary>
        public void Deselect()
        {
            IsSelected = false;
            ChangeSelectedStyle();
        }
    }
}
