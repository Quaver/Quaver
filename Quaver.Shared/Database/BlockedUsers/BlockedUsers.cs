using System;
using System.Collections.Generic;
using System.Linq;
using Quaver.Shared.Graphics.Notifications;
using SQLite;
using Wobble.Logging;

namespace Quaver.Shared.Database.BlockedUsers
{
    public static class BlockedUsers
    {
        /// <summary>
        ///     Users who are currently blocked.
        /// </summary>
        public static List<BlockedUser> Users { get; private set; } = new List<BlockedUser>();

        /// <summary>
        ///     Creates the BlockedUsers database table
        /// </summary>
        public static void Load()
        {
            try
            {
                DatabaseManager.Connection.CreateTable<BlockedUser>();
                Users = DatabaseManager.Connection.Table<BlockedUser>().ToList();
                Logger.Important($"BlockedUsers table has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Adds a user to the block list
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        public static void Block(int userId, string username)
        {
            try
            {
                var user = new BlockedUser { UserId = userId };
                DatabaseManager.Connection.Insert(user);
                Users.Add(user);

                NotificationManager.Show(NotificationLevel.Info, $"You have successfully blocked {username}. All communications will be hidden from this user.");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, $"An error occurred while adding this user to your block list.");
            }
        }

        /// <summary>
        ///     Removes a user from the block list
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="username"></param>
        public static void Unblock(int userId, string username)
        {
            try
            {
                var user = Users.Find(x => x.UserId == userId);

                if (user != null)
                {
                    DatabaseManager.Connection.Delete(user);
                    Users.RemoveAll(x => x.UserId == userId);
                }

                NotificationManager.Show(NotificationLevel.Info, $"You have removed {username} from your block list.");
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                NotificationManager.Show(NotificationLevel.Error, "An error occurred while removing this user from your block list.");
            }
        }

        public static bool IsUserBlocked(int userId) => Users.Any(x => x.UserId == userId);
    }

    public class BlockedUser
    {
        [PrimaryKey]
        [AutoIncrement]
        public int Id { get; set; }

        [Unique]
        public int UserId { get; set; }
    }
}