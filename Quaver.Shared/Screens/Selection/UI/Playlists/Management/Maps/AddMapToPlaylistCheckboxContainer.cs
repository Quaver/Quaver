using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Quaver.Shared.Screens.Selection.UI.Playlists.Management.Mapsets;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Management.Maps
{
    public class AddMapToPlaylistCheckboxContainer : CheckboxContainer
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="map"></param>
        public AddMapToPlaylistCheckboxContainer(Map map) : base(GetItems(map),
            new ScalableVector2(250, 400), 250)
        {
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        private static List<ICheckboxContainerItem> GetItems(Map map)
        {
            var items = new List<ICheckboxContainerItem>();

            foreach (var playlist in PlaylistManager.Playlists)
            {
                if (playlist.PlaylistGame != MapGame.Quaver)
                    continue;

                items.Add(new CheckboxItemPlaylistMap(playlist, map));
            }

            return items;
        }
    }
}