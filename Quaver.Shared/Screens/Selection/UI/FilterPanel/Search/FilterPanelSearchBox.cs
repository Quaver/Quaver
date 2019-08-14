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
        ///     The list of mapsets that are currently available.
        ///     In this case, we want to modify this list when the user searches for something new
        /// </summary>
        private Bindable<List<Mapset>> AvailableMapsets { get; }

        /// <summary>
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="availableMapsets"></param>
        /// <param name="initialText"></param>
        /// <param name="placeHolderText"></param>
        public FilterPanelSearchBox(Bindable<List<Mapset>> availableMapsets, string initialText, string placeHolderText)
            : base(new ScalableVector2(400, 40), FontManager.GetWobbleFont(Fonts.LatoBlack),22, initialText, placeHolderText)
        {
            AvailableMapsets = availableMapsets;

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
            AlwaysFocused = DialogManager.Dialogs.Count == 0;
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
                Size = new ScalableVector2(20, 20),
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
            lock (AvailableMapsets.Value)
            {
                ThreadScheduler.Run(() =>
                {
                    AvailableMapsets.Value = MapsetHelper.OrderMapsetsByConfigValue(MapsetHelper.SearchMapsets(MapManager.Mapsets, filter));
                });
            }
        }
    }
}