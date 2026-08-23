using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.Dialogs
{
    public abstract class ColorDialog : DialogScreen
    {
        private readonly string header;
        private EditorColorPicker colorPicker;
        private Color initialColor = Color.Black;
        private bool isClosing;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        protected ColorDialog(string header = null)
            : base(0.65f)
        {
            this.header = header ?? LocalizationManager.Get("Screen_Editor_SelectColor");
        }

        public override void CreateContent()
        {
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (IsOnTop && KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();
        }

        public override void Update(GameTime gameTime)
        {
            // Color dialogs can be queued from inside another SpriteImGui's layout.
            // Creating this renderer there would replace that layout's current ImGui context.
            EnsureColorPicker();
            base.Update(gameTime);
            colorPicker.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
            colorPicker?.Draw(gameTime);
        }

        public void UpdateColor(Color color)
        {
            initialColor = color;
            colorPicker?.SetColor(color);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        private void Close()
        {
            if (isClosing)
                return;

            isClosing = true;
            colorPicker?.Close();
            DialogManager.Dismiss(this);
        }

        public override void Destroy()
        {
            colorPicker?.Destroy();
            base.Destroy();
        }

        private void EnsureColorPicker()
        {
            colorPicker ??= new EditorColorPicker(header, initialColor, OnColorChange, Close);
        }

        protected abstract void OnColorChange(Color newColor);
    }
}
