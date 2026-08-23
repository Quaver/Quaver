/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Assets;
using Wobble.Assets;

namespace Quaver.Shared.Helpers
{
    public static class ImageDownloader
    {
        private static readonly object MapsetBannerLock = new object();

        private static Dictionary<int, Texture2D> MapsetBanners { get; } = new Dictionary<int, Texture2D>();

        private static Dictionary<int, Task<Texture2D>> MapsetBannerTasks { get; } = new Dictionary<int, Task<Texture2D>>();

        private static readonly object ProfileCoverLock = new object();

        private static Dictionary<int, Texture2D> ProfileCovers { get; } = new Dictionary<int, Texture2D>();

        private static Dictionary<int, Task<Texture2D>> ProfileCoverTasks { get; } =
            new Dictionary<int, Task<Texture2D>>();

        /// <summary>
        ///     Downloads a mapset banner and returns a stream for it
        /// </summary>
        /// <param name="id"></param>
        public static async Task<Texture2D> DownloadMapsetBanner(int id)
        {
            Task<Texture2D> task;

            lock (MapsetBannerLock)
            {
                if (MapsetBanners.ContainsKey(id))
                    return MapsetBanners[id];

                if (!MapsetBannerTasks.ContainsKey(id))
                    MapsetBannerTasks[id] = DownloadMapsetBannerTask(id);

                task = MapsetBannerTasks[id];
            }

            return await task;
        }

        private static async Task<Texture2D> DownloadMapsetBannerTask(int id)
        {
            Texture2D texture;

            var url = OnlineClient.GetBannerUrl(id);

            try
            {
                using (var webClient = new WebClient())
                {
                    var data = await webClient.DownloadDataTaskAsync(url);

                    using (var mem = new MemoryStream(data))
                    {
                        var img = AssetLoader.LoadTexture2D(mem);
                        texture = img;
                    }
                }
            }
            catch (Exception)
            {
                texture = UserInterface.MenuBackgroundBlurred;
            }

            lock (MapsetBannerLock)
            {
                MapsetBanners[id] = texture;
                MapsetBannerTasks.Remove(id);
            }

            return texture;
        }

        /// <summary>
        ///     Downloads and caches a donator's profile cover.
        /// </summary>
        /// <param name="userId"></param>
        public static async Task<Texture2D> DownloadProfileCover(int userId)
        {
            Task<Texture2D> task;

            lock (ProfileCoverLock)
            {
                if (ProfileCovers.TryGetValue(userId, out var cover))
                    return cover;

                if (!ProfileCoverTasks.TryGetValue(userId, out task))
                {
                    task = DownloadProfileCoverTask(userId);
                    ProfileCoverTasks[userId] = task;
                }
            }

            return await task;
        }

        private static async Task<Texture2D> DownloadProfileCoverTask(int userId)
        {
            Texture2D texture = null;

            try
            {
                using (var webClient = new WebClient())
                {
                    var url = $"https://cdn.quavergame.com/profile-covers/{userId}.jpg";
                    var data = await webClient.DownloadDataTaskAsync(url);

                    using (var mem = new MemoryStream(data))
                        texture = AssetLoader.LoadTexture2D(mem);
                }
            }
            catch (Exception)
            {
                // Missing or invalid covers use the dropdown's configured static color.
            }

            lock (ProfileCoverLock)
            {
                if (texture != null)
                    ProfileCovers[userId] = texture;

                ProfileCoverTasks.Remove(userId);
            }

            return texture;
        }
    }
}
