using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Common.Helpers;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Audio;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
using Wobble;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;

namespace Quaver.Shared.Screens.Multiplayer.UI
{
    public class PlayerListHeader : Sprite
    {
        /// <summary>
        /// </summary>
        private MultiplayerGame Game { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Count { get; }

        /// <summary>
        /// </summary>
        private SpriteTextBitmap Status { get; }

        /// <summary>
        /// </summary>
        private double TimeSinceLastElipsis { get; set; }

        /// <summary>
        /// </summary>
        private double LastNearestSecond { get; set; }

        /// <summary>
        /// </summary>
        private bool CompletedThisInterval { get; set; }

        /// <summary>
        /// </summary>
        private int LastPlayerCount { get; set; } = -1;

        /// <summary>
        /// </summary>
        private int LastMaxPlayerCount { get; set; }

        /// <summary>
        /// </summary>
        private bool HostSelectingMapInLastFrame { get; set; }

        /// <summary>
        /// </summary>
        public PlayerListHeader(MultiplayerGame game)
        {
            Game = game;
            Size = new ScalableVector2(590, 36);
            Tint = Color.Black;
            Alpha = 0f;

            Count = new SpriteTextBitmap(FontsBitmap.GothamRegular, "(0/16) Players")
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                FontSize = 16,
            };

            Status = new SpriteTextBitmap(FontsBitmap.GothamRegular, "Waiting to start")
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                FontSize = 16,
            };

            // ReSharper disable once ObjectCreationAsStatement
            new Sprite
            {
                Parent = this,
                Alignment = Alignment.TopCenter,
                Size = new ScalableVector2(Width, 2),
                Y = Status.Y + Status.Height + 12,
                Alpha = 0.85f
            };
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            TimeSinceLastElipsis += gameTime.ElapsedGameTime.TotalMilliseconds;

            if (OnlineManager.CurrentGame != null)
            {
                if (OnlineManager.CurrentGame.HostSelectingMap)
                {
                    if (!HostSelectingMapInLastFrame)
                        Status.Text = "Host is selecting a map.";
                }
                else if (OnlineManager.CurrentGame.CountdownStartTime == -1)
                {
                    Status.Tint = Color.White;

                    if (TimeSinceLastElipsis >= 600)
                    {
                        switch (Status.Text.Count(x => x == '.'))
                        {
                            case 0:
                                Status.Text = "Waiting to start.";
                                break;
                            case 1:
                                Status.Text = "Waiting to start..";
                                break;
                            case 2:
                                Status.Text = "Waiting to start...";
                                break;
                            case 3:
                                Status.Text = "Waiting to start";
                                break;
                        }

                        TimeSinceLastElipsis = 0;
                    }
                }
                else
                {
                    var targetTime = OnlineManager.CurrentGame.CountdownStartTime + 5000;
                    var timeLeft = (int) ((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - targetTime) / 1000);

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (LastNearestSecond != timeLeft)
                        CompletedThisInterval = false;

                    if (timeLeft >= LastNearestSecond && !CompletedThisInterval)
                    {
                        SkinManager.Skin.SoundHover.CreateChannel().Play();
                        CompletedThisInterval = true;
                    }

                    LastNearestSecond = timeLeft;
                    Status.Text = $"Match is now starting: {Math.Abs(timeLeft) + 1}";
                }

                if (Game.PlayerIds.Count != LastPlayerCount || Game.MaxPlayers != LastMaxPlayerCount)
                    Count.Text = $"({Game.PlayerIds.Count}/{Game.MaxPlayers}) Players";
            }

            LastPlayerCount = Game.PlayerIds.Count;
            LastMaxPlayerCount = Game.MaxPlayers;

            // ReSharper disable once PossibleNullReferenceException
            HostSelectingMapInLastFrame = OnlineManager.CurrentGame.HostSelectingMap;
            base.Update(gameTime);
        }
    }
}