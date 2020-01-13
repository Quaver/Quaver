using System.Collections.Generic;
using System.Linq;
using MonoGame.Extended;
using Quaver.API.Enums;
using Quaver.API.Maps;
using Quaver.Shared.Audio;
using Quaver.Shared.Database.Maps;
using Quaver.Shared.Database.Scores;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Modifiers;
using Quaver.Shared.Screens.Gameplay;
using Quaver.Shared.Screens.Menu.UI.Jukebox;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.UI.Buttons;
using Wobble.Graphics.UI.Dialogs;

namespace Quaver.Shared.Screens.Options.Items.Custom
{
    public class OptionsItemCalibrateOffset : OptionsItem
    {
        /// <summary>
        /// </summary>
        private IconButton Button { get; }

        /// <summary>
        /// </summary>
        /// <param name="containerWidth"></param>
        /// <param name="containerRect"></param>
        /// <param name="name"></param>
        public OptionsItemCalibrateOffset(RectangleF containerRect, string name) : base(containerRect, name)
        {
            const float scale = 0.85f;

            Button = new IconButton(AssetLoader.LoadTexture2DFromFile(@"C:\users\swan\desktop/calibrate-offset-button.png"))
            {
                Parent = this,
                Alignment = Alignment.MidRight,
                X = -Name.X,
                Size = new ScalableVector2(215 * scale, 36 * scale),
                UsePreviousSpriteBatchOptions = true
            };

            Button.Clicked += (sender, args) =>
            {
                var game = GameBase.Game as QuaverGame;

                if (game == null)
                    return;

                if (game?.CurrentScreen?.Type != QuaverScreenType.Menu &&
                    game?.CurrentScreen?.Type != QuaverScreenType.Select)
                {
                    NotificationManager.Show(NotificationLevel.Warning, "Finish what you're doing before calibrating a new offset");
                    return;
                }

                var path = $"Quaver.Resources/Maps/Offset/offset.qua";

                var qua = Qua.Parse(GameBase.Game.Resources.Get(path));

                if (AudioEngine.Track != null && !AudioEngine.Track.IsDisposed && AudioEngine.Track.IsPlaying)
                    AudioEngine.Track.Pause();

                game.CurrentScreen?.Exit(() =>
                {
                    MapManager.Selected.Value = Map.FromQua(qua, path, true);
                    MapManager.Selected.Value.Qua = qua;

                    // Make the user not allow to fail.
                    ModManager.RemoveAllMods();
                    ModManager.AddMod(ModIdentifier.NoFail);

                    // Load the background (usually the default one)
                    BackgroundHelper.Load(MapManager.Selected.Value);
                    DialogManager.Dismiss(DialogManager.Dialogs.Last());

                    return new GameplayScreen(qua, "", new List<Score>(), null, false, 0, true);
                });
            };
        }
    }
}