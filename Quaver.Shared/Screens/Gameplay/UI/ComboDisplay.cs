using System;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Timers;
using Quaver.Shared.Graphics;
using Quaver.Shared.Skinning;
using Wobble.Graphics.Animations;

namespace Quaver.Shared.Screens.Gameplay.UI;

public class ComboDisplay : GameplayNumberDisplay
{
    /// <summary>
    /// </summary>
    private GameplayScreen Screen { get; }

    /// <summary>
    ///     Timer for bumping the combo
    /// </summary>
    private readonly CountdownTimer bumpTimer;

    /// <summary>
    ///     Time to bump
    /// </summary>
    private readonly TimeSpan bumpTime;

    /// <summary>
    ///     Start Y of bumping
    /// </summary>
    private float bumpY;

    /// <summary>
    ///     The original Y position of the combo display.
    /// </summary>
    public float OriginalPosY { get; set; }

    private SkinKeys Skin => SkinManager.Skin.Keys[Screen.Map.Mode];

    internal ComboDisplay(NumberDisplayType type, string startingValue, Vector2 imageScale, GameplayScreen screen,
        float originalPosY) : base(type, startingValue, imageScale)
    {
        Screen = screen;
        OriginalPosY = originalPosY;
        Y = OriginalPosY;

        bumpTime = TimeSpan.FromMilliseconds(Skin.ComboDisplayBumpTime);
        bumpTimer = new CountdownTimer(bumpTime);
        bumpTimer.TimeRemainingChanged += LerpY;
        bumpTimer.Stopped += LerpY;
    }

    private void LerpY(object sender, EventArgs e)
    {
        var t = 1 - bumpTimer.TimeRemaining / bumpTime;
        Y = EasingFunctions.EaseOutExpo(bumpY, OriginalPosY, (float)t);
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        bumpTimer.Update(gameTime);
    }

    public void StartBump()
    {
        bumpY = OriginalPosY + Skin.ComboDisplayBumpY;
        bumpTimer.Restart();
    }
}