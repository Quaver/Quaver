using System;
using System.Drawing;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics.Online;
using Quaver.Screens.Menu.UI.Navigation;
using Wobble.Graphics;
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
        ///     The logged in user's playercard.
        /// </summary>
        private Playercard Playercard { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        public UserProfileContainer(MenuScreenView view) : base(new ScalableVector2(OriginalWidth, 0),
            new ScalableVector2(OriginalWidth, OriginalHeight))
        {
            View = view;

            Tint = Color.Black;
            Alpha = 0.75f;
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

            CreatePlayercard();
            AddContainedDrawable(Container);
        }

        private void CreatePlayercard()
        {
            Playercard = new Playercard()
            {
                Parent = Container,
                Alignment = Alignment.TopCenter,
                Y = 15
            };
        }
    }
}