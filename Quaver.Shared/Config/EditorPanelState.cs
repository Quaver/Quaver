using System;
using System.Numerics;
using Quaver.Shared.Screens.Edit;
using Wobble.Graphics;
using Wobble.Logging;

namespace Quaver.Shared.Config;

public class EditorPanelState : IWindowState
{
    public Vector2 Position { get; set; }
    public bool Enabled { get; set; } = true;
    public EditorPanelType EditorPanelType { get; set; }

    public void SetPosition(EditScreenView editScreen)
    {
        var scalablePosition = new ScalableVector2(Position.X, Position.Y);
        Logger.Debug($"Set position of {EditorPanelType} to {Position}", LogType.Runtime);
        switch (EditorPanelType)
        {
            case EditorPanelType.CompositionTools:
                editScreen.CompositionTools.Position = scalablePosition;
                break;
            case EditorPanelType.Details:
                editScreen.Details.Position = scalablePosition;
                break;
            case EditorPanelType.Hitsounds:
                editScreen.Hitsounds.Position = scalablePosition;
                break;
            case EditorPanelType.Layers:
                editScreen.Layers.Position = scalablePosition;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void RetrievePosition(EditScreenView editScreenView)
    {
        Drawable drawable;
        switch (EditorPanelType)
        {
            case EditorPanelType.CompositionTools:
                drawable = editScreenView.CompositionTools;
                break;
            case EditorPanelType.Details:
                drawable = editScreenView.Details;
                break;
            case EditorPanelType.Hitsounds:
                drawable = editScreenView.Hitsounds;
                break;
            case EditorPanelType.Layers:
                drawable = editScreenView.Layers;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        var scalablePosition = drawable.Position;
        Position = new Vector2(scalablePosition.X.Value, scalablePosition.Y.Value);
    }
}