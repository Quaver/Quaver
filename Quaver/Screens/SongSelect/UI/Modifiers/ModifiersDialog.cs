using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Assets;
using Quaver.Graphics;
using Quaver.Modifiers.Mods.Gameplay;
using Quaver.Online.Chat;
using Quaver.Scheduling;
using Quaver.Screens.Select.UI.Mods;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Window;

namespace Quaver.Screens.SongSelect.UI.Modifiers
{
    public class ModifiersDialog : DialogScreen
    {
        /// <summary>
        ///     The actual content of the interface
        /// </summary>
        private Sprite InterfaceContainer { get; set; }

        /// <summary>
        ///     The container for the header of the interface.
        /// </summary>
        private Sprite HeaderContainer { get; set; }

        /// <summary>
        ///     The scroll container which houses all of the chat channels.
        /// </summary>
        public ScrollContainer ModifierContainer { get; private set; }

        /// <summary>
        ///     The list of modifiers that are in the dialog.
        /// </summary>
        private List<DrawableModifier> ModsList { get; set; }

        private bool isClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public ModifiersDialog() : base(0)
        {
            CreateContent();

            Clicked += (sender, args) =>
            {
                if (!GraphicsHelper.RectangleContains(InterfaceContainer.ScreenRectangle,
                    MouseManager.CurrentState.Position) && !isClosing)
                {
                    Close();
                    isClosing = true;
                }
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override void CreateContent()
        {
            CreateInterfaceContainer();
            CreateHeaderContainer();
            CreateModifierContainer();
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
        ///     Creates the container for the entire dialog.
        /// </summary>
        private void CreateInterfaceContainer() => InterfaceContainer = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            SetChildrenAlpha = true,
            Size = new ScalableVector2(Width, 350),
            Tint = Color.Black,
            Y = 350,
            Alpha = 0,
            Animations =
            {
                new Animation(AnimationProperty.Alpha, Easing.Linear, 0, 1, 400),
                new Animation(AnimationProperty.Y, Easing.OutQuint, 400, 0, 800)
            },
        };

        /// <summary>
        ///     Creates the header container for the interface.
        /// </summary>
        private void CreateHeaderContainer()
        {
            HeaderContainer = new Sprite()
            {
                Parent = InterfaceContainer,
                Size = new ScalableVector2(Width, 75),
                Tint = Colors.DarkGray,
            };

            var line = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(HeaderContainer.Width, 2),
                Tint = Colors.MainAccent
            };

            var icon = new Sprite()
            {
                Parent = HeaderContainer,
                Alignment = Alignment.MidLeft,
                X = 25,
                Size = new ScalableVector2(HeaderContainer.Height * 0.50f, HeaderContainer.Height * 0.50f),
                Image = FontAwesome.Get(FontAwesomeIcon.fa_level_up),
            };

            var chatChannels = new SpriteText(BitmapFonts.Exo2SemiBold, "Modifiers", 14)
            {
                Parent = icon,
                Y = -3,
                X = icon.Width + 15,
            };

            var description = new SpriteText(BitmapFonts.Exo2Medium, "Switch it up for a change, and customize gameplay to your heart's desire.",
                13)
            {
                Parent = icon,
                Y = chatChannels.Y + chatChannels.Height - 2,
                X = icon.Width + 15,
            };
        }

        /// <summary>
        ///     Creates the scroll container that'll house all the modifiers
        /// </summary>
        private void CreateModifierContainer()
        {
            var size = new ScalableVector2(InterfaceContainer.Width, InterfaceContainer.Height - HeaderContainer.Height);
            ModifierContainer = new ScrollContainer(size, size)
            {
                Parent = InterfaceContainer,
                Y = HeaderContainer.Height,
                Tint = Color.Black,
                InputEnabled = true
            };

            ModifierContainer.Scrollbar.Tint = Color.White;
            ModifierContainer.Scrollbar.Width = 5;
            ModifierContainer.ScrollSpeed = 150;
            ModifierContainer.EasingType = Easing.OutQuint;
            ModifierContainer.TimeToCompleteScroll = 1500;
        }

        /// <summary>
        ///     Creates all of the modifiers to be used on-screen
        /// </summary>
        private void CreateModifierOptions()
        {
            ModsList = new List<DrawableModifier>()
            {
                new DrawableModifierSpeed(this)
                {
                    Alignment = Alignment.TopLeft
                },

                new DrawableModifierBool(this, new ModAutoplay())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ModNoFail())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ManiaModNoSliderVelocities())
                {
                    Alignment = Alignment.TopLeft,
                },

                new DrawableModifierBool(this, new ManiaModNoPause())
                {
                    Alignment = Alignment.TopLeft,
                },
            };

            for (var i = 0; i < ModsList.Count; i++)
            {
                var mod = ModsList[i];
                ModifierContainer.AddContainedDrawable(mod);
                mod.Y = i * mod.Height;
            }

            var proposedHeight = ModsList.Count * ModsList.First().Height;

            if (proposedHeight > ModifierContainer.ContentContainer.Height)
                ModifierContainer.ContentContainer.Height = proposedHeight;
        }

        /// <summary>
        ///     Closes the dialog.
        /// </summary>
        private void Close()
        {
            if (isClosing)
                return;

            isClosing = true;
            InterfaceContainer.Animations.Clear();
            InterfaceContainer.Animations.Add(new Animation(AnimationProperty.Y, Easing.OutQuint, InterfaceContainer.Y, InterfaceContainer.Height, 600));
            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), 450);
        }
    }
}