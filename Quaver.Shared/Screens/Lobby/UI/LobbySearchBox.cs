using System;
using Microsoft.Xna.Framework;
using Quaver.Shared.Assets;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics;
using Quaver.Shared.Screens.Select;
using Wobble.Graphics;
using Wobble.Graphics.UI.Form;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class LobbySearchBox : Textbox
    {
        /// <summary>
        /// </summary>
        private LobbyScreenView View { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="view"></param>
        /// <param name="size"></param>
        public LobbySearchBox(LobbyScreenView view, ScalableVector2 size)
            : base(size, FontManager.GetWobbleFont(Fonts.LatoSemiBold), 12, "", "Search For Games")
        {
            View = view;

            Tint = Colors.BlueishDarkGray;
            Alpha = 0.75f;
            AllowSubmission = false;
            Image = UserInterface.SelectSearchBackground;
            InputText.X = 8;

            Cursor.Alignment = Alignment.MidLeft;
            InputText.Alignment = Alignment.MidLeft;
            AddBorder(Color.White, 2);
            Border.Alpha = 1f;

            OnStoppedTyping += StoppedTyping;
        }

        /// <summary>
        ///     Called when the user stops typing in the search box.
        /// </summary>
        /// <param name="val"></param>
        private void StoppedTyping(string val) => View.MatchContainer.FilterGames(val, true);
    }
}