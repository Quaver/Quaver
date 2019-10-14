using System.Collections.Generic;
using Quaver.Server.Common.Enums;
using Quaver.Server.Common.Objects;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Modifiers;
using Wobble.Bindables;

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
        public Bindable<string> CurrentSearchQuery { get; set; }

        /// <summary>
        /// </summary>
        public Bindable<List<Mapset>> AvailableSongs { get; set; }

        /// <summary>
        /// </summary>
        public MusicPlayerScreen()
        {
            ModManager.RemoveSpeedMods();

            InitializeSearchQueryBindable();
            InitializeAvailableSongsBindable();

            View = new MusicPlayerScreenView(this);
        }

        /// <summary>
        ///     Initializes the bindable which stores the user's search query <see cref="CurrentSearchQuery"/>
        /// </summary>
        private void InitializeSearchQueryBindable() => CurrentSearchQuery = new Bindable<string>(null) { Value = "" };

        /// <summary>
        ///     Initializes the bindable which stores the available mapsets for the screen <see cref="AvailableSongs"/>
        /// </summary>
        private void InitializeAvailableSongsBindable()
            => AvailableSongs= new Bindable<List<Mapset>>(null) { Value = new List<Mapset>()};

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => new UserClientStatus(ClientStatus.InMenus, -1,"", 1, "", 0);
    }
}