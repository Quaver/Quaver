using System;
using System.Collections.Generic;
using System.Linq;
using ImGuiNET;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Audio;
using Quaver.Shared.Config;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys;
using Quaver.Shared.Screens.Editor.UI.Rulesets.Keys.Scrolling.HitObjects;
using Wobble;
using Wobble.Graphics.Animations;
using Wobble.Graphics.ImGUI;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;
using Vector2 = System.Numerics.Vector2;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.GoTo
{
    public class EditorGoToObjectsPanel : SpriteImGui
    {
        /// <summary>
        /// </summary>
        public bool Shown { get; set; }

        /// <summary>
        /// </summary>
        public EditorGoToObjectsDialog Dialog { get; }

        /// <summary>
        /// </summary>
        // ReSharper disable once InconsistentNaming
        private string Input = "";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorGoToObjectsPanel() => Dialog = new EditorGoToObjectsDialog(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                SelectObjectsAndHide();

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected override void RenderImguiLayout()
        {
            SetWindowSize();

            ImGui.Begin("Go To Objects", ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

            ImGui.TextWrapped("This dialog will allow you to go to and select objects that are copied to the clipboard.\n\n" +
                              "The format should look like the following: \n\nTime|Lane,Time|Lane,...\n\n" +
                              "Just paste it in the textbox and hit OK!");

            ImGui.Dummy(new Vector2(0, 5));
            ImGui.InputText("   ", ref Input, 100000, ImGuiInputTextFlags.None);

            ImGui.Dummy(new Vector2(0, 2));

            if (ImGui.Button("Cancel"))
                Hide();

            ImGui.SameLine();

            if (ImGui.Button("OK"))
                SelectObjectsAndHide();

            ImGui.End();
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static Vector2 GetSize()
        {
            var nativeSize = new Vector2(485, 265);

            var scale = WindowManager.DetermineDrawScaling();
            return new Vector2(nativeSize.X * scale.X, nativeSize.Y * scale.Y);
        }

        /// <summary>
        /// </summary>
        private static void SetWindowSize()
        {
            var size = GetSize();
            ImGui.SetNextWindowSize(size);
            ImGui.SetNextWindowPos(new Vector2(ConfigManager.WindowWidth.Value / 2f - size.X / 2f, ConfigManager.WindowHeight.Value/ 2f - size.Y / 2f));
        }

        /// <summary>
        /// </summary>
        public void Show()
        {
            Shown = true;
            Input = "";
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

        /// <summary>
        /// </summary>
        public void SelectObjectsAndHide()
        {
            if (string.IsNullOrEmpty(Input) || string.IsNullOrWhiteSpace(Input))
            {
                Hide();
                return;
            }

            // StartTime, Lane
            var objectList = new List<Tuple<int, int>>();
            var splitInput = Input.Split(",");

            foreach (var input in splitInput)
            {
                var split = input.Split("|");

                if (split.Length != 2)
                    continue;

                try
                {
                    var t = Tuple.Create(int.Parse(split[0]), int.Parse(split[1]));
                    objectList.Add(t);
                }
                catch (Exception)
                {
                }
            }

            var game = GameBase.Game as QuaverGame;
            var screen = game?.CurrentScreen as EditorScreen;
            var ruleset = screen?.Ruleset as EditorRulesetKeys;

            new List<DrawableEditorHitObject>(ruleset.SelectedHitObjects).ForEach(x => ruleset.DeselectHitObject(x));

            if (objectList.Count == 0)
                return;

            foreach (var obj in objectList)
            {
                var h = ruleset?.ScrollContainer.HitObjects.Find(x => x.Info.StartTime == obj.Item1 && x.Info.Lane == obj.Item2);

                if (h == null)
                    continue;

                ruleset.SelectHitObject(h);
            }

            if (!AudioEngine.Track.IsDisposed)
            {
                if (AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                AudioEngine.Track.Seek(ruleset.SelectedHitObjects.First().Info.StartTime);
            }

            screen?.SetHitSoundObjectIndex();

            Hide();
        }
    }
}