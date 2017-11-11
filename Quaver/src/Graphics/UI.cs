using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Main;

namespace Quaver.Graphics
{
    internal class UI
    {
        /// <summary>
        ///     diff-select-mask 
        /// </summary>
        internal Texture2D DiffSelectMask { get; set; }

        /// <summary>
        ///     set-select-mask 
        /// </summary>
        internal Texture2D SetSelectMask { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public void LoadElementsAsContent()
        {
            DiffSelectMask = GameBase.Content.Load<Texture2D>("diff-select-mask");
            SetSelectMask = GameBase.Content.Load<Texture2D>("set-select-mask");
        }
    }
}
