using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;
using Quaver.Shared.Screens.Gameplay.UI;
using Quaver.Shared.Screens.Gameplay.UI.Counter;
using Quaver.Shared.Screens.Gameplay.UI.Health;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Gameplay.ModCharting.Objects;

[MoonSharpUserData]
public class ModChartStage : ModChartGlobalVariable
{
    public ModChartStage(ElementAccessShortcut shortcut) : base(shortcut)
    {
    }


    #region PlayfieldStage

    public Sprite BgMask => Shortcut.GameplayPlayfieldKeysStage.BgMask;
    public GameplayNumberDisplay ComboDisplay => Shortcut.GameplayPlayfieldKeysStage.ComboDisplay;
    public HealthBar HealthBar => Shortcut.GameplayPlayfieldKeysStage.HealthBar;
    public List<GameplayPlayfieldLane> Lanes => Shortcut.GameplayPlayfieldKeysStage.LaneContainers;
    public List<JudgementHitBurst> JudgementHitBursts => Shortcut.GameplayPlayfieldKeysStage.JudgementHitBursts;
    public List<HitLighting> HitLightingObjects => Shortcut.GameplayPlayfieldKeysStage.HitLightingObjects;
    public HitErrorBar HitError => Shortcut.GameplayPlayfieldKeysStage.HitError;
    public Sprite StageLeft => Shortcut.GameplayPlayfieldKeysStage.StageLeft;
    public Sprite StageRight => Shortcut.GameplayPlayfieldKeysStage.StageRight;
    public Sprite DistantOverlay => Shortcut.GameplayPlayfieldKeysStage.DistantOverlay;

    #endregion

    #region View

    public Sprite Background => Shortcut.GameplayScreenView.Background;
    public GameplayNumberDisplay AccuracyDisplay => Shortcut.GameplayScreenView.AccuracyDisplay;
    public GameplayNumberDisplay RatingDisplay => Shortcut.GameplayScreenView.RatingDisplay;
    public KeysPerSecond KpsDisplay => Shortcut.GameplayScreenView.KpsDisplay;
    public GradeDisplay GradeDisplay => Shortcut.GameplayScreenView.GradeDisplay;
    public JudgementCounter JudgementCounter => Shortcut.GameplayScreenView.JudgementCounter;

    #endregion

    #region Playfield

    public Container ForegroundContainer => Shortcut.GameplayPlayfieldKeys.ForegroundContainer;
    public Container BackgroundContainer => Shortcut.GameplayPlayfieldKeys.BackgroundContainer;
    public Container PlayfieldContainer => Shortcut.GameplayPlayfieldKeys.Container;

    /// <summary>
    ///     Width of lane (receptor alone)
    /// </summary>
    /// <returns></returns>
    public float LaneSize => Shortcut.GameplayPlayfieldKeys.LaneSize;

    /// <summary>
    ///     Padding of receptor
    /// </summary>
    /// <returns></returns>
    public float ReceptorPadding => Shortcut.GameplayPlayfieldKeys.ReceptorPadding;

    #endregion

    /// <summary>
    ///     Separation between lanes
    /// </summary>
    /// <returns></returns>
    public float LaneSeparationWidth => LaneSize + ReceptorPadding;

    /// <summary>
    ///     Positions of each receptor
    /// </summary>
    /// <returns>Scalable vector (x, y, scale_x, scale_y) for each receptor</returns>
    public ScalableVector2[] GetLanePositions()
    {
        var positions = new ScalableVector2[Shortcut.GameplayScreen.Map.GetKeyCount()];
        for (var i = 0; i < Shortcut.GameplayScreen.Map.GetKeyCount(); i++)
        {
            positions[i] = Shortcut.GameplayPlayfieldKeysStage.LaneContainers[i].Position;
        }

        return positions;
    }

    /// <summary>
    ///     Sets the position of a receptor of a particular lane
    /// </summary>
    /// <param name="lane"></param>
    /// <param name="pos"></param>
    public void SetLanePosition(int lane, ScalableVector2 pos)
    {
        Shortcut.GameplayPlayfieldKeysStage.LaneContainers[lane - 1].Position = pos;
    }
}