using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Quaver.Config;
using Quaver.Logging;
using Quaver.Net;
using Quaver.Net.Constants;
using Quaver.Net.Handlers;
using Quaver.Net.Packets.Types.Client;

namespace Quaver.Online.Events
{
    public static class Login
    {
        /// <summary>
        ///     After logging in, the following event handler will execute
        ///     based on the error code received when logging in.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnLoginReplyEvent(object sender, LoginReplyEventArgs e)
        {
            switch (e.LoginErrorCode)
            {
                case LoginErrorCodes.Success:
                    Logger.LogSuccess($"Successfully logged in as {Flamingo.Self.Username} <{Flamingo.Self.SteamId}> ({Flamingo.Self.UserId})", LogType.Network);
                    Logger.LogSuccess($"There are currently {Flamingo.Clients.Count + 1} users online.", LogType.Network);

                    // Change the config's username to that of the currently logged in user.
                    Configuration.Username = Flamingo.Self.Username;
                    break;
                case LoginErrorCodes.Banned:
                    Logger.LogError($"You are banned.", LogType.Runtime);
                    break;
                case LoginErrorCodes.AlreadyConnected:
                    Logger.LogError($"You are already connected to the server in another location.", LogType.Network);
                    break;
                case LoginErrorCodes.ServerError:
                    Logger.LogError($"An internal server error has occurred", LogType.Network);
                    break;
                case LoginErrorCodes.CreateUsername:
                    // TODO: Fix this so that it's not blocking - can be solved with a UI
                    try
                    {
                        var createUsernameRespCode = ResponseCodes.None;

                        while (createUsernameRespCode == ResponseCodes.None || createUsernameRespCode == ResponseCodes.BadRequest)
                        {
                            // If the response code is 400, we want to display an error that the username
                            // is already taken, and that the user should choose a different one.
                            if (createUsernameRespCode == ResponseCodes.BadRequest)
                                Logger.LogError($"Username is invalid or already taken. Please choose a different one.", LogType.Network);

                            // TODO: Prompt to choose username
                            Logger.LogImportant($"Please choose a username to continue", LogType.Network);

                            var username = Console.ReadLine();
                            Logger.LogImportant($"Submitting username request for {username}...", LogType.Network);

                            createUsernameRespCode = FlamingoRequests.CreateUsername(username);
                            Logger.LogInfo(createUsernameRespCode.ToString(), LogType.Network);
                        }

                        // Handle unauthorized/success case.
                        switch (createUsernameRespCode)
                        {
                            case ResponseCodes.Unauthorized:
                            case ResponseCodes.ServerError:
                                // This is seen as a very very bad error and should be reported to developers ASAP.
                                Logger.LogError($"Error making create name request, please tell this to a developer ASAP!", LogType.Network);
                                Flamingo.Disconnect();
                                break;
                            case ResponseCodes.Success:
                                Logger.LogSuccess($"Successfully created username. Waiting for further server response...", LogType.Network);
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(ex, LogType.Network);
                        Flamingo.Disconnect();
                    }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }
}
