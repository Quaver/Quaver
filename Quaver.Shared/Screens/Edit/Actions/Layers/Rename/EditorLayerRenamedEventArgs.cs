using Quaver.API.Maps.Structures;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;

namespace Quaver.Shared.Screens.Edit.Actions.Layers.Rename
{
    public class EditorLayerRenamedEventArgs : EditorLayerCreatedEventArgs
    {
        public string PreviousName { get; }

        public EditorLayerRenamedEventArgs(EditorLayerInfo l, string previousName) : base(l) => PreviousName = previousName;
    }
}