using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Config;
using Quaver.Database.Maps;
using Quaver.Graphics;
using Quaver.Graphics.Overlays.Chat.Components.Users;
using Quaver.Helpers;
using Quaver.Assets;
using Quaver.Scheduling;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
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

        /// <summary>
        ///     The amount of mapsets that are found.
        /// </summary>
        private SpriteText TextMapsetsFound { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public MapsetSearchContainer(SongSelectScreenView view)
        {
            View = view;
            Size = new ScalableVector2(620, 90);

            Alpha = 0.90f;
            Image = UserInterface.SelectSearchBackground;

            CreateTextSearch();
            CreateSearchBox();
            CreateTextOrderBy();
            CreateOrderByArtistButton();
            CreateOrderByTitleButton();
            CreateOrderByCreatorButton();
            CreateTextMapsetsFound();

            var leftLine = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(2, Height),
                Alpha = 0.75f,
                Tint = Colors.MainAccent
            };

            var rightLine = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(2, Height),
                Alignment = Alignment.TopRight,
                Alpha = 0.75f,
                Tint = Colors.MainAccent
            };

            // Line displayed at the bottom of the container.
            var bottomline = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, 2),
                Alignment = Alignment.BotLeft,
                Alpha = 0.75f,
                Tint = Colors.MainAccent
            };

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
        private void CreateTextSearch() => TextSearch = new SpriteText(BitmapFonts.Exo2SemiBold, "Search:", 13)
        {
            Parent = this,
            X = 15,
            Y = 15,
        };

        /// <summary>
        ///     Creates the textbox to search for mapsets.
        /// </summary>
        private void CreateSearchBox()
        {
            SearchBox = new Textbox(new ScalableVector2(514, 30), BitmapFonts.Exo2Bold, 13)
            {
                Parent = TextSearch,
                Position = new ScalableVector2(TextSearch.Width + 5, 0),
                Alignment = Alignment.MidLeft,
                Tint = Colors.DarkGray,
                Alpha = 0.75f,
                AllowSubmission = false,
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
                        UpdateMapsetsFoundText();
                    }
                }
            };

            SearchBox.AddBorder(Colors.MainAccent);

            var searchIcon = new Sprite()
            {
                Parent = SearchBox,
                Alignment = Alignment.MidRight,
                X = -15,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass),
                Size = new ScalableVector2(15, 15)
            };
        }

        /// <summary>
        ///     Creates the text that displays "Order By:"
        /// </summary>
        private void CreateTextOrderBy() => OrderBy = new SpriteText(BitmapFonts.Exo2SemiBold, "Order By:", 13)
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
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(selectScreen.AvailableMapsets);
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
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(selectScreen.AvailableMapsets);
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
                    selectScreen.AvailableMapsets = MapsetHelper.OrderMapsetByConfigValue(selectScreen.AvailableMapsets);
                    View.MapsetScrollContainer.InitializeWithNewSets();
                }
            };

            ButtonOrderByCreator.Size = new ScalableVector2(ButtonOrderByCreator.Text.Width + 20, ButtonOrderByCreator.Text.Height + 8);
        }

        /// <summary>
        ///     The text that displays how many mapsets were found.
        /// </summary>
        private void CreateTextMapsetsFound()
        {
            TextMapsetsFound = new SpriteText(BitmapFonts.Exo2Medium, " ", 13)
            {
                Parent = ButtonOrderByCreator,
                Alignment = Alignment.MidLeft,
                X = ButtonOrderByCreator.Width + 84,
            };

            UpdateMapsetsFoundText();
        }

        /// <summary>
        ///    Updates the text with the amount of mapsets we have
        /// </summary>
        private void UpdateMapsetsFoundText()
        {
            var screen = View.Screen as SongSelectScreen;

            if (screen.AvailableMapsets.Count > 0)
            {
                TextMapsetsFound.Text = $"{screen.AvailableMapsets.Count} Mapsets Available!";
                TextMapsetsFound.Tint = Color.White;

                TextMapsetsFound.ClearAnimations();
                TextMapsetsFound.FadeToColor(Colors.MainAccent, Easing.OutQuint, 1000);
            }
            else
            {
                TextMapsetsFound.Text = $"No Mapsets Found!";

                TextMapsetsFound.ClearAnimations();
                TextMapsetsFound.FadeToColor(Colors.Negative, Easing.OutQuint, 1000);
            }
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