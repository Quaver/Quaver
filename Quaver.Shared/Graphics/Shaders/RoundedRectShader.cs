using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Shaders;

namespace Quaver.Shared.Graphics.Shaders
{
    /// <summary>
    ///     Draws an anti-aliased, resolution-independent rounded rectangle via an SDF pixel shader,
    ///     instead of relying on a pre-baked, stretch-blitted texture for corner rounding.
    /// </summary>
    public static class RoundedRectShader
    {
        /// <summary>
        ///     A <see cref="RasterizerState"/> that keeps scissor testing enabled (unlike the engine's
        ///     default <see cref="RasterizerState.CullNone"/>), so that anything drawn with it still gets
        ///     clipped when nested inside a <see cref="Wobble.Graphics.Sprites.ScrollContainer"/>. Shared
        ///     across every user since a <see cref="RasterizerState"/> is just an immutable state descriptor.
        /// </summary>
        public static RasterizerState ScissorSafeRasterizerState { get; } = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        /// <summary>
        ///     <see cref="SpriteBatchOptions"/> for plain (non-shader) content - such as an icon or label
        ///     sitting on top of a rounded-rect background - that still needs to respect an ancestor
        ///     <see cref="Wobble.Graphics.Sprites.ScrollContainer"/>'s clip region. Sharing the rounded-rect
        ///     background's own shader batch would corrupt it, since the shader's coverage math is sized to
        ///     the background's rectangle, not whatever smaller quad happens to reuse the batch.
        /// </summary>
        public static SpriteBatchOptions CreateScissorSafeOptions() => new SpriteBatchOptions
        {
            RasterizerState = ScissorSafeRasterizerState
        };

        /// <summary>
        ///     The compiled shader bytecode, read from disk once and reused to construct a brand new
        ///     <see cref="Effect"/> per <see cref="Create"/> call (rather than <c>Effect.Clone()</c>-ing a
        ///     shared template) so that every button's <c>p_size</c>/<c>p_radius</c> parameters are
        ///     guaranteed fully independent - some older/vendored MonoGame builds don't give clones their
        ///     own independent parameter storage, which would let one button's radius bleed into another's.
        ///     Constructing a handful of small Effects at menu-build time is negligible; this never happens
        ///     per-frame.
        /// </summary>
        private static byte[] CompiledEffectBytes { get; set; }

        /// <summary>
        ///     Creates a new <see cref="Shader"/> instance drawing a rounded rectangle with the given
        ///     corner radius. <see cref="UpdateSize"/> must be called whenever the drawn size changes.
        /// </summary>
        /// <param name="radius">The corner radius, in pixels.</param>
        public static Shader Create(float radius)
        {
            CompiledEffectBytes ??= GameBase.Game.Resources.Get("Quaver.Resources/Shaders/rounded-rect.mgfxo");

            var effect = new Effect(GameBase.Game.GraphicsDevice, CompiledEffectBytes);

            return new Shader(effect, new Dictionary<string, object>
            {
                { "p_size", Vector2.One },
                { "p_radius", radius }
            });
        }

        /// <summary>
        ///     Updates the pixel-space size parameter of a shader created by <see cref="Create"/>.
        ///     Only needs to be called when the drawn rectangle's size actually changes.
        /// </summary>
        public static void UpdateSize(Shader shader, Vector2 size) => shader.SetParameter("p_size", size, true);

        /// <summary>
        ///     Updates the corner radius parameter of a shader created by <see cref="Create"/>.
        /// </summary>
        public static void UpdateRadius(Shader shader, float radius) => shader.SetParameter("p_radius", radius, true);
    }
}
