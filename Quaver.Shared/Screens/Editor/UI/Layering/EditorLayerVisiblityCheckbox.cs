using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerVisiblityCheckbox : JukeboxButton
    {
        /// <summary>
        ///     If the layer
        /// </summary>
        private bool LayerVisible { get; set; }

        /// <summary>
        /// </summary>
        /// <param name="layerVisible"></param>
        public EditorLayerVisiblityCheckbox(bool layerVisible) : base(GetVisiblityTexture(layerVisible))
        {
            LayerVisible = layerVisible;
            Clicked += (sender, args) => ToggleLayerVisiblity();
        }

        /// <summary>
        /// </summary>
        private void ToggleLayerVisiblity()
        {
            LayerVisible = !LayerVisible;
            Image = GetVisiblityTexture(LayerVisible);
        }

        /// <summary>
        /// </summary>
        /// <param name="visible"></param>
        /// <returns></returns>
        private static Texture2D GetVisiblityTexture(bool visible)
            => visible ? FontAwesome.Get(FontAwesomeIcon.fa_check) : FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty);
    }
}