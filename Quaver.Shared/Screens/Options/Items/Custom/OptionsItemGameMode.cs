using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;
using Quaver.API.Helpers;
using Wobble.Bindables;
using System;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemGameMode : OptionsItemDropdown
    {
        public OptionsItemGameMode(RectangleF containerRect, string name, Bindable<GameMode> mode, string? noModeString = null) : base(containerRect, name,
            new Dropdown(GetOptions(noModeString), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex(mode)))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (mode == null)
                    return;

                if (noModeString != null && args.Index == 0)
                {
                    mode.Value = (GameMode)0;
                    return;
                }

                mode.Value = (GameMode)args.Index;
            };
        }

        private static List<string> GetOptions(string? noModeString)
        {
            var list = new List<string>();

            if (noModeString != null)
            {
                list.Add(noModeString);
            }

            foreach (GameMode mode in ModeHelper.AllModes)
            {
                list.Add(ModeHelper.ToLongHand(mode));
            }

            return list;
        }

        private static int GetSelectedIndex(Bindable<GameMode> mode)
        {
            if (mode == null)
                return 0;

            return (int)mode.Value;
        }
    }
}
