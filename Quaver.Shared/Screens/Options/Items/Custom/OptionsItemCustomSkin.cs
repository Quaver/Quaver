using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using IniFileParser;
using IniFileParser.Exceptions;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Shared.Config;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Graphics.Transitions;
using Quaver.Shared.Skinning;
using TagLib.Riff;
using Wobble;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Logging;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;
using File = System.IO.File;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemCustomSkin : OptionsItemDropdown
    {
        public OptionsItemCustomSkin(RectangleF containerRect, string name, Bindable<string> skin) : this(containerRect, name,
            skin, SkinStore.GetSkins())
        {
        }

        public OptionsItemCustomSkin(RectangleF containerRect, string name, Bindable<string> skin, List<string> skinOptions) : base(containerRect, name,
            new Dropdown(skinOptions, new ScalableVector2(300, 35), 20,
            Colors.MainAccent, GetSelectedIndex(skin, skinOptions), 240, 700))
        {
            Dropdown.ItemContainer.Scrollbar.Tint = Color.White;
            Dropdown.ItemContainer.Scrollbar.Width = 2;

            Dropdown.ItemContainer.EasingType = Easing.OutQuint;
            Dropdown.ItemContainer.TimeToCompleteScroll = 1200;
            Dropdown.ItemContainer.ScrollSpeed = 220;

            foreach (var item in Dropdown.Items)
            {
                var option = Dropdown.Options[item.Index];

                // Workshop Skin
                if (option.Contains("<") && option.Contains(">"))
                {
                    item.Text.Tint = ColorHelper.HexToColor("#4798d6");
                    item.Text.Text = item.Text.Text.Split("<")[0];
                }
            }

            Dropdown.ItemSelected += (sender, args) =>
            {
                if (skin == null)
                    return;

                var option = Dropdown.Options[args.Index];

                if (option.Contains("<") && option.Contains(">"))
                {
                    var workshopId = option.Split("<")[1].Replace(">", "");

                    ConfigManager.UseSteamWorkshopSkin.Value = true;
                    skin.Value = workshopId;
                }
                else
                {
                    ConfigManager.UseSteamWorkshopSkin.Value = false;
                    skin.Value = option;
                }

                Transitioner.FadeIn();
                SkinManager.TimeSkinReloadRequested = GameBase.Game.TimeRunning;
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            Dropdown.ItemContainer.Scrollbar.Visible = Dropdown.Opened;
            Dropdown.ItemContainer.InputEnabled = Dropdown.Opened;

            base.Update(gameTime);
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex(Bindable<string> skin, List<string> options)
        {
            if (skin == null)
                return 0;

            if (ConfigManager.UseSteamWorkshopSkin != null)
            {
                var index = options.FindIndex(x => x.Contains(skin.Value));

                if (index != -1)
                    return index;
            }
            else
            {
                var index = options.FindIndex(x => x == skin.Value);

                if (index != -1)
                    return index;
            }

            return 0;
        }
    }
}
