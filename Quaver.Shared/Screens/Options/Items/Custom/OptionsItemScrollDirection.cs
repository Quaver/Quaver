using System;
using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemScrollDirection : OptionsItemDropdown
    {
        /// <summary>
        /// </summary>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        /// <param name="direction"></param>
        public OptionsItemScrollDirection(RectangleF containerRect, string name, Bindable<ScrollDirection> direction)
            : base(containerRect, name, new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22,
                Colors.MainAccent, GetSelectedIndex(direction)))
        {
            Tags = new List<string>()
            {
                "upscroll",
                "downscroll"
            };

            Dropdown.ItemSelected += (sender, args) =>
            {
                if (direction == null)
                    return;

                direction.Value = (ScrollDirection) args.Index;
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetOptions()
        {
            var options = new List<string>();

            foreach (ScrollDirection val in Enum.GetValues(typeof(ScrollDirection)))
                options.Add(val.ToString());

            return options;
        }

        /// <summary>
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private static int GetSelectedIndex(Bindable<ScrollDirection> direction)
        {
            if (direction == null)
                return 0;

            return (int) direction.Value;
        }
    }
}