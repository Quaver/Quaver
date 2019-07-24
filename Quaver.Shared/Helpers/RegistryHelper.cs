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