using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Shared.Config;
using Wobble.Graphics;

namespace Quaver.Shared.Screens.Gameplay.UI.Scoreboard
{
    public class Scoreboard : Container
    {
        /// <summary>
        ///     The list of users on the scoreboard.
        /// </summary>
        public List<ScoreboardUser> Users { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal Scoreboard(IEnumerable<ScoreboardUser> users)
        {
            Users = users.OrderBy(x => x.Processor.Health <= 0).ThenByDescending(x => x.Processor.Score).ToList();
            SetTargetYPositions();
            Users.ForEach(x => x.Y = x.TargetYPosition);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            // Tween to target Y positions
            Users.ForEach(user =>
            {
                user.Y = MathHelper.Lerp(user.Y, user.TargetYPosition, (float) Math.Min(dt / 120, 1));

                // Tween X Position based on if the scoreboard is hidden
                if (ConfigManager.ScoreboardVisible.Value)
                    user.X = MathHelper.Lerp(user.X, 0, (float) Math.Min(dt / 120, 1));
                else
                    user.X = MathHelper.Lerp(user.X, -user.Width - 10, (float) Math.Min(dt / 120, 1));
            });

            base.Update(gameTime);
        }

        /// <summary>
        ///     Calculates scores for each user.
        /// </summary>
        internal void CalculateScores()
        {
            Users.ForEach(x => x.CalculateScoreForNextObject());

            // Set each user's target position
            // Set Y positions.
            SetTargetYPositions();
        }

        /// <summary>
        ///     Sets the target y positions (where the scoreboard should move to)
        ///     Based on their rank.
        /// </summary>
        public void SetTargetYPositions()
        {
            var users = Users.OrderBy(x => x.Processor.Health <= 0).ThenByDescending(x => x.Processor.Score).ToList();

            for (var i = 0; i < users.Count; i++)
            {
                // Set new username and rank.
                users[i].Rank = i + 1;
                users[i].Username.Text = users[i].GetUsernameFormatted();

                // Normalize the position of the first one so that all the rest will be completely in the middle.
                if (i == 0)
                {
                    users[i].TargetYPosition = users.Count * -users[i].Height / 2f;
                    continue;
                }

                users[i].TargetYPosition = users[i - 1].TargetYPosition + users[i].Height + 8;
            }
        }
    }
}
