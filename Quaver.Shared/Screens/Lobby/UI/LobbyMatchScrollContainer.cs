using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Config;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Graphics.Containers;
using Quaver.Shared.Online;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Input;
using Wobble.Logging;
using ColorHelper = Quaver.Shared.Helpers.ColorHelper;

namespace Quaver.Shared.Screens.Lobby.UI
{
    public class LobbyMatchScrollContainer : PoolableScrollContainer<MultiplayerGame>
    {
        /// <summary>
        /// </summary>
        public LobbyScreen Screen { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap NoMatchesFound { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        /// <param name="availableItems"></param>
        /// <param name="poolSize"></param>
        /// <param name="poolStartingIndex"></param>
        /// <param name="size"></param>
        /// <param name="contentSize"></param>
        /// <param name="startFromBottom"></param>
        public LobbyMatchScrollContainer(LobbyScreen screen, List<MultiplayerGame> availableItems, int poolSize,
            int poolStartingIndex, ScalableVector2 size, ScalableVector2 contentSize, bool startFromBottom = false)
            : base(availableItems, poolSize, poolStartingIndex, size, contentSize, startFromBottom)
        {
            Screen = screen;
            Tint = Color.Black;
            Alpha = 0.75f;

            Scrollbar.Tint = ColorHelper.HexToColor("#eeeeee");
            Scrollbar.Width = 6;
            Scrollbar.X = 14;
            ScrollSpeed = 150;
            EasingType = Easing.OutQuint;
            TimeToCompleteScroll = 1500;

            NoMatchesFound = new SpriteTextBitmap(FontsBitmap.GothamRegular, "No Matches Found. What are you waiting for? Create one!")
            {
                Parent = this,
                Alignment = Alignment.MidCenter,
                FontSize = 16,
                Visible = false
            };

            AddBorder(Color.White, 2);
            Border.Alpha = 1f;

            CreatePool();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            InputEnabled = GraphicsHelper.RectangleContains(ScreenRectangle, MouseManager.CurrentState.Position) && DialogManager.Dialogs.Count == 0;
            NoMatchesFound.Visible = AvailableItems.Count == 0;

            base.Update(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            Pool.ForEach(x => x.Destroy());
            base.Destroy();
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        protected override PoolableSprite<MultiplayerGame> CreateObject(MultiplayerGame item, int index)
            => new DrawableMultiplayerGame(this, item, index);

        /// <summary>
        /// </summary>
        public void AddOrUpdateGame(MultiplayerGame mp)
        {
            var index = Pool.FindIndex(x => x.Item.Id == mp.Id);

            if (index == -1)
            {
                AddObjectToBottom(mp, false);
                return;
            }

            Pool[index].UpdateContent(mp, index);
        }

        /// <summary>
        /// </summary>
        /// <param name="game"></param>
        public void DeleteObject(MultiplayerGame game) => RemoveObject(game);

        /// <summary>
        ///     Filters games in the queue by the search options
        /// </summary>
        /// <param name="search"></param>
        /// <param name="handleAnimations"></param>
        public void FilterGames(string search, bool handleAnimations)
        {
            if (OnlineManager.MultiplayerGames.Count == 0)
                return;

            lock (OnlineManager.MultiplayerGames)
            {
                var games = OnlineManager.MultiplayerGames.Values.ToList();

                if (!string.IsNullOrEmpty(search))
                {
                    games = OnlineManager.MultiplayerGames.Values.ToList().FindAll(x =>
                        x.Name.ToLower().Contains(search.ToLower())
                        || x.Map.ToLower().Contains(search.ToLower()));
                }

                games = games.FindAll(x =>
                {
                    if (!ConfigManager.LobbyFilterHasPassword.Value && x.HasPassword)
                        return false;

                    if (!ConfigManager.LobbyFilterFullGame.Value && x.Players.Count == x.MaxPlayers)
                        return false;

                    // TODO: This
                    if (ConfigManager.LobbyFilterHasFriends.Value)
                        return false;

                    var found = true;

                    if (ConfigManager.LobbyFilterOwnsMap.Value)
                    {
                        found = false;

                        foreach (var set in MapManager.Mapsets)
                        {
                            foreach (var map in set.Maps)
                            {
                                if (x.MapMd5 == map.Md5Checksum)
                                    return true;
                            }
                        }
                    }

                    return found;
                });

                foreach (var t in AvailableItems)
                {
                    var drawable = Pool.Find(x => x.Item.Id == t.Id);

                    if (drawable != null)
                    {
                        var index = games.FindIndex(x => x.Id == drawable.Item.Id);

                        if (index != -1)
                        {
                            if (drawable.Parent != ContentContainer)
                            {
                                if (handleAnimations)
                                {
                                    drawable.X = -drawable.Width;
                                    drawable.MoveToX(0, Easing.OutQuint, 600);
                                    drawable.Y = (PoolStartingIndex + index) * drawable.HEIGHT;
                                }

                                AddContainedDrawable(drawable);
                            }
                            else
                            {
                                if (handleAnimations)
                                    drawable.MoveToY((PoolStartingIndex + index) * drawable.HEIGHT, Easing.OutQuint, 600);
                                else
                                    drawable.Y = (PoolStartingIndex + index) * drawable.HEIGHT;
                            }

                            drawable.Index = index;
                        }
                        else
                        {
                            if (drawable.Parent == ContentContainer)
                                RemoveContainedDrawable(drawable);
                        }
                    }
                }

                // Resize the container
                if (ContentContainer.Children.Count == 0)
                    ContentContainer.Height = Height;
                else
                {
                    var totalUserHeight = Pool.First().Height * ContentContainer.Children.Count;
                    ContentContainer.Height = totalUserHeight > Height ? totalUserHeight : Height;
                }

                ContentContainer.ClearAnimations();
                ContentContainer.Y = 0;
                PreviousTargetY = 0;
                TargetY = 0;
            }
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="obj"></param>
        protected override void RemoveObject(MultiplayerGame obj)
        {
            var drawable = (DrawableMultiplayerGame) Pool.Find(x => x.Item.Id == obj.Id);

            AvailableItems.Remove(obj);
            AvailableItems.Remove(drawable.Item);
            drawable.Destroy();

            RemoveContainedDrawable(drawable);
            Pool.Remove(drawable);

            for (var i = 0; i < Pool.Count; i++)
            {
                Pool[i].MoveToY((PoolStartingIndex + i) * drawable.HEIGHT, Easing.OutQuint, 600);
                Pool[i].Index = i;
            }

            RecalculateContainerHeight();
        }
    }
}