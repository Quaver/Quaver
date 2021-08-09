using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Quaver.Shared.Scheduling;
using Wobble.Assets;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;
using Wobble.Scheduling;

namespace Quaver.Shared.Screens.Selection.UI.FilterPanel.Search
{
    public class FilterPanelSearchBox : Textbox
    {
        /// <summary>
        ///     The current search term to be used
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        ///     The list of mapsets that are currently available.
        ///     In this case, we want to modify this list when the user searches for something new
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> IsPlayTesting { get; }

        /// <summary>
        /// </summary>
        private Bindable<SelectContainerPanel> ActiveLeftPanel { get; }

        /// <summary>
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <summary>
        ///     The search term used in all instances of this search box.
        ///     Used so that we can save the search term across screen changes
        /// </summary>
        public static string PreviousSearchTerm { get; private set; } = "";

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="currentSearchQuery"></param>
        /// <param name="availableMapsets"></param>
        /// <param name="initialText"></param>
        /// <param name="isPlayTesting"></param>
        /// <param name="placeHolderText"></param>
        public FilterPanelSearchBox(Bindable<string> currentSearchQuery, Bindable<List<Mapset>> availableMapsets,
            Bindable<bool> isPlayTesting, Bindable<SelectContainerPanel> activeLeftPanel, string placeHolderText)
            : base(new ScalableVector2(280, 40), FontManager.GetWobbleFont(Fonts.LatoBlack),22, PreviousSearchTerm, placeHolderText)
        {
            CurrentSearchQuery = currentSearchQuery;
            AvailableMapsets = availableMapsets;
            IsPlayTesting = isPlayTesting;
            ActiveLeftPanel = activeLeftPanel;

            AllowSubmission = false;
            Tint = Colors.DarkGray;
            Image = UserInterface.SearchBox;
            AlwaysFocused = true;

            CreateSearchIcon();

            StoppedTypingActionCalltime = 400;
            OnStoppedTyping += StoppedTyping;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            AlwaysFocused = DialogManager.Dialogs.Count == 0 && (!IsPlayTesting.Value ||
                            IsPlayTesting.Value && ActiveLeftPanel.Value != SelectContainerPanel.MapPreview);

            Focused = AlwaysFocused;

            HandleSearchIconAnimations(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Creates the search icon over the textbox
        /// </summary>
        private void CreateSearchIcon()
        {
            SearchIcon = new Sprite
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                Size = new ScalableVector2(16, 16),
                X = -10,
                Image = FontAwesome.Get(FontAwesomeIcon.fa_magnifying_glass)
            };
        }

        /// <summary>
        ///     Animates the search icon when the text gets too long
        /// </summary>
        private void HandleSearchIconAnimations(GameTime gameTime)
        {
            var target = InputText.Width < Width - 20 - SearchIcon.Width ? 1 : 0;

            SearchIcon.Alpha = MathHelper.Lerp(SearchIcon.Alpha, target,
                (float) Math.Min(gameTime.ElapsedGameTime.TotalMilliseconds / 120, 1));
        }

        /// <summary>
        ///     Called when the user has stopped typing in the textbox
        /// </summary>
        /// <param name="filter"></param>
        private void StoppedTyping(string filter)
        {
            CurrentSearchQuery.Value = filter;
            PreviousSearchTerm = filter;
        }
    }
}