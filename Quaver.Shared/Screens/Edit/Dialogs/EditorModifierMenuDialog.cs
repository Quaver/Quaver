using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection.UI;
using Quaver.Shared.Screens.Selection.UI.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics.Animations;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public sealed class EditorModifierMenuDialog : DialogScreen
    {
        private ModifierSelectorContainer Modifiers { get; set; }

        private Bindable<SelectContainerPanel> Bindable { get; } = new Bindable<SelectContainerPanel>(SelectContainerPanel.Modifiers);

        public EditorModifierMenuDialog() : base(0)
        {
            FadeTo(0.85f, Easing.Linear, 250);
            CreateContent();
        }

        public override void CreateContent()
        {
            Modifiers = new ModifierSelectorContainer(Bindable)
            {
                Parent = Container,
                Alignment = Wobble.Graphics.Alignment.MidCenter,
            };

            Bindable.ValueChanged += (o, e) => Close();
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Modifiers.IsHovered())
            {
                Close();
                return;
            }
        }

        private void Close()
        {
            FadeTo(0, Easing.Linear, 150);
            Modifiers.Visible = false;
            Bindable.Dispose();

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 150);
        }
    }
}