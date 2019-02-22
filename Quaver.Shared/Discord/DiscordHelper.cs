/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using Wobble.Logging;

namespace Quaver.Shared.Discord
{
    public static class DiscordHelper
    {
        /// <summary>
        /// </summary>
        public static DiscordRpc.RichPresence Presence;

        /// <summary>
        /// </summary>
        private static DiscordRpc.EventHandlers _handlers;

        /// <summary>
        /// </summary>
        /// <param name="clientId"></param>
        public static void Initialize(string clientId)
        {
            _handlers = new DiscordRpc.EventHandlers();

            _handlers.ReadyCallback += ReadyCallback;
            _handlers.DisconnectedCallback += DisconnectedCallback;
            _handlers.ErrorCallback += ErrorCallback;

            DiscordRpc.Initialize(clientId, ref _handlers, true, null);
        }

        /// <summary>
        /// Calls ReadyCallback(), DisconnectedCallback(), ErrorCallback().
        /// </summary>
        private static void RunCallbacks()
        {
            DiscordRpc.RunCallbacks();
            Logger.Debug($"Discord RPC Callbacks ran", LogType.Runtime);
        }

        /// <summary>
        /// Stop RPC.
        /// </summary>
        public static void Shutdown()
        {
            DiscordRpc.Shutdown();
            Logger.Important($"Discord RPC has shutdown", LogType.Runtime);
        }

        /// <summary>
        /// Called after RunCallbacks() when ready.
        /// </summary>
        private static void ReadyCallback()
        {
            Logger.Important($"Discord RPC is ready", LogType.Runtime);
        }

        /// <summary>
        /// Called after RunCallbacks() in cause of disconnection.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        private static void DisconnectedCallback(int errorCode, string message)
        {
            Logger.Important($"Discord RPC Disconnected: {errorCode} - {message}", LogType.Runtime);
        }

        /// <summary>
        /// Called after RunCallbacks() in cause of error.
        /// </summary>
        /// <param name="errorCode"></param>
        /// <param name="message"></param>
        private static void ErrorCallback(int errorCode, string message)
        {
            Logger.Error($"Discord RPC Error: {errorCode} - {message}", LogType.Runtime);
        }
    }
}
