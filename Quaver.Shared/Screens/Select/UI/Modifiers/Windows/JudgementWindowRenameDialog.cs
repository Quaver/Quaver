using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Quaver.API.Maps.Processors.Scoring;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Judgements;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Online.Username;
using Quaver.Shared.Screens.Selection.UI.Modifiers.Dialogs.Windows;
using TagLib.Id3v2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Input;
using Wobble.Managers;
using Wobble.Window;

namespace Quaver.Shared.Screens.Select.UI.Modifiers.Windows
{
    public class JudgementWindowRenameDialog : DialogScreen
    {
        /// <summary>
        /// </summary>
        private JudgementWindows Windows { get; }

        /// <summary>
        ///     The containing box for the dialog.
        /// </summary>
        private Sprite ContainingBox { get; set; }

        /// <summary>
        ///     The header text to create a username.
        /// </summary>
        private SpriteText Header { get; set; }

        /// <summary>
        ///     The text content of the dialog which displays the requirements for usernames.
        /// </summary>
        private SpriteText TextContent { get; set; }

        /// <summary>
        ///     The textbox to enter a username.
        /// </summary>
        private Textbox Textbox { get; set; }

        private JudgementWindowContainer WindowContainer { get; }

        public JudgementWindowRenameDialog(JudgementWindows windows, JudgementWindowContainer windowContainer) : base(0.75f)
        {
            Windows = windows;
            WindowContainer = windowContainer;

            CreateContent();
        }

        public override void Update(GameTime gameTime)
        {
            Textbox.Focused = DialogManager.Dialogs.Last() == this;

            base.Update(gameTime);
        }

        public sealed override void CreateContent()
        {
            ContainingBox = new Sprite()
            {
                Parent = Container,
                Size = new ScalableVector2(WindowManager.Width, 200),
                Alignment = Alignment.MidCenter,
                Tint = Color.Black,
                Alpha = 0.95f
            };

            var line = new Sprite()
            {
                Parent = ContainingBox,
                Size = new ScalableVector2(ContainingBox.Width, 1),
                Tint = Colors.MainAccent
            };

            Header = new SpriteText(Fonts.Exo2Bold, $"Rename Judgement Window Preset", 20)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = 25
            };

            TextContent = new SpriteText(Fonts.Exo2SemiBold,
                "Choose a new name for your judgement window preset", 14)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = Header.Y + Header.Height + 5
            };

            Textbox = new Textbox(new ScalableVector2(400, 40), FontManager.GetWobbleFont(Fonts.LatoBlack), 16, Windows.Name)
            {
                Parent = ContainingBox,
                Alignment = Alignment.TopCenter,
                Y = TextContent.Y + 35,
                Alpha = 0,
                InputText =
                {
                    Alignment = Alignment.MidLeft
                },
                Cursor =
                {
                    Alignment = Alignment.MidLeft
                }
            };

            Textbox.AddBorder(Colors.MainAccent, 2);
            Textbox.OnSubmit += (e) =>
            {
                Windows.Name = e;
                JudgementWindowsDatabaseCache.Update(Windows);

                var windows = WindowContainer.Pool.Find(x => x.Item == Windows);

                if (windows != null)
                {
                    var w = (DrawableJudgementWindows) windows;
                    w.Name.Text = e;
                }

                DialogManager.Dismiss(this);
            };
        }

        public override void HandleInput(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Last() != this)
                return;

            if (KeyboardManager.IsUniqueKeyPress(Keys.Escape))
                DialogManager.Dismiss(this);
        }
    }
}