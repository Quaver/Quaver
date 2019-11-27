using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.Sprites.Text;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbySearchBox : Textbox
    {
        /// <summary>
        /// </summary>
        private Bindable<string> SearchQuery { get; }

        /// <summary>
        /// </summary>
        private Sprite SearchIcon { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerLobbySearchBox(Bindable<string> searchQuery) : base(new ScalableVector2(420, 40),
            FontManager.GetWobbleFont(Fonts.LatoBlack),22, "", "Search for games...")
        {
            SearchQuery = searchQuery;

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
        /// </summary>
        /// <param name="query"></param>
        private void StoppedTyping(string query) => SearchQuery.Value = query;
    }
}