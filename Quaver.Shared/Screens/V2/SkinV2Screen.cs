using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Screens.V2.SkinEditor;
using Quaver.Shared.Screens.V2.UI;
using Quaver.Shared.Skinning;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.V2
{
    /// <summary>
    ///     Base for replacement screens that can host the Skin V2 editor without exposing it to legacy screens.
    /// </summary>
    internal abstract class SkinV2Screen : QuaverScreen, IPersistentScreen
    {
        private SkinEditorController skinEditor;

        protected abstract ISkinV2EditorHost SkinEditorHost { get; }

        protected virtual bool UsesNavigation => true;

        protected bool IsSkinEditorActive => skinEditor?.IsOpen == true;

        IReadOnlyCollection<string> IPersistentScreen.PersistentElementKeys =>
            UsesNavigation ? new[] { ScreenNavigation.ElementKey } : new string[0];

        public override void OnActivated()
        {
            base.OnActivated();
            if (UsesNavigation)
                SkinEditorHost.EnsureNavigation();
        }

        public override void Update(GameTime gameTime)
        {
            if (!Exiting && SkinManager.TimeSkinReloadRequested == 0 &&
                skinEditor?.IsOpen != true && SkinEditorController.ReopenAfterSkinReload)
            {
                SkinEditorController.ReopenAfterSkinReload = false;
                skinEditor ??= new SkinEditorController(SkinEditorHost);
                skinEditor.Open();
            }

            if (DialogManager.Dialogs.Count == 0 && KeyboardManager.IsCtrlDown() &&
                IsShiftDown() && KeyboardManager.IsUniqueKeyPress(Keys.E))
            {
                if (skinEditor?.IsOpen == true)
                    skinEditor.RequestClose();
                else
                {
                    skinEditor ??= new SkinEditorController(SkinEditorHost);
                    skinEditor.Open();
                }
            }
            else if (skinEditor?.IsOpen == true && DialogManager.Dialogs.Count == 0 &&
                     KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                skinEditor.RequestClose();

            base.Update(gameTime);
        }

        public override void Destroy()
        {
            skinEditor?.Destroy();
            skinEditor = null;
            base.Destroy();
        }

        private static bool IsShiftDown() =>
            KeyboardManager.CurrentState.IsKeyDown(Keys.LeftShift) ||
            KeyboardManager.CurrentState.IsKeyDown(Keys.RightShift);
    }
}
