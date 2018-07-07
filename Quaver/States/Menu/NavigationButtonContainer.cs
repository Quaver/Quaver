using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Audio;
using Quaver.Graphics;
using Quaver.Graphics.Base;
using Quaver.Helpers;
using Quaver.Main;

namespace Quaver.States.Menu
{
    internal class NavigationButtonContainer : Container
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
        ///     The original sizes of each button when the button was clicked.
        /// </summary>
        private Dictionary<NavigationButton, Vector2> OriginalSizes { get; set; }

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
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {
            if (ClickedButton != null)
                PerformClickAnimation(dt);
            
            base.Update(dt);
        }

        /// <summary>
        ///     When an item is clicked, it will perform an animation.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemClicked(object sender, EventArgs e)
        {
            OriginalSizes = new Dictionary<NavigationButton, Vector2>();
            
            Buttons.ForEach(x =>
            {
                x.IsClickable = false;         
                OriginalSizes.Add(x, x.AbsoluteSize);
            });

            // Set the clicked button (Makes it so that it starts performing the exit animation.
            ClickedButton = (NavigationButton) sender;
            ClickedButton.FooterAlwaysShown = true;
            
            GameBase.AudioEngine.PlaySoundEffect(SFX.Woosh);
        }

        /// <summary>
        ///     Performs an animation where the the clicked button is dragged
        ///     to the middle of the screen, and all the other buttons are exited from
        ///     the screen.
        /// </summary>
        /// <param name="dt"></param>
        private void PerformClickAnimation(double dt)
        {
            Buttons.ForEach(x =>
            {
                // Put the clicked button in the middle of the screen.
                if (x == ClickedButton)
                {
                    x.PosX = GraphicsHelper.Tween(0, x.PosX, Math.Min(dt / 60, 1));
                    x.PosY = GraphicsHelper.Tween(GameBase.WindowRectangle.Height / 2f - x.SizeY / 2f, x.PosY, Math.Min(dt / 60, 1));                
                }
                else
                {                    
                    var pos = x.AbsolutePosition;

                    // If the button is on the left side of the screen, then we want to lerp it left.
                    var width = pos.X + x.SizeX;
                    var middleScreen = GameBase.WindowRectangle.Width / 2f;
                    
                    if (width < middleScreen)
                    {
                        x.PosX = GraphicsHelper.Tween(-GameBase.WindowRectangle.Width - x.SizeX, x.PosX, Math.Min(dt / 240, 1));
                    }
                    else if (width > middleScreen)
                    {
                        x.PosX = GraphicsHelper.Tween(GameBase.WindowRectangle.Width + x.SizeX, x.PosX, Math.Min(dt / 240, 1));
                    }                  
                }
            });
        }
    }
}