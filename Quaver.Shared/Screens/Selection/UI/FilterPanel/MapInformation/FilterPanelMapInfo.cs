using Quaver.API.Helpers;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.MapInformation
{
    public class FilterPanelMapInfo : ScrollContainer
    {
        /// <summary>
        ///     Displays the title of the song
        /// </summary>
        private SpriteTextPlus Title { get; set; }

        /// <summary>
        ///     Displays the artist of the song
        /// </summary>
        private SpriteTextPlus Artist { get; set; }

        /// <summary>
        ///     Displays the difficulty and mods
        /// </summary>
        private SpriteTextPlus DifficultyMods { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public FilterPanelMapInfo() : base(new ScalableVector2(800, 74), new ScalableVector2(800, 74))
        {
            Alpha = 0f;
            InputEnabled = false;

            CreateTitleText();
            CreateArtistText();
            CreateDifficultyModsText();

            UpdateText(MapManager.Selected.Value);

            MapManager.Selected.ValueChanged += OnMapChanged;
            ModManager.ModsChanged += OnModsChanged;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            MapManager.Selected.ValueChanged -= OnMapChanged;
            ModManager.ModsChanged -= OnModsChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the text that displays the title of the song
        /// </summary>
        private void CreateTitleText()
        {
            Title = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Title", 22);

            AddContainedDrawable(Title);
        }

        /// <summary>
        ///     Creates the text that displays the song artist
        /// </summary>
        private void CreateArtistText()
        {
            Artist = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "Artist", 22)
            {
                Y = Title.Height + 5,
            };

            AddContainedDrawable(Artist);
        }

        /// <summary>
        ///     Creates the text that displays both the difficulty and activated mods
        /// </summary>
        private void CreateDifficultyModsText()
        {
            DifficultyMods = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "[Difficulty] + Mods", 20)
            {
                Y = Artist.Y + Artist.Height + 5
            };

            AddContainedDrawable(DifficultyMods);
        }

        /// <summary>
        ///     Updates the data with a new map
        /// </summary>
        /// <param name="map"></param>
        private void UpdateText(Map map)
        {
            if (map == null)
                return;

            Title.Text = map.Title;
            Artist.Text = map.Artist;

            var mods = ModManager.CurrentModifiersList.Count > 0 ? $" + {ModHelper.GetModsString(ModManager.Mods)}": "";
            DifficultyMods.Text = $"[{map.DifficultyName}]{mods}";

            //  Reset positions of the text
            Title.ClearAnimations();
            Title.X = 0;

            Artist.ClearAnimations();
            Artist.X = 0;

            DifficultyMods.ClearAnimations();
            DifficultyMods.X = 0;
        }

        /// <summary>
        ///     Called when the map has changed. Updates the text with the new map
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMapChanged(object sender, BindableValueChangedEventArgs<Map> e) => UpdateText(e.Value);

        /// <summary>
        ///     Called when the player's activated mods change.
        ///     Updates the text of the mods
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnModsChanged(object sender, ModsChangedEventArgs e) => UpdateText(MapManager.Selected.Value);
    }
}