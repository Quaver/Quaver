using Quaver.Server.Common.Objects;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Scheduling;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Settings;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;

namespace Quaver.Shared.Screens.Importing
{
    public class ImportingScreen : QuaverScreen
    {
        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override QuaverScreenType Type { get; } = QuaverScreenType.Importing;

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public sealed override ScreenView View { get; protected set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public override UserClientStatus GetClientStatus() => null;


        /// <summary>
        /// </summary>
        public ImportingScreen() => View = new ImportingScreenView(this);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void OnFirstUpdate()
        {
            ThreadScheduler.Run(() =>
            {
                MapsetImporter.ImportMapsetsInQueue();
                OnImportCompletion();
            });

            base.OnFirstUpdate();
        }

        /// <summary>
        ///     Called after all maps have been imported to the database.
        /// </summary>
        private void OnImportCompletion()
        {
            Logger.Important($"Map import has completed", LogType.Runtime);

            Exit(() =>
            {
                AudioEngine.Track?.Fade(10, 300);
                return new SelectScreen();
            });
        }
    }
}