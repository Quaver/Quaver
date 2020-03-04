using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class EditorPanelLayers : EditorPanel
    {
        /// <summary>
        /// </summary>
        private Qua WorkingMap { get; }

        /// <summary>
        /// </summary>
        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        /// <summary>
        /// </summary>
        private IconButton DeleteLayer { get; set; }

        /// <summary>
        /// </summary>
        private IconButton CreateLayer { get; set; }

        /// <summary>
        /// </summary>
        private EditorPanelLayersScrollContainer ScrollContainer { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorPanelLayers(Qua workingMap, Bindable<EditorLayerInfo> selectedLayer) : base("Layers")
        {
            WorkingMap = workingMap;
            SelectedLayer = selectedLayer;
            Depth = 1;

            CreateDeleteButton();
            CreateAddButton();
            CreateScrollContainer();
        }

        /// <summary>
        ///     Creates <see cref="DeleteLayer"/>
        /// </summary>
        private void CreateDeleteButton()
        {
            DeleteLayer = new IconButton(UserInterface.EditorDeleteLayer)
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(20, 20),
                X = -HeaderText.X
            };
        }

        /// <summary>
        ///     Creates <see cref="CreateLayer"/>
        /// </summary>
        private void CreateAddButton()
        {
            CreateLayer = new IconButton(UserInterface.EditorCreateLayer)
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                Size = DeleteLayer.Size,
                X = DeleteLayer.X - DeleteLayer.Width + DeleteLayer.X
            };
        }

        /// <summary>
        ///     Creates <see cref="ScrollContainer"/>
        /// </summary>
        private void CreateScrollContainer()
        {
            ScrollContainer = new EditorPanelLayersScrollContainer(WorkingMap, SelectedLayer,
                new ScalableVector2(Content.Width - 7, Content.Height - 8))
            {
                Parent = Content,
                X = 4
            };
        }
    }
}