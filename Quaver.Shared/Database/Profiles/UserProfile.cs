using System;
using System.Collections.Generic;
using Quaver.API.Enums;
using Quaver.Server.Common.Objects;
using SQLite;

namespace Quaver.Shared.Database.Profiles
{
    public class UserProfile
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        /// <summary>
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// </summary>
        [Ignore]
        public Dictionary<GameMode, UserProfileStats> Stats { get; } = new Dictionary<GameMode, UserProfileStats>();

        /// <summary>
        /// </summary>
        [Ignore]
        public bool IsOnline { get; set; }

        /// <summary>
        /// </summary>
        public UserProfile PopulateStats()
        {
            foreach (GameMode mode in Enum.GetValues(typeof(GameMode)))
            {
                if (Stats.ContainsKey(mode))
                {
                    Stats[mode].FetchStats();
                    continue;
                }

                Stats.Add(mode, new UserProfileStats(this, mode));
            }

            return this;
        }
    }
}