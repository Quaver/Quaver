using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;

namespace Quaver.Shared.Screens.Settings
{
    public class SettingsDialog : DialogScreen
    {
        /// <summary>
        ///     The entire container for the settings menu
        /// </summary>
        public Sprite ContentContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite HeaderContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite FooterContainer { get; private set; }

        /// <summary>
        /// </summary>
        public Sprite DividerLine { get; private set; }

        /// <summary>
        ///     The list of available settings sections.
        /// </summary>
        private List<SettingsSection> Sections { get; set; }

        /// <summary>
        ///     The currently selected options section.
        /// </summary>
        public SettingsSection SelectedSection { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public SettingsDialog() : base(0)
        {
            Animations.Add(new Animation(AnimationProperty.Alpha, Easing.OutQuint, Alpha, 0.65f, 300));
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateContentContainer();
            CreateHeader();
            CreateFooter();
            CreateDividerLine();
            CreateSections();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }

        /// <summary>
        /// </summary>
        private void CreateContentContainer() => ContentContainer = new Sprite
        {
            Parent = Container,
            Alignment = Alignment.MidCenter,
            Size = new ScalableVector2(1200, 620),
            Tint = ColorHelper.HexToColor($"#414345"),
            Alpha = 1f
        };

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderContainer = new Sprite
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 45),
                Tint = ColorHelper.HexToColor($"#212121")
            };

            var headerFlag = new Sprite()
            {
                Parent = HeaderContainer,
                Size = new ScalableVector2(5, HeaderContainer.Height),
                Tint = Color.LightGray,
                Alpha = 0
            };

            var headerText = new SpriteText(BitmapFonts.Exo2Medium, "Options Menu", 16)
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = headerFlag.X + 15
            };

            var exitButton = new ImageButton(FontAwesome.Get(FontAwesomeIcon.fa_times), (sender, args) => DialogManager.Dismiss())
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(25, 25)
            };

            exitButton.X -= exitButton.Width / 2f + 5;
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            FooterContainer = new Sprite()
            {
                Parent = ContentContainer,
                Size = new ScalableVector2(ContentContainer.Width, 50),
                Tint = ColorHelper.HexToColor("#212121"),
                Alignment = Alignment.BotLeft,
                Y = 1
            };
        }

        /// <summary>
        /// </summary>
        private void CreateDividerLine() => DividerLine = new Sprite
        {
            Parent = ContentContainer,
            Size = new ScalableVector2(1, ContentContainer.Height - HeaderContainer.Height - FooterContainer.Height - 20),
            X = 230,
            Alignment = Alignment.MidLeft,
            Alpha = 0.75f
        };

        /// <summary>
        /// </summary>
        private void CreateSections()
        {
            Sections = new List<SettingsSection>
            {
                // Video
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_desktop_monitor), "Video", new List<Drawable>
                {
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),
                    new SettingsItem(this, "Resolution"),

                }),
                // Audio
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_volume_up_interface_symbol), "Audio", new List<Drawable>
                {
                }),
                // Gameplay
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_gamepad_console), "Gameplay", new List<Drawable>
                {
                }),
                // Skin
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_pencil), "Skin", new List<Drawable>
                {
                }),
                // Input
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_keyboard), "Input", new List<Drawable>
                {
                }),
                // Network
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), "Network", new List<Drawable>
                {
                }),
                // Misc
                new SettingsSection(this, FontAwesome.Get(FontAwesomeIcon.fa_question_sign), "Miscellaneous", new List<Drawable>
                {
                })
            };

            SelectedSection = Sections.First();
            AlignSectionButtons();
        }

        /// <summary>
        ///     Sets the position of the section buttons
        /// </summary>
        private void AlignSectionButtons()
        {
            for (var i = 0; i < Sections.Count; i++)
            {
                var button = Sections[i].Button;
                button.Parent = ContentContainer;

                button.X = 10;
                button.Y = HeaderContainer.Height + 15 + button.Height * i + 15 * i;

                if (Sections[i] == SelectedSection)
                {
                    button.DisplayAsSelected();
                    Sections[i].Container.Parent = ContentContainer;
                }
                else
                {
                    button.DisplayAsDeselected();
                    Sections[i].Container.Visible = false;
                }
            }
        }

        /// <summary>
        /// </summary>
        public void SwitchSelected(SettingsSection section)
        {
            SelectedSection.Button.DisplayAsDeselected();
            SelectedSection.Container.Visible = false;
            SelectedSection.Container.Parent = null;

            SelectedSection = section;
            SelectedSection.Button.DisplayAsSelected();
            SelectedSection.Container.Visible = true;
            SelectedSection.Container.Parent = ContentContainer;

            Logger.Debug($"Switched to options section: {section.Name}", LogType.Runtime);
        }
    }
}