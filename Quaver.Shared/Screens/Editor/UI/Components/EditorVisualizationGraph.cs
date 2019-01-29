using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps;
using Quaver.Shared.Screens.Editor.UI.Rulesets;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Components
{
    public abstract class EditorVisualizationGraph : Sprite
    {
        /// <summary>
        /// </summary>
        protected  EditorRuleset Ruleset { get; }

        /// <summary>
        /// </summary>
        protected Qua Qua { get; }

        /// <summary>
        /// </summary>
        protected EditorVisualizationGraphContainer Container { get; }

        /// <summary>
        /// </summary>
        protected Texture2D Pixel { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="container"></param>
        /// <param name="qua"></param>
        /// <param name="ruleset"></param>
        protected EditorVisualizationGraph(EditorVisualizationGraphContainer container, Qua qua, EditorRuleset ruleset)
        {
            Container = container;
            Ruleset = ruleset;
            Qua = qua;

            Size = new ScalableVector2(50, WindowManager.Height - 36 - 48);
            Tint = Color.Black;
            Alpha = 0.75f;

            Pixel = new Texture2D(GameBase.Game.GraphicsDevice, 1, 1);
            Pixel.SetData(new[] { Color.White });

            AddBorder(Color.White, 2);
            Border.Alpha = 0.45f;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Pixel.Dispose();
            base.Destroy();
        }
    }
}