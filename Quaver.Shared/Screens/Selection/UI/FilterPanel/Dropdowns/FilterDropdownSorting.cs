using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Form.Dropdowns;
using Quaver.Shared.Graphics.Form.Dropdowns.Custom;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Selection;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using Wobble.Bindables;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.Dropdowns
{
    public class FilterDropdownSorting : LabelledDropdown
    {
        /// <summary>
        ///     The mapsets that are available to select
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        ///     The active song select scroll container.
        /// </summary>
        private Bindable<SelectScrollContainerType> ActiveScrollContainer { get; }

        private static List<(string Name, OrderMapsetsBy Value)> DropdownItems { get; } = new List<(string, OrderMapsetsBy)>()
        {
            ("Artist", OrderMapsetsBy.Artist),
            ("Title", OrderMapsetsBy.Title),
            ("Creator", OrderMapsetsBy.Creator),
            ("Date Added", OrderMapsetsBy.DateAdded),
            ("Ranked Status", OrderMapsetsBy.Status),
            ("BPM", OrderMapsetsBy.BPM),
            ("Times Played", OrderMapsetsBy.TimesPlayed),
            ("Recently Played", OrderMapsetsBy.RecentlyPlayed),
            ("Game", OrderMapsetsBy.Game),
            ("Length", OrderMapsetsBy.Length),
            ("Source", OrderMapsetsBy.Source),
            ("Difficulty", OrderMapsetsBy.Difficulty),
            ("Online Grade", OrderMapsetsBy.OnlineGrade),
            ("Long Note %", OrderMapsetsBy.LongNotePercentage),
            ("Notes Per Sec.", OrderMapsetsBy.NotesPerSecond)
        };

        private static List<(string Name, OrderPlaylistsBy Value)> PlaylistDropdownItems { get; } = new List<(string, OrderPlaylistsBy)>()
        {
            ("Title", OrderPlaylistsBy.Title),
            ("Creator", OrderPlaylistsBy.Creator),
            ("Date Added", OrderPlaylistsBy.DateAdded),
            ("Game", OrderPlaylistsBy.Game),
            ("Length", OrderPlaylistsBy.Length),
            ("Difficulty", OrderPlaylistsBy.Difficulty)
        };

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        /// <param name="activeScrollContainer"></param>
        public FilterDropdownSorting(Bindable<List<Mapset>> availableMapsets,
            Bindable<SelectScrollContainerType> activeScrollContainer = null) : base(SelectionLocalization.Get("Sort By:"), 20, new Dropdown(GetInitialDropdownItems(activeScrollContainer),
            new ScalableVector2(192, 38), 18, ColorHelper.HexToColor($"#ffe76b"), GetInitialSelectedIndex(activeScrollContainer)))
        {
            AvailableMapsets = availableMapsets;
            ActiveScrollContainer = activeScrollContainer;
            Dropdown.ItemSelected += OnItemSelected;

            if (ConfigManager.SelectGroupMapsetsBy != null)
                ConfigManager.SelectGroupMapsetsBy.ValueChanged += OnDisplayModeChanged;

            if (ActiveScrollContainer != null)
                ActiveScrollContainer.ValueChanged += OnDisplayModeChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (ConfigManager.SelectGroupMapsetsBy != null)
                ConfigManager.SelectGroupMapsetsBy.ValueChanged -= OnDisplayModeChanged;

            if (ActiveScrollContainer != null)
                ActiveScrollContainer.ValueChanged -= OnDisplayModeChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Retrieves a list of dropdown items
        ///
        ///     TODO: Localize this
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems(List<(string Name, OrderMapsetsBy Value)> items)
            => items.Select(x => SelectionLocalization.Get(x.Name)).ToList();

        /// <summary>
        ///     Retrieves a list of dropdown items
        ///
        ///     TODO: Localize this
        /// </summary>
        /// <returns></returns>
        private static List<string> GetDropdownItems(List<(string Name, OrderPlaylistsBy Value)> items)
            => items.Select(x => SelectionLocalization.Get(x.Name)).ToList();

        /// <summary>
        ///     Retrieves the index of the selected mapset sort value
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedMapsetIndex()
        {
            if (ConfigManager.SelectOrderMapsetsBy == null)
                return 0;

            var index = DropdownItems.FindIndex(x => x.Value == ConfigManager.SelectOrderMapsetsBy.Value);

            return index == -1 ? 0 : index;
        }

        /// <summary>
        ///     Retrieves the index of the selected playlist sort value.
        /// </summary>
        /// <returns></returns>
        private static int GetSelectedPlaylistIndex()
        {
            if (ConfigManager.SelectOrderPlaylistsBy == null)
                return 0;

            var index = PlaylistDropdownItems.FindIndex(x => x.Value == ConfigManager.SelectOrderPlaylistsBy.Value);

            return index == -1 ? 0 : index;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnItemSelected(object sender, DropdownClickedEventArgs e)
        {
            if (ShouldShowPlaylistSortOptions())
            {
                if (ConfigManager.SelectOrderPlaylistsBy == null)
                    return;

                ConfigManager.SelectOrderPlaylistsBy.Value = PlaylistDropdownItems[e.Index].Value;
            }
            else
            {
                if (ConfigManager.SelectOrderMapsetsBy == null)
                    return;

                ConfigManager.SelectOrderMapsetsBy.Value = DropdownItems[e.Index].Value;
            }
        }

        /// <summary>
        ///     Refreshes the visible sort options when song select switches between playlists and mapsets.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnDisplayModeChanged<T>(object sender, BindableValueChangedEventArgs<T> e) => RefreshDropdownItems();

        /// <summary>
        ///     Refreshes the dropdown items for the active display mode.
        /// </summary>
        private void RefreshDropdownItems()
        {
            Dropdown.SetOptions(GetCurrentDropdownItems(), GetCurrentSelectedIndex());
        }

        /// <summary>
        ///     Returns if the dropdown should show playlist sort options.
        /// </summary>
        /// <returns></returns>
        private bool ShouldShowPlaylistSortOptions()
            => ConfigManager.SelectGroupMapsetsBy?.Value == GroupMapsetsBy.Playlists &&
               ActiveScrollContainer?.Value == SelectScrollContainerType.Playlists;

        /// <summary>
        ///     Gets the currently displayed dropdown items.
        /// </summary>
        /// <returns></returns>
        private List<string> GetCurrentDropdownItems()
            => ShouldShowPlaylistSortOptions() ? GetDropdownItems(PlaylistDropdownItems) : GetDropdownItems(DropdownItems);

        /// <summary>
        ///     Gets the currently selected dropdown index.
        /// </summary>
        /// <returns></returns>
        private int GetCurrentSelectedIndex()
            => ShouldShowPlaylistSortOptions() ? GetSelectedPlaylistIndex() : GetSelectedMapsetIndex();

        /// <summary>
        ///     Gets the initial dropdown item list before instance fields are assigned.
        /// </summary>
        /// <param name="activeScrollContainer"></param>
        /// <returns></returns>
        private static List<string> GetInitialDropdownItems(Bindable<SelectScrollContainerType> activeScrollContainer)
            => ConfigManager.SelectGroupMapsetsBy?.Value == GroupMapsetsBy.Playlists &&
               activeScrollContainer?.Value == SelectScrollContainerType.Playlists
                ? GetDropdownItems(PlaylistDropdownItems)
                : GetDropdownItems(DropdownItems);

        /// <summary>
        ///     Gets the initially selected dropdown index before instance fields are assigned.
        /// </summary>
        /// <param name="activeScrollContainer"></param>
        /// <returns></returns>
        private static int GetInitialSelectedIndex(Bindable<SelectScrollContainerType> activeScrollContainer)
            => ConfigManager.SelectGroupMapsetsBy?.Value == GroupMapsetsBy.Playlists &&
               activeScrollContainer?.Value == SelectScrollContainerType.Playlists
                ? GetSelectedPlaylistIndex()
                : GetSelectedMapsetIndex();
    }
}
