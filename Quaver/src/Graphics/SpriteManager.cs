using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics;

namespace Quaver.src.Graphics
{
    /// <summary>
    /// This class manages the drawing of every drawable object.
    /// </summary>
    static class SpriteManager
    {
        //TODO: Hiarchy system? So far we don't need one, but we'll probably need one in the future

        /// <summary>
        /// A list of every Drawable object that will be drawn
        /// </summary>
        private static List<Drawable> _drawList = new List<Drawable>();

        /// <summary>
        /// A list of every Drawable asset that is active (in memory)
        /// </summary>
        private static List<Drawable> _drawPool = new List<Drawable>();

        /// <summary>
        /// This method will add a drawable to the Draw List so it can be drawn.
        /// </summary>
        /// <param name="current"></param>
        private static void AddToDrawList(Drawable current)
        {
            
        }

        /// <summary>
        /// This method will remove a Drawable from the Draw List
        /// </summary>
        /// <param name="current"></param>
        private static void RemoveFromDrawList(Drawable current)
        {

        }

        /// <summary>
        /// This will remove every single Drawable object in the Draw List.
        /// </summary>
        private static void ClearDrawList()
        {
            
        }

        /// <summary>
        /// This method will draw every object in the Draw List
        /// </summary>
        private static void Draw()
        {
            
        }

    }
}
