using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorSurface
    {
        public string Id { get; }

        public string Label { get; }

        public SkinEditorSurface(string id, string label)
        {
            Id = id;
            Label = label;
        }
    }

    internal sealed class SkinEditorTarget
    {
        public string Id { get; }

        public string Label { get; }

        public string ConfigPath { get; }

        public IReadOnlyList<string> ExcludedConfigPaths { get; }

        public IReadOnlyList<Drawable> Drawables { get; }

        private Action Selected { get; }

        public SkinEditorTarget(string id, string label, string configPath, params Drawable[] drawables)
            : this(id, label, configPath, Array.Empty<string>(), drawables)
        {
        }

        public SkinEditorTarget(string id, string label, string configPath,
            IReadOnlyList<string> excludedConfigPaths, params Drawable[] drawables)
        {
            Id = id;
            Label = label;
            ConfigPath = configPath;
            ExcludedConfigPaths = excludedConfigPaths ?? Array.Empty<string>();
            Drawables = drawables.Where(x => x != null).ToArray();
        }

        public SkinEditorTarget(string id, string label, string configPath, Action selected,
            params Drawable[] drawables) : this(id, label, configPath, drawables)
        {
            Selected = selected;
        }

        public void Activate() => Selected?.Invoke();

        public bool IncludesConfigPath(string path) => ExcludedConfigPaths.All(excluded =>
            path != excluded && !path.StartsWith(excluded + ".", StringComparison.Ordinal));
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

    /// <summary>
    ///     Optional extension for a screen that can preview "multiple screens" at once (dialogs for example)
    /// </summary>
    internal interface ISkinV2MultiSurfaceEditorHost : ISkinV2EditorHost
    {
        IReadOnlyList<SkinEditorSurface> EditorSurfaces { get; }

        string ActiveEditorSurfaceId { get; }

        void SelectEditorSurface(string surfaceId, SkinV2Config config);
    }
}
