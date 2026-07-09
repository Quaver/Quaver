using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Main.UI.Nagivation
{
    public class NavigationButton : ImageButton
    {
        public Sprite Icon { get; private set; }

        /// <summary>
        /// </summary>
        public SpriteTextPlus Name { get; private set; }

        /// <summary>
        /// </summary>
        private Sprite HoverEffect { get; set; }

        /// <summary>
        /// </summary>
        public bool IsSelected { get; private set; }

        private static Texture2D DeselectedButton => SkinManager.Skin?.MainMenu?.NavigationButton ?? UserInterface.NavigationButton;

        private static Texture2D SelectedButton => SkinManager.Skin?.MainMenu?.NavigationButtonSelected ?? UserInterface.NavigationButtonSelected;

        private static Texture2D HoveredButton => SkinManager.Skin?.MainMenu?.NavigationButtonHovered ?? UserInterface.NavigationButtonHovered;

        private static ScalableVector2 OriginalSize = new ScalableVector2(434, 52);

        /// <summary>
        /// </summary>
        /// <param name="icon"></param>
        /// <param name="text"></param>
        /// <param name="clickAction"></param>
        public NavigationButton(Texture2D icon, string text, EventHandler clickAction = null)
            : base(DeselectedButton, clickAction)
        {
            Size = OriginalSize;

            CreateIcon(icon);
            CreateName(text);
            CreateHoverEffect();

            Hovered += (sender, args) => SkinManager.Skin?.SoundHover.CreateChannel().Play();
            Clicked += (sender, args) => SkinManager.Skin?.SoundClick.CreateChannel().Play();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            HoverEffect.Size = new ScalableVector2(Width - 4, Height - 4);
            HoverEffect.Alpha = IsHovered ? (SkinManager.Skin?.MainMenu?.NavigationButtonHoveredAlpha ?? 0.35f) : 0;

            base.Update(gameTime);
        }

        private void CreateIcon(Texture2D icon) => Icon = new Sprite
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            Size = new ScalableVector2(20, 20),
            X = 20,
            Image = icon,
            Tint = SkinManager.Skin?.MainMenu?.NavigationButtonTextColor ?? Color.White
        };

        private void CreateName(string name) => Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold),
            name.ToUpper(), 18)
        {
            Parent = this,
            Alignment = Alignment.MidLeft,
            X = Icon.X + Icon.Width + 14,
            Tint = SkinManager.Skin?.MainMenu?.NavigationButtonTextColor ?? Color.White
        };

        public void SetText(string text) => Name.Text = text.ToUpper();

        public void Select(bool instantWidth = false)
        {
            ClearWidthAnimations();

            var width = OriginalSize.X.Value + 20;

            if (instantWidth)
                Width = width;
            else
            {
                ChangeWidthTo((int) width, Easing.OutQuint, 450);
                SkinManager.Skin?.SoundHover.CreateChannel().Play();
            }

            Image = SelectedButton;
            IsSelected = true;
        }

        public void Deselect()
        {
            ClearWidthAnimations();
            ChangeWidthTo((int) OriginalSize.X.Value, Easing.OutQuint, 450);
            Image = DeselectedButton;
            IsSelected = false;
        }

        /// <summary>
        ///     Clears only the width-related animations, leaving other in-progress animations
        ///     (such as the entrance slide-in MoveToX from <see cref="NavigationButtonContainer.AlignButtons"/>)
        ///     untouched. Calling ClearAnimations() here would wipe that slide-in mid-flight if the
        ///     user navigates with the arrow keys right as the main menu is entered, leaving the
        ///     button stuck off-screen.
        /// </summary>
        private void ClearWidthAnimations()
        {
            lock (Animations)
            {
                for (var i = Animations.Count - 1; i >= 0; i--)
                {
                    if (Animations[i].Properties == AnimationProperty.Width)
                        Animations.RemoveAt(i);
                }
            }
        }

        private void CreateHoverEffect() => HoverEffect = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(Width - 4, Height - 4),
            Image = HoveredButton,
            Alpha = 0
        };
    }
}
