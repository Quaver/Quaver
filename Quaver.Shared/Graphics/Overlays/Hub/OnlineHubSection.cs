using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Graphics.Overlays.Hub
{
    public abstract class OnlineHubSection : IUpdate
    {
        /// <summary>
        /// </summary>
        protected OnlineHub Hub { get; }

        /// <summary>
        ///     The name of the section
        /// </summary>
        public abstract string Name { get; }

        /// <summary>
        /// </summary>
        public Sprite Container { get; set; }

        /// <summary>
        /// </summary>
        public IconButton Icon { get; private set; }

        /// <summary>
        /// </summary>
        private Color IconColor => ColorHelper.HexToColor("#808080");

        /// <summary>
        /// </summary>
        public OnlineHubSection(OnlineHub hub)
        {
            Hub = hub;

            CreateContainer();
            CreateIcon();

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public void Update(GameTime gameTime)
        {
            AnimateIcon();
        }

        /// <summary>
        /// </summary>
        private void CreateContainer()
        {
            Container = new Sprite()
            {
                Alpha = 0,
                DestroyIfParentIsNull = false,
                Size = new ScalableVector2(Hub.Width, Hub.Height - Hub.IconContainer.Height - Hub.IconContainer.Y)
            };
        }

        /// <summary>
        /// </summary>
        private void CreateIcon()
        {
            Icon = new IconButton(GetIcon(), (sender, args) => Hub.SelectSection(this))
            {
                Size = new ScalableVector2(42, 42),
                Tint = IconColor
            };
        }

        /// <summary>
        /// </summary>
        protected void CreateNotImplementedText()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), $"{Name}\nARE NOT IMPLEMENTED YET.\nCHECK BACK LATER!",
                22)
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// </summary>
        private void AnimateIcon()
        {
            if (Hub.SelectedSection == this)
            {
                Icon.IsPerformingFadeAnimations = false;
                Icon.Tint = ColorHelper.HexToColor("#0FBAE5");
                Icon.Alpha = 1;
            }
            else
            {
                Icon.Tint = IconColor;
                Icon.IsPerformingFadeAnimations = true;
            }
        }

        /// <summary>
        ///     The icon of the section
        /// </summary>
        /// <returns></returns>
        public abstract Texture2D GetIcon();

        /// <summary>
        ///     Creates the content for the section
        /// </summary>
        public abstract void CreateContent();
    }
}