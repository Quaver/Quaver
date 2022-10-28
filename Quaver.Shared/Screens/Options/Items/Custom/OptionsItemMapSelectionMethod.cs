using System;
using System.ComponentModel;
using System.Collections.Generic;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Options.Items.Custom;

public class OptionsItemMapSelectionMethod : OptionsItemDropdown
{
	public OptionsItemMapSelectionMethod(RectangleF containerRect, string name) : base(containerRect, name,
		new Dropdown(GetOptions(), new ScalableVector2(180, 35), 22, Colors.MainAccent, GetSelectedIndex()))
		{
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (ConfigManager.TargetMapSelectionMethod == null)
                    return;

                ConfigManager.TargetMapSelectionMethod.Value = args.Text switch
				{
					"Closest" => MapSelectionMethod.Closest,
					"At Least" => MapSelectionMethod.AtLeast,
					"At Most" => MapSelectionMethod.AtMost,
					_ => throw new InvalidEnumArgumentException()
				};
            };
		}

    private static List<string> GetOptions() => new() {"Closest", "At Least", "At Most"};

    private static int GetSelectedIndex()
    {
        if (ConfigManager.TargetMapSelectionMethod == null)
            return 0;

        return (int)ConfigManager.TargetMapSelectionMethod.Value;
    }
}
