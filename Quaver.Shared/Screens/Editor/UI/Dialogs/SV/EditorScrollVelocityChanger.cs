using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Editor.UI.Dialogs.Metadata;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Quaver.Shared.Screens.Menu.UI.Navigation.User;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Editor.UI.Dialogs.SV
{
    public class EditorScrollVelocityChanger : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScrollVelocityDialog Dialog { get; }

        /// <summary>
        /// </summary>
        public Sprite HeaderBackground { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite FooterBackground { get; private set; }

        /// <summary>
        /// </summary>
        private Button OkButton { get; set; }

        /// <summary>
        /// </summary>
        private Button CancelButton { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="dialog"></param>
        public EditorScrollVelocityChanger(EditorScrollVelocityDialog dialog)
        {
            Dialog = dialog;
            Size = new ScalableVector2(400, 502);
            Tint = ColorHelper.HexToColor($"#414345");
            Alpha = 1f;

            CreateHeader();
            CreateFooter();
            CreateOkButton();
            CreateCancelButton();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderBackground = new Sprite
            {
                Parent = this,
                Size = new ScalableVector2(Width, 45),
                Tint = ColorHelper.HexToColor($"#212121")
            };

            var headerFlag = new Sprite()
            {
                Parent = HeaderBackground,
                Size = new ScalableVector2(5, HeaderBackground.Height),
                Tint = Color.LightGray,
                Alpha = 0
            };

            // ReSharper disable once ObjectCreationAsStatement
            new SpriteText(Fonts.Exo2SemiBold, "Edit Scroll Velocities", 14)
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidLeft,
                X = headerFlag.X + 15,
            };

            var exitButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_times), (sender, args) => Dialog.Close())
            {
                Parent = HeaderBackground,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(20, 20)
            };

            exitButton.X -= exitButton.Width / 2f + 5;
        }

        /// <summary>
        /// </summary>
        private void CreateFooter() => FooterBackground = new Sprite
        {
            Parent = this,
            Size = new ScalableVector2(Width, 50),
            Tint = ColorHelper.HexToColor("#212121"),
            Alignment = Alignment.BotLeft,
            Y = 1
        };

        /// <summary>
        /// </summary>
        private void CreateOkButton() => OkButton = new BorderedTextButton("OK", Color.LimeGreen, (sender, args) => Dialog.Close())
        {
            Parent = FooterBackground,
            Alignment = Alignment.MidRight,
            X = -20,
            Text = {Font = Fonts.Exo2SemiBold}
        };

        /// <summary>
        /// </summary>
        private void CreateCancelButton() => CancelButton = new BorderedTextButton("Cancel", Color.Crimson, (sender, args) => Dialog.Close())
        {
            Parent = FooterBackground,
            Alignment = Alignment.MidRight,
            X = OkButton.X - OkButton.Width - 20,
            Text = { Font = Fonts.Exo2SemiBold }
        };
    }
}