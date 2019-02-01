using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using IDrawable = Wobble.Graphics.IDrawable;
using Sprite = Wobble.Graphics.Sprites.Sprite;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerCompositor : Sprite
    {
        /// <summary>
        /// </summary>
        public EditorScreen Screen { get; }

        /// <summary>
        /// </summary>
        private Sprite HeaderBackground { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton DeleteButton { get; set; }

        /// <summary>
        /// </summary>
        private JukeboxButton AddButton { get; set; }

        /// <summary>
        /// </summary>
        public EditorLayerContainer Container { get; private set; }

        /// <summary>
        ///     The index of the currently selected layer.
        /// </summary>
        public BindableInt SelectedLayerIndex { get; } = new BindableInt(0, 0, int.MaxValue);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorLayerCompositor(EditorScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorLayerPanel;

            CreateHeader();
            CreateScrollContainer();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectedLayerIndex.Dispose();
            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            HeaderBackground = new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                Size = new ScalableVector2(Width, 34),
                Tint = Color.Transparent,
            };

            CreateDeleteButton();
            CreateAddButton();
        }

        /// <summary>
        /// </summary>
        private void CreateDeleteButton() => DeleteButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_times))
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = Color.Crimson,
            X = -8
        };

        /// <summary>
        /// </summary>
        private void CreateAddButton() => AddButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol))
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = Color.LimeGreen,
            X = DeleteButton.X - DeleteButton.Width - 10
        };

        /// <summary>
        /// </summary>
        private void CreateScrollContainer() => Container = new EditorLayerContainer(this, 6, 0, new ScalableVector2(Width, Height - HeaderBackground.Height))
        {
            Parent = this,
            Y = HeaderBackground.Height,
        };
    }
}