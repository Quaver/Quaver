using Quaver.Config;
using Quaver.Modifiers;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Wobble.Screens;

namespace Quaver.Screens.SongSelect
{
    public class SongSelectScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Select;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <summary>
        /// </summary>
        public SongSelectScreen() => View = new SongSelectScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.Selecting,
            -1, "", (byte) ConfigManager.SelectedGameMode.Value, "", (long) ModManager.Mods);
    }
}