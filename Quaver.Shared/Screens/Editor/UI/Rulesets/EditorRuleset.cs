using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Wobble.Graphics;
using IDrawable = Wobble.Graphics.IDrawable;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets
{
    public abstract class EditorRuleset : IDrawable
    {
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        public Qua WorkingMap => Screen.WorkingMap;

        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorRuleset(EditorScreen screen) => Screen = screen;

        /// <summary>
        ///     The container for the ruleset used to draw things.
        /// </summary>
        public Container Container { get; } = new Container();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Update(GameTime gameTime)
        {
            HandleInput(gameTime);
            Container.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public virtual void Draw(GameTime gameTime) => Container?.Draw(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public virtual void Destroy() => Container?.Destroy();

        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected abstract void HandleInput(GameTime gameTime);
    }
}