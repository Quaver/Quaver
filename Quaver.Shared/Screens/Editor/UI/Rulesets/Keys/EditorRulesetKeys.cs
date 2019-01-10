using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling;
using Wobble.Graphics;
using Wobble.Input;
using Keys = Microsoft.Xna.Framework;

namespace Quaver.Shared.Screens.Editor.UI.Rulesets.Keys
{
    public class EditorRulesetKeys : EditorRuleset
    {
        /// <summary>
        ///     Used for scrolling hitobjects & timing lines.
        /// </summary>
        private EditorScrollContainerKeys ScrollContainer { get; set; }

        /// <summary>
        /// </summary>
        private EditorTimeProgress TimeProgress { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditorRulesetKeys(EditorScreen screen) : base(screen) => CreateScrollContainer();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        protected override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.PageUp))
                ConfigManager.EditorScrollSpeedKeys.Value++;

            if (KeyboardManager.IsUniqueKeyPress(Microsoft.Xna.Framework.Input.Keys.PageDown))
                ConfigManager.EditorScrollSpeedKeys.Value--;
        }

        /// <summary>
        /// </summary>
        private void CreateScrollContainer() => ScrollContainer = new EditorScrollContainerKeys(this) { Parent = Container };
    }
}