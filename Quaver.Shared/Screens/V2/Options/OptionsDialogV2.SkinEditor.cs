using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Screens.V2.Options.Catalog;
using Quaver.Shared.Screens.V2.Options.UI;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Skinning.V2;
using Wobble.Graphics;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.Options
{
    /// <summary>
    ///     Skin editor support: the editor shows the menu as a live preview between its panels.
    /// </summary>
    internal sealed partial class OptionsDialogV2
    {
        private SkinEditorController SkinEditor { get; set; }

        /// <summary>
        ///     Holds the menu. The skin editor scales it down to fit between its panels.
        /// </summary>
        public Container PreviewRoot { get; }

        /// <summary>
        ///     Holds the skin editor's own panels.
        /// </summary>
        public Container EditorRoot { get; }

        public string EditorGroupLabel => LocalizationManager.Get("Screen_Main_Options");

        public IReadOnlyList<SkinEditorTarget> EditorTargets { get; private set; } = Array.Empty<SkinEditorTarget>();

        private bool EditorLayoutActive { get; set; }

        private float EditorLeftWidth { get; set; }

        private float EditorRightWidth { get; set; }

        private float EditorBottomHeight { get; set; }

        public void SetSkinEditorLayout(bool active, float leftPanelWidth = 0, float rightPanelWidth = 0,
            float assetPanelHeight = 0)
        {
            EditorLayoutActive = active;
            EditorLeftWidth = leftPanelWidth;
            EditorRightWidth = rightPanelWidth;
            EditorBottomHeight = assetPanelHeight;
            EditorRoot.Visible = active;
            UpdateEditorLayout();
        }

        /// <summary>
        ///     Rebuilds the whole menu with the skin config being edited.
        /// </summary>
        public void ApplySkinEditorPreview(SkinV2Config config)
        {
            RootConfig = config;
            Tint = SkinV2Color.Parse(Config.Backdrop.Color);
            Alpha = Config.Backdrop.Opacity;

            RailExpanded = false;
            CategoryButtons.Clear();
            SubcategoryButtons.Clear();
            SearchQuery = string.Empty;
            SearchResults = Array.Empty<OptionsRowGroup>();
            SearchRailButton = null;

            foreach (var child in PreviewRoot.Children.ToArray())
                OptionsDrawableCleanup.DestroyTree(child);

            CreateContent();
            UpdateEditorLayout();
        }

        /// <summary>
        ///     Screens use this to add the navigation bar. The options menu does not have one.
        /// </summary>
        public void EnsureNavigation()
        {
        }

        private void ToggleSkinEditor()
        {
            SkinEditor ??= new SkinEditorController(this);

            if (SkinEditor.IsOpen)
                SkinEditor.RequestClose();
            else
                SkinEditor.Open();
        }

        private SkinEditorTarget[] CreateEditorTargets() => new[]
        {
            new SkinEditorTarget("options-backdrop", LocalizationManager.Get("SkinEditor_Component_Backdrop"),
                "Screens.Options.Backdrop", RootSurface),
            new SkinEditorTarget("options-header", LocalizationManager.Get("SkinEditor_Component_Header"),
                "Screens.Options.Header", Header),
            new SkinEditorTarget("options-panels", LocalizationManager.Get("SkinEditor_Component_Panels"),
                "Screens.Options.Panels", RailOverlay, CategoryPanel, ContentPanel),
            new SkinEditorTarget("options-rail", LocalizationManager.Get("SkinEditor_Component_RailToggle"),
                "Screens.Options.Rail", RailToggle),
            new SkinEditorTarget("options-categories", LocalizationManager.Get("SkinEditor_Component_Categories"),
                "Screens.Options.Categories", CategoryNavigationScroll, SubcategoryNavigationScroll),
            new SkinEditorTarget("options-preset", LocalizationManager.Get("SkinEditor_Component_Preset"),
                "Screens.Options.Preset", PresetDropdown),
            new SkinEditorTarget("options-rows", LocalizationManager.Get("SkinEditor_Component_Rows"),
                "Screens.Options.Rows", ContentPanel),
            new SkinEditorTarget("shared-toggle", LocalizationManager.Get("SkinEditor_Component_Toggles"),
                "Shared.Toggle"),
            new SkinEditorTarget("shared-button", LocalizationManager.Get("SkinEditor_Component_Buttons"),
                "Shared.Button"),
            new SkinEditorTarget("shared-slider", LocalizationManager.Get("SkinEditor_Component_Sliders"),
                "Shared.Slider"),
            new SkinEditorTarget("shared-search", LocalizationManager.Get("SkinEditor_Component_SearchBar"),
                "Shared.Search", SearchBar)
        };

        /// <summary>
        ///     While the skin editor is open, scales the menu down to fit the space between its panels.
        /// </summary>
        private void UpdateEditorLayout()
        {
            if (PreviewRoot == null || EditorRoot == null)
                return;

            Container.Size = new ScalableVector2(WindowManager.Width, WindowManager.Height);
            PreviewRoot.Size = Container.Size;
            EditorRoot.Size = Container.Size;

            if (!EditorLayoutActive)
            {
                PreviewRoot.Position = new ScalableVector2(0, 0);
                PreviewRoot.Scale = Vector2.One;
                return;
            }

            const float margin = 16;
            var availableWidth = Math.Max(1, WindowManager.Width - EditorLeftWidth - EditorRightWidth - margin * 2);
            var availableHeight = Math.Max(1, WindowManager.Height - EditorBottomHeight - margin * 2);
            var scale = Math.Min(availableWidth / WindowManager.Width, availableHeight / WindowManager.Height);

            PreviewRoot.Scale = new Vector2(scale);
            PreviewRoot.Position = new ScalableVector2(
                EditorLeftWidth + margin + (availableWidth - WindowManager.Width * scale) / 2f,
                margin + (availableHeight - WindowManager.Height * scale) / 2f);
        }
    }
}
