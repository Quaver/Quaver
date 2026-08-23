using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorTarget
    {
        public string Id { get; }

        public string Label { get; }

        public string ConfigPath { get; }

        public IReadOnlyList<Drawable> Drawables { get; }

        public SkinEditorTarget(string id, string label, string configPath, params Drawable[] drawables)
        {
            Id = id;
            Label = label;
            ConfigPath = configPath;
            Drawables = drawables.Where(x => x != null).ToArray();
        }
    }

    internal interface ISkinV2EditorHost
    {
        string EditorGroupLabel { get; }

        Container PreviewRoot { get; }

        Container EditorRoot { get; }

        IReadOnlyList<SkinEditorTarget> EditorTargets { get; }

        void SetSkinEditorLayout(bool active, float leftPanelWidth = 0, float rightPanelWidth = 0,
            float assetPanelHeight = 0);

        void ApplySkinEditorPreview(SkinV2Config config);

        void EnsureNavigation();
    }
}
