using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;

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
        ///     blank-box
        /// </summary>
        internal Texture2D BlankBox { get; set; }

        /// <summary>
        ///     hollow-box
        /// </summary>
        internal Texture2D HollowBox { get; set; }

        internal Texture2D BarCap { get; set; }

        /// <summary>
        ///     Loads all the ui elements into content
        /// </summary>
        public void LoadElementsAsContent()
        {
            DiffSelectMask = GameBase.Content.Load<Texture2D>("diff-select-mask");
            SetSelectMask = GameBase.Content.Load<Texture2D>("set-select-mask");
            BlankBox = GameBase.Content.Load<Texture2D>("blank-box");
            HollowBox = GameBase.Content.Load<Texture2D>("hollow-box");
            BarCap = GameBase.Content.Load<Texture2D>("bar-cap");
        }
    }
}
