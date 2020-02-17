using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Edit.UI.Footer;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Edit
{
    public class EditScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        private EditScreen EditScreen => (EditScreen) Screen;

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private EditorPlayfield Playfield { get; set; }

        /// <summary>
        /// </summary>
        private EditorPlayfield UnEditablePlayfield { get; set; }

        /// <summary>
        /// </summary>
        private EditorFooter Footer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public EditScreenView(Screen screen) : base(screen)
        {
            CreateBackground();
            CreatePlayfield();
            CreateFooter();

            EditScreen.UneditableMap.ValueChanged += OnUneditableMapChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(ColorHelper.HexToColor("#2F2F2F"));
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Container?.Destroy();

            // ReSharper disable twice DelegateSubtraction
            EditScreen.UneditableMap.ValueChanged -= OnUneditableMapChanged;
        }

        /// <summary>
        /// </summary>
        private void CreateBackground()
        {
            Background = new BackgroundImage(UserInterface.Triangles, 0, false)
            {
                Parent = Container
            };
        }

        /// <summary>
        /// </summary>
        private void CreatePlayfield() => Playfield = new EditorPlayfield(EditScreen.WorkingMap, EditScreen.Skin,
            EditScreen.Track, EditScreen.BeatSnap, EditScreen.PlayfieldScrollSpeed, EditScreen.AnchorHitObjectsAtMidpoint) { Parent = Container};

        /// <summary>
        ///
        /// </summary>
        private void CreateFooter() => Footer = new EditorFooter(EditScreen.Track)
        {
            Parent = Container,
            Alignment = Alignment.BotLeft
        };

        /// <summary>
        /// </summary>
        private void CreateOtherDifficultyPlayfield()
        {
            UnEditablePlayfield = new EditorPlayfield(EditScreen.UneditableMap.Value, EditScreen.Skin, EditScreen.Track,
                EditScreen.BeatSnap, EditScreen.PlayfieldScrollSpeed, EditScreen.AnchorHitObjectsAtMidpoint, true)
            {
                Parent = Container,
            };

            // Reset the parent of the footer, so it draws over this playfield.
            Footer.Parent = Container;
        }

        /// <summary>
        /// </summary>
        private void PositionPlayfields()
        {
            if (UnEditablePlayfield == null)
                return;

            const int spacing = 60;

            Playfield.X = -Playfield.Width / 2 - spacing;
            UnEditablePlayfield.X = Playfield.Width / 2 + spacing;

            Playfield.ResetObjectPositions();
            UnEditablePlayfield.ResetObjectPositions();
        }

        /// <summary>
        ///     Called when the user wants to view an uneditable map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUneditableMapChanged(object sender, BindableValueChangedEventArgs<Qua> e)
        {
            UnEditablePlayfield?.Destroy();

            CreateOtherDifficultyPlayfield();
            PositionPlayfields();
        }
    }
}