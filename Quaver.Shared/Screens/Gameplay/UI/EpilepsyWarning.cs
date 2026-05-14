using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
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
        Visible = ShouldDisplayWarning();
    }

    private bool ShouldDisplayWarning()
    {
        if (!ConfigManager.DisplayEpilepsyWarning.Value)
            return false;

        var map = Screen.Map;

        if (map == null)
            return true;

        var hasSvTag = map.Tags?.Contains("sv", StringComparison.InvariantCultureIgnoreCase) ??
                       false;

        const float extremeSvThreshold = 5;
        var hasExtremeSv = map.TimingGroups.Values
            .OfType<ScrollGroup>()
            .Any(group => group.ScrollVelocities.Any(sv =>
                sv.Multiplier is > extremeSvThreshold or < -extremeSvThreshold));

        return hasSvTag || hasExtremeSv;
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
