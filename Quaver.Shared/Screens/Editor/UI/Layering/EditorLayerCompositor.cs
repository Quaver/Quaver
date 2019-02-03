using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGame.Extended.Sprites;
using Quaver.API.Maps.Structures;
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
        public EditorLayerScrollContainer ScrollContainer { get; private set; }

        /// <summary>
        ///     The index of the currently selected layer.
        /// </summary>
        public BindableInt SelectedLayerIndex { get; } = new BindableInt(0, 0, int.MaxValue);

        /// <summary>
        ///     Invoked whenever a layer has been updated
        /// </summary>
        public event EventHandler<EditorLayerUpdatedEventArgs> LayerUpdated;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorLayerCompositor(EditorScreen screen)
        {
            Screen = screen;
            Size = new ScalableVector2(230, 194);
            Image = UserInterface.EditorLayerPanel;

            CreateHeader();

            try
            {
                CreateScrollContainer();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }

        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            SelectedLayerIndex.Dispose();
            LayerUpdated = null;
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
        private void CreateDeleteButton() => DeleteButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_times),
            (sender, args) =>
            {
                Screen.Ruleset.ActionManager.RemoveLayer(Screen.WorkingMap, this, ScrollContainer.AvailableItems[SelectedLayerIndex.Value]);
            })
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = Color.Crimson,
            X = -8
        };

        /// <summary>
        /// </summary>
        private void CreateAddButton() => AddButton = new JukeboxButton(FontAwesome.Get(FontAwesomeIcon.fa_plus_black_symbol),
            (sender, args) => Screen.Ruleset.ActionManager.AddLayer(Screen.WorkingMap, this,
                new EditorLayerInfo {Name = $"Layer {ScrollContainer.AvailableItems.Count}"}))
        {
            Parent = HeaderBackground,
            Alignment = Alignment.MidRight,
            Size = new ScalableVector2(20, 20),
            Tint = Color.LimeGreen,
            X = DeleteButton.X - DeleteButton.Width - 10
        };

        /// <summary>
        /// </summary>
        private void CreateScrollContainer() => ScrollContainer = new EditorLayerScrollContainer(this, int.MaxValue, 0,
            new ScalableVector2(Width, Height - HeaderBackground.Height))
        {
            Parent = this,
            Y = HeaderBackground.Height,
        };

        /// <summary>
        /// </summary>
        /// <param name="type"></param>
        /// <param name="layer"></param>
        /// <param name="index"></param>
        public void InvokeUpdatedEvent(EditorLayerUpdateType type, EditorLayerInfo layer, int index)
            => LayerUpdated?.Invoke(this, new EditorLayerUpdatedEventArgs(type, layer, index));
    }
}