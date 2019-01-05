/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2018 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.IO;
using System.Net;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Server.Client;
using Quaver.Shared.Online;
using Quaver.Shared.Screens.Download;
using Wobble;
using Wobble.Assets;
using Wobble.Logging;

namespace Quaver.Shared.Helpers
{
    public static class ImageDownloader
    {
        /// <summary>
        ///     Downloads a mapset banner and returns a stream for it
        /// </summary>
        /// <param name="id"></param>
        public static Texture2D DownloadMapsetBanner(int id)
        {
            if (DownloadScreen.MapsetBanners.ContainsKey(id))
                return DownloadScreen.MapsetBanners[id];

            var url = OnlineClient.GetBannerUrl(id);

            try
            {
                using (var webClient = new WebClient())
                {
                    var data = webClient.DownloadData(url);

                    using (var mem = new MemoryStream(data))
                    {
                        var img = AssetLoader.LoadTexture2D(mem);
                        DownloadScreen.MapsetBanners[id] = img;
                        return img;
                    }
                }
            }
            catch (Exception)
            {
                // ignored
            }

            // Make a transparent texture.
            var texture = new Texture2D(GameBase.Game.GraphicsDevice, 1, 1);
            texture.SetData(new[] { Color.Transparent });
            DownloadScreen.MapsetBanners[id] = texture;

            return texture;
        }
    }
}