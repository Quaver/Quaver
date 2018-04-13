using System;
using Microsoft.Xna.Framework;
using Quaver.Config;
using Quaver.GameState;
using Quaver.Graphics.Buttons.Sliders;
using Quaver.Graphics.Enums;
using Quaver.Graphics.Sprites;
using Quaver.Graphics.UniversalDim;
using Quaver.Main;
using Quaver.States;

namespace Quaver.Graphics.Buttons
{
    /// <summary>
    ///     Sliders, used to change 
    /// </summary>
    internal class QuaverSlider: IGameStateComponent
    {
        /// <summary>
        ///     Reference to the value that's changing in the slider.
        /// </summary>
        internal BindedInt BindedValue { get; }

        /// <summary>
        ///     The surrounding containing box around the slider.
        /// </summary>
        internal QuaverSprite SurroundingBox { get; set; }

        /// <summary>
        ///     The containing object
        /// </summary>
        private QuaverContainer Container { get; set; }

        /// <inheritdoc />
        /// <summary>
        ///     Ctor - Creates the slider object.
        /// </summary>
        internal QuaverSlider(BindedInt bindedValue)
        {
            BindedValue = bindedValue;
        }
        
        
        public void Initialize(IGameState state)
        {
            Container = new QuaverContainer();

            // Create the surrounding box 
            SurroundingBox = new QuaverSprite()
            {
                Size = new UDim2D(10, 40, 0.15f, 0),
                Alignment = Alignment.MidCenter,
                Tint = new Color(0f, 0f, 0f, 0.40f),
                Parent = Container
            };

            var button = new SliderButton(BindedValue)
            {
                Parent = SurroundingBox,
                Alignment = Alignment.MidCenter,
                Tint = new Color(255f, 0f, 0f, 0.40f),
                Size = new UDim2D(SurroundingBox.SizeX - 12, 1.8f, 1, 0)
            };
        }

        public void UnloadContent()
        {
            Container.Destroy();
        }

        public void Update(double dt)
        {
            Container.Update(dt);
        }

        public void Draw()
        {
            Container.Draw();
        }
    }
}