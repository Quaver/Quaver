using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Playlists;
using Quaver.Shared.Graphics.Form.Checkboxes;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.Playlists.Management
{
    public class AddMapsetToPlaylistCheckboxContainer : CheckboxContainer
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        public AddMapsetToPlaylistCheckboxContainer(Mapset mapset) : base(GetItems(mapset),
            new ScalableVector2(250, 400), 250)
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="mapset"></param>
        /// <returns></returns>
        private static List<ICheckboxContainerItem> GetItems(Mapset mapset)
        {
            var items = new List<ICheckboxContainerItem>();

            foreach (var playlist in PlaylistManager.Playlists)
            {
                if (playlist.PlaylistGame != MapGame.Quaver)
                    continue;

                items.Add(new CheckboxItemPlaylistMapset(playlist, mapset));
            }

            return items;
        }
    }
}