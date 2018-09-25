using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Graphics.Notifications;
using Quaver.Helpers;
using Quaver.Modifiers;
using Quaver.Modifiers.Mods.Mania;
using Quaver.Scheduling;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Transformations;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;
using Keys = Microsoft.Xna.Framework.Input.Keys;

namespace Quaver.Screens.Select.UI.Mods
{
    public class ModsDialog : DialogScreen
    {
        /// <summary>
        ///     The background for the scene.
        /// </summary>
        public Sprite SceneBackground { get; private set; }

        /// <summary>
        ///     The header background
        /// </summary>
        public Sprite SceneHeaderBackground { get; private set; }

        /// <summary>
        ///     Border line above the header.
        /// </summary>
        public Sprite SceneHeaderBorderLineTop { get; private set; }

        /// <summary>
        ///     Border line below the header.
        /// </summary>
        public Sprite SceneHeaderBorderLineBottom { get; private set; }

        /// <summary>
        ///     Header icon.
        /// </summary>
        public Sprite SceneHeaderIcon { get; private set; }

        /// <summary>
        ///     Header text.
        /// </summary>
        public SpriteText SceneHeaderText { get; private set; }

        /// <summary>
        ///     Header sub text.
        /// </summary>
        public SpriteText SceneHeaderSubText { get; private set; }

        /// <summary>
        ///    Background for the footer.
        /// </summary>
        public Sprite FooterBackground { get; private set; }

        /// <summary>
        ///     Border line for the footer.
        /// </summary>
        public Sprite FooterBorderLineTop { get; private set; }

        /// <summary>
        ///     Button to close the dialog
        /// </summary>
        private TextButton CloseButton { get; set; }

        /// <summary>
        ///     Button to remove all mods.
        /// </summary>
        private TextButton RemoveAllModsButton { get; set; }

        /// <summary>
        ///     The list of modifiers that are in the dialog.
        /// </summary>
        private List<ModsDialogModifier> Modifiers { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ModsDialog() : base(0f) => CreateContent();

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateBackground();
            CreateHeader();
            CreateFooter();
            CreateModifierOptions();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                Close();
        }

        /// <summary>
        ///    Creates the background scene of the dialog.
        /// </summary>
        private void CreateBackground() => SceneBackground = new Sprite
        {
            Parent = Container,
            Size = new ScalableVector2(WindowManager.Width, WindowManager.Height - 200),
            Y = WindowManager.Height,
            Tint = new Color(63, 68, 91),
            Alpha = 1,
            Transformations =
            {
                new Transformation(TransformationProperty.Y, Easing.EaseOutQuint, WindowManager.Height, 200, 600)
            }
        };

        /// <summary>
        ///     Creates the header of the dialog.
        /// </summary>
        private void CreateHeader()
        {
            SceneHeaderBackground = new Sprite()
            {
                Parent = SceneBackground,
                Size = new ScalableVector2(WindowManager.Width, 85),
                Alignment = Alignment.TopLeft,
                Tint = Colors.DarkGray
            };

            SceneHeaderBorderLineTop = new Sprite()
            {
                Parent = SceneHeaderBackground,
                Size =  new ScalableVector2(WindowManager.Width, 1),
                Alignment = Alignment.TopLeft,
                Tint = Colors.SecondaryAccent
            };

            SceneHeaderBorderLineBottom = new Sprite()
            {
                Parent = SceneHeaderBackground,
                Size =  new ScalableVector2(WindowManager.Width, 1),
                Alignment = Alignment.BotLeft,
                Tint = Colors.SecondaryAccent
            };

            SceneHeaderIcon = new Sprite()
            {
                Parent = SceneHeaderBackground,
                Size = new ScalableVector2(50, 50),
                X = 25,
                Image = FontAwesome.ArrowCircle,
                Alignment = Alignment.MidLeft
            };

            SceneHeaderText = new SpriteText(Fonts.Exo2Regular24, "Game Modifiers", 0.65f)
            {
                Parent = SceneHeaderIcon,
                X = SceneHeaderIcon.Width + 15,
                Y = 3
            };

            SceneHeaderText.X += SceneHeaderText.MeasureString().X / 2f;
            SceneHeaderText.Y += SceneHeaderText.MeasureString().Y / 2f;

            SceneHeaderSubText = new SpriteText(Fonts.Exo2Italic24, "Game modifiers allow you to increase " +
                                                                    "or decrease the difficulty of gameplay. \"Who Dares, Wins!\"", 0.45f)
            {
                Parent = SceneHeaderIcon,
                X = SceneHeaderIcon.Width + 15,
                Y = SceneHeaderText.Y + 15
            };

            SceneHeaderSubText.Y += SceneHeaderSubText.MeasureString().Y / 2f;
            SceneHeaderSubText.X += SceneHeaderSubText.MeasureString().X / 2f;
        }

        /// <summary>
        ///     Creates the footer of the dialog.
        /// </summary>
        private void CreateFooter()
        {
            FooterBackground = new Sprite()
            {
                Parent = SceneBackground,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(WindowManager.Width, 65),
                Tint = Colors.DarkGray,
            };

            FooterBorderLineTop = new Sprite()
            {
                Parent = FooterBackground,
                Size =  new ScalableVector2(WindowManager.Width, 1),
                Alignment = Alignment.TopLeft,
                Tint = Colors.MainAccent
            };

            CloseButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Close", 0.50f,
                (o, e) => { Close(); })
            {
                Parent = FooterBackground,
                X = 25,
                Size = new ScalableVector2(250, FooterBackground.Height * 0.60f),
                Alignment = Alignment.MidLeft,
                Tint = Color.Black
            };

            RemoveAllModsButton = new TextButton(UserInterface.BlankBox, Fonts.Exo2Regular24, "Remove all mods", 0.50f, (sender, args) => ModManager.RemoveAllMods())
            {
                Parent = FooterBackground,
                X = CloseButton.Width + CloseButton.X + 15,
                Size = new ScalableVector2(250, FooterBackground.Height * 0.60f),
                Alignment = Alignment.MidLeft,
                Tint = Color.Black
            };
        }

        /// <summary>
        ///     Creates all of the modifier options.
        /// </summary>
        private void CreateModifierOptions()
        {
            Modifiers = new List<ModsDialogModifier>
            {
                // Autoplay
                new ModsDialogModifierBool(this, new ManiaModAutoplay())
                {
                    Parent = SceneBackground,
                    Alignment = Alignment.TopLeft,
                    Y = SceneHeaderBackground.Y + SceneHeaderBackground.Height + 90
                },

                // No Pause
                new ModsDialogModifierBool(this, new ManiaModNoPause())
                {
                    Parent = SceneBackground,
                    Alignment = Alignment.TopLeft,
                    Y = SceneHeaderBackground.Y + SceneHeaderBackground.Height + 20
                }
            };

            for (var i = 0; i < Modifiers.Count; i++)
            {
                Modifiers[i].Y = SceneHeaderBackground.Y + SceneHeaderBackground.Height + i * Modifiers[0].Height + i * 5;
            }
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        private void Close()
        {
            Height = SceneBackground.Height;
            Alignment = Alignment.BotLeft;


            Transformations.Clear();
            Transformations.Add(new Transformation(TransformationProperty.Y, Easing.EaseOutQuint, Y, WindowManager.Height, 900));
            Scheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }
    }
}