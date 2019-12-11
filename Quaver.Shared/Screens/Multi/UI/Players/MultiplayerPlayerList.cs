using System.Collections.Generic;
using System.Linq;
using Quaver.API.Enums;
using Quaver.Server.Client.Structures;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Selection.UI.Mapsets;
using TagLib.Riff;
using Wobble.Bindables;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public class MultiplayerPlayerList : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(DrawableMapset.WIDTH, 556);

        /// <summary>
        /// </summary>
        private List<MultiplayerSlot> Players { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerPlayerList(Bindable<MultiplayerGame> game) : base(ContainerSize, ContainerSize)
        {
            Game = game;
            Size = ContainerSize;
            Alpha = 0f;

            Scrollbar.Visible = false;
            InputEnabled = true;

            CreatePlayers();
            SortPlayers();
        }

        /// <summary>
        /// </summary>
        private void CreatePlayers()
        {
            Players = new List<MultiplayerSlot>();

            for (var i = 0; i < 16; i++)
            {
                //var player = new EmptyMultiplayerSlot { Parent = this };
                var player = new MultiplayerPlayer(Game, new User(new OnlineUser()
                {
                    Id = i,
                    Username = $"BaconGuy{i}",
                    CountryFlag = "KR",
                }));

                player.User.Stats[GameMode.Keys4] = new UserStats()
                {
                    Rank = 1
                };

                Players.Add(player);
                AddContainedDrawable(player);
            }

            RecalculateContainerHeight();
        }

        /// <summary>
        /// </summary>
        private void SortPlayers()
        {
            for (var i = 0; i < Players.Count; i++)
            {
                var player = Players[i];

                var row = i / 2;

                player.Y = row * (player.Height + 20);
                player.X = i % 2 == 0 ? 0 : Width - player.Width;
            }
        }

        /// <summary>
        ///     Recalculates the content height of the container
        /// </summary>
        private void RecalculateContainerHeight()
        {
            ContentContainer.Height = (Players.First().Height + 20) * 8 - 20;
        }
    }
}