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
using Quaver.Shared.Screens.Download;
using Wobble.Assets;

namespace Quaver.Shared.Helpers
{
    public static class ImageDownloader
    {
        private static readonly object MapsetBannerLock = new object();

        private static Dictionary<int, Task<Texture2D>> MapsetBannerTasks { get; } = new Dictionary<int, Task<Texture2D>>();

        /// <summary>
        ///     Downloads a mapset banner and returns a stream for it
        /// </summary>
        /// <param name="id"></param>
        public static async Task<Texture2D> DownloadMapsetBanner(int id)
        {
            Task<Texture2D> task;

            lock (MapsetBannerLock)
            {
                if (DownloadScreen.MapsetBanners.ContainsKey(id))
                    return DownloadScreen.MapsetBanners[id];

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
                DownloadScreen.MapsetBanners[id] = texture;
                MapsetBannerTasks.Remove(id);
            }

            return texture;
        }
    }
}
