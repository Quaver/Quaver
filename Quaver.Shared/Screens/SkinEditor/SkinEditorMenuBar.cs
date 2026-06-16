using System;
using System.Linq;
using ImGuiNET;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Edit.UI.Menu;
using Quaver.Shared.Skinning;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Vector2 = System.Numerics.Vector2;
using Vector4 = System.Numerics.Vector4;

namespace Quaver.Shared.Screens.SkinEditor
{
    internal sealed class SkinEditorMenuBar : SpriteImGui
    {
        private SkinEditorScreenView View { get; }

        private float Scale { get; }

        public float Height { get; private set; }

        public bool IsActive { get; private set; }

#if VISUAL_TESTS
        private static bool DestroyContext { get; } = false;
#else
        private static bool DestroyContext { get; } = true;
#endif

        public SkinEditorMenuBar(SkinEditorScreenView view)
            : base(DestroyContext, EditorFileMenuBar.GetOptions(), GetScale())
        {
            View = view;
            Scale = GetScale();
        }

        protected override void RenderImguiLayout()
        {
            if (!View.IsHeaderVisible)
            {
                Button.IsGloballyClickable = true;
                IsActive = false;
                return;
            }

            RenderVisibleHeader();
            Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            IsActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemHovered() || ImGui.IsAnyItemFocused();
        }

        private void RenderVisibleHeader()
        {
            ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 2 * Scale);
            ImGui.PushStyleVar(ImGuiStyleVar.FramePadding, new Vector2(0, 10) * Scale);
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(12, 4) * Scale);
            ImGui.PushStyleColor(ImGuiCol.FrameBg, new Vector4(0, 0, 24, 0));
            ImGui.PushStyleColor(ImGuiCol.WindowBg, new Vector4(0, 0, 24, 0));

            if (ImGui.BeginMainMenuBar())
            {
                Height = ImGui.GetWindowSize().Y;

                CreateFileSection();
                CreateScreenSection();
                CreatePropertiesSection();
                CreateStatusText();

                ImGui.EndMenuBar();
            }

            ImGui.PopStyleColor(2);
            ImGui.PopStyleVar(3);
        }

        private void CreateFileSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("File"))
            {
                ImGui.PopFont();
                return;
            }

            var newSkinName = View.NewSkinFolderName;
            ImGui.PushItemWidth(240 * Scale);
            if (ImGui.InputText("New Skin Folder", ref newSkinName, 128))
                View.NewSkinFolderName = newSkinName;
            ImGui.PopItemWidth();

            if (ImGui.MenuItem("Create New Skin"))
                View.CreateSkin();

            if (ImGui.BeginMenu("Switch Skin"))
            {
                var skins = SkinEditorManager.GetEditableSkins();

                if (skins.Count == 0)
                {
                    ImGui.MenuItem("No Local Skins", "", false, false);
                }
                else
                {
                    foreach (var skin in skins)
                    {
                        if (ImGui.MenuItem(skin, "", View.LoadedSkinName == skin))
                            View.SwitchToSkin(skin);
                    }
                }

                ImGui.EndMenu();
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Save"))
                View.SaveSkin();

            if (ImGui.MenuItem("Use & Reload"))
                View.UseAndReloadSkin();

            if (ImGui.MenuItem("Open Skin Folder", "", false, !string.IsNullOrWhiteSpace(View.LoadedSkinName)))
                View.OpenSkinFolder();

            ImGui.Separator();

            if (ImGui.MenuItem("Exit", "ESC"))
                View.ExitToMenu();

            ImGui.EndMenu();
            ImGui.PopFont();
        }

        private void CreateScreenSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Screen"))
            {
                ImGui.PopFont();
                return;
            }

            foreach (SkinEditorPreviewScreen screen in Enum.GetValues(typeof(SkinEditorPreviewScreen)))
            {
                if (ImGui.MenuItem(View.GetPreviewDisplayName(screen), "", View.SelectedPreviewScreen == screen))
                    View.SelectPreview(screen);
            }

            ImGui.EndMenu();
            ImGui.PopFont();
        }

        private void CreatePropertiesSection()
        {
            ImGui.PushFont(Options.Fonts.First().Context);

            if (!ImGui.BeginMenu("Properties"))
            {
                ImGui.PopFont();
                return;
            }

            var properties = View.GetCurrentProperties().ToList();

            if (properties.Count == 0)
            {
                ImGui.MenuItem("No Editable Properties", "", false, false);
            }
            else
            {
                ImGui.PushItemWidth(260 * Scale);

                string currentSection = null;

                foreach (var property in properties)
                {
                    if (currentSection != property.Section)
                    {
                        if (currentSection != null)
                            ImGui.Separator();

                        currentSection = property.Section;
                        ImGui.TextDisabled(currentSection);
                    }

                    var value = View.GetPropertyValue(property);

                    if (ImGui.InputText($"{property.Label}##{property.Id}", ref value, 256))
                        View.SetPropertyValue(property, value);

                    if (!string.IsNullOrWhiteSpace(property.Placeholder) && ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text(property.Placeholder);
                        ImGui.EndTooltip();
                    }
                }

                ImGui.PopItemWidth();
            }

            ImGui.EndMenu();
            ImGui.PopFont();
        }

        private void CreateStatusText()
        {
            var text = string.IsNullOrWhiteSpace(View.LoadedSkinName)
                ? "Previewing default values"
                : $"Editing: {View.LoadedSkinName}";

            ImGui.TextDisabled(text);
        }

        private static float GetScale() => ConfigManager.EditorImGuiScalePercentage?.Value / 100f ?? 1f;
    }
}
