using ImGuiNET;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Edit.UI.Menu
{
    public class EditorFileMenuBar : SpriteImGui
    {
        /// <summary>
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// </summary>
        public bool IsActive { get; private set; }

#if VISUAL_TESTS
        private static bool DestroyContext { get; } = false;
#else
        private static bool DestroyContext { get; } = true;
#endif

        public EditorFileMenuBar() : base(DestroyContext)
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            if (!ImGui.BeginMainMenuBar())
                return;

            Height = ImGui.GetWindowSize().Y;

            ImGui.EndMenuBar();
            Button.IsGloballyClickable = !ImGui.IsAnyItemHovered();
            IsActive = ImGui.IsAnyItemActive() || ImGui.IsAnyItemHovered() || ImGui.IsAnyItemFocused();
        }
    }
}