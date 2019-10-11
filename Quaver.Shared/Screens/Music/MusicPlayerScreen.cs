using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;

namespace Quaver.Shared.Screens.Music
{
    public sealed class MusicPlayerScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Music;

        /// <summary>
        /// </summary>
        public MusicPlayerScreen() => View = new MusicPlayerScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1,"", 1, "", 0);
    }
}