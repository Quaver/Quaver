using System;
using System.Collections.Generic;
using Quaver.Shared.Config;
using Wobble.Bindables;
using Wobble.Logging;

namespace Quaver.Shared.Database.Profiles
{
    public static class UserProfileDatabaseCache
    {
        /// <summary>
        /// </summary>
        public static Bindable<UserProfile> Selected { get; private set; }

        /// <summary>
        /// </summary>
        public static BindableList<UserProfile> Profiles { get; } = new BindableList<UserProfile>(new List<UserProfile>())
        {
            Value = new List<UserProfile>()
        };

        /// <summary>
        /// </summary>
        public static void Load()
        {
            CreateTable();
            Profiles.Value = DatabaseManager.Connection.Table<UserProfile>().ToList();

            Selected = new Bindable<UserProfile>(null)
            {
                Value = new UserProfile()
            };

            switch (ConfigManager.SelectedProfileId.Value)
            {
                // Local profile is -1
                case -1:
                    break;
                // Online profile is 0, so just set the online property
                case 0:
                    Selected.Value.IsOnline = true;
                    break;
                default:
                    var profile = Profiles.Value.Find(x => x.Id == ConfigManager.SelectedProfileId.Value);

                    if (profile == null)
                        ConfigManager.SelectedProfileId.Value = -1;
                    else
                        Selected.Value = profile;
                    break;
            }

            Selected.ValueChanged += (sender, args) =>
            {
                if (args.Value.Id == 0)
                {
                    if (!args.Value.IsOnline)
                        ConfigManager.SelectedProfileId.Value = -1;
                    else
                        ConfigManager.SelectedProfileId.Value = 0;
                }
                else
                {
                    ConfigManager.SelectedProfileId.Value = args.Value.Id;
                };
            };
        }

        /// <summary>
        ///     Creates the `UserProfile` database table.
        /// </summary>
        private static void CreateTable()
        {
            try
            {
                DatabaseManager.Connection.CreateTable<UserProfile>();
                Logger.Important($"UserProfile table has been created", LogType.Runtime);
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                throw;
            }
        }

        /// <summary>
        ///     Creates a user profile and adds it to the database and loaded profiles
        /// </summary>
        /// <param name="profile"></param>
        /// <returns></returns>
        public static bool CreateProfile(UserProfile profile)
        {
            try
            {
                DatabaseManager.Connection.Insert(profile);

                if (!Profiles.Value.Contains(profile))
                    Profiles.Add(profile);

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return false;
            }
        }

        /// <summary>
        ///     Deletes a user profile from the database
        /// </summary>
        /// <returns></returns>
        public static bool DeleteProfile(UserProfile profile)
        {
            try
            {
                Profiles.Remove(profile);
                DatabaseManager.Connection.Delete(profile);

                return true;
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
                return false;
            }
        }
    }
}