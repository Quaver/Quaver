using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Options.Search
{
    public class OptionsHeaderSearch : Textbox
    {
        /// <summary>
        ///     The current search term to be used
        /// </summary>
        private Bindable<string> CurrentSearchQuery { get; }

        /// <summary>
        /// </summary>
        private Bindable<bool> IsKeybindFocused { get; }

        /// <summary>
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="currentSearchQuery"></param>
        /// <param name="isKeybindFocused"></param>
        public OptionsHeaderSearch(Bindable<string> currentSearchQuery, Bindable<bool> isKeybindFocused)
            : base(new ScalableVector2(300, 34), FontManager.GetWobbleFont(Fonts.LatoBlack),20, "",
                "Search for options...")
        {
            CurrentSearchQuery = currentSearchQuery;
            IsKeybindFocused = isKeybindFocused;

            AllowSubmission = false;
            Tint = ColorHelper.HexToColor("#2F2F2F");
            Image = UserInterface.SearchBox;
            AlwaysFocused = false;

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
            AlwaysFocused = !IsKeybindFocused.Value;
            Focused = !IsKeybindFocused.Value;

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
        private void StoppedTyping(string filter) =>  CurrentSearchQuery.Value = filter;
    }
}