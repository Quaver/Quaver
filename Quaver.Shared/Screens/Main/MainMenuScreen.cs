using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Config;
using Quaver.Shared.Discord;
using Quaver.Shared.Modifiers;

namespace Quaver.Shared.Screens.Main
{
    public sealed class MainMenuScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Menu;

        /// <summary>
        /// </summary>
        public MainMenuScreen()
        {
            // SetDiscordRichPresence();
            View = new MainMenuScreenView(this);
        }

        /// <summary>
        /// </summary>
        private void SetDiscordRichPresence()
        {
            DiscordHelper.Presence.Details = "Main Menu";
            DiscordHelper.Presence.State = "In the menus";
            DiscordHelper.Presence.EndTimestamp = 0;
            DiscordRpc.UpdatePresence(ref DiscordHelper.Presence);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus()
            => new UserClientStatus(ClientStatus.InMenus, -1, "", (byte) ConfigManager.SelectedGameMode.Value,
                "", (long) ModManager.Mods);
    }
}