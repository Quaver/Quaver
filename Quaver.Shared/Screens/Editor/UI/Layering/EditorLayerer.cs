using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using IDrawable = Wobble.Graphics.IDrawable;
using Sprite = Wobble.Graphics.Sprites.Sprite;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerer : Sprite
    {
        /// <summary>
        /// </summary>
        private EditorScreen Screen { get; }

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
        public EditorLayererContainer Container { get; private set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorLayerer(EditorScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorLayerPanel;

            CreateHeader();
            CreateScrollContainer();

            // purely for test until actual implementation ----

            Container.AddContainedDrawable(new EditorDrawableLayer(this, "Default Layer")
            {
                Parent = this,
            });

            Container.AddContainedDrawable(new EditorDrawableLayer(this, "Drums")
            {
                Parent = this,
                Y = 40,
                Tint = Color.Transparent
            });

            Container.AddContainedDrawable(new EditorDrawableLayer(this, "Claps")
            {
                Parent = this,
                Y = 80,
                Tint = Color.Transparent
            });

            Container.AddContainedDrawable(new EditorDrawableLayer(this, "Melody")
            {
                Parent = this,
                Y = 120,
                Tint = Color.Transparent
            });
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
        private void CreateScrollContainer()
        {
            Container = new EditorLayererContainer(new ScalableVector2(Width, Height - HeaderBackground.Height))
            {
                Parent = this,
                Y = HeaderBackground.Height,
            };

            Container.ContentContainer.Height = Container.Height + 200;
        }
    }
}