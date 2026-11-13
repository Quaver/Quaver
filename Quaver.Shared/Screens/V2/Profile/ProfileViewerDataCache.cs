using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Structures;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.API.User;
using Wobble.Logging;

namespace Quaver.Shared.Screens.V2.Profile
{
    internal sealed class ProfileViewerApiData
    {
        public APIResponseUsersFull Response { get; }

        public ProfileAccentPalette Accent { get; }

        public ProfileViewerApiData(APIResponseUsersFull response, ProfileAccentPalette accent)
        {
            Response = response;
            Accent = accent;
        }
    }

    internal sealed class ProfileViewerData
    {
        public APIResponseUsersFull Response { get; }

        public ProfileAccentPalette Accent { get; }

        public Texture2D Cover { get; }

        public UserRelationshipStatus Relationship { get; }

        public ProfileViewerData(ProfileViewerApiData apiData, Texture2D cover, UserRelationshipStatus relationship = null)
        {
            Response = apiData?.Response;
            Accent = apiData?.Accent;
            Cover = cover;
            Relationship = relationship;
        }
    }

    internal static class ProfileViewerDataCache
    {
        private static object SyncRoot { get; } = new object();

        private static Dictionary<int, Task<ProfileViewerApiData>> ApiRequests { get; } = new Dictionary<int, Task<ProfileViewerApiData>>();

        private static Dictionary<int, Task<ProfileViewerData>> ProfileRequests { get; } = new Dictionary<int, Task<ProfileViewerData>>();

        public static Task<ProfileViewerApiData> GetApiDataAsync(int userId)
        {
            if (userId <= 0)
                return Task.FromResult(new ProfileViewerApiData(null, null));

            lock (SyncRoot)
            {
                if (ApiRequests.TryGetValue(userId, out var request))
                    return request;

                request = Task.Run(() => LoadApiData(userId));
                ApiRequests[userId] = request;
                return request;
            }
        }

        public static Task<ProfileViewerData> GetAsync(User user)
        {
            var userId = user?.OnlineUser?.Id ?? 0;
            if (userId <= 0)
                return Task.FromResult(new ProfileViewerData(null, null));

            lock (SyncRoot)
            {
                if (ProfileRequests.TryGetValue(userId, out var request))
                    return request;

                var hasProfileCover = user.OnlineUser.UserGroups.HasFlag(UserGroups.Donator);
                request = LoadProfile(userId, hasProfileCover);
                ProfileRequests[userId] = request;
                return request;
            }
        }

        public static void Prefetch(User user)
        {
            _ = GetAsync(user);
        }

        public static async Task<ProfileViewerData> GetViewerDataAsync(User user)
        {
            var profileTask = GetAsync(user);
            var userId = user?.OnlineUser?.Id ?? 0;
            var client = OnlineManager.Client;
            var relationshipTask = userId > 0 && userId != OnlineManager.Self?.OnlineUser?.Id && client != null
                ? Task.Run(() => client.RetrieveUserRelationship(userId))
                : Task.FromResult<UserRelationshipStatus>(null);

            await Task.WhenAll(profileTask, relationshipTask);
            var profile = await profileTask;
            return new ProfileViewerData(new ProfileViewerApiData(profile.Response, profile.Accent), profile.Cover, await relationshipTask);
        }

        private static ProfileViewerApiData LoadApiData(int userId)
        {
            try
            {
                var response = new APIRequestUsersFull(userId).ExecuteRequest();
                var responseUser = response?.User;
                var accent = ProfileAccentColors.TryCreate(responseUser?.AccentColorCustomizable == true, responseUser?.AccentColor, out var palette) ? palette : null;
                return new ProfileViewerApiData(response, accent);
            }
            catch (Exception exception)
            {
                Logger.Error($"Could not preload profile viewer data for user {userId}: {exception}", LogType.Network);
                return new ProfileViewerApiData(null, null);
            }
        }

        private static async Task<ProfileViewerData> LoadProfile(int userId, bool hasProfileCover)
        {
            var apiDataTask = GetApiDataAsync(userId);
            var coverTask = hasProfileCover ? ImageDownloader.DownloadProfileCover(userId) : Task.FromResult<Texture2D>(null);

            await Task.WhenAll(apiDataTask, coverTask);
            return new ProfileViewerData(await apiDataTask, await coverTask);
        }
    }
}
