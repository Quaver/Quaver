using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Config;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Gameplay.Rulesets.Keys.Playfield;

public class GameplayPlayfieldLane : Container
{
    private GameplayPlayfieldKeys Playfield { get; set; }

    private GameplayPlayfieldKeysStage Stage { get; set; }

    public Container HitObjectContainer { get; private set; }

    public Sprite Receptor { get; private set; }

    public ColumnLighting ColumnLighting { get; private set; }

    public Container HitContainer { get; private set; }

    public int Lane { get; }
    public float LaneSize { get; }
    public float LaneScale { get; }

    public GameplayPlayfieldLane(int lane, float laneSize, float laneScale, GameplayPlayfieldKeysStage stage)
    {
        Lane = lane;
        LaneSize = laneSize;
        LaneScale = laneScale;
        Stage = stage;
        Playfield = Stage.Playfield;
    }

    public void CreateHitObjectContainer()
    {
        HitObjectContainer = new Container
        {
            Size = new ScalableVector2(LaneSize * LaneScale, 0, 0, 1),
            Alignment = Alignment.TopCenter,
            Parent = this
        };
    }

    public void CreateHitContainer() => HitContainer = new Container
    {
        Size = new ScalableVector2(Width, 0, WidthScale, 1),
        Alignment = Alignment.TopCenter,
        Parent = this
    };

    public void CreateColumnLighting()
    {
        // Create the column lighting sprite.
        var size = Stage.Skin.ColumnLightingScale * Playfield.LaneSize *
                   ((float)Stage.Skin.ColumnLighting.Height / Stage.Skin.ColumnLighting.Width);
        ColumnLighting = new ColumnLighting
        {
            Parent = this,
            Image = Stage.Skin.ColumnLighting,
            Size = new ScalableVector2(Playfield.LaneSize, size),
            Tint = Stage.Skin.ColumnColors[Lane],
            X = 0,
            Y = Playfield.ColumnLightingPositionY[Lane],
            // todo: case statement for scroll direction
            SpriteEffect = !Playfield.ScrollDirections[Lane].Equals(ScrollDirection.Down) &&
                           Stage.Skin.FlipNoteImagesOnUpscroll
                ? SpriteEffects.FlipVertically
                : SpriteEffects.None,
            Alignment = Alignment.TopCenter,
        };
    }

    public void CreateReceptor() =>
        Receptor = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(LaneSize * LaneScale,
                (Playfield.LaneSize * Stage.Skin.NoteReceptorsUp[Lane].Height /
                 Stage.Skin.NoteReceptorsUp[Lane].Width) * LaneScale),
            Position = new ScalableVector2(0, Playfield.ReceptorPositionY[Lane]),
            Alignment = Alignment.TopCenter,
            Image = Stage.Skin.NoteReceptorsUp[Lane],
            SpriteEffect =
                !Playfield.ScrollDirections[Lane].Equals(ScrollDirection.Down) && Stage.Skin.FlipNoteImagesOnUpscroll
                    ? SpriteEffects.FlipVertically
                    : SpriteEffects.None,
        };
}