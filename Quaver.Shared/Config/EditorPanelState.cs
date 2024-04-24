using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.Edit;
using Wobble.Graphics;

namespace Quaver.Shared.Config;

public class EditorPanelState
{
    public Vector2 Position { get; set; } = Vector2.Zero;
    public bool Enabled { get; set; } = true;
    public Alignment Alignment { get; set; }

    public EditorPanelState()
    {
    }

    public EditorPanelState(Vector2 position, bool enabled = true)
    {
        Position = position;
        Enabled = enabled;
    }

    private Drawable GetPanelDrawable(EditorPanelType editorPanelType, EditScreenView editScreenView)
    {
        switch (editorPanelType)
        {
            case EditorPanelType.CompositionTools:
                return editScreenView.CompositionTools;
            case EditorPanelType.Details:
                return editScreenView.Details;
            case EditorPanelType.Hitsounds:
                return editScreenView.Hitsounds;
            case EditorPanelType.Layers:
                return editScreenView.Layers;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ApplyState(EditorPanelType editorPanelType, EditScreenView editScreenView)
    {
        var targetDrawable = GetPanelDrawable(editorPanelType, editScreenView);
        targetDrawable.Parent = editScreenView.Container;
        targetDrawable.Position = new ScalableVector2(Position.X, Position.Y);
        targetDrawable.Visible = Enabled;
        targetDrawable.Alignment = Alignment;
    }

    public void RetrieveState(EditorPanelType editorPanelType, EditScreenView editScreenView)
    {
        var targetDrawable = GetPanelDrawable(editorPanelType, editScreenView);
        Position = new Vector2(targetDrawable.X, targetDrawable.Y);
        Enabled = targetDrawable.Visible;
        Alignment = targetDrawable.Alignment;
    }
}