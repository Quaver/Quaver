using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Graphics.Notifications;
using Wobble.Graphics;
using Wobble.Input;

namespace Quaver.Shared.Helpers.Input
{
    public abstract class CheatCode : Container
    {
        /// <summary>
        ///     The order the keys that must be pressed
        /// </summary>
        public abstract Keys[] Combination { get; }

        /// <summary>
        /// </summary>
        private int Index { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            foreach (var key in KeyboardManager.CurrentState.GetPressedKeys())
            {
                if (Index >= Combination.Length)
                {
                    Index = 0;
                    break;
                }

                if (!KeyboardManager.IsUniqueKeyPress(key))
                    break;

                if (key != Combination[Index])
                {
                    Index = 0;
                    break;
                }

                Index++;

                if (Index == Combination.Length)
                {
                    OnActivated();
                    Index = 0;
                }
            }

            base.Update(gameTime);
        }

        protected abstract void OnActivated();
    }
}