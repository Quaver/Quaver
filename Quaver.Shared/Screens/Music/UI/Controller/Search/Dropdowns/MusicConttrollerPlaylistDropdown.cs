using System.Collections.Generic;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Music.UI.Controller.Search.Dropdowns
{
    public class MusicControllerPlaylistDropdown : LabelledDropdown
    {
        public MusicControllerPlaylistDropdown() : base("PLAYLIST: ", 22, new Dropdown(GetDropdownItems(),
            new ScalableVector2(180, 38), 22, ColorHelper.HexToColor($"#10C8F6"), GetSelectedIndex(), 125))
        {
            Dropdown.ItemSelected += (sender, args) =>
            {
                if (args.Index == 0)
                {
                    PlaylistManager.Selected.Value = null;

                    if (ConfigManager.SelectGroupMapsetsBy != null)
                        ConfigManager.SelectGroupMapsetsBy.Value = GroupMapsetsBy.None;

                    return;
                }

                PlaylistManager.Selected.Value = PlaylistManager.Playlists[args.Index - 1];

                if (ConfigManager.SelectGroupMapsetsBy != null)
                    ConfigManager.SelectGroupMapsetsBy.Value = GroupMapsetsBy.Playlists;
            };
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems()
        {
            var options = new List<string>
            {
                "All Songs"
            };

            foreach (var playlist in PlaylistManager.Playlists)
                options.Add(playlist.Name);

            return options;
        }

        /// <summary>
        ///     Retrieves the index of the selected value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedIndex()
        {
            return 0;
        }
    }
}