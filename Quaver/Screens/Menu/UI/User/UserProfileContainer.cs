using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Config;
using Quaver.Graphics;
using Quaver.Graphics.Online;
using Quaver.Graphics.Online.Playercard;
using Quaver.Screens.Menu.UI.Navigation;
using Wobble.Graphics;
using Wobble.Graphics.BitmapFonts;
using Wobble.Graphics.Primitives;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Logging;
using Color = Microsoft.Xna.Framework.Color;

namespace Quaver.Screens.Menu.UI.User
{
    public class UserProfileContainer : ScrollContainer
    {
        /// <summary>
        ///     Reference to the parent menu screern view.
        /// </summary>
        private MenuScreenView View { get; }

        /// <summary>
        ///     The original width of the profile container.
        /// </summary>
        private const int OriginalWidth = 500;

        /// <summary>
        ///     The original height of the profile container.
        /// </summary>
        private const int OriginalHeight = 250;

        /// <summary>
        ///     The container for the user profile.
        ///
        ///     Note: The container itself is a button to prevent clicking on objects under.
        /// </summary>
        private ImageButton Container { get; set; }

        /// <summary>
        ///     Reference to the navbar button.
        /// </summary>
        private NavbarItemUser NavbarButton { get; }

        /// <summary>
        ///     The line at the bottom of the container.
        /// </summary>
        private Sprite BottomLine { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public UserProfileContainer(MenuScreenView view) : base(new ScalableVector2(OriginalWidth, 0),
            new ScalableVector2(OriginalWidth, OriginalHeight))
        {
            View = view;

            Tint = Color.Black;
            Alpha = 0.85f;
            Scrollbar.Visible = false;

            NavbarButton = View.Navbar.RightAlignedItems.First() as NavbarItemUser;

            if (NavbarButton == null)
                throw new InvalidOperationException("Tried to get NavbarItemUser, but it's null!");

            CreateContainer();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Container.IsClickable = NavbarButton.Selected;
            BottomLine.Visible = NavbarButton.Selected;
            base.Update(gameTime);
        }

        /// <summary>
        ///     When the navbar button is clicked, this will
        /// </summary>
        public void PerformClickAnimation(bool selected)
        {
            lock (Transformations)
            {
                Transformations.Clear();

                var targetHeight = selected ? OriginalHeight : 0;
                Transformations.Add(new Transformation(TransformationProperty.Height, Easing.EaseOutQuint, Height, targetHeight, 500));
            }
        }

        /// <summary>
        ///     Creates the container where everything will live.
        /// </summary>
        private void CreateContainer()
        {
            Container = new ImageButton(UserInterface.BlankBox)
            {
                Size = ContentContainer.Size,
                Alpha = 0
            };

            Container.ClickedOutside += (sender, args) =>
            {
                // User clicked the navbar button, that handles closing automatically.
                if (NavbarButton.IsHovered)
                    return;

                NavbarButton.Selected = false;
                PerformClickAnimation(false);
            };

            BottomLine = new Sprite()
            {
                Parent = this,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(OriginalWidth, 3),
                Tint = Color.White,
                Visible = false
            };

            AddContainedDrawable(Container);
        }
    }
}