using System;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Managers;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    /// <summary>
    ///     Generates a canonical skin.yml containing only properties opted into skin-author editing.
    /// </summary>
    public sealed class OptionsItemGenerateSkinV2Config : OptionsItem
    {
        private RoundedButton Button { get; }

        public OptionsItemGenerateSkinV2Config(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new RoundedButton
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                Tint = ColorHelper.HexToColor("#0FBAE5")
            };

            Button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), "GENERATE", 18, Color.White);
            Button.Clicked += OnClicked;
        }

        private void OnClicked(object sender, EventArgs args)
        {
            var store = SkinManager.SkinV2;
            if (store == null)
            {
                ShowUnavailable();
                return;
            }

            if (!File.Exists(store.ConfigPath))
            {
                Generate();
                return;
            }

            Focused = true;
            DialogManager.Show(new YesNoDialog("Overwrite skin.yml?",
                "This will keep only required metadata and properties marked ConfigEditable.",
                () =>
                {
                    Focused = false;
                    Generate();
                },
                () => Focused = false));
        }

        private static void Generate()
        {
            var store = SkinManager.SkinV2;
            if (store == null)
            {
                ShowUnavailable();
                return;
            }

            if (store.TrySaveEditableConfig(out var errors))
            {
                NotificationManager.Show(NotificationLevel.Success,
                    "Generated skin.yml with the skin-author editable properties.");
                return;
            }

            var message = errors.Count == 0
                ? "The Skin V2 configuration could not be generated."
                : string.Join(" ", errors.Take(3));
            NotificationManager.Show(NotificationLevel.Error, message);
        }

        private static void ShowUnavailable() => NotificationManager.Show(NotificationLevel.Error,
            "The selected skin's V2 configuration is not loaded.");
    }
}
