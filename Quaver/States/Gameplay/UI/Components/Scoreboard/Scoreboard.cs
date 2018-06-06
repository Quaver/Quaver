using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Config;
using Quaver.Graphics.Base;
using Quaver.Helpers;

namespace Quaver.States.Gameplay.UI.Components.Scoreboard
{
    internal class Scoreboard : Container
    {
        /// <summary>
        ///     The list of users on the scoreboard.
        /// </summary>
        private List<ScoreboardUser> Users { get; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        internal Scoreboard(IEnumerable<ScoreboardUser> users)
        {
            Users = users.OrderBy(x => x.Processor.Health <= 0).ThenByDescending(x => x.Processor.Score).ToList();
            SetTargetYPositions();
            Users.ForEach(x => x.PosY = x.TargetYPosition);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="dt"></param>
        internal override void Update(double dt)
        {     
            // Tween to target Y positions
            Users.ForEach(x =>
            {
                x.PosY = GraphicsHelper.Tween(x.TargetYPosition, x.PosY, Math.Min(dt / 120, 1));
                                            
                // Tween X Position based on if the scoreboard is hidden
                if (ConfigManager.ScoreboardVisible.Value)
                    x.PosX = GraphicsHelper.Tween(0, x.PosX, Math.Min(dt / 120, 1));
                else
                    x.PosX = GraphicsHelper.Tween(-x.SizeX - 10, x.PosX, Math.Min(dt / 120, 1));
            });
                
            base.Update(dt);
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
        private void SetTargetYPositions()
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
                    users[i].TargetYPosition = users.Count * -users[i].SizeY / 2f;
                    continue;
                }
                
                users[i].TargetYPosition = users[i - 1].TargetYPosition + users[i].SizeY + 8;
            } 
        }
    }
}