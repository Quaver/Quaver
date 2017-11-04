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
        internal static List<Drawable> DrawPool = new List<Drawable>();

        /// <summary>
        /// A list of every Drawable asset that is active (in memory)
        /// </summary>
        internal static List<Drawable> SpritePool = new List<Drawable>();

        //These lists will be updated at the end of every draw cycle
        private static List<Drawable> AddToSpritePoolList = new List<Drawable>();
        private static List<Drawable> RemoveFromSpritePoolList = new List<Drawable>();
        private static List<Drawable> AddToDrawPoolList = new List<Drawable>();
        private static List<Drawable> RemoveFromDrawPoolList = new List<Drawable>();

        /// <summary>
        /// Add a new Drawable object to the Sprite Pool.
        /// </summary>
        /// <param name="current"></param>
        internal static void AddToSpritePool(Drawable current)
        {
            AddToSpritePoolList.Add(current);
        }

        /// <summary>
        /// Remove the Drawable object from the Sprite Pool.
        /// </summary>
        /// <param name="current"></param>
        internal static void RemoveFromSpritePool(Drawable current)
        {
            RemoveFromDrawPoolList.Add(current);
        }

        /// <summary>
        /// This method will add a drawable to the Draw List so it can be drawn.
        /// </summary>
        /// <param name="current"></param>
        internal static void AddToDrawPool(Drawable current)
        {
            AddToDrawPoolList.Add(current);
        }

        /// <summary>
        /// This method will remove a Drawable from the Draw List.
        /// </summary>
        /// <param name="current"></param>
        internal static void RemoveFromDrawPool(Drawable current)
        {
            RemoveFromDrawPoolList.Add(current);
        }

        /// <summary>
        /// This will remove every single Drawable object in the Draw Pool.
        /// </summary>
        internal static void ClearDrawList()
        {
            DrawPool.Clear();
        }

        /// <summary>
        /// This method will draw every object in the Draw Pool.
        /// </summary>
        internal static void Draw(double dt)
        {
            //Update every drawable in drawpool
            foreach (var current in DrawPool)
            {
                current.Update(dt);
                current.Draw();
            }
            GameBase.Cursor.Update(dt);

            //Check method lists
            //AddToSpritePool
            foreach (var current in AddToSpritePoolList)
            {
                SpritePool.Add(current);
            }

            //RemoveFromSpritePool
            foreach (var current in RemoveFromSpritePoolList)
            {
                RemoveFromDrawPool(current);
                var cIndex = DrawPool.FindIndex(r => r == current);
                if (cIndex >= 0) SpritePool.RemoveAt(cIndex);
            }

            //AddToDrawPool
            foreach (var current in AddToDrawPoolList)
            {
                DrawPool.Add(current);
            }

            //RemoveFromDrawPool
            foreach (var current in RemoveFromDrawPoolList)
            {
                var cIndex = DrawPool.FindIndex(r => r == current);
                if (cIndex >= 0) DrawPool.RemoveAt(cIndex);
            }
        }

    }
}
