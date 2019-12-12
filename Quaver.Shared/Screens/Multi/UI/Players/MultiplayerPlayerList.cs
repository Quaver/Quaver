using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
using Wobble.Input;

namespace Quaver.Shared.Screens.Multi.UI.Players
{
    public class MultiplayerPlayerList : ScrollContainer
    {
        /// <summary>
        /// </summary>
        private Bindable<MultiplayerGame> Game { get; }

        /// <summary>
        /// </summary>
        public static ScalableVector2 ContainerSize { get; } = new ScalableVector2(DrawableMapset.WIDTH, 474);

        /// <summary>
        /// </summary>
        private List<MultiplayerSlot> Players { get; set; } = new List<MultiplayerSlot>();

        /// <summary>
        ///     The amount of slots allowed in a multiplayer match
        /// </summary>
        private const int SLOT_COUNT = 16;

        private Random RNG = new Random();

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
        ///     Adds a player to the list
        /// </summary>
        /// <param name="user"></param>
        /// <param name="sort"></param>
        public void AddPlayer(User user, bool sort = false)
        {
            var player = new MultiplayerPlayer(Game, this, user);
            Players.Add(player);
            AddContainedDrawable(player);

            RecalculateContainerHeight();

            if (sort)
                SortPlayers();
        }

        /// <summary>
        ///     Removes a player from the list
        /// </summary>
        /// <param name="user"></param>
        public void RemovePlayer(User user)
        {
            var player = Players.Find(x => x is MultiplayerPlayer p && p.User == user);

            if (player != null)
            {
                player.Destroy();
                RemoveContainedDrawable(player);
                Players.Remove(player);
                RecalculateContainerHeight();
            }

            SortPlayers();
        }

        /// <summary>
        /// </summary>
        private void CreatePlayers()
        {
            for (var i = 0; i < 12; i++)
            {
                AddPlayer(new User(new OnlineUser()
                {
                    Id = i,
                    Username = $"User_{i}",
                    CountryFlag = "KR",
                }));
            }
        }

        /// <summary>
        /// </summary>
        private void SortPlayers()
        {
            if (Game.Value.Ruleset == MultiplayerGameRuleset.Team)
                Players = SortTeamPlayers();
            else
                Players = SortFreeForAllPlayers();

            RemoveUnneededDrawables();

            // Position players
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
        private void RecalculateContainerHeight() => ContentContainer.Height = (Players.First().Height + 20) * 8 - 20;

        /// <summary>
        ///     Sorts players in team play
        /// </summary>
        /// <returns></returns>
        private List<MultiplayerSlot> SortTeamPlayers()
        {
            var sorted = new List<MultiplayerSlot>();

            var redTeam = Players.FindAll(x => x is MultiplayerPlayer p
                                               && OnlineManager.GetTeam(p.User.OnlineUser.Id, Game.Value) == MultiplayerTeam.Red
                                               && Game.Value.RefereeUserId != p.User.OnlineUser.Id);

            var blueTeam = Players.FindAll(x => x is MultiplayerPlayer p
                                                && OnlineManager.GetTeam(p.User.OnlineUser.Id, Game.Value) == MultiplayerTeam.Blue
                                                && Game.Value.RefereeUserId != p.User.OnlineUser.Id);

            var referee = Players.Find(x => x is MultiplayerPlayer p
                                            && Game.Value.RefereeUserId == p.User.OnlineUser.Id);

            for (var i = 0; i < SLOT_COUNT; i++)
            {
                // Add blue team players to the left column
                if (i % 2 == 0 && blueTeam.Count != 0)
                {
                    sorted.Add(blueTeam.First());
                    blueTeam.Remove(blueTeam.First());
                }
                // Add red players to the right column
                else if (i % 2 != 0 && redTeam.Count != 0)
                {
                    sorted.Add(redTeam.First());
                    redTeam.Remove(redTeam.First());
                }
                // Add the referee if possible
                else if (referee != null && !sorted.Contains(referee))
                    sorted.Add(referee);
                // Add empty slots
                else
                {
                    var emptySlot = new EmptyMultiplayerSlot();
                    sorted.Add(emptySlot);
                    AddContainedDrawable(emptySlot);
                }
            }

            return sorted;
        }

        /// <summary>
        ///     Sorts players in a free for all setting
        /// </summary>
        private List<MultiplayerSlot> SortFreeForAllPlayers()
        {
            var sorted = new List<MultiplayerSlot>();

            var players = Players.FindAll(x => x is MultiplayerPlayer p && Game.Value.RefereeUserId != p.User.OnlineUser.Id);
            var referee = Players.Find(x => x is MultiplayerPlayer p && Game.Value.RefereeUserId == p.User.OnlineUser.Id);

            // Add players first
            sorted = sorted.Concat(players).ToList();

            // Add referee at the bottom
            if (referee != null)
                sorted.Add(referee);

            var neededEmptySlots = SLOT_COUNT - sorted.Count;

            // Add empty slots
            for (var i = 0; i < neededEmptySlots; i++)
            {
                var emptySlot = new EmptyMultiplayerSlot();
                sorted.Add(emptySlot);
                AddContainedDrawable(emptySlot);
            }

            return sorted;
        }

        /// <summary>
        ///     Removes drawables that are no longer needed
        /// </summary>
        private void RemoveUnneededDrawables()
        {
            for (var i = ContentContainer.Children.Count - 1; i >= 0; i--)
            {
                var child = ContentContainer.Children[i];

                if (Players.Contains(child))
                    continue;

                child.Destroy();
                RemoveContainedDrawable(child);
            }
        }
    }
}