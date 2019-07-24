using System;
using System.Reflection;
using Microsoft.Win32;
using Wobble.Logging;

namespace Quaver.Shared.Helpers
{
    public static class RegistryHelper
    {
        private const string UriScheme = "quaver";
        private const string FriendlyName = "Quaver";

        /// <summary>
        ///     (Windows) Registers a URI scheme for quaver://
        ///
        ///     Assumes the user is on windows and has a Quaver.exe file when trying to open the game
        ///
        ///     NOTE: Using .net run  will not work in this case and should typically only be used in published builds.
        ///           A workaround for this is to do a dotnet publish and have the Quaver.exe executable sitting in
        ///           the same directory.
        /// </summary>
        public static void RegisterUriScheme()
        {
            try
            {
                using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
                {
                    var applicationLocation = $"{AppDomain.CurrentDomain.BaseDirectory}Quaver.exe";
                    Logger.Important("Setting registry URI handler for localtion: " + applicationLocation, LogType.Runtime);

                    key.SetValue("", "URL:" + FriendlyName);
                    key.SetValue("URL Protocol", "");

                    using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                    {
                        defaultIcon.SetValue("", applicationLocation + ",1");
                    }

                    using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                    {
                        commandKey.SetValue("", "\"" + applicationLocation + "\" \"%1\"");
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Error(e, LogType.Runtime);
            }
        }
    }
}