using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Quaver.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Window;

namespace Quaver.Screens.Menu.UI.Buttons.Navigation
{
    public class NavigationButtonContainer : Container
    {
        /// <summary>
        ///     The list of navigation buttons that are in the container.
        /// </summary>
        private List<NavigationButton> Buttons { get; }

        /// <summary>
        ///     The button that was clicked, if any.
        ///     If this is not null, then an animation will perform.
        /// </summary>
        private NavigationButton ClickedButton { get; set; }

        /// <summary>
        ///     If the click action has already been called, to
        ///     prevent it from being called multiple times.
        /// </summary>
        private bool ClickActionCalled { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor -
        /// </summary>
        /// <param name="buttons"></param>
        internal NavigationButtonContainer(List<NavigationButton> buttons)
        {
            Buttons = buttons;

            // For every button, add an event handler when it's clicked, so that we are aware of when
            // this event happens.
            Buttons.ForEach(x =>
            {
                x.Parent = this;
                x.Clicked += OnItemClicked;
            });
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (ClickedButton != null)
                PerformClickAnimation(gameTime);

            base.Update(gameTime);
        }

        /// <summary>
        ///     When an item is clicked, it will perform an animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemClicked(object sender, EventArgs e)
        {
            // Get the button that was clicked.
            var button = (NavigationButton)sender;

            // If the button's action was set to be called immediately, then
            // we'll want to just call whatever action was there and disregard the entire
            // animation process.
            if (button.CallActionImmediately)
            {
                button.OnClick();
                return;
            }

            Buttons.ForEach(x =>
            {
                x.IsClickable = false;
            });

            // Set the clicked button (Makes it so that it starts performing the exit animation.
            // We set this down here as opposed to above because WHENEVER this is set, it will
            // perform the animation. We don't want to perform any animations if the button
            // was set to call its action immediately.
            ClickedButton = button;
            ClickedButton.FooterAlwaysShown = true;

            // TODO: Play effect
        }

        /// <summary>
        ///     Performs an animation where the the clicked button is dragged
        ///     to the middle of the screen, and all the other buttons are exited from
        ///     the screen.
        /// </summary>
        private void PerformClickAnimation(GameTime gameTime)
        {
            // Keeps track of if the animation is actually done,
            // when it is, we'll call the OnClick action that the button
            // has.
            var animationDone = false;

            // Perform animations for each button.
            Buttons.ForEach(btn =>
            {
                // Put the clicked button in the middle of the screen.
                if (btn == ClickedButton)
                {
                    var targetPosition = new Vector2(0, WindowManager.Height / 2f - btn.Height / 2f);

                    btn.X = MathHelper.Lerp(btn.X, targetPosition.X, (float) Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));
                    btn.Y = MathHelper.Lerp(btn.Y, targetPosition.Y, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 60, 1));

                    if (Math.Abs(btn.X - targetPosition.X) < 0.02f && Math.Abs(btn.Y - targetPosition.Y) < 0.02f)
                        animationDone = true;
                }
                else
                {
                    // If the button is on the left side of the screen, then we want to lerp it left.
                    var width = btn.AbsolutePosition.X + btn.Width;
                    var middleScreen = WindowManager.Width / 2f;

                    // Based on where the button is, we'll want to move it in the horizontal direction closest
                    // to where it can go off-screen.
                    if (width < middleScreen)
                        btn.X = MathHelper.Lerp(btn.X, -WindowManager.Width - btn.Width, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 240, 1));
                    else if (width > middleScreen)
                        btn.X = MathHelper.Lerp(btn.X, WindowManager.Width + btn.Width, (float)Math.Min(GameBase.Game.TimeSinceLastFrame / 240, 1));
                }
            });

            // Call click action if the animation was completed.
            if (animationDone && !ClickActionCalled)
            {
                ClickActionCalled = true;
                ClickedButton.OnClick();
            }
        }
    }
}
