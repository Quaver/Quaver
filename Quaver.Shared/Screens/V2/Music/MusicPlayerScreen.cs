using Quaver.Server.Client.Enums;
using Quaver.Server.Client.Objects;
using Quaver.Shared.Database.Maps;

namespace Quaver.Shared.Screens.V2.Music
{
    internal sealed class MusicPlayerScreen : PlaceholderScreen
    {
        public override QuaverScreenType Type { get; } = QuaverScreenType.Music;

        public MusicPlayerScreen() => View = new MusicPlayerScreenView(this);

        public override UserClientStatus GetClientStatus()
        {
            if (MapManager.Selected.Value == null)
                return new UserClientStatus(ClientStatus.Listening, -1, "-1", 1, "", 0);

            var map = MapManager.Selected.Value;
            return new UserClientStatus(ClientStatus.Listening, map.MapId, map.Md5Checksum,
                (byte) map.Mode, $"{map.Artist} - {map.Title}", 0);
        }
    }
}
