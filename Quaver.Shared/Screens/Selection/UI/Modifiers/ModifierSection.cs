using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Selection.UI.Modifiers.Components;
using TagLib.Id3v2;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.Modifiers
{
    public class ModifierSection
    {
        /// <summary>
        ///     Header sprite for the section
        /// </summary>
        public Sprite Header { get; }

        /// <summary>
        ///     The icon to represent the section
        /// </summary>
        public Sprite Icon { get; }

        /// <summary>
        ///     The name of the modifier section
        /// </summary>
        public SpriteTextPlus Name { get; }

        /// <summary>
        ///     Describes when the modifier section is for
        /// </summary>
        public SpriteTextPlus SubText { get; }

        /// <summary>
        ///     The list of modifiers to go under this section
        /// </summary>
        public List<SelectableModifier> Modifiers { get; }

        /// <summary>
        /// </summary>
        /// <param name="width"></param>
        /// <param name="icon"></param>
        /// <param name="name"></param>
        /// <param name="subText"></param>
        /// <param name="color"></param>
        /// <param name="modifiers"></param>
        public ModifierSection(int width, Texture2D icon, string name, string subText, Color color, List<SelectableModifier> modifiers)
        {
            Modifiers = modifiers;

            Header = new Sprite
            {
                Size = new ScalableVector2(width, 70),
                UsePreviousSpriteBatchOptions = true,
                Tint = ColorHelper.HexToColor("#181818")
            };

            const int paddingLeft = 12;

            Icon = new Sprite()
            {
                Parent = Header,
                Tint = color,
                Size = new ScalableVector2(22, 22),
                Alignment = Alignment.MidLeft,
                X = paddingLeft,
                Image = icon,
                UsePreviousSpriteBatchOptions = true
            };

            Name = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), name.ToUpper(), 22)
            {
                Parent = Header,
                Tint = color,
                X = Icon.X + Icon.Width + paddingLeft,
                Y = 12,
                UsePreviousSpriteBatchOptions = true
            };

            SubText = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), subText.ToUpper(), 18)
            {
                Parent = Header,
                Alignment = Alignment.BotLeft,
                UsePreviousSpriteBatchOptions = true,
                X = Name.X,
                Y = -Name.Y
            };
        }
    }
}