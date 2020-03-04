using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Buttons;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Edit.UI.Panels
{
    public class EditorPanel : DraggableButton
    {
        /// <summary>
        /// </summary>
        public Container Header { get; private set; }

        /// <summary>
        /// </summary>
        protected SpriteTextPlus HeaderText { get; private set; }

        /// <summary>
        /// </summary>
        public Container Content { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="header"></param>
        public EditorPanel(string header) : base(UserInterface.EditorPanelBackground)
        {
            Size = new ScalableVector2(288, 237);
            CreateHeader(header);
            CreateContent();
        }

        /// <summary>
        /// </summary>
        /// <param name="text"></param>
        private void CreateHeader(string text)
        {
            Header = new Container()
            {
                Parent = this,
                Size = new ScalableVector2(Width, 45)
            };

            HeaderText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), text.ToUpper(), 22)
            {
                Parent = Header,
                Alignment = Alignment.MidLeft,
                X = 18
            };
        }

        /// <summary>
        /// </summary>
        private void CreateContent()
        {
            Content = new Container()
            {
                Parent = this,
                Y = Header.Height,
                Size = new ScalableVector2(Width, Height - Header.Height)
            };
        }
    }
}