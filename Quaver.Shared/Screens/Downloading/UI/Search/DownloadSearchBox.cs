using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Helpers;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Downloading.UI.Search
{
    public class DownloadSearchBox: Textbox
    {
        /// <summary>
        /// </summary>
        private Bindable<string> SearchQuery { get; }

        /// <summary>
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="searchQuery"></param>
        /// <param name="size"></param>
        public DownloadSearchBox(Bindable<string> searchQuery, ScalableVector2 size)
            : base(size, FontManager.GetWobbleFont(Fonts.LatoBlack), 21, "",
            "Type to search...")
        {
            SearchQuery = searchQuery;
            AllowSubmission = false;
            Focused = true;

            Image = UserInterface.SearchBox;
            Tint = ColorHelper.HexToColor("#181818");
            CreateSearchIcon();

            StoppedTypingActionCalltime = 250;
            OnStoppedTyping += s => SearchQuery.Value = s;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            if (DialogManager.Dialogs.Count != 0)
                Focused = false;
            
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
    }
}