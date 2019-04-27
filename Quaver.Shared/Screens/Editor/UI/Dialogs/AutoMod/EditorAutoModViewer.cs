using System.Numerics;
using ImGuiNET;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.AutoMod;
using Wobble.Graphics.Animations;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Window;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.AutoMod
{
    public class EditorAutoModViewer : SpriteImGui
    {
        /// <summary>
        /// </summary>
        public bool Shown { get; set; }

        /// <summary>
        /// </summary>
        public EditorAutoModDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public AutoModManager AutoMod { get; }

        public EditorAutoModViewer(AutoModManager autoMod)
        {
            Dialog = new EditorAutoModDialog(this);
            AutoMod = autoMod;
            UpdateDialogInfo();
        }

        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            SetWindowSize();

            ImGui.Begin("Auto Mod", ImGuiWindowFlags.Modal | ImGuiWindowFlags.NoCollapse |
                                             ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoSavedSettings |
                                             ImGuiWindowFlags.AlwaysAutoResize);

            AddDialogInfo();
            //AddHeaderText();
            /*
            HandleAddRemoveButtons();
            HandleTextboxes();
            AddTableHeaders();
            AddSvTable();*/

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetSize()
        {
            var nativeSize = new Vector2(390, 500);
            ;

            var scale = WindowManager.DetermineDrawScaling();
            return new Vector2(nativeSize.X * scale.X, nativeSize.Y * scale.Y);
        }

        /// <summary>
        /// </summary>
        private static void SetWindowSize()
        {
            var size = GetSize();
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(new Vector2(ConfigManager.WindowWidth.Value - size.X,
                ConfigManager.WindowHeight.Value / 2f - size.Y / 2f));
        }

        /// <summary>
        /// </summary>
        private static void AddHeaderText()
        {
            /*
            ImGui.TextWrapped(
                "AutoMod stuff");

            ImGui.Dummy(new Vector2(0, 10));
            */
        }

        /// <summary>
        /// </summary>
        public void Show()
        {
            Shown = true;
            DialogManager.Show(Dialog);

            if (!ButtonManager.Buttons.Contains(Dialog))
                ButtonManager.Add(Dialog);

            Dialog.Animations.Clear();
            Dialog.Animations.Add(new Animation(AnimationProperty.Alpha, Easing.Linear, Dialog.Alpha, 0.75f, 100));
        }

        /// <summary>
        /// </summary>
        public void Hide()
        {
            Shown = false;
            Dialog.Close();
        }

        private void AddDialogInfo()
        {
            var text = "";

            foreach (var mod in AutoMod.Log)
            {
                text += $"{mod.GetInfo()}\n";
            }

            ImGui.Text(text);
            ImGui.Dummy(new Vector2(0, 10));
        }

        private void UpdateDialogInfo()
        {
            //RenderImguiLayout();
        }
    }
}