using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
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
        public Vector2? IconSize { get; }

        /// <summary>
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// </summary>
        public string LocalizationLabel { get; }

        /// <summary>
        /// </summary>
        public List<OptionsSubcategory> Subcategories { get; }

        /// <summary>
        /// </summary>
        /// <param name="name"></param>
        /// <param name="icon"></param>
        /// <param name="subcategories"></param>
        public OptionsSection(string name, Texture2D icon, List<OptionsSubcategory> subcategories, Vector2? iconSize = null)
        {
            LocalizationLabel = name;
            Name = OptionsLocalization.Get(name);
            Icon = icon;
            IconSize = iconSize;
            Subcategories = subcategories;
        }
    }
}
