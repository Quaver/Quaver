using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;

namespace Quaver.Shared.Screens.Options.Sections
{
    public class OptionsSection
    {
        /// <summary>
        /// </summary>
        public Texture2D Icon { get; }

        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public List<OptionsSubcategory> Subcategories { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="subcategories"></param>
        public OptionsSection(string name, Texture2D icon, List<OptionsSubcategory> subcategories)
        {
            Name = name;
            Icon = icon;
            Subcategories = subcategories;
        }
    }
}