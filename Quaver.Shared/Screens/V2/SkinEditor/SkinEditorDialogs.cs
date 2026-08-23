using System;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Skinning.V2;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Buttons;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.V2.SkinEditor
{
    internal sealed class SkinEditorChoiceDialog : DialogScreen
    {
        private readonly string title;
        private readonly string message;
        private readonly string primaryLabel;
        private readonly Action primaryAction;
        private readonly string secondaryLabel;
        private readonly Action secondaryAction;
        private readonly string cancelLabel;
        private readonly Action cancelAction;

        public SkinEditorChoiceDialog(string title, string message,
            string primaryLabel, Action primaryAction,
            string secondaryLabel, Action secondaryAction,
            string cancelLabel, Action cancelAction) : base(0.75f)
        {
            this.title = title;
            this.message = message;
            this.primaryLabel = primaryLabel;
            this.primaryAction = primaryAction;
            this.secondaryLabel = secondaryLabel;
            this.secondaryAction = secondaryAction;
            this.cancelLabel = cancelLabel;
            this.cancelAction = cancelAction;
            CreateContent();
        }

        public override void CreateContent()
        {
            var panel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(720, 230),
                Tint = new Color(19, 27, 38)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), title, 24)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 26,
                Tint = Color.White
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium), message, 16)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 72,
                Tint = new Color(210, 220, 232),
                TextAlignment = TextAlignment.Center
            };

            CreateButton(panel, primaryLabel, 80, primaryAction, new Color(39, 176, 110));
            CreateButton(panel, secondaryLabel, 270, secondaryAction, new Color(31, 136, 255));
            CreateButton(panel, cancelLabel, 460, cancelAction, new Color(249, 100, 93));
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
            {
                cancelAction?.Invoke();
                DialogManager.Dismiss(this);
            }
        }

        private void CreateButton(Drawable parent, string label, float x, Action action, Color color)
        {
            var button = new RoundedButton((sender, args) =>
            {
                action?.Invoke();
                DialogManager.Dismiss(this);
            })
            {
                Parent = parent,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(x, -30),
                Size = new ScalableVector2(180, 42),
                Tint = color,
                CornerRadius = 6
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), label, 16, Color.White);
        }
    }

    internal sealed class SkinEditorTextPromptDialog : DialogScreen
    {
        private readonly string title;
        private readonly string initialValue;
        private readonly Action<string> action;
        private Textbox textbox;

        public SkinEditorTextPromptDialog(string title, string initialValue, Action<string> action) : base(0.75f)
        {
            this.title = title;
            this.initialValue = initialValue;
            this.action = action;
            CreateContent();
        }

        public override void CreateContent()
        {
            var panel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(620, 210),
                Tint = new Color(19, 27, 38)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), title, 24)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 25,
                Tint = Color.White
            };

            textbox = new Textbox(new ScalableVector2(520, 42),
                FontManager.GetWobbleFont(Fonts.InterMedium), 16, initialValue)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 72,
                Tint = new Color(35, 47, 62)
            };

            CreateButton(panel, LocalizationManager.Get("SkinEditor_Copy"), -105, () =>
            {
                if (string.IsNullOrWhiteSpace(textbox.RawText))
                    return;
                action(textbox.RawText);
                DialogManager.Dismiss(this);
            });
            CreateButton(panel, LocalizationManager.Get("SkinEditor_Cancel"), 105,
                () => DialogManager.Dismiss(this));
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }

        private static void CreateButton(Drawable parent, string label, float x, Action action)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = parent,
                Alignment = Alignment.BotCenter,
                X = x,
                Y = -25,
                Size = new ScalableVector2(180, 40),
                Tint = new Color(31, 136, 255),
                CornerRadius = 6
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), label, 16, Color.White);
        }
    }

    internal sealed class SkinEditorMetadataDialog : DialogScreen
    {
        private readonly SkinV2MetadataConfig metadata;
        private readonly string initialPreviewPath;
        private readonly Action<string, string, string, string> apply;
        private Textbox nameTextbox;
        private Textbox authorTextbox;
        private Textbox versionTextbox;
        private Sprite preview;
        private Texture2D previewTexture;
        private string droppedPreviewPath;

        public SkinEditorMetadataDialog(SkinV2MetadataConfig metadata, string initialPreviewPath,
            Action<string, string, string, string> apply) : base(0.75f)
        {
            this.metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
            this.initialPreviewPath = initialPreviewPath;
            this.apply = apply ?? throw new ArgumentNullException(nameof(apply));
            CreateContent();
            GameBase.Game.Window.FileDropped += OnFileDropped;
        }

        public override void CreateContent()
        {
            var panel = new Sprite
            {
                Parent = Container,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(760, 680),
                Tint = new Color(19, 27, 38)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold),
                LocalizationManager.Get("SkinEditor_MetadataTitle"), 24)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 24,
                Tint = Color.White
            };

            nameTextbox = CreateField(panel, LocalizationManager.Get("SkinEditor_MetadataName"),
                metadata.Name, 40, 78, 210);
            authorTextbox = CreateField(panel, LocalizationManager.Get("SkinEditor_MetadataAuthor"),
                metadata.Author, 275, 78, 210);
            versionTextbox = CreateField(panel, LocalizationManager.Get("SkinEditor_MetadataVersion"),
                metadata.Version, 510, 78, 210);

            var previewBackground = new Sprite
            {
                Parent = panel,
                Position = new ScalableVector2(56, 160),
                Size = new ScalableVector2(648, 370),
                Tint = new Color(32, 44, 58)
            };

            preview = new Sprite
            {
                Parent = previewBackground,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(636, 358),
                Image = UserInterface.BlankBox,
                Tint = new Color(26, 36, 49)
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold),
                LocalizationManager.Get("SkinEditor_PreviewDrop"), 16)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 548,
                Tint = Color.White,
                TextAlignment = TextAlignment.Center
            };

            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterMedium),
                LocalizationManager.Get("SkinEditor_PreviewDropHint"), 13)
            {
                Parent = panel,
                Alignment = Alignment.TopCenter,
                Y = 578,
                Tint = new Color(160, 175, 193),
                TextAlignment = TextAlignment.Center
            };

            CreateDialogButton(panel, LocalizationManager.Get("SkinEditor_MetadataApply"), -105,
                Apply);
            CreateDialogButton(panel, LocalizationManager.Get("SkinEditor_Cancel"), 105,
                () => DialogManager.Dismiss(this), new Color(249, 100, 93));

            if (File.Exists(initialPreviewPath))
                TryShowPreview(initialPreviewPath, false);
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }

        public override void Destroy()
        {
            GameBase.Game.Window.FileDropped -= OnFileDropped;
            previewTexture?.Dispose();
            previewTexture = null;
            base.Destroy();
        }

        private static Textbox CreateField(Drawable parent, string label, string value,
            float x, float y, float width)
        {
            new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterSemiBold), label, 14)
            {
                Parent = parent,
                Position = new ScalableVector2(x, y),
                Tint = new Color(210, 220, 232)
            };

            return new Textbox(new ScalableVector2(width, 40),
                FontManager.GetWobbleFont(Fonts.InterMedium), 16, value ?? string.Empty)
            {
                Parent = parent,
                Position = new ScalableVector2(x, y + 23),
                Tint = new Color(32, 44, 58)
            };
        }

        private static void CreateDialogButton(Drawable parent, string label, float x,
            Action action, Color? color = null)
        {
            var button = new RoundedButton((sender, args) => action())
            {
                Parent = parent,
                Alignment = Alignment.BotCenter,
                X = x,
                Y = -18,
                Size = new ScalableVector2(180, 42),
                Tint = color ?? new Color(31, 136, 255),
                CornerRadius = 6
            };
            button.SetLabel(FontManager.GetWobbleFont(Fonts.InterBold), label, 16, Color.White);
        }

        private void Apply()
        {
            if (string.IsNullOrWhiteSpace(nameTextbox.RawText) ||
                string.IsNullOrWhiteSpace(authorTextbox.RawText) ||
                string.IsNullOrWhiteSpace(versionTextbox.RawText))
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("SkinEditor_MetadataRequired"));
                return;
            }

            apply(nameTextbox.RawText.Trim(), authorTextbox.RawText.Trim(),
                versionTextbox.RawText.Trim(), droppedPreviewPath);
            DialogManager.Dismiss(this);
        }

        private void OnFileDropped(object sender, string path)
        {
            if (!IsOnTop || string.IsNullOrWhiteSpace(path) ||
                !(path.EndsWith(".png", StringComparison.OrdinalIgnoreCase) ||
                  path.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) ||
                  path.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)))
            {
                if (IsOnTop)
                    NotificationManager.Show(NotificationLevel.Error,
                        LocalizationManager.Get("SkinEditor_PreviewInvalid"));
                return;
            }

            TryShowPreview(path, true);
        }

        private void TryShowPreview(string path, bool stage)
        {
            try
            {
                var texture = AssetLoader.LoadTexture2DFromFile(path);
                preview.Image = UserInterface.BlankBox;
                previewTexture?.Dispose();
                previewTexture = texture;
                preview.Image = previewTexture;
                preview.Tint = Color.White;
                if (stage)
                    droppedPreviewPath = path;
            }
            catch
            {
                NotificationManager.Show(NotificationLevel.Error,
                    LocalizationManager.Get("SkinEditor_PreviewInvalid"));
            }
        }
    }
}
