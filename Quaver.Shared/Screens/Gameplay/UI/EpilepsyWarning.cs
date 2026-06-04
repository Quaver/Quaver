using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Audio;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Skinning;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.UI;

public class EpilepsyWarning : Sprite
{
    private const int WarningHeight = 180;

    private const string WarningBody =
        "This map contains rapid movements, flashing lights, or visual patterns that may trigger seizures for people with photosensitive epilepsy. Viewer discretion is advised.";

    private GameplayScreen Screen { get; }

    /// <summary>
    ///     The scale used for fade animations.
    /// </summary>
    private static float AnimationScale => 120f / AudioEngine.Track.Rate;

    public EpilepsyWarning(GameplayScreen screen)
    {
        Screen = screen;
        Image = UserInterface.BlankBox;
        Tint = Color.Transparent;
        SetChildrenAlpha = true;
        Alpha = 0;
        Visible = ShouldDisplayWarning();

        CreateBackground();
        CreateIcon();
        CreateTitle();
        CreateBody();
    }

    private void CreateBackground() => new Sprite
    {
        Parent = this,
        Image = SkinManager.Skin.EpilepsyWarning,
        Alignment = Alignment.MidCenter,
        Size = new ScalableVector2(0, WarningHeight, 1, 0)
    };

    private void CreateIcon() => new Sprite
    {
        Parent = this,
        Image = SkinManager.Skin.EpilepsyWarningIcon,
        Alignment = Alignment.MidCenter,
        Size = new ScalableVector2(42, 37),
        Y = -50
    };

    private void CreateTitle() =>
        new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "WARNING!", 28)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            TextAlignment = TextAlignment.Center,
            Tint = new Color(255, 209, 67),
            Y = 0
        };

    private void CreateBody() =>
        new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), WarningBody, 22)
        {
            Parent = this,
            Alignment = Alignment.MidCenter,
            MaxWidth = 1720,
            TextAlignment = TextAlignment.Center,
            Y = 30
        };

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