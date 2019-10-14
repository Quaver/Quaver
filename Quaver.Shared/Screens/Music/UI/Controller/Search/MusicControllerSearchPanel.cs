using System.Collections.Generic;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Helpers;
using Quaver.Shared.Screens.Music.UI.Controller.Search.Dropdowns;
using Quaver.Shared.Screens.Selection.UI.FilterPanel;
using Quaver.Shared.Screens.Selection.UI.FilterPanel.Search;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Music.UI.Controller.Search
{
    public class MusicControllerSearchPanel : Sprite
    {
        /// <summary>
        ///     Items that are aligned from right to left
        /// </summary>
        private List<Drawable> RightItems { get; }

        /// <summary>
        /// </summary>
        private MusicControllerPrivacyDropdown PrivacyDropdown { get; set; }

        /// <summary>
        /// </summary>
        private MusicControllerSortDropdown SortDropdown { get; set; }

        /// <summary>
        /// </summary>
        private FilterPanelSearchBox SearchBox { get; set; }

        /// <summary>
        /// </summary>
        private FilterPanelMapsAvailable SongsFound { get; set; }

        /// <summary>
        /// </summary>
        public MusicControllerSearchPanel(float width)
        {
            Size = new ScalableVector2(width, 74);
            Tint = ColorHelper.HexToColor("#242424");

            RightItems = new List<Drawable>();

            CreatePrivacyDropdown();
            CreateSortDropdown();
            CreateSearchBox();
            CreateSongsFound();

            AlignRightItems();
        }

        /// <summary>
        ///     Aligns the items from right to left
        /// </summary>
        private void AlignRightItems()
        {
            for (var i = 0; i < RightItems.Count; i++)
            {
                var item = RightItems[i];

                item.Parent = this;

                item.Alignment = Alignment.MidRight;

                const int padding = 25;
                var spacing = 50;

                if (i == 0)
                    item.X = -padding;
                else
                    item.X = RightItems[i - 1].X - RightItems[i - 1].Width - spacing;
            }
        }

        /// <summary>
        /// </summary>
        private void CreatePrivacyDropdown()
        {
            PrivacyDropdown = new MusicControllerPrivacyDropdown
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            RightItems.Add(PrivacyDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateSortDropdown()
        {
            SortDropdown = new MusicControllerSortDropdown
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            RightItems.Add(SortDropdown);
        }

        /// <summary>
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new FilterPanelSearchBox(new Bindable<string>("")
            {
                Value = ""
            }, new Bindable<List<Mapset>>(null)
            {
                Value = null
            }, "Type to search...")
            {
                Parent = this,
                Alignment = Alignment.MidRight
            };

            RightItems.Add(SearchBox);
        }

        /// <summary>
        /// </summary>
        private void CreateSongsFound()
        {
            SongsFound = new FilterPanelMapsAvailable(new Bindable<List<Mapset>>(MapManager.Mapsets)
            {
                Value = MapManager.Mapsets
            }, true)
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = 25
            };

            RightItems.Add(SongsFound);
        }
    }
}