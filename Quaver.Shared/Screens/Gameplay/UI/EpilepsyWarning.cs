using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Audio;
using Quaver.Shared.Skinning;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.UI;

public class EpilepsyWarning : Sprite
{
    private GameplayScreen Screen { get; }

    /// <summary>
    ///     The scale used for fade animations.
    /// </summary>
    private static float AnimationScale => 120f / AudioEngine.Track.Rate;

    public EpilepsyWarning(GameplayScreen screen)
    {
        Screen = screen;
        Image = SkinManager.Skin.EpilepsyWarning;
        Alpha = 0;
        Visible = screen.Map?.Tags?.Contains("sv", StringComparison.InvariantCultureIgnoreCase) ?? true;
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

        // Fade in on map start
        if (Screen.Timing.Time < -500)
        {
            var alpha = MathHelper.Lerp(Alpha, 1, (float)Math.Min(dt / AnimationScale, 1));

            Alpha = alpha;
        }
        else
        {
            var alpha = MathHelper.Lerp(Alpha, 0, (float)Math.Min(dt / AnimationScale, 1));

            Alpha = alpha;
        }
    }
}
