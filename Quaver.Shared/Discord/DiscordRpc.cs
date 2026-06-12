/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
 */

using System;
using DiscordRPC;
using DiscordRPC.Logging;
using Wobble.Logging;

namespace Quaver.Shared.Discord
{
    // https://github.com/Lachee/discord-rpc-csharp.
    public class DiscordRpc
    {
        public delegate void ReadyCallback();

        public delegate void DisconnectedCallback(int errorCode, string message);

        public delegate void ErrorCallback(int errorCode, string message);

        private static EventHandlers _handlers;
        private static DiscordRpcClient _client;

        public struct EventHandlers
        {
            public ReadyCallback ReadyCallback;
            public DisconnectedCallback DisconnectedCallback;
            public ErrorCallback ErrorCallback;
        }

        // Values explanation and example: https://discordapp.com/developers/docs/rich-presence/how-to#updating-presence-update-presence-payload-fields
        [System.Serializable]
        public struct RichPresence
        {
            public const int MaxStateLength = 128, MaxDetailsLength = 128;
            public string State; /* max 128 bytes */
            public string Details; /* max 128 bytes */
            public long StartTimestamp;
            public long EndTimestamp;
            public string LargeImageKey; /* max 32 bytes */
            public string LargeImageText; /* max 128 bytes */
            public string SmallImageKey; /* max 32 bytes */
            public string SmallImageText; /* max 128 bytes */
            public string PartyId; /* max 128 bytes */
            public int PartySize;
            public int PartyMax;
            public string MatchSecret; /* max 128 bytes */
            public string JoinSecret; /* max 128 bytes */
            public string SpectateSecret; /* max 128 bytes */
            public bool Instance;
        }

        public static void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId)
        {
            _handlers = handlers;

            if (_client != null && !_client.IsDisposed)
                _client.Dispose();

            _client = new DiscordRpcClient(applicationId, -1, new ConsoleLogger { Level = DiscordRPC.Logging.LogLevel.None });

            _client.OnReady += (sender, message) => _handlers.ReadyCallback?.Invoke();
            _client.OnClose += (sender, message) => _handlers.DisconnectedCallback?.Invoke(message.Code, message.Reason);
            _client.OnError += (sender, message) => _handlers.ErrorCallback?.Invoke((int) message.Code, message.Message);

            if (autoRegister)
            {
                try
                {
                    _client.RegisterUriScheme(optionalSteamId);
                }
                catch (Exception e)
                {
                    // It appears to be a common bug where Discord RPC fails to register the URI scheme on some platforms like Linux.
                    // There is not yet a known fix, so we ignore the error.
                    Logger.Error($"Failed to register Discord RPC URI scheme: {e}", LogType.Runtime);
                }
            }

            _client.Initialize();
        }

        public static void UpdatePresence(ref RichPresence presence) => _client?.SetPresence(ToManagedPresence(presence));

        public static void RunCallbacks()
        {
            if (_client != null && !_client.AutoEvents)
                _client.Invoke();
        }

        public static void ClearPresence() => _client?.ClearPresence();

        public static void Shutdown()
        {
            _client?.Dispose();
            _client = null;
        }

        private static DiscordRPC.RichPresence ToManagedPresence(RichPresence presence)
        {
            return new DiscordRPC.RichPresence
            {
                State = Truncate(presence.State, RichPresence.MaxStateLength),
                Details = Truncate(presence.Details, RichPresence.MaxDetailsLength),
                Timestamps = CreateTimestamps(presence),
                Assets = CreateAssets(presence),
                Party = CreateParty(presence),
                Secrets = CreateSecrets(presence)
            };
        }

        private static DiscordRPC.Timestamps CreateTimestamps(RichPresence presence)
        {
            if (presence.StartTimestamp <= 0 && presence.EndTimestamp <= 0)
                return null;

            return new DiscordRPC.Timestamps
            {
                Start = presence.StartTimestamp > 0 ? FromUnixSeconds(presence.StartTimestamp) : (DateTime?) null,
                End = presence.EndTimestamp > 0 ? FromUnixSeconds(presence.EndTimestamp) : (DateTime?) null
            };
        }

        private static DiscordRPC.Assets CreateAssets(RichPresence presence)
        {
            if (string.IsNullOrEmpty(presence.LargeImageKey) &&
                string.IsNullOrEmpty(presence.LargeImageText) &&
                string.IsNullOrEmpty(presence.SmallImageKey) &&
                string.IsNullOrEmpty(presence.SmallImageText))
                return null;

            return new DiscordRPC.Assets
            {
                LargeImageKey = Truncate(presence.LargeImageKey, 32),
                LargeImageText = Truncate(presence.LargeImageText, 128),
                SmallImageKey = Truncate(presence.SmallImageKey, 32),
                SmallImageText = Truncate(presence.SmallImageText, 128)
            };
        }

        private static DiscordRPC.Party CreateParty(RichPresence presence)
        {
            if (presence.PartySize <= 0 && presence.PartyMax <= 0 && string.IsNullOrEmpty(presence.PartyId))
                return null;

            return new DiscordRPC.Party
            {
                ID = string.IsNullOrEmpty(presence.PartyId) ? "quaver" : Truncate(presence.PartyId, 128),
                Size = presence.PartySize,
                Max = presence.PartyMax
            };
        }

        private static DiscordRPC.Secrets CreateSecrets(RichPresence presence)
        {
            if (string.IsNullOrEmpty(presence.JoinSecret) &&
                string.IsNullOrEmpty(presence.SpectateSecret))
                return null;

            return new DiscordRPC.Secrets
            {
                JoinSecret = Truncate(presence.JoinSecret, 128),
                SpectateSecret = Truncate(presence.SpectateSecret, 128)
            };
        }

        private static string Truncate(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value) || value.Length <= maxLength)
                return value;

            return $"{value[..(maxLength - 1)]}…";
        }

        private static DateTime FromUnixSeconds(long unixSeconds) =>
            new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(unixSeconds);
    }
}
