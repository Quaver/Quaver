/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System.Runtime.InteropServices;

namespace Quaver.Shared.Discord
{
    	// https://github.com/discordapp/discord-rpc/blob/master/examples/button-clicker/Assets/DiscordRpc.cs
	public class DiscordRpc
	{
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ReadyCallback();

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void DisconnectedCallback(int errorCode, string message);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public delegate void ErrorCallback(int errorCode, string message);

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

		[DllImport("discord-rpc", EntryPoint = "Discord_Initialize", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Initialize(string applicationId, ref EventHandlers handlers, bool autoRegister, string optionalSteamId);

		[DllImport("discord-rpc", EntryPoint = "Discord_UpdatePresence", CallingConvention = CallingConvention.Cdecl)]
		public static extern void UpdatePresence(ref RichPresence presence);

		[DllImport("discord-rpc", EntryPoint = "Discord_RunCallbacks", CallingConvention = CallingConvention.Cdecl)]
		public static extern void RunCallbacks();

		[DllImport("discord-rpc", EntryPoint = "Discord_Shutdown", CallingConvention = CallingConvention.Cdecl)]
		public static extern void Shutdown();
	}
}
