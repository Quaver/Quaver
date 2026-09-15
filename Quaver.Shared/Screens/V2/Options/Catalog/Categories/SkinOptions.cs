using System;
using System.IO;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Dialogs;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Platform;

namespace Quaver.Shared.Screens.V2.Options.Catalog.Categories
{
    internal static class SkinOptions
    {
        internal static OptionsRowGroup[] Create() => new[]
        {
            new OptionsRowGroup(OptionsCategoryId.Skin, "Screen_Options_Selection",
                CreateSkinDropdownRow("Screen_Options_CustomSkin", ConfigManager.Skin),
                CreateSkinDropdownRow("Screen_Options_CoopPlayer2Skin", ConfigManager.TournamentPlayer2Skin),
                CreateDefaultSkinDropdownRow()),
            new OptionsRowGroup(OptionsCategoryId.Skin, "Screen_Options_Navigation",
                OptionsRowDefinition.Button("Screen_Options_OpenSkinFolder", "Screen_Options_OpenSkinFolderButton",
                    OpenSkinFolder),
                OptionsRowDefinition.Button("Screen_Options_GenerateEditableSkinYml",
                    "Screen_Options_GenerateEditableSkinYmlButton", GenerateEditableSkinConfig)),
            new OptionsRowGroup(OptionsCategoryId.Skin, "Screen_Options_Sharing",
                OptionsRowDefinition.Button("Screen_Options_ExportSkin", "Screen_Options_ExportSkinButton",
                    SkinManager.Export),
                OptionsRowDefinition.Button("Screen_Options_UploadSkinToSteamWorkshop",
                    "Screen_Options_UploadSkinToSteamWorkshopButton", UploadSkinToWorkshop)),
            new OptionsRowGroup(OptionsCategoryId.Skin, "Screen_Options_Configuration",
                OptionsRowDefinition.Slider("Screen_Options_NoteReceptorSizeScale", ConfigManager.GameplayNoteScale,
                    value => $"{value / 100f:0.00}x"),
                OptionsRowDefinition.Slider("Screen_Options_PlayfieldScale", ConfigManager.PlayfieldScale,
                    value => $"{value / 100f:0.00}x"),
                OptionsRowDefinition.Toggle("Screen_Options_DisplaySongSelectBanners",
                    ConfigManager.DisplaySongSelectBanners),
                OptionsRowDefinition.Toggle("Screen_Options_TintHitlightingBasedOnJudgementColor",
                    ConfigManager.TintHitLightingBasedOnJudgementColor))
        };

        /// <summary>
        ///     A dropdown of every installed skin. Steam Workshop skins are listed as "Name &lt;id&gt;".
        /// </summary>
        private static OptionsRowDefinition CreateSkinDropdownRow(string labelLocalizationKey, Bindable<string> skin)
        {
            var options = SkinStore.GetSkins();

            int GetIndex() => Math.Max(0, options.FindIndex(x => x.Contains(skin.Value)));

            bool OnChanged(string value)
            {
                var isWorkshopSkin = value.Contains("<") && value.Contains(">");

                ConfigManager.UseSteamWorkshopSkin.Value = isWorkshopSkin;
                skin.Value = isWorkshopSkin ? value.Split('<')[1].Replace(">", "") : value;

                RequestSkinReload();
                return true;
            }

            return OptionsRowDefinition.Dropdown(labelLocalizationKey, options, GetIndex, OnChanged);
        }

        /// <summary>
        ///     Which built-in skin is used when no custom skin is selected.
        /// </summary>
        private static OptionsRowDefinition CreateDefaultSkinDropdownRow() =>
            OptionsRowDefinition.Dropdown("Screen_Options_DefaultSkin", Enum.GetNames(typeof(DefaultSkins)),
                () => (int) ConfigManager.DefaultSkin.Value,
                value =>
                {
                    if (!Enum.TryParse<DefaultSkins>(value, out var parsed))
                        return false;

                    ConfigManager.DefaultSkin.Value = parsed;
                    RequestSkinReload();
                    return true;
                });

        private static void RequestSkinReload()
        {
            Transitioner.FadeIn();
            SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
        }

        private static void OpenSkinFolder()
        {
            if (string.IsNullOrEmpty(ConfigManager.Skin.Value))
            {
                NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a skin selected!");
                return;
            }

            var dir = ConfigManager.UseSteamWorkshopSkin.Value
                ? $"{ConfigManager.SteamWorkshopDirectory.Value}/{ConfigManager.Skin.Value}"
                : $"{ConfigManager.SkinDirectory.Value}/{ConfigManager.Skin.Value}";

            if (!Directory.Exists(dir))
            {
                NotificationManager.Show(NotificationLevel.Warning, "Your skin folder does not exist!");
                return;
            }

            Utils.NativeUtils.OpenNatively(dir);
        }

        /// <summary>
        ///     Writes the skin's skin.yml, keeping only the properties skin authors are allowed to edit.
        ///     Asks first when the file already exists.
        /// </summary>
        private static void GenerateEditableSkinConfig()
        {
            var store = SkinManager.SkinV2;

            if (store == null)
            {
                NotificationManager.Show(NotificationLevel.Error, "The selected skin's V2 configuration is not loaded.");
                return;
            }

            void Generate()
            {
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

            if (!File.Exists(store.ConfigPath))
            {
                Generate();
                return;
            }

            DialogManager.Show(new YesNoDialog("Overwrite skin.yml?",
                "This will keep only required metadata and properties marked ConfigEditable.", Generate, () => { }));
        }

        private static void UploadSkinToWorkshop()
        {
            if (string.IsNullOrEmpty(ConfigManager.Skin.Value) || ConfigManager.Skin.Value == "Default Skin")
            {
                NotificationManager.Show(NotificationLevel.Warning, "You currently do not have a selected custom skin!");
                return;
            }

            var skin = new SteamWorkshopItem(ConfigManager.Skin.Value, SkinManager.Skin.Dir.Replace("\\", "/"));

            if (skin.HasUploaded)
                return;

            DialogManager.Show(new UploadWorkshopSkinDialog(skin));
        }
    }
}
