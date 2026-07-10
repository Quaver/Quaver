using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Wobble.Graphics.Sprites.Text;
using Wobble.Managers;
using Quaver.Shared.Online;
using Quaver.Shared.Skinning;
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
        private SpriteTextPlus Count { get; }

        /// <summary>
        /// </summary>
        private SpriteTextPlus Status { get; }

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
        public PlayerListHeader(MultiplayerGame game)
        {
            Game = game;
            Size = new ScalableVector2(590, 36);
            Tint = Color.Black;
            Alpha = 0f;

            Count = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), MultiplayerLocalization.Get("PlayersCount", 0, 14))
            {
                Parent = this,
                Alignment = Alignment.TopRight,
                FontSize = 14,
            };

            Status = new SpriteTextPlus(FontManager.GetWobbleFont(Fonts.InterBold), MultiplayerLocalization.Get("WaitingToStart"))
            {
                Parent = this,
                Alignment = Alignment.TopLeft,
                FontSize = 14,
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
                if (OnlineManager.CurrentGame.CountdownStartTime == -1)
                {
                    Status.Tint = Color.White;

                    if (TimeSinceLastElipsis >= 600)
                    {
                        switch (Status.Text.Count(x => x == '.'))
                        {
                            case 0:
                                Status.Text = MultiplayerLocalization.Get("WaitingToStartOneDot");
                                break;
                            case 1:
                                Status.Text = MultiplayerLocalization.Get("WaitingToStartTwoDots");
                                break;
                            case 2:
                                Status.Text = MultiplayerLocalization.Get("WaitingToStartThreeDots");
                                break;
                            case 3:
                                Status.Text = MultiplayerLocalization.Get("WaitingToStart");
                                break;
                        }

                        TimeSinceLastElipsis = 0;
                    }
                }
                else
                {
                    var targetTime = OnlineManager.CurrentGame.CountdownStartTime + 5000;
                    var timeLeft = (int)((DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - targetTime) / 1000);

                    // ReSharper disable once CompareOfFloatsByEqualityOperator
                    if (LastNearestSecond != timeLeft)
                        CompletedThisInterval = false;

                    if (timeLeft >= LastNearestSecond && !CompletedThisInterval)
                    {
                        SkinManager.Skin.SoundHover.CreateChannel().Play();
                        CompletedThisInterval = true;
                    }

                    LastNearestSecond = timeLeft;
                    Status.Text = timeLeft > 0 ? MultiplayerLocalization.Get("WaitingForServer") : MultiplayerLocalization.Get("MatchIsNowStarting", Math.Abs(timeLeft) + 1);
                }

                if (Game.PlayerIds.Count != LastPlayerCount || Game.MaxPlayers != LastMaxPlayerCount)
                    Count.Text = MultiplayerLocalization.Get("PlayersCount", Game.PlayerIds.Count, Game.MaxPlayers);

                if (Game.InProgress)
                    Status.Text = MultiplayerLocalization.Get("MatchInProgress");

                LastPlayerCount = Game.PlayerIds.Count;
                LastMaxPlayerCount = Game.MaxPlayers;
            }

            base.Update(gameTime);
        }
    }
}
