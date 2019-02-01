using System;
using Microsoft.Xna.Framework.Graphics;
using Quaver.API.Maps.Structures;
using Quaver.Shared.Assets;
using Quaver.Shared.Screens.Menu.UI.Jukebox;

namespace Quaver.Shared.Screens.Editor.UI.Layering
{
    public class EditorLayerVisiblityCheckbox : JukeboxButton
    {
        /// <summary>
        /// </summary>
        public EditorLayerInfo Layer { get; }

        /// <summary>
        /// </summary>
        public EditorLayerVisiblityCheckbox(EditorLayerInfo layer) : base(GetVisiblityTexture(layer.Hidden))
        {
            Layer = layer;
            Clicked += (sender, args) => ToggleLayerVisiblity();
        }

        /// <summary>
        /// </summary>
        private void ToggleLayerVisiblity()
        {
            Layer.Hidden = !Layer.Hidden;
            Image = GetVisiblityTexture(Layer.Hidden);
        }

        /// <summary>
        /// </summary>
        /// <param name="hidden"></param>
        /// <returns></returns>
        private static Texture2D GetVisiblityTexture(bool hidden)
            => hidden ? FontAwesome.Get(FontAwesomeIcon.fa_check_box_empty) : FontAwesome.Get(FontAwesomeIcon.fa_check);
    }
}