using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics.Menu.Border.Components;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Wobble.Bindables;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;

namespace Quaver.Shared.Screens.Multi.UI.Footer
{
    public class IconTextButtonMultiplayerReady : IconTextButton
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public IconTextButtonMultiplayerReady(Bindable<MultiplayerGame> game) : base(FontAwesome.Get(FontAwesomeIcon.fa_check),
            FontManager.GetWobbleFont(Fonts.LatoBlack),"", (sender, args) =>
            {
                if (game.Value.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                {
                    OnlineManager.Client?.MultiplayerGameIsNotReady();

                    if (game.Value.HostId == OnlineManager.Self.OnlineUser.Id)
                        OnlineManager.Client?.MultiplayerGameStopCountdown();
                }
                else
                {
                    OnlineManager.Client?.MultiplayerGameIsReady();

                    if (game.Value.HostId == OnlineManager.Self.OnlineUser.Id)
                        OnlineManager.Client?.MultiplayerGameStartCountdown();
                }
            })
        {
            Game = game;
            UpdateReadyText();

            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnPlayerReady += OnPlayerReady;
                OnlineManager.Client.OnPlayerNotReady += OnPlayerNotReady;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            if (OnlineManager.Client != null)
            {
                OnlineManager.Client.OnPlayerReady -= OnPlayerReady;
                OnlineManager.Client.OnPlayerNotReady -= OnPlayerNotReady;
            }

            base.Destroy();
        }

        /// <summary>
        /// </summary>
        private void UpdateReadyText()
        {
            if (Game.Value.PlayersReady.Contains(OnlineManager.Self.OnlineUser.Id))
                UpdateText("Not Ready");
            else
                UpdateText("Ready Up");
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerNotReady(object sender, PlayerNotReadyEventArgs e) => AddScheduledUpdate(UpdateReadyText);

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPlayerReady(object sender, PlayerReadyEventArgs e) => AddScheduledUpdate(UpdateReadyText);
    }
}