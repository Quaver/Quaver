using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Quaver.Peppy
{
    internal struct QuaverOsuElementMap
    {
        /// <summary>
        ///     The Quaver skin element file name
        /// </summary>
        public string QuaverElement { get; }

        /// <summary>
        ///     The osu! skin element file name
        /// </summary>
        public string OsuElement { get; }

        /// <summary>
        ///     The type of element
        /// </summary>
        public ElementType Type { get; }

        /// <summary>
        ///     The value in skin.ini that pertains to this element
        /// </summary>
        public string SkinIniValue { get; }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="quaverElement"></param>
        /// <param name="osuElement"></param>
        /// <param name="type"></param>
        public QuaverOsuElementMap(string quaverElement, string osuElement, ElementType type)
        {
            QuaverElement = quaverElement;
            OsuElement = osuElement;
            Type = type;
            SkinIniValue = "";
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="quaverElement"></param>
        /// <param name="osuElement"></param>
        /// <param name="type"></param>
        public QuaverOsuElementMap(string quaverElement, string osuElement, ElementType type, string iniVal)
        {
            QuaverElement = quaverElement;
            OsuElement = osuElement;
            Type = type;
            SkinIniValue = iniVal;
        }
    }

    /// <summary>
    ///     Defines the type of element for a partciular osu! skin element
    /// </summary>
    public enum ElementType
    {
        Image,
        AnimatableImage,
        Sound
    }
}
