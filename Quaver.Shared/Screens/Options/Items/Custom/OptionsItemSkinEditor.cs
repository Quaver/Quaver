using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.SkinEditor;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemSkinEditor : OptionsItem
    {
        private TextButton Button { get; }

        public OptionsItemSkinEditor(RectangleF containerRect, string name) : base(containerRect, name)
        {
            Button = new TextButton(UserInterface.BlankBox, Fonts.Exo2Medium, "Open Editor", 14,
                (sender, args) =>
                {
                    if (DialogManager.Dialogs.Count != 0)
                        DialogManager.Dismiss();

                    var game = (QuaverGame)GameBase.Game;
                    game.CurrentScreen.Exit(() => new SkinEditorScreen());
                })
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * 0.85f, 36 * 0.85f),
                Tint = ColorHelper.HexToColor("#2F2F2F"),
                UsePreviousSpriteBatchOptions = true,
                Text =
                {
                    Tint = Colors.MainAccent
                }
            };

            Button.AddBorder(ColorHelper.HexToColor("#45D6F5"), 2);
        }
    }
}
