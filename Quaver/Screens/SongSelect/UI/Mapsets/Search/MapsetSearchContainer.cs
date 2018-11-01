using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Overlays.Chat.Components.Users;
using Quaver.Helpers;
using Quaver.Resources;
using Quaver.Scheduling;
using Quaver.Screens.Select.UI.Search;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;

namespace Quaver.Screens.SongSelect.UI.Mapsets.Search
{
    public class MapsetSearchContainer : Sprite
    {
        /// <summary>
        ///     Reference to the parent ScreenView.
        /// </summary>
        private SongSelectScreenView View { get; }

        /// <summary>
        ///     Text that prompts the user to search
        /// </summary>
        private SpriteText TextSearch { get; set; }

        /// <summary>
        ///     The box to search for mapsets.
        /// </summary>
        private Textbox SearchBox { get; set; }

        /// <summary>
        ///     Text that says "Filter by:"
        /// </summary>
        private SpriteText OrderBy { get; set; }

        /// <summary>
        ///     The button to order by artist.
        /// </summary>
        private SelectableBorderedTextButton ButtonOrderByArtist { get; set; }

        /// <summary>
        ///     The button to order mapsets by their title.
        /// </summary>
        private SelectableBorderedTextButton ButtonOrderByTitle { get; set; }

        /// <summary>
        ///     The button to order mapsets by their creator.
        /// </summary>
        private SelectableBorderedTextButton ButtonOrderByCreator { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MapsetSearchContainer(SongSelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(374, 80);
            Tint = Color.Black;
            Alpha = 0.65f;

            CreateTextSearch();
            CreateSearchBox();
            CreateTextOrderBy();
            CreateOrderByArtistButton();
            CreateOrderByTitleButton();
            CreateOrderByCreatorButton();

            ConfigManager.SelectOrderMapsetsBy.ValueChanged += OnSelectOrderMapsetsByChanged;
        }

        /// <inheritdoc />
        ///  <summary>
        ///  </summary>
        ///  <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            SearchBox.AlwaysFocused = DialogManager.Dialogs.Count == 0;
            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            ConfigManager.SelectOrderMapsetsBy.ValueChanged -= OnSelectOrderMapsetsByChanged;

            base.Destroy();
        }

        /// <summary>
        ///     Creates the heading text that says "search"
        /// </summary>
        private void CreateTextSearch() => TextSearch = new SpriteText(BitmapFonts.Exo2Bold, "Search:", 13)
        {
            Parent = this,
            X = 10,
            Y = 10,
        };

        /// <summary>
        ///     Creates the textbox to search for mapsets.
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new Textbox(new ScalableVector2(280, TextSearch.Height + 5), BitmapFonts.Exo2Bold, 13)
            {
                Parent = TextSearch,
                Position = new ScalableVector2(TextSearch.Width + 5, 0),
                Alignment = Alignment.MidLeft,
                Tint = Colors.DarkGray,
                Alpha = 0.75f,
                InputText =
                {
                    Tint = Color.White
                },
                StoppedTypingActionCalltime = 300,
                OnStoppedTyping = (text) =>
                {
                    var selectScreen = View.Screen as SongSelectScreen;

                    lock (selectScreen.AvailableMapsets)
                    {
                        selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(
                            MapsetHelper.SearchMapsets(MapManager.Mapsets, text));

                        View.MapsetScrollContainer.InitializeWithNewSets();
                    }
                }
            };

            SearchBox.AddBorder(Colors.MainAccent);
        }

        /// <summary>
        ///     Creates the text that displays "Order By:"
        /// </summary>
        private void CreateTextOrderBy() => OrderBy = new SpriteText(BitmapFonts.Exo2Bold, "Order By:", 13)
        {
            Parent = this,
            X = TextSearch.X,
            Y = TextSearch.Y + TextSearch.Height + 15,
        };

        /// <summary>
        ///     Creates the button to select to order by artist.
        /// </summary>
        private void CreateOrderByArtistButton()
        {
            ButtonOrderByArtist = new SelectableBorderedTextButton("Artist", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Artist)
            {
                Parent = OrderBy,
                X = OrderBy.Width + 10,
                Text =
                {
                    Font = BitmapFonts.Exo2SemiBold,
                    FontSize = 10,
                    ForceDrawAtSize = false,
                    Alignment = Alignment.MidCenter
                }
            };

            ButtonOrderByArtist.Clicked += (sender, args) =>
            {
                if (ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Artist)
                    return;

                ConfigManager.SelectOrderMapsetsBy.Value = OrderMapsetsBy.Artist;

                var selectScreen = View.Screen as SongSelectScreen;

                lock (selectScreen.AvailableMapsets)
                {
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(MapManager.Mapsets);
                    View.MapsetScrollContainer.InitializeWithNewSets();
                }
            };

            ButtonOrderByArtist.Size = new ScalableVector2(ButtonOrderByArtist.Text.Width + 20, ButtonOrderByArtist.Text.Height + 8);
        }

        /// <summary>
        ///     Creates the button to order mapsets by their title.
        /// </summary>
        private void CreateOrderByTitleButton()
        {
            ButtonOrderByTitle = new SelectableBorderedTextButton("Title", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Title)
            {
                Parent = OrderBy,
                X = ButtonOrderByArtist.X + ButtonOrderByArtist.Width + 15,
                Text =
                {
                    Font = BitmapFonts.Exo2SemiBold,
                    FontSize = 10,
                    ForceDrawAtSize = false,
                    Alignment = Alignment.MidCenter
                }
            };

            ButtonOrderByTitle.Clicked += (o, e) =>
            {
                if (ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Title)
                    return;

                ConfigManager.SelectOrderMapsetsBy.Value = OrderMapsetsBy.Title;

                var selectScreen = View.Screen as SongSelectScreen;

                lock (selectScreen.AvailableMapsets)
                {
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(MapManager.Mapsets);
                    View.MapsetScrollContainer.InitializeWithNewSets();
                }
            };

            ButtonOrderByTitle.Size = new ScalableVector2(ButtonOrderByTitle.Text.Width + 20, ButtonOrderByTitle.Text.Height + 8);
        }

        /// <summary>
        ///     Creates the button to order mapsets by their creator.
        /// </summary>
        private void CreateOrderByCreatorButton()
        {
            ButtonOrderByCreator= new SelectableBorderedTextButton("Creator", ColorHelper.HexToColor("#9d84ec"),
                ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Creator)
            {
                Parent = OrderBy,
                X = ButtonOrderByTitle.X + ButtonOrderByTitle.Width + 15,
                Text =
                {
                    Font = BitmapFonts.Exo2SemiBold,
                    FontSize = 10,
                    ForceDrawAtSize = false,
                    Alignment = Alignment.MidCenter
                }
            };

            ButtonOrderByCreator.Clicked += (o, e) =>
            {
                if (ConfigManager.SelectOrderMapsetsBy.Value == OrderMapsetsBy.Creator)
                    return;

                ConfigManager.SelectOrderMapsetsBy.Value = OrderMapsetsBy.Creator;

                var selectScreen = View.Screen as SongSelectScreen;

                lock (selectScreen.AvailableMapsets)
                {
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(MapManager.Mapsets);
                    View.MapsetScrollContainer.InitializeWithNewSets();
                }
            };

            ButtonOrderByCreator.Size = new ScalableVector2(ButtonOrderByCreator.Text.Width + 20, ButtonOrderByCreator.Text.Height + 8);
        }

        /// <summary>
        ///     Called when changing the order by.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnSelectOrderMapsetsByChanged(object sender, BindableValueChangedEventArgs<OrderMapsetsBy> e)
        {
            ButtonOrderByArtist.Selected = e.Value == OrderMapsetsBy.Artist;
            ButtonOrderByTitle.Selected = e.Value == OrderMapsetsBy.Title;
            ButtonOrderByCreator.Selected = e.Value == OrderMapsetsBy.Creator;
        }
    }
}