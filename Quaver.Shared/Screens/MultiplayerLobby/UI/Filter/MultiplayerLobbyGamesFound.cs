using System.Collections.Generic;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.MultiplayerLobby.UI.Filter
{
    public class MultiplayerLobbyGamesFound : Container
    {
        /// <summary>
        /// </summary>
        private Bindable<List<MultiplayerGame>> VisibleGames { get; }

        /// <summary>
        ///    The amount of matches
        /// </summary>
        public SpriteTextPlus TextCount { get; private set; }

        /// <summary>
        ///     The text that displays "Matches Found"
        /// </summary>
        public SpriteTextPlus TextMatchesFound { get; private set; }

        /// <summary>
        ///     The amount of space between <see cref="TextCount"/> and <see cref="TextSpacing"/>
        /// </summary>
        private const int TextSpacing = 4;

        /// <summary>
        /// </summary>
        public MultiplayerLobbyGamesFound(Bindable<List<MultiplayerGame>> visibleGames)
        {
            VisibleGames = visibleGames;

            CreateTextCount();
            CreateTextMatchesFound();

            UpdateText();
            VisibleGames.ValueChanged += OnVisibleGamesChanged;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnMultiplayerGameInfoReceived += OnGameInfoReceived;
                OnlineManager.Client.OnGameDisbanded += OnGameDisbanded;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            // ReSharper disable once DelegateSubtraction
            VisibleGames.ValueChanged -= OnVisibleGamesChanged;

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnMultiplayerGameInfoReceived -= OnGameInfoReceived;
                OnlineManager.Client.OnGameDisbanded -= OnGameDisbanded;
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateTextCount()
        {
            TextCount = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0", 22)
            {
                Parent = this,
                Tint = Colors.MainAccent
            };
        }

        /// <summary>
        /// </summary>
        private void CreateTextMatchesFound()
        {
            TextMatchesFound = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.LatoBlack), "0 GAMES FOUND", 22)
            {
                Parent = this,
                X = TextCount.Width + TextSpacing
            };
        }

        /// <summary>
        ///     Updates the text with the proper state and updates the container size
        /// </summary>
        public void ScheduleUpdateText() => ScheduleUpdate(UpdateText);

        /// <summary>
        /// </summary>
        public void UpdateText()
        {
            var count = VisibleGames.Value.Count;

            TextCount.Text = $"{count:n0}";

            if (count == 0 || count > 1)
                TextMatchesFound.Text = "GAMES FOUND";
            else
                TextMatchesFound.Text = "GAME FOUND";

            TextMatchesFound.X = TextCount.Width + TextSpacing;

            Size = new ScalableVector2((int) (TextCount.Width + TextSpacing + TextMatchesFound.Width),
                (int) TextMatchesFound.Height);
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnVisibleGamesChanged(object sender, BindableValueChangedEventArgs<List<MultiplayerGame>> e)
            => ScheduleUpdateText();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameInfoReceived(object sender, MultiplayerGameInfoEventArgs e) => ScheduleUpdateText();

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnGameDisbanded(object sender, GameDisbandedEventArgs e) => ScheduleUpdateText();
    }
}