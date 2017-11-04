using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Graphics;
using Quaver.Main;

namespace Quaver.Graphics
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
        internal static List<Drawable> DrawList = new List<Drawable>();

        /// <summary>
        /// A list of every Drawable asset that is active (in memory)
        /// </summary>
        internal static List<Drawable> SpritePool = new List<Drawable>();

        /// <summary>
        /// Add a new Drawable object to the Sprite Pool.
        /// </summary>
        /// <param name="current"></param>
        internal static void AddToSpritePool(Drawable current)
        {
            SpritePool.Add(current);
        }

        /// <summary>
        /// Remove the Drawable object from the Sprite Pool.
        /// </summary>
        /// <param name="current"></param>
        internal static void RemoveFromSpritePool(Drawable current)
        {
            RemoveFromDrawList(current);
            var cIndex = DrawList.FindIndex(r => r == current);
            if (cIndex >= 0) SpritePool.RemoveAt(cIndex);
        }

        /// <summary>
        /// This method will add a drawable to the Draw List so it can be drawn.
        /// </summary>
        /// <param name="current"></param>
        internal static void AddToDrawList(Drawable current)
        {
            DrawList.Add(current);
        }

        /// <summary>
        /// This method will remove a Drawable from the Draw List.
        /// </summary>
        /// <param name="current"></param>
        internal static void RemoveFromDrawList(Drawable current)
        {
            var cIndex = DrawList.FindIndex(r => r == current);
            if (cIndex >= 0) DrawList.RemoveAt(cIndex);
        }

        /// <summary>
        /// This will remove every single Drawable object in the Draw List.
        /// </summary>
        internal static void ClearDrawList()
        {
            DrawList.Clear();
        }

        /// <summary>
        /// This method will draw every object in the Draw List.
        /// </summary>
        internal static void Draw(double dt)
        {
            foreach (var current in DrawList)
            {
                current.Update(dt);
                current.Draw();
            }
            GameBase.Cursor.Update(dt);
        }

    }
}
