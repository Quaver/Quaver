using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Audio;
using Quaver.Config;
using Quaver.Helpers;
using Quaver.Logging;
using Quaver.Main;
using Quaver.Modifiers;
using Quaver.States.Gameplay;

namespace Quaver.Discord
{
    internal class DiscordController
    {
        public DiscordRPC.RichPresence presence;
        DiscordRPC.EventHandlers handlers;
        public string applicationId = "376180410490552320";
        public string optionalSteamId;

        /// <summary>
        ///     Initializes Discord RPC
        /// </summary>
        public void Initialize()
        {
            try
            {
                handlers = new DiscordRPC.EventHandlers();
                handlers.readyCallback = ReadyCallback;
                handlers.disconnectedCallback += DisconnectedCallback;
                handlers.errorCallback += ErrorCallback;
                DiscordRPC.Initialize(applicationId, ref handlers, true, optionalSteamId);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        public void ReadyCallback()
        {
            Console.WriteLine("Discord RPC is ready!");
        }

        public void DisconnectedCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }

        public void ErrorCallback(int errorCode, string message)
        {
            Console.WriteLine($"Error: {errorCode} - {message}");
        }

        /// <summary>
        ///     Responsible for changing the discord rich presence.
        /// </summary>
        /// <param name="details"></param>
        public static void ChangeDiscordPresence(string details, string state, double timeLeft = 0)
        {
            try
            {
                // Initialize Discord RPC if it isn't already
                if (!GameBase.DiscordRichPresencedInited)
                {
                    InitializeDiscordPresence();

                    // Create a new Rich Presence
                    GameBase.DiscordController.presence = new DiscordRPC.RichPresence()
                    {
                        details = "Idle",
                        largeImageKey = "quaver",
                        largeImageText = ConfigManager.Username.Value
                    };

                    GameBase.DiscordRichPresencedInited = true;
                }
                
                GameBase.DiscordController.presence.details = details;

                if (timeLeft != 0)
                {
                    // Get Current Unix Time
                    var epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    var unixDateTime = (DateTime.Now.ToLocalTime().ToUniversalTime() - epoch).TotalSeconds + GameplayTiming.StartDelay / 1000f;

                    // Set Discord presence to the "time left" specified.
                    GameBase.DiscordController.presence.endTimestamp = (long)(unixDateTime + timeLeft / 1000);
                }
                else
                {
                    GameBase.DiscordController.presence.endTimestamp = 0;
                }

                GameBase.DiscordController.presence.smallImageKey = "4k";

                if (GameBase.SelectedMap != null)
                {
                    // Set presence based Mode
                    switch (GameBase.SelectedMap.Mode)
                    {
                        case GameMode.Keys4:
                            GameBase.DiscordController.presence.smallImageText = "Mania: 4 Keys";
                            break;
                        case GameMode.Keys7:
                            GameBase.DiscordController.presence.smallImageText = "Mania: 7 Keys";
                            break;
                        default:
                            break;
                    }
                }

                GameBase.DiscordController.presence.state = state;
                DiscordRPC.UpdatePresence(ref GameBase.DiscordController.presence);
                
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for handling discord presence w/ mods if any exist.
        /// </summary>
        public static void ChangeDiscordPresenceGameplay(bool skippedSong, DiscordPlayingState state, string name = "")
        {
            if (!GameBase.DiscordRichPresencedInited || GameBase.SelectedMap == null)
                return;

            try
            {
                var mapString = $"{GameBase.SelectedMap.Qua.Artist} - {GameBase.SelectedMap.Qua.Title} [{GameBase.SelectedMap.Qua.DifficultyName}]";

                // Get the original map length. 
                double mapLength = Qua.FindSongLength(GameBase.SelectedMap.Qua) / GameBase.AudioEngine.PlaybackRate;

                // Get the new map length if it was skipped.
                if (skippedSong)
                    mapLength = (Qua.FindSongLength(GameBase.SelectedMap.Qua) - GameBase.AudioEngine.Position) / GameBase.AudioEngine.PlaybackRate;

                var sb = new StringBuilder();
                sb.Append(state + (name != "" ? " " + name : ""));
                sb.Append(ModHelper.GetActivatedModsString(true));

                ChangeDiscordPresence(mapString, sb.ToString(), mapLength);
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }

        /// <summary>
        ///     Responsible for initializing the Discord Presence
        /// </summary>
        private static void InitializeDiscordPresence()
        {
            if (GameBase.DiscordController != null)
                return;

            try
            {
                GameBase.DiscordController = new DiscordController();
                GameBase.DiscordController.Initialize();
            }
            catch (Exception e)
            {
                Logger.LogError(e, LogType.Runtime);
            }
        }
    }

    public enum DiscordPlayingState
    {
        Playing,
        Watching
    }
}
