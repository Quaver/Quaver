using Microsoft.Xna.Framework;
using Wobble.Graphics;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Options
{
    public sealed class OptionsDialog : DialogScreen
    {
        public OptionsDialog() : base(0.75f)
        {
            CreateContent();
        }

        public override void CreateContent() => new OptionsMenu()
        {
            Parent = this,
            Alignment = Alignment.MidCenter
        };

        public override void HandleInput(GameTime gameTime)
        {
        }
    }
}