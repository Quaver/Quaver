using Microsoft.Xna.Framework;
using Quaver.API.Maps;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Screens.Edit.Actions;
using Quaver.Shared.Screens.Edit.Actions.Layers.Create;
using Quaver.Shared.Screens.Edit.Actions.Layers.Remove;
using Quaver.Shared.Screens.Edit.UI.Playfield;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;

namespace Quaver.Shared.Screens.Edit.UI.Panels.Layers
{
    public class EditorPanelLayers : EditorPanel
    {
        private Qua WorkingMap { get; }

        private Bindable<EditorLayerInfo> SelectedLayer { get; }

        private IconButton DeleteLayer { get; set; }

        private IconButton CreateLayer { get; set; }

        private Checkbox ToggleLayers { get; set; }

        private EditorPanelLayersScrollContainer ScrollContainer { get; set; }

        private EditorLayerInfo DefaultLayer { get; }

        private EditorActionManager ActionManager { get; }

        private BindableList<HitObjectInfo> SelectedHitObjects { get; }

        private Bindable<HitObjectColoring> HitObjectColoring { get; }

        private Bindable<bool> ViewLayers { get; }

        private bool _viewLayerNotifyEnabled = true;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public EditorPanelLayers(EditorActionManager actionManager, Qua workingMap, Bindable<EditorLayerInfo> selectedLayer,
            EditorLayerInfo defaultLayer, BindableList<HitObjectInfo> selectedHitObjects, Bindable<HitObjectColoring> hitObjectColoring) : base("Layers")
        {
            ActionManager = actionManager;
            WorkingMap = workingMap;
            SelectedLayer = selectedLayer;
            DefaultLayer = defaultLayer;
            SelectedHitObjects = selectedHitObjects;
            HitObjectColoring = hitObjectColoring;
            ViewLayers = new Bindable<bool>(HitObjectColoring.Value == Playfield.HitObjectColoring.Layer);
            HitObjectColoring.ValueChanged += ColoringChanged;
            ViewLayers.ValueChanged += ViewLayersChanged;

            Depth = 1;

            CreateDeleteButton();
            CreateAddButton();
            CreateToggleLayersCheckbox();
            CreateScrollContainer();
        }

        private void ViewLayersChanged(object sender, BindableValueChangedEventArgs<bool> e)
        {
            if (!_viewLayerNotifyEnabled)
                return;

            _viewLayerNotifyEnabled = false;
            HitObjectColoring.Value = e.Value ? Playfield.HitObjectColoring.Layer : Playfield.HitObjectColoring.None;
        }

        private void ColoringChanged(object sender, BindableValueChangedEventArgs<HitObjectColoring> e)
        {
            _viewLayerNotifyEnabled = false;
            ViewLayers.Value = e.Value == Playfield.HitObjectColoring.Layer;
            _viewLayerNotifyEnabled = true;
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

            DeleteLayer.Clicked += (sender, args) =>
            {
                if (SelectedLayer.Value == DefaultLayer || SelectedLayer.Value == null)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "You cannot delete the default layer!");
                    return;
                }

                ActionManager.Perform(new EditorActionRemoveLayer(ActionManager, WorkingMap, SelectedHitObjects, SelectedLayer.Value));
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

            CreateLayer.Clicked += (sender, args) =>
            {
                var layer = new EditorLayerInfo
                {
                    Name = $"Layer {WorkingMap.EditorLayers.Count + 1}",
                    ColorRgb = "255,255,255"
                };

                // FindIndex() returns -1 when the default layer is selected
                int index = WorkingMap.EditorLayers.FindIndex(l => l == SelectedLayer.Value) + 2;

                ActionManager.Perform(new EditorActionCreateLayer(WorkingMap, ActionManager, SelectedHitObjects, layer, index));
            };
        }

        /// <summary>
        /// </summary>
        private void CreateToggleLayersCheckbox()
        {
            ToggleLayers = new Checkbox(ViewLayers, new Vector2(DeleteLayer.Width, DeleteLayer.Height),
                FontAwesome.Get(FontAwesomeIcon.fa_eye_open),
                FontAwesome.Get(FontAwesomeIcon.fa_eye_with_a_diagonal_line_interface_symbol_for_invisibility),
                false)
            {
                Parent = Header,
                Alignment = Alignment.MidRight,
                X = CreateLayer.X - CreateLayer.Width + DeleteLayer.X
            };
        }

        /// <summary>
        ///     Creates <see cref="ScrollContainer"/>
        /// </summary>
        private void CreateScrollContainer()
        {
            ScrollContainer = new EditorPanelLayersScrollContainer(ActionManager, WorkingMap, SelectedLayer, DefaultLayer,
                new ScalableVector2(Content.Width - 7, Content.Height - 8))
            {
                Parent = Content,
                X = 4
            };
        }

        public override void Destroy()
        {
            ViewLayers.ValueChanged -= ViewLayersChanged;
            HitObjectColoring.ValueChanged -= ColoringChanged;
            base.Destroy();
        }
    }
}