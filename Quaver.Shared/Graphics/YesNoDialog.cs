using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.Shared.Assets;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Managers;

namespace Quaver.Shared.Graphics
{
    public class YesNoDialog : DialogScreen
    {
        /// <summary>
        ///     The time it takes to fade in/out the dialog
        /// </summary>
        private const int FadeTime = 150;

        /// <summary>
        /// </summary>
        private string HeaderText { get; }

        /// <summary>
        /// </summary>
        private string ConfirmationText { get; }

        /// <summary>
        /// </summary>
        protected Sprite Panel { get; private set; }

        /// <summary>
        /// </summary>
        protected Sprite Banner { get; set; }

        /// <summary>
        /// </summary>
        private Sprite Blackness { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Header { get; set; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Confirmation { get; set; }

        /// <summary>
        /// </summary>
        protected IconButton YesButton { get; set; }

        /// <summary>
        /// </summary>
        protected IconButton NoButton { get; set; }

        /// <summary>
        /// </summary>
        protected Action YesAction { get; set; }

        /// <summary>
        /// </summary>
        protected Action NoAction { get; set; }

        /// <summary>
        /// </summary>
        protected Func<bool> ValidateBeforeClosing { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public YesNoDialog(string header, string confirmationText, Action yesAction = null, Action noAction = null) : base(0)
        {
            HeaderText = header;
            ConfirmationText = confirmationText;
            YesAction = yesAction;
            NoAction = noAction;

            FadeTo(0.85f, Easing.Linear, FadeTime);

            // ReSharper disable once VirtualMemberCallInConstructor
            CreateContent();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void CreateContent()
        {
            CreatePanel();
            CreateBanner();
            CreateHeader();
            CreateConfirmation();
            CreateButtons();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.First() == this)
            {
                if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                {
                    NoAction?.Invoke();
                    Close();
                    return;
                }

                if (KeyboardManager.IsUniqueKeyPress(Keys.Enter))
                {
                    YesAction?.Invoke();

                    if (ValidateBeforeClosing == null)
                    {
                        Close();
                        return;
                    }

                    var canClose = ValidateBeforeClosing?.Invoke();

                    if (canClose.Value)
                        Close();

                    return;
                }
            }

            if (MouseManager.IsUniqueClick(MouseButton.Left) && !Panel.IsHovered())
            {
                NoAction?.Invoke();
                Close();
                return;
            }
        }

        /// <summary>
        /// </summary>
        public virtual void Close()
        {
            FadeTo(0, Easing.Linear, FadeTime);

            const int fadeTime = FadeTime - 50;

            Panel.ClearAnimations();
            Panel.FadeTo(0, Easing.Linear, fadeTime);

            Blackness.ClearAnimations();
            Blackness.FadeTo(0, Easing.Linear, fadeTime);

            Confirmation.ClearAnimations();
            Confirmation.FadeTo(0, Easing.Linear, fadeTime);

            NoButton.IsPerformingFadeAnimations = false;
            NoButton.IsClickable = false;

            YesButton.IsPerformingFadeAnimations = false;
            YesButton.IsClickable = false;

            ThreadScheduler.RunAfter(() => DialogManager.Dismiss(this), FadeTime);
        }

        /// <summary>
        /// </summary>
        private void CreatePanel()
        {
            Panel = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                Size = new ScalableVector2(770, 286),
                Image = UserInterface.YesNoPanel,
                Alpha = 0,
                SetChildrenAlpha = true
            };

            Panel.FadeTo(1, Easing.Linear, FadeTime + 50);
        }

        /// <summary>
        /// </summary>
        private void CreateBanner()
        {
            Banner = new Sprite
            {
                Parent = Panel,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(766, 151),
                Image = UserInterface.DefaultBanner,
                Alpha = 0,
                Y = 2,
            };

            Blackness = new Sprite()
            {
                Parent = Banner,
                Size = Banner.Size,
                Tint = Color.Black,
                Alpha = 0
            };

            Blackness.FadeTo(0.7f, Easing.Linear, FadeTime + 50);
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), HeaderText.ToUpper(), 26)
            {
                Parent = Panel,
                Alignment = Alignment.TopLeft,
            };

            Header.Y = -Header.Height - 6;
        }

        /// <summary>
        /// </summary>
        private void CreateConfirmation()
        {
            Confirmation = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), ConfirmationText, 26)
            {
                Parent = Banner,
                Alignment = Alignment.MidCenter,
                TextAlignment = TextAlignment.Center
            };
        }

        /// <summary>
        /// </summary>
        private void CreateButtons()
        {
            YesButton = new IconButton(UserInterface.CreateButton, (o, e) =>
            {
                YesAction?.Invoke();

                if (ValidateBeforeClosing == null)
                {
                    Close();
                    return;
                }

                var canClose = ValidateBeforeClosing?.Invoke();

                if (canClose.Value)
                    Close();
            })
            {
                Parent = Panel,
                Alignment = Alignment.BotLeft,
                Size = new ScalableVector2(221, 40),
                Y = -50,
                X = 140
            };

            NoButton = new IconButton(UserInterface.CancelButton, (o, e) =>
            {
                NoAction?.Invoke();
                Close();
            })
            {
                Parent = Panel,
                Alignment = Alignment.BotRight,
                Size = YesButton.Size,
                Y = YesButton.Y,
                X = -YesButton.X
            };
        }
    }
}